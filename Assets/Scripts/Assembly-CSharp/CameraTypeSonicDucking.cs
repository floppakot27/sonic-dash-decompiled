using UnityEngine;

public class CameraTypeSonicDucking : CameraType
{
	[SerializeField]
	private float m_backDistance = 5f;

	[SerializeField]
	private float m_upDistance = 3f;

	[SerializeField]
	private float m_lookForwardDistance = 2f;

	[SerializeField]
	private float m_targetLateralOffset = 0.5f;

	[SerializeField]
	private float m_lateralOffsetSmoothing = 0.25f;

	[SerializeField]
	private float m_tiltExaggeration = 0.1f;

	[SerializeField]
	private float m_sonicLowDangerZone = 0.1f;

	[SerializeField]
	private float m_maximumSafeJumpHeight = 3f;

	[SerializeField]
	private float m_dangerZoneHeightDamping = 0.2f;

	private SplineTracker m_tracker;

	private Track.Lane m_currentLane;

	private float m_lateralOffset;

	private float m_idealLateralOffset;

	private float m_lateralVelocity;

	private Camera m_mainCamera;

	private float m_smoothedYPosition;

	private float m_yPosVel;

	private bool IsTrackerUsable => m_tracker != null && m_tracker.Target != null;

	private void Awake()
	{
		EventDispatch.RegisterInterest("StartGameState", this);
		EventDispatch.RegisterInterest("ResetGameState", this);
		Sonic.OnMovementCallback += OnSonicMovement;
		Sonic.OnStrafeCallback += OnSonicStrafe;
		m_lateralVelocity = 0f;
		EventDispatch.RegisterInterest("RequestNewTrackMidGame", this);
		EventDispatch.RegisterInterest("OnSonicResurrection", this);
	}

	private void Event_OnSonicResurrection()
	{
		m_tracker = null;
	}

	private void Event_RequestNewTrackMidGame(TrackGenerator.MidGameNewTrackRequest request)
	{
		m_tracker = null;
	}

	private void OnSonicStrafe(SplineTracker newSpline)
	{
		if (m_tracker != null && newSpline != null)
		{
			newSpline.UpdatePositionByDelta(0f);
			m_tracker = new SplineTracker(newSpline);
			m_tracker.UpdatePositionByDelta(m_lookForwardDistance);
			if (null != m_tracker.Target)
			{
				m_currentLane = Sonic.Tracker.Track.GetLaneOfSpline(m_tracker.Target);
				m_idealLateralOffset = CalculateLaneLateralOffset(m_currentLane);
				LightweightTransform currentSplineTransform = newSpline.CurrentSplineTransform;
				currentSplineTransform.Location += Vector3.up * CalculateJumpHeight();
				Vector3 vector = CalculateIdealPos(currentSplineTransform, 0f);
				m_lateralOffset = Vector3.Dot(newSpline.CurrentSplineTransform.Right, base.transform.position - vector);
			}
		}
	}

	private void OnSonicMovement(SonicSplineTracker.MovementInfo info)
	{
		if (m_tracker != null && !(info.Delta < 0f))
		{
			m_tracker.UpdatePositionByDelta(info.Delta);
		}
	}

	private float CalculateLaneLateralOffset(Track.Lane lane)
	{
		return m_targetLateralOffset * lane switch
		{
			Track.Lane.Left => 1f, 
			Track.Lane.Right => -1f, 
			_ => 0f, 
		};
	}

	private Vector3 CalculateIdealPos(LightweightTransform sonicTransform, float targetLateralOffset)
	{
		return sonicTransform.Location + sonicTransform.Forwards * (0f - m_backDistance) + sonicTransform.Up * CalculateSmoothedHeight() + sonicTransform.Right * targetLateralOffset;
	}

	private float CalculateJumpHeight()
	{
		return Mathf.Max(0f, Sonic.Tracker.JumpHeight - m_maximumSafeJumpHeight);
	}

	private void Event_ResetGameState(GameState.Mode mode)
	{
		if (mode != GameState.Mode.PauseMenu)
		{
			Event_RequestNewTrackMidGame(TrackGenerator.MidGameNewTrackRequest.Invalid);
		}
	}

	private void Event_StartGameState(GameState.Mode mode)
	{
		if (mode != GameState.Mode.PauseMenu)
		{
			Event_RequestNewTrackMidGame(TrackGenerator.MidGameNewTrackRequest.Invalid);
			CameraTypeMain mainCamera = BehindCamera.GetMainCamera();
			m_mainCamera = mainCamera.GetComponentInChildren<Camera>();
		}
	}

	private void LateUpdate()
	{
		if (!(m_mainCamera == null))
		{
			if (!IsTrackerUsable && Sonic.Tracker.IsTrackerAvailable)
			{
				m_tracker = Sonic.Tracker.CloneTracker();
				m_tracker.UpdatePositionByDelta(m_lookForwardDistance);
				m_lateralVelocity = 0f;
				m_lateralOffset = 0f;
				m_idealLateralOffset = 0f;
				m_currentLane = Sonic.Tracker.Track.GetLaneOfSpline(m_tracker.Target);
				m_smoothedYPosition = CalculateHeight();
				m_yPosVel = 0f;
			}
			Vector3 vector = CalculateLookAt();
			LightweightTransform sonicTransform = ((!Sonic.Tracker.IsTrackerAvailable) ? new LightweightTransform(Sonic.Tracker.transform) : Sonic.Tracker.IdealPosition);
			float num = ((!Sonic.Tracker.IsTrackerAvailable) ? 0f : CalculateJumpHeight());
			sonicTransform.Location += Vector3.up * num;
			if (Time.deltaTime > 0f)
			{
				m_lateralOffset = Mathf.SmoothDamp(m_lateralOffset, m_idealLateralOffset, ref m_lateralVelocity, m_lateralOffsetSmoothing);
			}
			base.transform.position = CalculateIdealPos(sonicTransform, m_lateralOffset);
			Vector3 vector2 = new Vector3(sonicTransform.Right.x, 0f, sonicTransform.Right.z);
			Vector3 normalized = vector2.normalized;
			Vector3 from = Vector3.Cross(sonicTransform.Forwards, normalized);
			float num2 = Vector3.Angle(from, sonicTransform.Up) * ((!(Vector3.Dot(sonicTransform.Up, normalized) > 0f)) ? 1f : (-1f));
			float angle = num2 * m_tiltExaggeration;
			Quaternion quaternion = Quaternion.AngleAxis(angle, Vector3.forward);
			base.transform.rotation = Quaternion.LookRotation((vector - base.transform.position).normalized, sonicTransform.Up) * quaternion;
			base.CachedLookAt = base.transform.position + base.transform.forward * 5f;
		}
	}

	private Vector3 CalculateLookAt()
	{
		if (!IsTrackerUsable)
		{
			return Sonic.Transform.position;
		}
		return m_tracker.CurrentSplineTransform.Location + m_tracker.CurrentSplineTransform.Right * m_lateralOffset + Vector3.up * Sonic.Tracker.JumpHeight;
	}

	private float CalculateHeight()
	{
		float inVal = m_mainCamera.WorldToScreenPoint(Sonic.Transform.position).y / m_mainCamera.pixelHeight;
		return Utils.MapValue(inVal, 0f, m_sonicLowDangerZone, 0.5f, m_upDistance);
	}

	private float CalculateSmoothedHeight()
	{
		float target = CalculateHeight();
		if (Time.deltaTime > 0f)
		{
			m_smoothedYPosition = Mathf.SmoothDamp(m_smoothedYPosition, target, ref m_yPosVel, m_dangerZoneHeightDamping);
		}
		return m_smoothedYPosition;
	}
}
