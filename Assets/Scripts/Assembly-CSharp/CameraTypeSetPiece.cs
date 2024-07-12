using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[AddComponentMenu("Dash/Cameras/Set Piece")]
public class CameraTypeSetPiece : CameraType
{
	private class SubSpline
	{
		private float[] m_cachedDistances;

		private LightweightTransform[] m_cachedTransforms;

		private float m_length;

		public LightweightTransform this[float normalisedDistance]
		{
			get
			{
				if (normalisedDistance >= 0.99999f)
				{
					return m_cachedTransforms[m_cachedTransforms.Length - 1];
				}
				if (normalisedDistance == 0f)
				{
					return m_cachedTransforms[0];
				}
				return CalculateSubIndexTransform(normalisedDistance);
			}
		}

		public SubSpline(int knotIndex, LightweightTransform firstTransform, LightweightTransform[] knots, float splineStep, float splineResolution)
		{
			LightweightTransform[] array = new LightweightTransform[4]
			{
				(knotIndex > 0) ? knots[knotIndex - 1] : firstTransform,
				(knotIndex != -1) ? knots[knotIndex] : firstTransform,
				knots[knotIndex + 1],
				(knotIndex != knots.Length - 2) ? knots[knotIndex + 2] : knots[knots.Length - 1]
			};
			Spline.ControlList controlPoints = default(Spline.ControlList);
			Spline.ControlList controlPoints2 = default(Spline.ControlList);
			Spline.ControlList controlPoints3 = default(Spline.ControlList);
			for (int i = 0; i < 4; i++)
			{
				controlPoints[i] = array[i].Location;
				controlPoints2[i] = array[i].Forwards;
				controlPoints3[i] = array[i].Up;
			}
			float num = 0f;
			float num2 = splineResolution;
			float num3 = 0f;
			Vector3 vector = controlPoints[1];
			List<LightweightTransform> list = new List<LightweightTransform>();
			List<float> list2 = new List<float>();
			while (true)
			{
				float t = Mathf.Min(1f, num3);
				Vector3 vector2 = Spline.CalculatePoint(ref controlPoints, t);
				if (num2 >= splineResolution || num3 >= 1f)
				{
					Vector3 forward = Spline.CalculatePoint(ref controlPoints2, t);
					Vector3 upwards = Spline.CalculatePoint(ref controlPoints3, t);
					LightweightTransform item = new LightweightTransform(vector2, Quaternion.LookRotation(forward, upwards));
					list.Add(item);
					list2.Add(num);
					num2 = 0f;
				}
				float magnitude = (vector2 - vector).magnitude;
				num += magnitude;
				num2 += magnitude;
				vector = vector2;
				if (num3 >= 1f)
				{
					break;
				}
				num3 += splineStep;
			}
			m_cachedDistances = list2.ToArray();
			m_cachedTransforms = list.ToArray();
			m_length = num;
		}

		public void DebugDraw(Transform parent)
		{
			LightweightTransform[] cachedTransforms = m_cachedTransforms;
			foreach (LightweightTransform lightweightTransform in cachedTransforms)
			{
				Vector3 start = parent.TransformPoint(lightweightTransform.Location);
				Debug.DrawRay(start, Vector3.up * 0.2f, Color.cyan);
				Debug.DrawRay(start, Vector3.right * 0.2f, Color.cyan);
			}
		}

		private LightweightTransform CalculateSubIndexTransform(float normalisedDistance)
		{
			float num = normalisedDistance * m_length;
			int num2 = Array.BinarySearch(m_cachedDistances, num);
			if (num2 >= 0)
			{
				return m_cachedTransforms[num2];
			}
			int num3 = ~num2;
			int num4 = num3 - 1;
			float num5 = m_cachedDistances[num3] - m_cachedDistances[num4];
			float factor = (num - m_cachedDistances[num4]) / num5;
			return LightweightTransform.Lerp(m_cachedTransforms[num4], m_cachedTransforms[num3], factor);
		}
	}

	private LightweightTransform m_startCameraLocalTransform;

	private Transform m_entryTransform;

	private Transform[] m_sonicGates;

	private SubSpline[] m_cameraSpline;

	private bool m_finished;

	private bool m_isFirstUpdate;

	private int m_previousBestKeyframe = -1;

	private int m_startTransitionFrame;

	[SerializeField]
	private float m_splineStepping = 0.05f;

	[SerializeField]
	private float m_splineResolution = 0.2f;

	[SerializeField]
	private float m_introSmoothing = 0.2f;

	public override bool EnableSmoothing => true;

	public void Awake()
	{
		m_cameraSpline = null;
	}

	public void ActivateSetPiece(Transform entryTransform, IEnumerable<TrackSegment.CameraKeyframe> cameraKeyframes, int startTranstionFrame)
	{
		m_startTransitionFrame = startTranstionFrame;
		m_sonicGates = cameraKeyframes.Select((TrackSegment.CameraKeyframe kf) => kf.SonicGate).ToArray();
		m_entryTransform = entryTransform;
		m_startCameraLocalTransform = new LightweightTransform(entryTransform.InverseTransformPoint(Camera.main.transform.position), Camera.main.transform.rotation);
		LightweightTransform[] array = new LightweightTransform[cameraKeyframes.Count()];
		for (int i = 0; i < cameraKeyframes.Count(); i++)
		{
			TrackSegment.CameraKeyframe cameraKeyframe = cameraKeyframes.ElementAt(i);
			ref LightweightTransform reference = ref array[i];
			reference = new LightweightTransform(entryTransform.InverseTransformPoint(cameraKeyframe.CameraTransform.Location), cameraKeyframe.CameraTransform.Orientation);
		}
		m_cameraSpline = new SubSpline[cameraKeyframes.Count()];
		for (int j = -1; j < cameraKeyframes.Count() - 1; j++)
		{
			m_cameraSpline[j + 1] = new SubSpline(j, m_startCameraLocalTransform, array, m_splineStepping, m_splineResolution);
		}
		base.transform.position = Camera.main.transform.position;
		base.transform.rotation = Camera.main.transform.rotation;
		m_finished = false;
		m_isFirstUpdate = true;
		BehindCamera.Instance.SetActiveCamera(this, m_introSmoothing);
		m_previousBestKeyframe = -1;
	}

	private void Cleanup()
	{
		m_cameraSpline = null;
		m_sonicGates = null;
		m_entryTransform = null;
	}

	private void LateUpdate()
	{
		if (m_cameraSpline == null)
		{
			return;
		}
		if (m_entryTransform == null)
		{
			Cleanup();
			return;
		}
		Utils.ClosestPoint closestPoint = CalculateNormalisedClosestPoint(m_entryTransform, m_sonicGates.First());
		int num = -1;
		for (int i = 0; i < m_sonicGates.Count() - 1; i++)
		{
			Utils.ClosestPoint closestPoint2 = CalculateNormalisedClosestPoint(m_sonicGates.ElementAt(i), m_sonicGates.ElementAt(i + 1));
			if (closestPoint2.SqrError < closestPoint.SqrError)
			{
				num = i;
				closestPoint = closestPoint2;
			}
		}
		if (num < m_previousBestKeyframe)
		{
			num = m_previousBestKeyframe;
		}
		SubSpline subSpline = m_cameraSpline[num + 1];
		LightweightTransform lightweightTransform = subSpline[closestPoint.LineDistance];
		LightweightTransform lightweightTransform2 = new LightweightTransform(m_entryTransform.TransformPoint(lightweightTransform.Location), lightweightTransform.Orientation);
		if (m_isFirstUpdate)
		{
			base.transform.position = lightweightTransform2.Location;
			base.transform.rotation = lightweightTransform2.Orientation;
			base.CachedLookAt = base.transform.position + base.transform.forward * 5f;
		}
		else
		{
			base.transform.position = lightweightTransform2.Location;
			Debug.DrawLine(base.transform.position, lightweightTransform2.Location, Color.red);
			base.CachedLookAt = base.transform.position + lightweightTransform2.Forwards * 10f;
			base.transform.rotation = Quaternion.LookRotation((base.CachedLookAt - base.transform.position).normalized, lightweightTransform2.Up);
			Debug.DrawLine(base.transform.position, base.CachedLookAt, Color.green);
		}
		if (!m_finished && num == m_startTransitionFrame)
		{
			BehindCamera instance = BehindCamera.Instance;
			instance.ResetToGameCamera(0.5f);
			m_finished = true;
		}
		m_isFirstUpdate = false;
		m_previousBestKeyframe = num;
	}

	private float FindTargetSpeed()
	{
		SonicSplineTracker tracker = Sonic.Tracker;
		return (!(tracker == null)) ? tracker.Speed : 0f;
	}

	private Utils.ClosestPoint CalculateNormalisedClosestPoint(Transform fromT, Transform toT)
	{
		Vector3 vector = toT.position - fromT.position;
		float magnitude = vector.magnitude;
		Vector3 lineDir = vector / magnitude;
		Utils.ClosestPoint result = Utils.CalculateClosestPoint(fromT.position, lineDir, magnitude, Sonic.MeshTransform.position);
		result.LineDistance /= magnitude;
		return result;
	}
}
