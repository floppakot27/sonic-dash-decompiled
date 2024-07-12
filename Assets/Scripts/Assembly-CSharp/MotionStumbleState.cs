using UnityEngine;

public class MotionStumbleState : MotionState
{
	private float m_stumbleSpeed;

	private float m_stumbleDuration;

	private GameObject m_animatingObject;

	private TargetManager m_targetManager;

	public MotionStumbleState(GameObject animatingObject, SonicHandling handling)
	{
		m_stumbleDuration = handling.StumbleDuration;
		m_animatingObject = animatingObject;
		m_targetManager = TargetManager.instance();
	}

	public override void Enter()
	{
		m_animatingObject.SendMessage("StartStumbleAnim", SendMessageOptions.DontRequireReceiver);
		EventDispatch.GenerateEvent("OnSonicStumble");
	}

	public override void Exit()
	{
		m_animatingObject.SendMessage("StopStumbleAnim", SendMessageOptions.DontRequireReceiver);
	}

	public override LightweightTransform CalculateNewTransform(TransformParameters tParams)
	{
		m_stumbleDuration -= Time.deltaTime;
		if (m_stumbleDuration <= 0f)
		{
			m_animatingObject.SendMessage("StopStumbleAnim", SendMessageOptions.DontRequireReceiver);
			tParams.StateMachine.PopTopState();
		}
		tParams.Physics.AccelerationOverride = 9999999f;
		LightweightTransform result = tParams.Tracker.Update(tParams.CurrentTransform);
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
		return result;
	}

	public override InterruptCode OnInterrupt(MotionStateMachine machine, MotionState newState)
	{
		return InterruptCode.KillMe;
	}

	public override MotionState OnRoll(GameObject animatingObject, SonicPhysics physics, SonicHandling handling)
	{
		bool flag = false;
		flag = ((!DashMonitor.instance().isDashing() && !HeadstartMonitor.instance().isHeadstarting()) ? true : false);
		if (!(physics.TimeSpentNotRolling > handling.RollCoolDownDuration))
		{
			flag = false;
		}
		if (flag)
		{
			return new MotionRollState(animatingObject, handling);
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

	public override MotionState OnStrafe(SplineTracker tracker, Track track, SideDirection direction, float strafeDuration, GameObject animatingObject)
	{
		return MotionGroundStrafeState.Create(tracker, track, direction, strafeDuration, animatingObject);
	}

	public override MotionState OnAttack(SonicPhysics physics, float initialJumpHeight, GameObject animatingObject, SonicHandling handling, float tapX, float tapY)
	{
		if (m_targetManager.isFloorTargetingEnabled())
		{
			Enemy closestTarget = m_targetManager.getClosestTarget(tapX, tapY);
			if (closestTarget != null)
			{
				Sonic.Controller.activateAutoJump();
				closestTarget.beginAttack();
				m_targetManager.setAutoAttackTarget(closestTarget);
				return new MotionJumpState(physics, 0f, animatingObject, handling);
			}
		}
		return null;
	}
}
