using System;
using UnityEngine;

public class MotionJumpState : MotionState
{
	private GameObject m_animatingObject;

	private float m_currentJumpHeight;

	private TargetManager m_targetManager;

	private MotionStrafeState m_strafeHelper;

	private SonicPhysics m_physics;

	private Func<MotionState> m_queuedState;

	private Vector3 m_lastGroundPosition;

	private float m_initialGroundHeight;

	public MotionJumpState(SonicPhysics physics, float initialGroundHeight, GameObject animatingObject, SonicHandling handling)
	{
		m_targetManager = TargetManager.instance();
		m_currentJumpHeight = physics.JumpHeight;
		m_animatingObject = animatingObject;
		if (!m_targetManager.isFloorTargetingEnabled())
		{
			m_targetManager.activate();
		}
		m_physics = physics;
		EventDispatch.RegisterInterest("OnSonicDeath", this);
		m_initialGroundHeight = initialGroundHeight;
	}

	public override InterruptCode OnInterrupt(MotionStateMachine machine, MotionState newState)
	{
		return InterruptCode.KillMe;
	}

	public override void Enter()
	{
		m_physics.StartJump(m_initialGroundHeight);
		m_animatingObject.SendMessage("StartJumpAnim", new Pair<float, float>(0f, 0f), SendMessageOptions.DontRequireReceiver);
	}

	public override void Execute()
	{
	}

	public override void Exit()
	{
		Sonic.Controller.deactivateAutoJump();
	}

	public void Event_OnSonicDeath()
	{
		Exit();
	}

	public override LightweightTransform CalculateNewTransform(TransformParameters tParams)
	{
		bool flag = false;
		bool flag2 = DashMonitor.instance().isDashing() || HeadstartMonitor.instance().isHeadstarting();
		LightweightTransform lightweightTransform;
		if (m_strafeHelper != null)
		{
			lightweightTransform = m_strafeHelper.CalculateNewTransform(tParams);
			if (m_strafeHelper.IsFinished)
			{
				m_strafeHelper = ((m_strafeHelper.QueuedState == null) ? null : (m_strafeHelper.QueuedState() as MotionStrafeState));
			}
		}
		else
		{
			tParams.Tracker.UpdatePosition();
			lightweightTransform = tParams.Tracker.CurrentSplineTransform;
		}
		Vector3 vector = (m_lastGroundPosition = lightweightTransform.Location);
		bool pauseHalfway = flag2 && (tParams.OverGap || tParams.OverSmallIsland);
		bool flag3 = tParams.Physics.UpdateJump(pauseHalfway);
		m_currentJumpHeight = tParams.Physics.JumpHeight;
		Vector3 pos = vector;
		pos += lightweightTransform.Up * m_currentJumpHeight;
		if (Sonic.Controller.isAutoJumpActive() && tParams.Physics.JumpHeight >= m_targetManager.getAutoAttackHeight())
		{
			Exit();
			Enemy autoAttackTarget = m_targetManager.getAutoAttackTarget();
			autoAttackTarget.beginAttack();
			tParams.StateMachine.RequestState(new MotionAttackState(tParams.Physics, m_currentJumpHeight, m_animatingObject, Sonic.Handling, autoAttackTarget));
			flag = true;
		}
		if (!flag && flag3)
		{
			if (!m_targetManager.isFloorTargetingEnabled())
			{
				m_targetManager.deactivate();
			}
			Exit();
			m_animatingObject.SendMessage("StopJumpAnim", SendMessageOptions.DontRequireReceiver);
			if (tParams.OverGap)
			{
				if (flag2)
				{
					tParams.StateMachine.RequestState(new MotionJumpState(tParams.Physics, 0f, Sonic.Tracker.gameObject, Sonic.Handling));
				}
				else
				{
					tParams.StateMachine.ForceState(new MotionFallState(tParams));
				}
			}
			else if (m_queuedState != null)
			{
				tParams.StateMachine.RequestState(m_queuedState());
			}
			else
			{
				tParams.StateMachine.PopTopState();
			}
		}
		return new LightweightTransform(pos, lightweightTransform.Orientation);
	}

	public override bool IsFlying()
	{
		return true;
	}

	public override MotionState OnStrafe(SplineTracker tracker, Track track, SideDirection direction, float strafeDuration, GameObject animatingObject)
	{
		float num = m_physics.JumpTimeRemaining / strafeDuration;
		if (num < Sonic.Handling.AirStrafeDurationMultiplier * 0.5f)
		{
			m_queuedState = () => MotionGroundStrafeState.Create(tracker, track, direction, strafeDuration, animatingObject);
			return null;
		}
		strafeDuration *= Mathf.Min(num, Sonic.Handling.AirStrafeDurationMultiplier);
		if (m_strafeHelper != null)
		{
			if (m_strafeHelper.OnStrafe(tracker, track, direction, strafeDuration, animatingObject) is MotionStrafeState strafeHelper)
			{
				m_strafeHelper = strafeHelper;
				Sonic.AudioControl.PlayStrafeSFX();
			}
		}
		else
		{
			m_strafeHelper = MotionStrafeState.CreateHelper(tracker, track, direction, strafeDuration, animatingObject, m_lastGroundPosition, Sonic.Handling.AirStrafeSmoothness);
			Sonic.AudioControl.PlayStrafeSFX();
		}
		return null;
	}

	public override MotionState OnStumble(GameObject animatingObject, SonicHandling handling)
	{
		m_queuedState = () => new MotionStumbleState(animatingObject, handling);
		return null;
	}

	public override MotionState OnDive(SonicPhysics physics, float initialJumpHeight, GameObject animatingObject, SonicHandling handling)
	{
		bool flag = false;
		if ((!DashMonitor.instance().isDashing() && !HeadstartMonitor.instance().isHeadstarting()) ? true : false)
		{
			return CreateDiveState(null);
		}
		return null;
	}

	public override MotionState OnAttack(SonicPhysics physics, float initialJumpHeight, GameObject animatingObject, SonicHandling handling, float tapX, float tapY)
	{
		if (!Sonic.Controller.isAutoJumpActive())
		{
			Enemy closestTarget = m_targetManager.getClosestTarget(tapX, tapY);
			if (closestTarget != null)
			{
				closestTarget.beginAttack();
				return new MotionAttackState(physics, m_currentJumpHeight, animatingObject, handling, closestTarget);
			}
		}
		return null;
	}

	public override MotionState OnSetPiece()
	{
		MotionState postDiveState = base.OnSetPiece();
		return CreateDiveState(postDiveState);
	}

	public override MotionState OnSpring(Track track, SonicHandling handling, SonicPhysics physics, SpringTV.Type springType, SpringTV.Destination destination, SpringTV.CreateFlags createFlags)
	{
		MotionState motionState = base.OnSpring(track, handling, physics, springType, destination, createFlags);
		if (DashMonitor.instance().isDashing() || HeadstartMonitor.instance().isHeadstarting())
		{
			return motionState;
		}
		return CreateDiveState(motionState);
	}

	private MotionState CreateDiveState(MotionState postDiveState)
	{
		if (!m_targetManager.isFloorTargetingEnabled())
		{
			m_targetManager.deactivate();
		}
		Exit();
		return new MotionDiveState(m_physics, m_currentJumpHeight, m_animatingObject, Sonic.Handling, m_strafeHelper, postDiveState);
	}
}
