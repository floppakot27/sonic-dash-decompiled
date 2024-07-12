using System;
using UnityEngine;

public class MotionSpringDescentState : MotionSpringState
{
	public enum TrackState
	{
		NotReady,
		Ready,
		Processed
	}

	private CameraTypeMain m_mainCamera;

	private TrackState m_trackState;

	private LightweightTransform m_lastGroundTransform;

	private MotionStrafeState m_strafeHelper;

	private float m_previousHeight;

	private Func<float, float> m_calculateLandingSpeed;

	public MotionSpringDescentState(TrackState trackState, SonicHandling handling, SonicPhysics physics)
		: base(physics, handling)
	{
		m_trackState = trackState;
	}

	public override void Enter()
	{
		base.Enter();
		m_previousHeight = base.Physics.JumpHeight;
		m_mainCamera = UnityEngine.Object.FindObjectOfType(typeof(CameraTypeMain)) as CameraTypeMain;
	}

	public override LightweightTransform CalculateNewTransform(TransformParameters tParams)
	{
		float jumpHeight = tParams.Physics.JumpHeight;
		LightweightTransform lightweightTransform = CalculateGroundTransform(tParams);
		LightweightTransform result = CalculateDescentTransform(tParams, lightweightTransform, jumpHeight);
		m_lastGroundTransform = lightweightTransform;
		m_previousHeight = jumpHeight;
		return result;
	}

	public override MotionState OnStrafe(SplineTracker tracker, Track track, SideDirection direction, float strafeDuration, GameObject animatingObject)
	{
		if (m_trackState != TrackState.Processed)
		{
			return null;
		}
		if (m_strafeHelper != null)
		{
			if (m_strafeHelper.OnStrafe(tracker, track, direction, strafeDuration, animatingObject) is MotionStrafeState strafeHelper)
			{
				Sonic.AudioControl.PlayStrafeSFX();
				m_strafeHelper = strafeHelper;
			}
		}
		else
		{
			m_strafeHelper = MotionStrafeState.CreateHelper(tracker, track, direction, strafeDuration, animatingObject, m_lastGroundTransform.Location, Sonic.Handling.AirStrafeSmoothness);
			if (m_strafeHelper != null)
			{
				Sonic.AudioControl.PlayStrafeSFX();
			}
		}
		return null;
	}

	private LightweightTransform CalculateGroundTransform(TransformParameters tParams)
	{
		if (m_strafeHelper != null)
		{
			LightweightTransform result = m_strafeHelper.CalculateNewTransformPostUpdate(tParams);
			if (m_strafeHelper.IsFinished)
			{
				m_strafeHelper = ((m_strafeHelper.QueuedState == null) ? null : (m_strafeHelper.QueuedState() as MotionStrafeState));
			}
			return result;
		}
		if (tParams.Tracker != null)
		{
			return tParams.Tracker.CurrentSplineTransform;
		}
		return new LightweightTransform(tParams.CurrentTransform.Location - Vector3.up * m_previousHeight, tParams.CurrentTransform.Orientation);
	}

	private LightweightTransform CalculateDescentTransform(TransformParameters tParams, LightweightTransform groundTransform, float springHeight)
	{
		bool flag = false;
		if (m_trackState == TrackState.NotReady)
		{
			return tParams.CurrentTransform;
		}
		if (m_trackState == TrackState.Ready)
		{
			if (tParams.Tracker == null)
			{
				return tParams.CurrentTransform;
			}
			flag = true;
			m_trackState = TrackState.Processed;
			float landingSpeed = Sonic.Handling.CalculateVelocityAt(GameState.TimeInGame);
			float timeRemaining = base.Physics.JumpTimeRemaining;
			float num = tParams.Tracker.Target.Length - tParams.Tracker.CurrentDistance;
			float startSpeed = 0f - landingSpeed + 2f * num / timeRemaining;
			m_calculateLandingSpeed = (float jumpTimeRemaining) => Utils.MapValue(jumpTimeRemaining, timeRemaining, 0f, startSpeed, landingSpeed);
			EventDispatch.GenerateEvent("OnSpringDescent", base.Physics.JumpTimeRemaining);
		}
		float num2 = m_calculateLandingSpeed(base.Physics.JumpTimeRemaining);
		tParams.Physics.TargetSpeed = num2;
		tParams.Physics.AccelerationOverride = float.MaxValue;
		float num3 = num2 * Time.deltaTime;
		tParams.Tracker.UpdatePositionByDelta(num3);
		if (flag)
		{
			groundTransform = tParams.Tracker.CurrentSplineTransform;
		}
		Vector3 pos = groundTransform.Location + Vector3.up * springHeight + base.FlatForward * num3;
		LightweightTransform result = new LightweightTransform(pos, tParams.CurrentTransform.Orientation);
		if (tParams.Physics.UpdateJump(pauseHalfway: false))
		{
			tParams.Physics.ClearJump();
			EventDispatch.GenerateEvent("OnSpringEnd");
			tParams.StateMachine.PopTopState();
		}
		if (flag)
		{
			Vector3 location = result.Location;
			Vector3 vector = location - tParams.CurrentTransform.Location;
			m_mainCamera.GetCurrentCameraType().gameObject.transform.position += vector;
			m_mainCamera.GetCurrentCameraType().CachedLookAt += vector;
			m_mainCamera.CachedLookAt += vector;
		}
		return result;
	}

	private void Event_OnTrackGenerationComplete()
	{
		m_trackState = TrackState.Ready;
	}
}
