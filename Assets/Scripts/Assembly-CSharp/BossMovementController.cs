using UnityEngine;

public class BossMovementController : MonoBehaviour
{
	public struct MovementParameters
	{
		public Vector3 m_destination;

		public float m_duration;

		public bool m_faceMovementDirection;

		public Quaternion m_orientation;

		public bool m_moveWithTracker;

		public Track.Lane m_lane;

		public float m_splineDistanceFromSonic;
	}

	private MovementParameters m_freeMovementParameters;

	private bool m_configureTracker;

	private Vector3 m_velocity = Vector3.zero;

	private LightweightTransform m_transform;

	private float m_travelProgress;

	private float m_progressPerSecond;

	private Vector3 m_startLocation;

	private SplineTracker m_tracker;

	private bool m_snapToDestination;

	private Vector3 m_driftValues = Vector3.zero;

	private float m_driftStrength;

	[SerializeField]
	private Vector3 m_driftOffsets = Vector3.zero;

	[SerializeField]
	private Vector3 m_driftSpeed = Vector3.zero;

	[SerializeField]
	private float m_driftFadeTime = 1f;

	public bool UseDrift { get; set; }

	public float SplineDistanceTravelled { get; private set; }

	public float SplineDistanceFromSonic => m_freeMovementParameters.m_splineDistanceFromSonic;

	public float MovementProgress => m_travelProgress;

	public void MoveToDestination(MovementParameters parameters, bool useDrift)
	{
		MoveToDestination(parameters, useDrift, snapToDestination: false);
	}

	public void MoveToDestination(MovementParameters parameters, bool useDrift, bool snapToDestination)
	{
		m_freeMovementParameters = parameters;
		UseDrift = useDrift;
		m_snapToDestination = snapToDestination;
		if (parameters.m_duration > 0f)
		{
			m_progressPerSecond = 1f / parameters.m_duration;
		}
		else
		{
			m_progressPerSecond = float.MaxValue;
			snapToDestination = true;
		}
		m_configureTracker = true;
	}

	public void MoveToDestination(Vector3 destination, bool useDrift)
	{
		MoveToDestination(destination, useDrift, snapToDestination: false);
	}

	public void MoveToDestination(Vector3 destination, bool useDrift, bool snapToDestination)
	{
		m_freeMovementParameters.m_destination = destination;
		m_travelProgress = 0f;
		m_startLocation = m_transform.Location;
		m_freeMovementParameters.m_duration *= 1f - m_travelProgress;
		UseDrift = useDrift;
		m_snapToDestination = snapToDestination;
	}

	public bool IsInDesiredPosition(float error)
	{
		return (m_freeMovementParameters.m_destination - m_transform.Location).sqrMagnitude <= error * error;
	}

	public SplineTracker GetTracker()
	{
		return m_tracker;
	}

	public void SetOrientation(Vector3 directionVec)
	{
		SetOrientation(directionVec, trackerSpace: false);
	}

	public void SetOrientation(Vector3 directionVec, bool trackerSpace)
	{
		if (directionVec != Vector3.zero)
		{
			Quaternion quaternion = Quaternion.LookRotation(directionVec, Vector3.up);
			if (trackerSpace && m_tracker != null)
			{
				m_transform.Orientation = quaternion * m_tracker.CurrentSplineTransform.Orientation;
			}
			else
			{
				m_transform.Orientation = quaternion;
			}
		}
	}

	private void Awake()
	{
		Sonic.OnMovementCallback += OnSonicMovement;
	}

	private void OnDestroy()
	{
		Sonic.OnMovementCallback -= OnSonicMovement;
	}

	private void Update()
	{
		UpdateSplineMovement();
		UpdateFreeMovement();
		LightweightTransform lightweightTransform = m_transform;
		if (m_tracker != null)
		{
			lightweightTransform.Location = m_tracker.CurrentSplineTransform.Orientation * lightweightTransform.Location;
			lightweightTransform.Location += m_tracker.CurrentSplineTransform.Location;
		}
		lightweightTransform.ApplyTo(base.transform);
		lightweightTransform.ApplyTo(Boss.GetInstance().LookAtPoint);
		Vector3 vector = CalculateDrift();
		base.transform.position += vector;
	}

	private void UpdateFreeMovement()
	{
		if (m_snapToDestination)
		{
			m_transform.Location = m_freeMovementParameters.m_destination;
			m_transform.Orientation = m_freeMovementParameters.m_orientation;
			m_snapToDestination = false;
			return;
		}
		m_travelProgress += m_progressPerSecond * Time.deltaTime;
		m_travelProgress = Mathf.Clamp01(m_travelProgress);
		float t = Mathf.SmoothStep(0f, 1f, m_travelProgress);
		Vector3 location = Vector3.Lerp(m_startLocation, m_freeMovementParameters.m_destination, t);
		m_transform.Location = location;
		if (m_freeMovementParameters.m_faceMovementDirection)
		{
			Vector3 vector = m_freeMovementParameters.m_destination - m_transform.Location;
			if (vector != Vector3.zero)
			{
				Quaternion identity = Quaternion.identity;
				identity.SetLookRotation(vector, m_transform.Up);
				m_transform.Orientation = identity;
			}
		}
		else if (m_tracker == null)
		{
			m_transform.Orientation = m_freeMovementParameters.m_orientation;
		}
	}

	private void UpdateSplineMovement()
	{
		if (Sonic.Tracker == null || Sonic.Tracker.InternalTracker == null || !m_configureTracker)
		{
			return;
		}
		if (m_tracker != null)
		{
			m_transform.Location = m_tracker.CurrentSplineTransform.Orientation * m_transform.Location;
			m_transform.Location += m_tracker.CurrentSplineTransform.Location;
		}
		if (m_freeMovementParameters.m_moveWithTracker)
		{
			Spline target = Sonic.Tracker.InternalTracker.Target;
			Spline spline = target.getTrackSegment().GetSpline((int)m_freeMovementParameters.m_lane);
			m_tracker = new SplineTracker(Sonic.Tracker.InternalTracker);
			m_tracker.Start(Sonic.Tracker.InternalTracker.CurrentSplineTransform.Location, spline, 0f, Direction_1D.Forwards);
			if (!Mathf.Approximately(m_freeMovementParameters.m_splineDistanceFromSonic, 0f))
			{
				if (m_freeMovementParameters.m_splineDistanceFromSonic < 0f)
				{
					m_tracker.RunBackwards = true;
					m_tracker.Reverse();
				}
				m_tracker.UpdatePositionByDelta(Mathf.Abs(m_freeMovementParameters.m_splineDistanceFromSonic));
				if (m_freeMovementParameters.m_splineDistanceFromSonic < 0f)
				{
					m_tracker.RunBackwards = false;
					m_tracker.Reverse();
				}
			}
			SplineDistanceTravelled = Sonic.Tracker.DistanceTravelled + m_freeMovementParameters.m_splineDistanceFromSonic;
		}
		else
		{
			m_tracker = null;
			SplineDistanceTravelled = 0f;
		}
		if (m_tracker != null)
		{
			m_transform.Location -= m_tracker.CurrentSplineTransform.Location;
			m_transform.Location = Quaternion.Inverse(m_tracker.CurrentSplineTransform.Orientation) * m_transform.Location;
		}
		m_travelProgress = 0f;
		m_startLocation = m_transform.Location;
		m_configureTracker = false;
	}

	private Vector3 CalculateDrift()
	{
		if (UseDrift)
		{
			if (m_driftStrength < 1f)
			{
				m_driftStrength += Time.deltaTime * m_driftFadeTime;
			}
		}
		else if (m_driftStrength > 0f)
		{
			m_driftStrength -= Time.deltaTime * m_driftFadeTime;
		}
		m_driftStrength = Mathf.Clamp01(m_driftStrength);
		m_driftValues += m_driftSpeed * Time.deltaTime;
		Vector3 vector = m_driftStrength * m_driftOffsets;
		Vector3 direction = new Vector3(Mathf.Cos(m_driftValues.x) * vector.x, Mathf.Cos(m_driftValues.y) * vector.y, Mathf.Cos(m_driftValues.z) * vector.z);
		return base.transform.TransformDirection(direction);
	}

	private void OnSonicMovement(SonicSplineTracker.MovementInfo info)
	{
		if (m_tracker != null && !(info.Delta < 0f))
		{
			m_tracker.UpdatePositionByDelta(info.Delta);
			SplineDistanceTravelled += info.Delta;
		}
	}
}
