using UnityEngine;

public class MotionSprintState : MotionState
{
	private bool m_dashing;

	private TargetManager m_targetManager;

	private int m_subZoneIndex = -1;

	public MotionSprintState()
	{
		m_targetManager = TargetManager.instance();
	}

	public override InterruptCode OnInterrupt(MotionStateMachine machine, MotionState newState)
	{
		return InterruptCode.SuspendMe;
	}

	public override void Enter()
	{
		m_dashing = false;
		TrackSegment segmentOfSpline = TrackSegment.GetSegmentOfSpline(Sonic.Tracker.CurrentSpline);
		m_subZoneIndex = segmentOfSpline.Template.SubzoneIndex;
		Sonic.AudioControl.StartSprintAudio((m_subZoneIndex != 0) ? SonicAudioControl.MaterialType.Stone : SonicAudioControl.MaterialType.Grass);
		EventDispatch.GenerateEvent("EnterMotionSprintState");
	}

	public override void Exit()
	{
		Sonic.AudioControl.StopSprintAudio();
		EventDispatch.GenerateEvent("ExitMotionSprintState");
	}

	public override void Execute()
	{
		if (m_dashing != DashMonitor.instance().isDashing() || HeadstartMonitor.instance().isHeadstarting())
		{
			m_dashing = DashMonitor.instance().isDashing() || HeadstartMonitor.instance().isHeadstarting();
			if (m_dashing)
			{
				Sonic.AnimationControl.OnDashBegin();
			}
			else
			{
				Sonic.AnimationControl.OnDashEnd();
			}
		}
	}

	public override LightweightTransform CalculateNewTransform(TransformParameters tParams)
	{
		tParams.Physics.ClearJump();
		LightweightTransform result = tParams.Tracker.Update(tParams.CurrentTransform);
		if (tParams.OverGap)
		{
			if (Sonic.Tracker.gapRespawnIsPermitted())
			{
				if (m_dashing)
				{
					tParams.StateMachine.RequestState(new MotionJumpState(tParams.Physics, 0f, Sonic.Tracker.gameObject, Sonic.Handling));
				}
				else
				{
					tParams.StateMachine.ForceState(new MotionFallState(tParams));
				}
			}
			else
			{
				tParams.StateMachine.ForceState(new MotionFallState(tParams));
			}
		}
		SonicPhysics physics = Sonic.Tracker.getPhysics();
		if (physics.IdealSpeed > 0f)
		{
			float num = physics.CurrentSpeed / physics.IdealSpeed;
			if ((CharacterManager.Singleton.GetCurrentCharacter() == Characters.Type.Tails || CharacterManager.Singleton.GetCurrentCharacter() == Characters.Type.Shadow) && num > 0.95f)
			{
				num = 0f;
			}
			Sonic.AudioControl.SetSprintAudioFrequencyMultiplier(num);
		}
		return result;
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
		flag = ((!DashMonitor.instance().isDashing() && !HeadstartMonitor.instance().isHeadstarting()) ? true : false);
		if (Sonic.Tracker.TrackerOverGap)
		{
			flag = false;
		}
		if (flag)
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

	public override bool IsReadyForDash()
	{
		return true;
	}
}
