using UnityEngine;

internal class MotionGroundStrafeState : MotionStrafeState
{
	private GameObject m_animatingObject;

	protected MotionGroundStrafeState(SplineUtils.SplineParameters newSpline, SplineTracker mainTracker, GameObject animatingObject, float strafeDuration, SideDirection strafeDirection)
		: base(newSpline, mainTracker, animatingObject, strafeDuration, strafeDirection, animatingObject.transform.position, 3f)
	{
		m_animatingObject = animatingObject;
	}

	public static MotionStrafeState Create(SplineTracker tracker, Track track, SideDirection direction, float strafeDuration, GameObject animatingObject)
	{
		return MotionStrafeState.Create(tracker, track, direction, strafeDuration, animatingObject, (SplineUtils.SplineParameters newSpline) => new MotionGroundStrafeState(newSpline, tracker, animatingObject, strafeDuration, direction));
	}

	public override MotionState OnStrafe(SplineTracker tracker, Track track, SideDirection direction, float strafeDuration, GameObject animatingObject)
	{
		if (base.StrafeTimer > Sonic.Handling.DoubleStrafeCutoffTime)
		{
			base.QueuedState = () => Create(tracker, track, direction, strafeDuration, animatingObject);
			return null;
		}
		return Create(tracker, track, direction, strafeDuration, animatingObject);
	}

	public override MotionState OnKnockedSideways(SplineTracker tracker, Track track, SideDirection direction, float strafeDuration, GameObject animatingObject)
	{
		return MotionKnockedSidewaysState.Create(tracker, track, direction, strafeDuration, animatingObject);
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

	public override MotionState OnRoll(GameObject animatingObject, SonicPhysics physics, SonicHandling handling)
	{
		if (!DashMonitor.instance().isDashing() && !HeadstartMonitor.instance().isHeadstarting() && physics.TimeSpentNotRolling > handling.RollCoolDownDuration)
		{
			base.QueuedState = () => new MotionRollState(animatingObject, handling);
		}
		return null;
	}

	public override MotionState OnStumble(GameObject animatingObject, SonicHandling handling)
	{
		return new MotionStumbleState(animatingObject, handling);
	}

	public override void Enter()
	{
		base.Enter();
		m_animatingObject.SendMessage("StartHopAnim", m_strafeDirection, SendMessageOptions.DontRequireReceiver);
		Sonic.AudioControl.PlayHopSFX();
	}

	public override void Exit()
	{
	}

	protected override MotionStrafeState CreateNewStrafe(SplineUtils.SplineParameters newSpline, SplineTracker mainTracker, GameObject animatingObject, float strafeDuration, SideDirection strafeDirection)
	{
		return new MotionGroundStrafeState(newSpline, mainTracker, animatingObject, strafeDuration, strafeDirection);
	}

	protected override void OnStrafeCompletion(TransformParameters tParams, bool isFalling)
	{
		if (isFalling)
		{
			if (DashMonitor.instance().isDashing() || HeadstartMonitor.instance().isHeadstarting())
			{
				tParams.StateMachine.RequestState(new MotionJumpState(tParams.Physics, 0f, Sonic.Tracker.gameObject, Sonic.Handling));
			}
			else
			{
				tParams.StateMachine.ForceState(new MotionFallState(tParams));
			}
			tParams.StateMachine.PopTopState();
		}
		else
		{
			if (base.QueuedState != null)
			{
				tParams.StateMachine.RequestState(base.QueuedState());
			}
			tParams.StateMachine.PopTopState();
		}
	}
}
