using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("Dash/Cameras/Sonic Follow")]
public class CameraTypeSonicFollow : CameraType
{
	private float m_dashTransitionSpeed = 2f;

	private float m_dashAmount;

	private Vector3 m_smoothVelocity = Vector3.zero;

	public float m_lookAtSmooth = 0.15f;

	[SerializeField]
	private AnimationCurve m_backDistanceOverCurvature;

	private float m_backDistance = 3f;

	private float m_backDistanceVelocity;

	[SerializeField]
	private float m_backDistanceSmoothing = 0.15f;

	[SerializeField]
	private float m_upDistance = 3f;

	[SerializeField]
	private float m_upDuckDistance = 0.5f;

	private float m_duckForwardScanDistance;

	[SerializeField]
	private float m_duckBackwardsScanDistance = 2f;

	[SerializeField]
	private float m_duckMaxTransitionDuration = 0.25f;

	[SerializeField]
	private float m_lookForwardDistance = 2f;

	[SerializeField]
	private float m_targetLateralOffset = 0.3f;

	[SerializeField]
	private float m_lateralOffsetSmoothing = 0.25f;

	[SerializeField]
	private AnimationCurve m_lateralOffsetOutOfCornerByCurvature;

	[SerializeField]
	private float m_tiltExaggeration = 0.1f;

	[SerializeField]
	private float m_maximumSafeJumpHeight = 3f;

	[SerializeField]
	private float m_lookatJumpProportion;

	private float m_currentCurvature;

	private float m_normalFOV = 60f;

	public float m_dashingFOV = 110f;

	private SplineTracker m_tracker;

	private Track.Lane m_currentLane;

	private float m_lateralOffset;

	private float m_idealLateralOffset;

	private float m_lateralVelocity;

	private Camera m_mainCamera;

	private CameraProperties m_cameraProperties;

	private float m_currentSplineLead;

	private float m_duckDesiredProportion;

	private float m_duckProportion;

	[SerializeField]
	private bool m_EnableSmoothingOnMainCamera;

	private Vector3 m_localPosVelocity = Vector3.zero;

	[SerializeField]
	private float m_positionSmoothing = 0.1f;

	private bool m_noSmoothingNextFrame;

	private IList<Pair<float, TrackEntity>> m_duckObstaclesScratch = new List<Pair<float, TrackEntity>>();

	private bool IsTrackerUsable => m_tracker != null && m_tracker.Target != null;

	public override bool EnableSmoothing => m_EnableSmoothingOnMainCamera;

	private void Awake()
	{
		EventDispatch.RegisterInterest("StartGameState", this);
		EventDispatch.RegisterInterest("ResetGameState", this);
		Sonic.OnMovementCallback += OnSonicMovement;
		Sonic.OnStrafeCallback += OnSonicStrafe;
		ResetCamera();
		EventDispatch.RegisterInterest("RequestNewTrackMidGame", this);
		EventDispatch.RegisterInterest("OnSonicResurrection", this);
	}

	private void Event_OnSonicResurrection()
	{
		ResetCamera();
	}

	private void Start()
	{
		m_cameraProperties = GetComponent<CameraProperties>();
		m_normalFOV = m_cameraProperties.FOV;
	}

	private void Event_RequestNewTrackMidGame(TrackGenerator.MidGameNewTrackRequest request)
	{
		ResetCamera();
	}

	private void OnSonicStrafe(SplineTracker newSpline)
	{
		if (!(null == this) && m_tracker != null && newSpline != null)
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
			float nextObstacleDelta = CalculateDeltaToDuckObstacle(info.Lane);
			m_duckDesiredProportion = CalculateDesiredDuckProp(nextObstacleDelta);
		}
	}

	private float CalculateCurvature()
	{
		if (Sonic.Tracker == null || m_tracker == null || !m_tracker.Tracking)
		{
			return 0f;
		}
		Vector3 vector = m_tracker.CurrentSplineTransform.Location - Sonic.Tracker.transform.position;
		vector.y = 0f;
		Vector3 vector2 = ((vector.sqrMagnitude != 0f) ? vector.normalized : Sonic.Tracker.transform.forward);
		Vector3 vector3 = new Vector3(Sonic.Tracker.transform.forward.x, 0f, Sonic.Tracker.transform.forward.z);
		Vector3 normalized = vector3.normalized;
		float num = Vector3.Angle(vector2, normalized);
		float num2 = ((!(Vector3.Dot(vector2, Sonic.Tracker.transform.right) > 0f)) ? (-1f) : 1f);
		return num2 * num / m_lookForwardDistance;
	}

	private float CalculateLaneLateralOffset(Track.Lane lane)
	{
		float time = Mathf.Abs(m_currentCurvature);
		Track.Lane lane2 = ((m_currentCurvature > 0f) ? Track.Lane.Right : Track.Lane.Left);
		float num = m_lateralOffsetOutOfCornerByCurvature.Evaluate(time) * ((lane == lane2) ? 1.5f : ((lane != Track.Lane.Middle) ? 0.5f : 1f));
		float num2 = ((!(m_currentCurvature > 0f)) ? 1f : (-1f)) * num;
		float num3 = m_targetLateralOffset * lane switch
		{
			Track.Lane.Left => 1f, 
			Track.Lane.Right => -1f, 
			_ => 0f, 
		};
		return num3 + num2;
	}

	private float CalculateTargetBackDistance()
	{
		return m_backDistanceOverCurvature.Evaluate(Mathf.Abs(m_currentCurvature));
	}

	private void UpdateBackDistance()
	{
		float target = CalculateTargetBackDistance();
		float backDistance = Utils.SmoothDamp(m_backDistance, target, ref m_backDistanceVelocity, m_backDistanceSmoothing);
		m_backDistance = backDistance;
	}

	private Vector3 CalculateIdealPos(LightweightTransform sonicTransform, float targetLateralOffset)
	{
		float num = Mathf.Lerp(1f, 0.35f, m_dashAmount);
		float num2 = CalculateSmoothedHeight();
		return sonicTransform.Location + sonicTransform.Forwards * (0f - m_backDistance) * num + sonicTransform.Up * num2 * num + sonicTransform.Right * targetLateralOffset * num;
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
			if (m_mainCamera == null)
			{
				CameraTypeMain mainCamera = BehindCamera.GetMainCamera();
				m_mainCamera = mainCamera.GetComponentInChildren<Camera>();
			}
			Event_RequestNewTrackMidGame(TrackGenerator.MidGameNewTrackRequest.Invalid);
		}
	}

	private void LateUpdate()
	{
		if (m_mainCamera == null || Sonic.Tracker == null)
		{
			return;
		}
		if (((DashMonitor.instance().isDashing() && !DashMonitor.instance().isDashNearlyFinished()) || HeadstartMonitor.instance().isHeadstarting()) && !Sonic.Tracker.isJumping())
		{
			m_dashAmount += Time.deltaTime * m_dashTransitionSpeed;
		}
		else
		{
			m_dashAmount -= Time.deltaTime * m_dashTransitionSpeed;
		}
		if (Sonic.Tracker.IsSpringJumping)
		{
			m_dashAmount = 0f;
		}
		m_dashAmount = Mathf.Clamp(m_dashAmount, 0f, 1f);
		if (!IsTrackerUsable)
		{
			if (Sonic.Tracker.IsTrackerAvailable)
			{
				m_tracker = Sonic.Tracker.CloneTracker();
				m_currentLane = Sonic.Tracker.Track.GetLaneOfSpline(m_tracker.Target);
				m_currentSplineLead = 0f;
			}
			m_lateralVelocity = 0f;
			m_lateralOffset = 0f;
			m_idealLateralOffset = 0f;
		}
		if (IsTrackerUsable && m_currentSplineLead < m_lookForwardDistance)
		{
			float delta = m_lookForwardDistance - m_currentSplineLead;
			if (!m_tracker.CanUpdate)
			{
				m_tracker.Restart();
			}
			m_tracker.UpdatePositionByDelta(delta);
			m_currentSplineLead += m_tracker.PreviousDelta;
		}
		UpdateDuck();
		m_currentCurvature = CalculateCurvature();
		UpdateBackDistance();
		Vector3 vector = CalculateLookAt();
		LightweightTransform sonicTransform = ((!Sonic.Tracker.IsTrackerAvailable) ? new LightweightTransform(Sonic.Tracker.transform) : Sonic.Tracker.IdealPosition);
		float num = ((!Sonic.Tracker.IsTrackerAvailable) ? 0f : CalculateJumpHeight());
		sonicTransform.Location += Vector3.up * num;
		if (Time.deltaTime > 0f)
		{
			m_idealLateralOffset = CalculateLaneLateralOffset(m_currentLane);
			m_lateralOffset = Utils.SmoothDamp(m_lateralOffset, m_idealLateralOffset, ref m_lateralVelocity, m_lateralOffsetSmoothing);
		}
		Vector3 vector2 = CalculateIdealPos(sonicTransform, m_lateralOffset);
		if (m_noSmoothingNextFrame)
		{
			base.transform.position = vector2;
			m_noSmoothingNextFrame = false;
		}
		else
		{
			base.transform.position = base.transform.parent.TransformPoint(SmoothTowardsTargetLocalPosition(vector2));
		}
		Vector3 vector3 = new Vector3(sonicTransform.Right.x, 0f, sonicTransform.Right.z);
		Vector3 normalized = vector3.normalized;
		Vector3 from = Vector3.Cross(sonicTransform.Forwards, normalized);
		float num2 = Vector3.Angle(from, sonicTransform.Up) * ((!(Vector3.Dot(sonicTransform.Up, normalized) > 0f)) ? 1f : (-1f));
		float angle = num2 * m_tiltExaggeration;
		Quaternion quaternion = Quaternion.AngleAxis(angle, Vector3.forward);
		Vector3 target = vector - base.transform.position;
		if (target.sqrMagnitude == 0f)
		{
			target = base.transform.forward;
		}
		else
		{
			target.Normalize();
			Vector3 current = base.transform.rotation * Vector3.forward;
			current.Normalize();
			target = Vector3.SmoothDamp(current, target, ref m_smoothVelocity, m_lookAtSmooth);
		}
		base.transform.rotation = Quaternion.LookRotation(target, sonicTransform.Up) * quaternion;
		base.CachedLookAt = base.transform.position + base.transform.forward * 5f;
		m_cameraProperties.FOV = Mathf.Lerp(m_normalFOV, m_dashingFOV, m_dashAmount);
	}

	private Vector3 SmoothTowardsTargetLocalPosition(Vector3 targetWorldPos)
	{
		Vector3 target = base.transform.parent.InverseTransformPoint(targetWorldPos);
		return Vector3.SmoothDamp(base.transform.localPosition, target, ref m_localPosVelocity, m_positionSmoothing);
	}

	private Vector3 CalculateLookAt()
	{
		if (!IsTrackerUsable)
		{
			Vector3 vector = Sonic.Tracker.transform.forward * m_lookForwardDistance;
			return Sonic.Tracker.transform.position + vector;
		}
		return m_tracker.CurrentSplineTransform.Location + m_tracker.CurrentSplineTransform.Right * m_lateralOffset + Vector3.up * Sonic.Tracker.JumpHeight * m_lookatJumpProportion;
	}

	private float CalculateHeight()
	{
		return Mathf.Lerp(m_upDistance, m_upDuckDistance, m_duckProportion);
	}

	private float CalculateSmoothedHeight()
	{
		return CalculateHeight();
	}

	private float CalculateDeltaToDuckObstacle(Track.Lane currentLane)
	{
		if (!Sonic.Tracker.IsRolling)
		{
			return float.MinValue;
		}
		float trackPosition = Sonic.Tracker.TrackPosition;
		m_duckForwardScanDistance = Sonic.Tracker.Speed * m_duckMaxTransitionDuration;
		float num = trackPosition - m_duckBackwardsScanDistance;
		float maxInclusive = num + m_duckForwardScanDistance;
		float num2 = float.MinValue;
		Sonic.Tracker.Track.Info.DistanceEntitiesInRange(num, maxInclusive, 96u, currentLane, ref m_duckObstaclesScratch);
		for (int i = 0; i < m_duckObstaclesScratch.Count; i++)
		{
			Pair<float, TrackEntity> pair = m_duckObstaclesScratch[i];
			if (!pair.Second.IsValid)
			{
				continue;
			}
			float num3 = pair.First - trackPosition;
			if (num3 > 0f)
			{
				if (num2 < 0f || num2 > num3)
				{
					num2 = num3;
				}
			}
			else if (num2 < 0f && num3 > num2)
			{
				num2 = num3;
			}
		}
		return num2;
	}

	private float CalculateDesiredDuckProp(float nextObstacleDelta)
	{
		return (nextObstacleDelta == float.MinValue) ? 0f : ((!(nextObstacleDelta < 0f)) ? Utils.MapValue(nextObstacleDelta, 0f, m_duckForwardScanDistance, 1f, 0f) : 1f);
	}

	private void UpdateDuck()
	{
		if (m_duckDesiredProportion > m_duckProportion)
		{
			m_duckProportion = m_duckDesiredProportion;
			return;
		}
		float num = Time.deltaTime / m_duckMaxTransitionDuration;
		m_duckProportion = Mathf.Max(m_duckDesiredProportion, m_duckProportion - num);
	}

	private void ResetCamera()
	{
		m_localPosVelocity = Vector3.zero;
		m_backDistanceVelocity = 0f;
		m_lateralVelocity = 0f;
		m_tracker = null;
		m_dashAmount = 0f;
		m_noSmoothingNextFrame = true;
	}
}
