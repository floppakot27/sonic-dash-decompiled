using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using UnityEngine;

[ExecuteInEditMode]
[AddComponentMenu("Dash/Track/Track Segment")]
public class TrackSegment : MonoBehaviour
{
	public class LaneCollection : IEnumerable, IEnumerable<int>
	{
		private IList<WorldTransformLock> m_lanePositions;

		public LaneCollection(IEnumerable<JoinPoint> joinPoints, float laneWidth)
		{
			m_lanePositions = new List<WorldTransformLock>();
			foreach (JoinPoint join in joinPoints)
			{
				int laneCount = join.Lanes.Count();
				Func<int, Vector3> func = (int laneIndex) => laneCount switch
				{
					1 => join.Transform.position, 
					2 => join.Transform.position + (laneIndex - join.Lanes.First()) * join.Transform.right * (0f - laneWidth), 
					3 => join.Transform.position + (laneIndex - join.Lanes.ElementAt(1)) * join.Transform.right * (0f - laneWidth), 
					_ => throw new UnityException("can't handle track joins with more than three pieces!"), 
				};
				foreach (int lane in join.Lanes)
				{
					Vector3 pos = func(lane);
					LightweightTransform worldTransform = new LightweightTransform(pos, Quaternion.identity);
					m_lanePositions.Add(new WorldTransformLock(join.Transform, worldTransform));
				}
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public IEnumerator<int> GetEnumerator()
		{
			return Enumerable.Range(0, m_lanePositions.Count()).GetEnumerator();
		}

		public bool HasLane(int laneIndex)
		{
			return laneIndex < m_lanePositions.Count();
		}

		public Vector3 GetLanePosition(int laneIndex)
		{
			return m_lanePositions[laneIndex].CurrentTransform.Location;
		}
	}

	[StructLayout(0, Size = 1)]
	public struct JoinPoint
	{
		public Transform Transform { get; private set; }

		public IEnumerable<int> Lanes { get; private set; }

		public string Name => Transform.gameObject.name;

		public JoinPoint(Transform t, IEnumerable<int> lanes)
		{
			Transform = t;
			Lanes = lanes;
		}

		public void StitchTo(Transform t)
		{
			Transform parent = Transform.parent.parent;
			Transform.parent.parent = null;
			Matrix4x4 worldToLocalMatrix = Transform.worldToLocalMatrix;
			Matrix4x4 localToWorldMatrix = Transform.root.localToWorldMatrix;
			Matrix4x4 matrix4x = worldToLocalMatrix * localToWorldMatrix;
			Matrix4x4 localToWorldMatrix2 = t.localToWorldMatrix;
			Matrix4x4 matrix4x2 = Utils.CopyMatrixWithNoScaling(localToWorldMatrix2);
			Matrix4x4 m = matrix4x2 * matrix4x;
			Transform.root.position = m.MultiplyPoint(Vector3.zero);
			Transform.root.rotation = Utils.GetQuaternionFromMatrix(m);
			Transform.parent.parent = parent;
		}
	}

	public struct CameraKeyframe
	{
		private Transform m_cameraTransform;

		public bool IsValid => m_cameraTransform != null;

		public string CameraName { get; private set; }

		public LightweightTransform CameraTransform => new LightweightTransform(m_cameraTransform.position, Quaternion.LookRotation(-m_cameraTransform.forward, m_cameraTransform.up));

		public Transform SonicGate { get; private set; }

		public CameraKeyframe(Transform cameraTransform, Transform sonicGate)
		{
			m_cameraTransform = cameraTransform;
			SonicGate = sonicGate;
			CameraName = cameraTransform.gameObject.name;
		}
	}

	private IList<GameplayTemplate> m_templatesStartedOnThisSegment;

	private IList<Transform> m_templateContainers;

	private IList<JoinPoint> m_entrances;

	private IList<JoinPoint> m_exits;

	private List<CameraKeyframe> m_setpieceCameraKeyframes;

	private LaneCollection m_entranceLanes;

	private LaneCollection m_exitLanes;

	private BehindCamera m_gameCameraControl;

	private TrackSegment m_nextPiece;

	private TrackSegment m_previousSegment;

	private bool m_endSegment;

	private List<Enemy> m_enemies;

	private IList<Spline> m_splines;

	[SerializeField]
	private float m_laneWidth = 2f;

	[SerializeField]
	private int m_setPieceStartCameraTransitionKeyFrame = 10;

	public Spline MiddleSpline => m_splines[1];

	public TrackSegment NextSegment
	{
		get
		{
			return m_nextPiece;
		}
		set
		{
			m_nextPiece = value;
		}
	}

	public TrackSegment PreviousSegment
	{
		get
		{
			return m_previousSegment;
		}
		set
		{
			m_previousSegment = value;
		}
	}

	public List<Enemy> Enemies => m_enemies;

	public IEnumerable<Transform> TemplateContainers => m_templateContainers;

	public IEnumerable<GameplayTemplate> TemplatesStartedOnThis => m_templatesStartedOnThisSegment;

	public float TrackPosition { get; set; }

	public float DifficultyRelevantTrackPosition { get; set; }

	public TrackDatabase.TrackPiece Template { get; set; }

	public bool IsMissingAnyLanes => TrackDatabase.IsGapType(Template.PieceType.Type);

	public LaneCollection EntranceLanes => m_entranceLanes;

	public LaneCollection ExitLanes => m_exitLanes;

	public IEnumerable<JoinPoint> EntrancePoints => m_entrances;

	public IEnumerable<JoinPoint> ExitPoints => m_exits;

	public IEnumerable<CameraKeyframe> SetpieceCameraKeyframes => m_setpieceCameraKeyframes;

	public int SetPieceStartCameraTransitionKeyFrame => m_setPieceStartCameraTransitionKeyFrame;

	public bool IsSetpieceCameraAvailable => SetpieceCameraKeyframes.Any();

	public bool IsGameplayEnabled { get; private set; }

	public bool EndSegment
	{
		get
		{
			return m_endSegment;
		}
		set
		{
			m_endSegment = value;
		}
	}

	private TrackSegment()
	{
		m_enemies = new List<Enemy>();
	}

	private void Awake()
	{
		m_gameCameraControl = BehindCamera.Instance;
		RegenerateMetadata();
		ListenForTrackers();
		IsGameplayEnabled = true;
		m_templateContainers = new List<Transform>();
		m_templatesStartedOnThisSegment = new List<GameplayTemplate>(2);
		m_splines = new List<Spline>(GetComponentsInChildren<Spline>());
	}

	private void Start()
	{
		if (!FeatureSupport.IsSupported("Water Scene"))
		{
			DeleteReflections();
		}
	}

	private void DeleteReflections()
	{
		foreach (Transform item in base.transform)
		{
			if (item.gameObject.name.EndsWith("_ref"))
			{
				UnityEngine.Object.Destroy(item.gameObject);
			}
		}
	}

	private void OnSpawned()
	{
	}

	private void OnDespawned()
	{
		SpawnableObject[] componentsInChildren = GetComponentsInChildren<SpawnableObject>();
		SpawnableObject[] array = componentsInChildren;
		foreach (SpawnableObject spawnableObject in array)
		{
			spawnableObject.transform.parent = null;
			spawnableObject.RequestDestruction();
		}
		NextSegment = null;
		Enemies.Clear();
		foreach (Transform templateContainer in TemplateContainers)
		{
			UnityEngine.Object.Destroy(templateContainer.gameObject);
		}
		m_templateContainers.Clear();
		m_templatesStartedOnThisSegment.Clear();
	}

	public IEnumerable<TrackSegment> GetTrackFromHere()
	{
		TrackSegment currentTrackSegment = this;
		while (currentTrackSegment != null)
		{
			yield return currentTrackSegment;
			currentTrackSegment = currentTrackSegment.NextSegment;
		}
	}

	public bool IsTrackReady(float forTrackDelta)
	{
		float untilTrackPosition = TrackPosition + forTrackDelta;
		IEnumerable<TrackSegment> source = GetTrackFromHere().TakeWhile((TrackSegment segment) => segment.TrackPosition <= untilTrackPosition);
		return source.All((TrackSegment segment) => segment.m_splines.All((Spline s) => !s.calculateIsDirty()));
	}

	public void OnStartingTemplate(GameplayTemplate newTemplate)
	{
		m_templatesStartedOnThisSegment.Add(newTemplate);
	}

	public void AddTemplateContainer(Transform container)
	{
		container.parent = base.transform;
		container.localPosition = Vector3.zero;
		m_templateContainers.Add(container);
	}

	public bool IsMissingLane(Track.Lane lane)
	{
		TrackDatabase.PieceType type = Template.PieceType.Type;
		return TrackDatabase.IsGapInLane(type, lane);
	}

	public static TrackSegment GetSegmentOfSpline(Spline s)
	{
		if (null != s)
		{
			return s.getTrackSegment();
		}
		return null;
	}

	public void RegenerateMetadata()
	{
		m_entrances = new List<JoinPoint>();
		m_exits = new List<JoinPoint>();
		m_setpieceCameraKeyframes = new List<CameraKeyframe>();
		int num = 0;
		int num2 = 0;
		Component component = GetComponentInChildren(typeof(MeshRenderer));
		if (component == null)
		{
			component = this;
		}
		SortedList<int, Pair<Transform, Transform>> sortedList = new SortedList<int, Pair<Transform, Transform>>();
		Transform[] componentsInChildren = component.GetComponentsInChildren<Transform>();
		Transform[] array = componentsInChildren;
		foreach (Transform transform in array)
		{
			string text = transform.gameObject.name;
			if (text.StartsWith("cam_"))
			{
				int key = int.Parse(text.Split('_')[2]);
				Pair<Transform, Transform> value = ((!sortedList.ContainsKey(key)) ? default(Pair<Transform, Transform>) : sortedList[key]);
				if (text.StartsWith("cam_transform"))
				{
					value.First = transform;
				}
				else
				{
					if (!text.StartsWith("cam_sonic"))
					{
						throw new UnityException("unrecognised camera hint \"" + text + "\" in segment " + ToString());
					}
					value.Second = transform;
				}
				sortedList[key] = value;
			}
			else
			{
				int num3 = CalculateLaneCount(text);
				if (text.StartsWith("in_"))
				{
					m_entrances.Add(new JoinPoint(transform, Enumerable.Range(num2, num3)));
					num2 += num3;
				}
				else if (text.StartsWith("out_"))
				{
					m_exits.Add(new JoinPoint(transform, Enumerable.Range(num, num3)));
					num += num3;
				}
			}
		}
		m_entrances.OrderBy((JoinPoint joinPoint) => joinPoint.Name);
		m_exits.OrderBy((JoinPoint joinPoint) => joinPoint.Name);
		m_setpieceCameraKeyframes.AddRange(sortedList.Values.Select((Pair<Transform, Transform> keyframe) => new CameraKeyframe(keyframe.First, keyframe.Second)));
		m_entranceLanes = new LaneCollection(m_entrances, m_laneWidth);
		m_exitLanes = new LaneCollection(m_exits, m_laneWidth);
		m_splines = new List<Spline>(GetComponentsInChildren<Spline>());
	}

	public void SetGameplayEnabled(bool isEnabled)
	{
		IsGameplayEnabled = isEnabled;
		SegmentEnabler.RequestUpdate(this);
	}

	public void PushTransformsToEnable(Queue<Transform> transformUpdateQueue)
	{
		transformUpdateQueue.Enqueue(base.transform);
		foreach (Transform item in base.transform)
		{
			transformUpdateQueue.Enqueue(item);
		}
	}

	public void SetGameplayEnabledOn(Transform t)
	{
		if (t == base.transform)
		{
			Renderer[] components = GetComponents<Renderer>();
			foreach (Renderer renderer in components)
			{
				renderer.enabled = IsGameplayEnabled;
			}
		}
		else if (TemplateContainers.Contains(t))
		{
			t.gameObject.SetActive(IsGameplayEnabled);
		}
		else
		{
			Renderer[] componentsInChildren = t.GetComponentsInChildren<Renderer>();
			foreach (Renderer renderer2 in componentsInChildren)
			{
				renderer2.enabled = IsGameplayEnabled;
			}
		}
	}

	private static int CalculateLaneCount(string joinName)
	{
		MatchCollection matchCollection = Regex.Matches(joinName, "(in|out)_(\\d)_(\\d+)");
		foreach (Match item in matchCollection)
		{
			if (item.Groups.Count < 3 || !item.Groups[2].Success)
			{
				continue;
			}
			return int.Parse(item.Groups[2].Value);
		}
		return 0;
	}

	private void ListenForTrackers()
	{
		foreach (Spline spline in m_splines)
		{
			spline.OnTrackStart += OnCharacterEnterSpline;
			spline.OnTrackEnd += OnCharacterLeaveSpline;
		}
	}

	private void OnCharacterEnterSpline(Spline spline)
	{
		if (IsSetpieceCameraAvailable && m_gameCameraControl != null && m_gameCameraControl.SetPieceCamera != null)
		{
			Transform entryTransform = MiddleSpline[0].transform;
			m_gameCameraControl.SetPieceCamera.ActivateSetPiece(entryTransform, SetpieceCameraKeyframes, SetPieceStartCameraTransitionKeyFrame);
		}
	}

	private void OnCharacterLeaveSpline(Spline spline)
	{
	}

	public Track.Lane GetLaneOfSpline(Spline spline)
	{
		return (Track.Lane)m_splines.IndexOf(spline);
	}

	public Spline GetSpline(int lane)
	{
		return m_splines[lane];
	}

	public bool IsSmallIsland(Track.Lane lane)
	{
		if (!IsMissingLane(lane))
		{
			int num = 0;
			int num2 = 0;
			TrackSegment trackSegment = this;
			float num3 = MiddleSpline.Length;
			while (trackSegment != null)
			{
				if (trackSegment.EndSegment)
				{
					return false;
				}
				if (!trackSegment.IsMissingLane(lane))
				{
					num3 += trackSegment.MiddleSpline.Length;
					num++;
					trackSegment = trackSegment.NextSegment;
					continue;
				}
				break;
			}
			trackSegment = this;
			while (num2 < 20 && trackSegment != null && !trackSegment.IsMissingLane(lane))
			{
				num3 += trackSegment.MiddleSpline.Length;
				num2++;
				trackSegment = trackSegment.PreviousSegment;
			}
			if (num3 < 80f)
			{
				return true;
			}
			return false;
		}
		return false;
	}

	public float GetDistanceToNextGap(Track.Lane lane, float currentPosition)
	{
		TrackSegment trackSegment = this;
		if (trackSegment.IsMissingLane(lane))
		{
			return 0f;
		}
		while (trackSegment != null)
		{
			if (trackSegment.IsMissingLane(lane))
			{
				return trackSegment.TrackPosition - currentPosition;
			}
			trackSegment = trackSegment.NextSegment;
		}
		return 1000000f;
	}
}
