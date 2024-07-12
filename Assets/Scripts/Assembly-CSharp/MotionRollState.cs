using UnityEngine;

public class MotionRollState : MotionState
{
	private enum RollState
	{
		ePoweringUp,
		eRolling,
		eEnded
	}

	private float m_SpinUpDuration;

	private float m_spinUpVelocity;

	private float m_spinVelocity;

	private float m_initialSpinVelocity;

	private float m_rollDuration;

	private float m_initialRollDuration;

	private float m_elapsedTime;

	private float m_totalElaspedTime;

	private int m_numberOfBoosts;

	private bool m_stepDownPFXPlayed;

	private RollState m_rollState;

	private GameObject m_animatingObject;

	private MotionStrafeState m_strafeHelper;

	private LightweightTransform m_lastGroundTransform;

	private bool m_cancelRollAnimOnExit = true;

	public MotionRollState(GameObject animatingObject, SonicHandling handling)
	{
		m_animatingObject = animatingObject;
	}

	public override void Enter()
	{
		m_rollState = RollState.ePoweringUp;
		SonicHandling.RollStats rollStats = Sonic.Handling.CalculateRollStats(GameState.TimeInGame);
		m_rollDuration = rollStats.Duration - rollStats.SpinUpDuration;
		m_initialRollDuration = m_rollDuration;
		m_SpinUpDuration = rollStats.SpinUpDuration;
		m_spinUpVelocity = rollStats.SpinUpVelocity;
		m_spinVelocity = rollStats.SpinVelocity * Sonic.Handling.RollBoostSpeedModifier;
		m_initialSpinVelocity = rollStats.SpinVelocity;
		m_numberOfBoosts = 0;
		m_elapsedTime = 0f;
		m_totalElaspedTime = 0f;
		m_stepDownPFXPlayed = false;
		m_animatingObject.SendMessage("StartRollAnim", new Pair<float, float>(m_SpinUpDuration, m_spinVelocity), SendMessageOptions.DontRequireReceiver);
		EventDispatch.GenerateEvent("EnterMotionRollState");
		EventDispatch.GenerateEvent("SpinballCharge", 1);
		Sonic.AudioControl.PlayRollSpinUpSFX();
		Sonic.Tracker.getPhysics().ClearJump();
	}

	public override void Exit()
	{
		Sonic.AudioControl.StopRollSpinUpSFX();
		if (m_cancelRollAnimOnExit)
		{
			m_animatingObject.SendMessage("StopRollAnim", SendMessageOptions.DontRequireReceiver);
		}
		EventDispatch.GenerateEvent("ExitMotionRollState");
	}

	public override LightweightTransform CalculateNewTransform(TransformParameters tParams)
	{
		m_totalElaspedTime += Time.deltaTime;
		switch (m_rollState)
		{
		case RollState.ePoweringUp:
			tParams.Physics.TargetSpeed = m_spinUpVelocity;
			if (m_elapsedTime > m_SpinUpDuration)
			{
				Sonic.AudioControl.PlayRollGoSFX();
				m_rollState = RollState.eRolling;
				m_elapsedTime = 0f;
			}
			m_elapsedTime += Time.deltaTime;
			break;
		case RollState.eRolling:
			if (m_elapsedTime < m_initialRollDuration)
			{
				float fPercentageTime = m_elapsedTime / m_initialRollDuration;
				float t = Sonic.Handling.EvaluateRollSpeed(fPercentageTime);
				tParams.Physics.TargetSpeed = Mathf.Lerp(m_initialSpinVelocity, m_spinVelocity, t);
			}
			else
			{
				tParams.Physics.TargetSpeed = m_initialSpinVelocity;
			}
			if (m_elapsedTime > m_rollDuration && IsSafeToUnroll())
			{
				m_animatingObject.SendMessage("StopRollAnim", SendMessageOptions.DontRequireReceiver);
				tParams.StateMachine.PopTopState();
				m_rollState = RollState.eEnded;
			}
			else if (((m_numberOfBoosts == 2 && m_elapsedTime > m_rollDuration - 0.9f) || (m_numberOfBoosts == 1 && m_elapsedTime > m_rollDuration - 0.6f) || (m_numberOfBoosts == 0 && m_elapsedTime > m_rollDuration - 0.3f)) && !m_stepDownPFXPlayed)
			{
				EventDispatch.GenerateEvent("LeavingRollState");
				m_stepDownPFXPlayed = true;
			}
			m_elapsedTime += Time.deltaTime;
			break;
		}
		tParams.Physics.AccelerationOverride = 9999999f;
		tParams.Physics.TimeSpentNotRolling = 0f;
		if (m_strafeHelper != null)
		{
			m_lastGroundTransform = m_strafeHelper.CalculateNewTransform(tParams);
			if (m_strafeHelper.IsFinished)
			{
				m_strafeHelper = ((m_strafeHelper.QueuedState == null) ? null : (m_strafeHelper.QueuedState() as MotionStrafeState));
			}
		}
		else
		{
			m_lastGroundTransform = tParams.Tracker.Update(tParams.CurrentTransform);
		}
		if (tParams.OverGap)
		{
			if (DashMonitor.instance().isDashing() || HeadstartMonitor.instance().isHeadstarting())
			{
				tParams.StateMachine.RequestState(new MotionJumpState(tParams.Physics, 0f, Sonic.Tracker.gameObject, Sonic.Handling));
			}
			else
			{
				tParams.StateMachine.ForceState(new MotionFallState(tParams));
			}
		}
		return m_lastGroundTransform;
	}

	public override MotionState OnRoll(GameObject animatingObject, SonicPhysics physics, SonicHandling handling)
	{
		if (m_totalElaspedTime < handling.RollTimeToAllowBoosts && m_numberOfBoosts < handling.RollMaxNumBoosts)
		{
			m_stepDownPFXPlayed = false;
			m_rollDuration += handling.RollBoostTimeIncrementPercent;
			m_numberOfBoosts++;
			EventDispatch.GenerateEvent("SpinballCharge", m_numberOfBoosts + 1);
		}
		return null;
	}

	public override MotionState OnJump(SonicPhysics physics, float initialGroundHeight, GameObject animatingObject, SonicHandling handling)
	{
		bool flag = false;
		if ((!DashMonitor.instance().isDashing() && !HeadstartMonitor.instance().isHeadstarting()) ? true : false)
		{
			return new MotionJumpState(physics, initialGroundHeight, animatingObject, handling);
		}
		return null;
	}

	public override MotionState OnStumble(GameObject animatingObject, SonicHandling handling)
	{
		return new MotionStumbleState(animatingObject, handling);
	}

	public override MotionState OnStrafe(SplineTracker tracker, Track track, SideDirection direction, float strafeDuration, GameObject animatingObject)
	{
		if (m_strafeHelper != null)
		{
			if (m_strafeHelper.OnStrafe(tracker, track, direction, strafeDuration, animatingObject) is MotionStrafeState strafeHelper)
			{
				m_strafeHelper = strafeHelper;
			}
		}
		else
		{
			m_strafeHelper = MotionStrafeState.CreateHelper(tracker, track, direction, strafeDuration, animatingObject, m_lastGroundTransform.Location, Sonic.Handling.AirStrafeSmoothness);
		}
		Sonic.AudioControl.PlayStrafeSFX();
		return null;
	}

	public override MotionState OnSetPiece()
	{
		m_cancelRollAnimOnExit = false;
		return base.OnSetPiece();
	}

	public override InterruptCode OnInterrupt(MotionStateMachine machine, MotionState newState)
	{
		return InterruptCode.KillMe;
	}

	private bool IsSafeToUnroll()
	{
		float minDistance = Sonic.Tracker.TrackPosition - 2f;
		float maxDistance = Sonic.Tracker.TrackPosition + Sonic.Tracker.Speed * Sonic.Handling.SafeUnrollTimeWindow;
		Track.Lane laneOfSpline = Sonic.Tracker.Track.GetLaneOfSpline(Sonic.Tracker.CurrentSpline);
		uint entitiesMask = 96u;
		bool flag = Sonic.Tracker.Track.Info.IsEntityInRange(entitiesMask, minDistance, maxDistance, laneOfSpline);
		return !flag;
	}
}
