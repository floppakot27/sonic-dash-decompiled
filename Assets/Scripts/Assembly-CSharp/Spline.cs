using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

[AddComponentMenu("Dash/Spline")]
[ExecuteInEditMode]
public class Spline : MonoBehaviour
{
	public enum Type
	{
		Linear,
		Curved
	}

	public enum ControlPoint
	{
		Start,
		End
	}

	public class TrackerContext
	{
		public Direction_1D Direction;

		public int Knot;

		public float SplineDistance;

		public float KnotTraversal;

		public float LastDistanceRequest;

		public bool IsReset => Knot < 0;

		public TrackerContext()
		{
			Direction = Direction_1D.Forwards;
			Reset();
		}

		public TrackerContext Clone()
		{
			return MemberwiseClone() as TrackerContext;
		}

		public void Reset()
		{
			Knot = -1;
			KnotTraversal = -1f;
			SplineDistance = -1f;
			LastDistanceRequest = 0f;
		}
	}

	public struct ControlList
	{
		public Vector3 Control0;

		public Vector3 Control1;

		public Vector3 Control2;

		public Vector3 Control3;

		public Vector3 this[int index]
		{
			get
			{
				return index switch
				{
					0 => Control0, 
					1 => Control1, 
					2 => Control2, 
					_ => Control3, 
				};
			}
			set
			{
				switch (index)
				{
				case 0:
					Control0 = value;
					break;
				case 1:
					Control1 = value;
					break;
				case 2:
					Control2 = value;
					break;
				case 3:
					Control3 = value;
					break;
				}
			}
		}
	}

	private struct CatmullRomWalker
	{
		private Vector3 m_firstSplineCP;

		private Vector3 m_lastSplineCP;

		private IList<SplineKnot> m_knots;

		public CatmullRomWalker(IList<SplineKnot> knots, Spline spline)
		{
			m_knots = knots;
			m_firstSplineCP = spline.GetControlPoint(ControlPoint.Start);
			m_lastSplineCP = spline.GetControlPoint(ControlPoint.End);
		}

		public void GetControlPointsFor(int knotIndex, ref ControlList controlPointsOut)
		{
			controlPointsOut.Control0 = ((knotIndex != 0) ? m_knots[knotIndex - 1].transform.position : m_firstSplineCP);
			controlPointsOut.Control1 = m_knots[knotIndex].transform.position;
			controlPointsOut.Control2 = m_knots[knotIndex + 1].transform.position;
			controlPointsOut.Control3 = ((knotIndex >= m_knots.Count() - 2) ? m_lastSplineCP : m_knots[knotIndex + 2].transform.position);
		}

		public void GetControlUpsFor(int knotIndex, ref ControlList controlPointsOut)
		{
			controlPointsOut.Control0 = ((knotIndex != 0) ? m_knots[knotIndex - 1].transform.up : m_knots.First().transform.up);
			controlPointsOut.Control1 = m_knots[knotIndex].transform.up;
			controlPointsOut.Control2 = m_knots[knotIndex + 1].transform.up;
			controlPointsOut.Control3 = ((knotIndex >= m_knots.Count() - 2) ? m_knots.Last().transform.up : m_knots[knotIndex + 2].transform.up);
		}
	}

	public delegate void OnEvent(Spline spline);

	public static readonly int MaxNodeCount = int.MaxValue;

	public static readonly Color DefaultSplineColour = new Color(0.5f, 0.5f, 0.5f, 1f);

	public static readonly Color DefaultKnotColour = new Color(0.9f, 0.6f, 0.2f, 1f);

	[SerializeField]
	private Type m_splineType;

	[SerializeField]
	private Color m_splineColour;

	[SerializeField]
	private Color m_knotColour;

	[SerializeField]
	private int m_renderSegments;

	[SerializeField]
	private float m_splineAccuracy = 1f;

	[SerializeField]
	private List<SplineKnot> m_knots;

	private float m_length;

	private int m_cachedKnotCount;

	private bool m_finalised;

	private TrackSegment m_trackSegment;

	public Type SplineType
	{
		get
		{
			return m_splineType;
		}
		set
		{
			if (m_splineType != value)
			{
				forceRecacheNextUpdate();
			}
			m_splineType = value;
		}
	}

	public Color SplineColour
	{
		get
		{
			return m_splineColour;
		}
		set
		{
			m_splineColour = value;
		}
	}

	public Color KnotColour
	{
		get
		{
			return m_knotColour;
		}
		set
		{
			m_knotColour = value;
		}
	}

	public int RenderSegments
	{
		get
		{
			return m_renderSegments;
		}
		set
		{
			m_renderSegments = value;
		}
	}

	public float SplineAccuracy
	{
		get
		{
			return m_splineAccuracy;
		}
		set
		{
			if (m_splineAccuracy != value)
			{
				forceRecacheNextUpdate();
			}
			m_splineAccuracy = Mathf.Max(0.05f, value);
		}
	}

	public float Length
	{
		get
		{
			return m_length;
		}
		private set
		{
			m_length = value;
		}
	}

	public int KnotCount => m_knots.Count;

	public IEnumerable<SplineKnot> Knots => m_knots;

	public SplineKnot this[int knotIndex] => m_knots[knotIndex];

	[method: MethodImpl(32)]
	public event OnEvent OnTrackStart;

	[method: MethodImpl(32)]
	public event OnEvent OnTrackEnd;

	public void Awake()
	{
		SplineType = Type.Curved;
		SplineColour = DefaultSplineColour;
		KnotColour = DefaultKnotColour;
		RenderSegments = 5;
		forceRecacheNextUpdate();
	}

	public void OnSpawned()
	{
		base.enabled = true;
		forceRecacheNextUpdate();
	}

	public void Start()
	{
		Cache();
		m_trackSegment = base.gameObject.transform.parent.parent.GetComponent<TrackSegment>();
	}

	public TrackSegment getTrackSegment()
	{
		return m_trackSegment;
	}

	public void Finalise()
	{
		Cache();
		m_finalised = true;
		for (int i = 0; i < m_knots.Count; i++)
		{
			m_knots[i].gameObject.SetActive(value: false);
		}
	}

	public void Update()
	{
		if (!m_finalised)
		{
			Cache();
		}
	}

	public void OnStartTracking()
	{
		if (this.OnTrackStart != null)
		{
			this.OnTrackStart(this);
		}
	}

	public void OnEndTracking()
	{
		if (this.OnTrackEnd != null)
		{
			this.OnTrackEnd(this);
		}
	}

	public SplineKnot.EndBehaviour GetBehaviourAt(ControlPoint end)
	{
		return (end != 0) ? m_knots.Last().BehaviourIfEnd : m_knots.First().BehaviourIfEnd;
	}

	public Vector3 GetControlPoint(ControlPoint point)
	{
		Vector3 result = default(Vector3);
		if (m_knots == null || m_knots.Count() < 3)
		{
			return result;
		}
		if (point == ControlPoint.Start)
		{
			return GetAdditionalControlPoint(m_knots[0].transform, m_knots[1].transform);
		}
		return GetAdditionalControlPoint(m_knots.Last().transform, m_knots[m_knots.Count() - 2].transform);
	}

	public float CalculateCurvatureAt(float position, float scope)
	{
		LightweightTransform lightweightTransform = GetTransform(position);
		float distance = Mathf.Max(0f, position - scope);
		LightweightTransform lightweightTransform2 = GetTransform(distance);
		float distance2 = Mathf.Max(Length, position + scope);
		LightweightTransform lightweightTransform3 = GetTransform(distance2);
		Vector3 normalized = (lightweightTransform.Location - lightweightTransform2.Location).normalized;
		Vector3 normalized2 = (lightweightTransform3.Location - lightweightTransform.Location).normalized;
		float num = Vector3.Angle(normalized, normalized2);
		return num / scope * 2f;
	}

	public Utils.ClosestPoint EstimateDistanceAlongSpline(Vector3 position)
	{
		if (m_knots == null)
		{
			return default(Utils.ClosestPoint);
		}
		float num = float.MaxValue;
		float lineDistance = 0f;
		float num2 = 0f;
		bool flag = false;
		for (int i = 0; i < m_knots.Count() - 1; i++)
		{
			SplineKnot splineKnot = m_knots[i];
			float num3 = num2;
			num2 += splineKnot.Length;
			bool flag2 = flag;
			if (i < m_knots.Count() - 2)
			{
				SplineKnot splineKnot2 = m_knots[i + 1];
				Vector3 rhs = position - splineKnot2.transform.position;
				float num4 = Vector3.Dot(splineKnot2.TowardsNextKnot, rhs);
				bool flag3 = num4 >= 0f;
				flag = !flag3;
				if (flag3)
				{
					continue;
				}
			}
			if (!flag2)
			{
				Utils.ClosestPoint closestPoint = Utils.CalculateClosestPoint(splineKnot.transform.position, splineKnot.TowardsNextKnot, splineKnot.WorldDistanceToNextKnot, position);
				if (num > closestPoint.SqrError)
				{
					num = closestPoint.SqrError;
					lineDistance = num3 + closestPoint.LineDistance;
				}
			}
		}
		Utils.ClosestPoint result = default(Utils.ClosestPoint);
		result.LineDistance = lineDistance;
		result.SqrError = num;
		return result;
	}

	public LightweightTransform GetTransform(float distance)
	{
		return GetTransform(distance, null);
	}

	public LightweightTransform GetTransform(float distance, TrackerContext context)
	{
		if (m_knots == null)
		{
			return default(LightweightTransform);
		}
		if (distance == 0f)
		{
			return new LightweightTransform(m_knots[0].transform);
		}
		float distance2 = Mathf.Max(0f, distance);
		return (SplineType != 0 && m_knots.Count() >= 3) ? GetCatmullRomTransform(distance2, m_knots, context) : GetLinearTransform(distance2, m_knots);
	}

	public void OnKnotDestruction(SplineKnot knotToBeDestroyed)
	{
		m_knots.Remove(knotToBeDestroyed);
		RenameKnots();
	}

	public SplineKnot CreateNewKnot(ControlPoint pointToCreate)
	{
		return CreateNewKnot(pointToCreate, overridePosition: false, Vector3.zero);
	}

	public SplineKnot CreateNewKnot(ControlPoint pointToCreate, bool overridePosition, Vector3 atPos)
	{
		GameObject gameObject = null;
		int childCount = base.transform.childCount;
		if (childCount == 0)
		{
			gameObject = new GameObject("Knot 0");
			gameObject.AddComponent<SplineKnot>();
			gameObject.transform.position = ((!overridePosition) ? base.transform.position : atPos);
			gameObject.transform.parent = base.transform;
			m_knots = new List<SplineKnot>();
			m_knots.Add(gameObject.GetComponent<SplineKnot>());
			RenameKnots();
		}
		else
		{
			int index = ((pointToCreate != 0) ? (childCount - 1) : 0);
			int newIndex = ((pointToCreate != 0) ? childCount : 0);
			SplineKnot splineKnot = m_knots[index];
			gameObject = CreateKnotCopyFrom(splineKnot, newIndex).gameObject;
			gameObject.transform.position = ((!overridePosition) ? (splineKnot.transform.position + splineKnot.Tangent * ((pointToCreate != 0) ? 1f : (-1f))) : atPos);
		}
		return gameObject.GetComponent<SplineKnot>();
	}

	public SplineKnot AddKnotBefore(SplineKnot beforeKnot)
	{
		if (m_knots == null)
		{
			throw new UnityException("cannot add relative knot to empty spline");
		}
		int knotPosition = GetKnotPosition(beforeKnot);
		if (knotPosition == 0)
		{
			return CreateNewKnot(ControlPoint.Start);
		}
		return CreateKnotCopyFrom(m_knots[knotPosition], knotPosition);
	}

	public SplineKnot AddKnotAfter(SplineKnot afterKnot)
	{
		if (m_knots == null)
		{
			throw new UnityException("cannot add relative knot to empty spline");
		}
		int knotPosition = GetKnotPosition(afterKnot);
		if (knotPosition == base.transform.childCount - 1)
		{
			return CreateNewKnot(ControlPoint.End);
		}
		return CreateKnotCopyFrom(m_knots[knotPosition], knotPosition + 1);
	}

	public int GetKnotPosition(SplineKnot knotToFind)
	{
		if (m_knots == null)
		{
			throw new UnityException("No knots to search in " + base.gameObject.name);
		}
		if (knotToFind.Spline != this)
		{
			throw new UnityException(string.Concat("knot ", knotToFind, " is part of spline ", knotToFind.Spline, ", not us, ", ToString()));
		}
		int num = 0;
		foreach (SplineKnot knot in m_knots)
		{
			if (knot == knotToFind)
			{
				return num;
			}
			num++;
		}
		throw new UnityException("Couldn't find position of knot " + knotToFind.gameObject.name);
	}

	public static Vector3 CalculatePoint(ref ControlList controlPoints, float t)
	{
		float num = t * t;
		float num2 = num * t;
		return 0.5f * (2f * controlPoints[1] + (-controlPoints[0] + controlPoints[2]) * t + (2f * controlPoints[0] - 5f * controlPoints[1] + 4f * controlPoints[2] - controlPoints[3]) * num + (-controlPoints[0] + 3f * controlPoints[1] - 3f * controlPoints[2] + controlPoints[3]) * num2);
	}

	public static Vector3 CalculateTangent(ref ControlList controlPoints, float t)
	{
		float num = t * t;
		return (0.5f * ((-controlPoints[0] + 3f * controlPoints[1] - 3f * controlPoints[2] + controlPoints[3]) * 3f * num + (2f * controlPoints[0] - 5f * controlPoints[1] + 4f * controlPoints[2] - controlPoints[3]) * 2f * t - controlPoints[0] + controlPoints[2])).normalized;
	}

	public bool calculateIsDirty()
	{
		if (m_knots == null)
		{
			return m_cachedKnotCount != 0;
		}
		if (m_cachedKnotCount != m_knots.Count())
		{
			return true;
		}
		for (int i = 0; i < m_knots.Count(); i++)
		{
			if (m_knots[i].IsDirty)
			{
				return true;
			}
		}
		return false;
	}

	public override string ToString()
	{
		return base.gameObject.ToString() + ((!(base.transform.parent != null) || !(base.transform.parent != null)) ? string.Empty : (" of " + base.transform.parent.parent.gameObject.name));
	}

	private void Cache()
	{
		if (m_finalised)
		{
			return;
		}
		if (!calculateIsDirty())
		{
			base.enabled = false;
			return;
		}
		if (SplineType == Type.Linear || m_knots == null || m_knots.Count() < 3)
		{
			CacheLinearKnots(m_knots);
		}
		else
		{
			CacheCatmullRomKnots(m_knots);
		}
		m_knots.ToList().ForEach(delegate(SplineKnot knot)
		{
			knot.ClearDirtyFlag();
		});
		m_cachedKnotCount = ((m_knots != null) ? m_knots.Count() : 0);
	}

	private SplineKnot CreateKnotCopyFrom(SplineKnot originalKnot, int newIndex)
	{
		GameObject gameObject = (GameObject)Object.Instantiate(originalKnot.gameObject);
		gameObject.name = "Knot " + newIndex;
		gameObject.transform.parent = base.transform;
		SplineKnot component = gameObject.GetComponent<SplineKnot>();
		if (newIndex > 0)
		{
			float num = m_knots.Take(newIndex - 1).Sum((SplineKnot knot) => knot.Length);
			float num2 = num + m_knots[newIndex - 1].Length;
			Cache();
			component.transform.position = GetTransform(0.5f * (num + num2)).Location;
		}
		m_knots.Insert(newIndex, component);
		RenameKnots();
		return component;
	}

	private void RenameKnots()
	{
		if (m_knots == null)
		{
			return;
		}
		int num = 0;
		foreach (SplineKnot knot in m_knots)
		{
			knot.transform.name = $"Knot {num}";
			num++;
		}
	}

	private void forceRecacheNextUpdate()
	{
		m_cachedKnotCount = -1;
	}

	private void DrawLinearSpline(IList<SplineKnot> knots)
	{
		for (int i = 0; i < knots.Count() - 1; i++)
		{
			SplineKnot splineKnot = knots[i];
			SplineKnot splineKnot2 = knots[i + 1];
			Vector3 position = splineKnot.transform.position;
			Vector3 position2 = splineKnot2.transform.position;
			Gizmos.DrawLine(position, position2);
		}
	}

	private void DrawCatmullRomSpline(IList<SplineKnot> knots)
	{
		if (knots.Count() < 3)
		{
			return;
		}
		CatmullRomWalker catmullRomWalker = new CatmullRomWalker(knots, this);
		Vector3 from = knots[0].transform.position;
		for (int i = 0; i < knots.Count() - 1; i++)
		{
			ControlList controlPointsOut = default(ControlList);
			catmullRomWalker.GetControlPointsFor(i, ref controlPointsOut);
			float num = 0f;
			float num2 = 1f / (float)((RenderSegments < 1) ? 1 : RenderSegments);
			float num3;
			for (; num <= 1f; num += num3)
			{
				Vector3 vector = CalculatePoint(ref controlPointsOut, num);
				Gizmos.DrawLine(from, vector);
				from = vector;
				num3 = num2;
				if (1f - num < num3 && 1f - num > 0.0001f)
				{
					num3 = 1f - num;
				}
			}
		}
	}

	private Vector3 GetAdditionalControlPoint(Transform source, Transform dest)
	{
		Vector3 vector = dest.position - source.position;
		vector.x = 0f - vector.x;
		vector.y = 0f - vector.y;
		vector.z = 0f - vector.z;
		return vector + source.position;
	}

	private LightweightTransform GetLinearTransform(float distance, IList<SplineKnot> knots)
	{
		float num = distance;
		LightweightTransform result;
		for (int i = 0; i < knots.Count() - 1; i++)
		{
			float length = knots[i].Length;
			if (length > num)
			{
				float t = num / length;
				Vector3 normalized = Vector3.Lerp(knots[i].transform.up, knots[i + 1].transform.up, t).normalized;
				result = default(LightweightTransform);
				result.Location = Vector3.Lerp(knots[i].transform.position, knots[i + 1].transform.position, t);
				result.Orientation = Quaternion.LookRotation(knots[i].TowardsNextKnot, normalized);
				return result;
			}
			num -= length;
		}
		result = default(LightweightTransform);
		result.Location = knots.Last().transform.position;
		result.Orientation = Quaternion.LookRotation(knots.Last().TowardsNextKnot);
		return result;
	}

	private LightweightTransform GetCatmullRomTransform(float distance, IList<SplineKnot> knots, TrackerContext context)
	{
		CatmullRomWalker catmullRomWalker = new CatmullRomWalker(knots, this);
		if (context != null)
		{
			if (context.Direction != 0)
			{
				context.Reset();
			}
			context.LastDistanceRequest = distance;
		}
		float num = ((context != null && !context.IsReset) ? context.SplineDistance : 0f);
		int num2 = ((context != null && !context.IsReset) ? context.Knot : 0);
		for (int i = num2; i < knots.Count() - 1; i++)
		{
			float num3 = Mathf.Max(distance - num, 0f);
			if (num3 > knots[i].Length)
			{
				num += knots[i].Length;
				continue;
			}
			ControlList controlPointsOut = default(ControlList);
			catmullRomWalker.GetControlPointsFor(i, ref controlPointsOut);
			ControlList controlPointsOut2 = default(ControlList);
			catmullRomWalker.GetControlUpsFor(i, ref controlPointsOut2);
			float num4 = m_splineAccuracy / knots[i].Length;
			float num5 = 0f;
			Vector3 vector = controlPointsOut[1];
			LightweightTransform result;
			while (num5 < 1f)
			{
				num5 = Mathf.Min(1f, num5 + num4);
				Vector3 vector2 = CalculatePoint(ref controlPointsOut, num5);
				float magnitude = (vector2 - vector).magnitude;
				if (magnitude > num3)
				{
					Vector3 to = CalculateTangent(ref controlPointsOut, num5);
					Vector3 to2 = CalculatePoint(ref controlPointsOut2, num5);
					float t = Mathf.Max(0f, num5 - num4);
					Vector3 from = CalculateTangent(ref controlPointsOut, t);
					Vector3 from2 = CalculatePoint(ref controlPointsOut2, t);
					float t2 = num3 / magnitude;
					Vector3 normalized = Vector3.Lerp(from, to, t2).normalized;
					Vector3 normalized2 = Vector3.Lerp(from2, to2, t2).normalized;
					if (context != null)
					{
						context.Knot = i;
						context.SplineDistance = num;
					}
					result = default(LightweightTransform);
					result.Location = Vector3.Lerp(vector, vector2, t2);
					result.Orientation = Quaternion.LookRotation(normalized, normalized2);
					return result;
				}
				num3 -= magnitude;
				vector = vector2;
			}
			num += knots[i].Length;
			i++;
			if (context != null)
			{
				context.Knot = i;
				context.SplineDistance = num;
			}
			result = default(LightweightTransform);
			result.Location = knots[i].transform.position;
			result.Orientation = Quaternion.LookRotation(knots[i].Tangent);
			return result;
		}
		if (context != null)
		{
			context.Knot = knots.Count();
		}
		LightweightTransform result2 = default(LightweightTransform);
		result2.Location = knots.Last().transform.position;
		result2.Orientation = Quaternion.LookRotation(knots.Last().Tangent);
		return result2;
	}

	private void OnDrawGizmos()
	{
		if (m_knots != null)
		{
			Gizmos.color = ((!calculateIsDirty()) ? SplineColour : Color.red);
			if (SplineType == Type.Linear || m_knots.Count() < 3)
			{
				DrawLinearSpline(m_knots);
			}
			else
			{
				DrawCatmullRomSpline(m_knots);
			}
		}
	}

	private void CacheLinearKnots(IList<SplineKnot> knots)
	{
		if (knots != null)
		{
			float num = 0f;
			for (int i = 0; i < knots.Count() - 1; i++)
			{
				Vector3 vector = knots[i + 1].transform.position - knots[i].transform.position;
				SplineKnot splineKnot = knots[i];
				float magnitude = vector.magnitude;
				knots[i].WorldDistanceToNextKnot = magnitude;
				splineKnot.Length = magnitude;
				SplineKnot splineKnot2 = knots[i];
				Vector3 vector2 = vector / knots[i].Length;
				knots[i].TowardsNextKnot = vector2;
				splineKnot2.Tangent = vector2;
				num += knots[i].Length;
			}
			if (knots.Count() >= 2)
			{
				SplineKnot splineKnot3 = knots.Last();
				Vector3 vector2 = knots[knots.Count() - 2].Tangent;
				knots.Last().TowardsNextKnot = vector2;
				splineKnot3.Tangent = vector2;
			}
			Length = num;
		}
	}

	private void CacheCatmullRomKnots(IList<SplineKnot> knots)
	{
		float num = 0f;
		CatmullRomWalker catmullRomWalker = new CatmullRomWalker(knots, this);
		for (int i = 0; i < knots.Count() - 1; i++)
		{
			ControlList controlPointsOut = default(ControlList);
			catmullRomWalker.GetControlPointsFor(i, ref controlPointsOut);
			Vector3 vector = controlPointsOut[2] - controlPointsOut[1];
			float magnitude = vector.magnitude;
			float num2 = magnitude * 2f;
			float num3 = m_splineAccuracy / num2;
			knots[i].Tangent = CalculateTangent(ref controlPointsOut, 0f);
			knots[i].TowardsNextKnot = vector / magnitude;
			float num4 = 0f;
			float num5 = 0f;
			Vector3 vector2 = controlPointsOut[1];
			while (num5 < 1f)
			{
				num5 = Mathf.Min(1f, num5 + num3);
				Vector3 vector3 = CalculatePoint(ref controlPointsOut, num5);
				float magnitude2 = (vector3 - vector2).magnitude;
				num4 += magnitude2;
				vector2 = vector3;
			}
			knots[i].Length = num4;
			knots[i].WorldDistanceToNextKnot = magnitude;
			num += num4;
			if (i == knots.Count() - 2)
			{
				SplineKnot splineKnot = knots.Last();
				Vector3 towardsNextKnot = knots[i].TowardsNextKnot;
				knots.Last().TowardsNextKnot = towardsNextKnot;
				splineKnot.Tangent = towardsNextKnot;
			}
		}
		Length = num;
	}
}
