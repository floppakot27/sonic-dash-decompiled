using UnityEngine;

public class MotionGlideState : MotionState
{
	public MotionGlideState(TransformParameters tParams)
	{
	}

	public override void Enter()
	{
		DashMonitor.instance().preventExit();
		HeadstartMonitor.instance().preventExit();
		Sonic.AnimationControl.StartGlideAnim();
	}

	public override void Exit()
	{
		Sonic.AnimationControl.StopGlideAnim();
	}

	public override LightweightTransform CalculateNewTransform(TransformParameters tParams)
	{
		tParams.Physics.ClearJump();
		LightweightTransform result = tParams.Tracker.Update(tParams.CurrentTransform);
		if (!tParams.OverGap)
		{
			DashMonitor.instance().permitExit();
			HeadstartMonitor.instance().permitExit();
			tParams.StateMachine.PopTopState();
		}
		return result;
	}

	public override MotionState OnRoll(GameObject animatingObject, SonicPhysics physics, SonicHandling handling)
	{
		return null;
	}

	public override MotionState OnJump(SonicPhysics physics, float initialGroundHeight, GameObject animatingObject, SonicHandling handling)
	{
		return null;
	}

	public override MotionState OnStumble(GameObject animatingObject, SonicHandling handling)
	{
		return null;
	}

	public override MotionState OnStrafe(SplineTracker tracker, Track track, SideDirection direction, float strafeDuration, GameObject animatingObject)
	{
		return MotionGroundStrafeState.Create(tracker, track, direction, strafeDuration, animatingObject);
	}

	public override InterruptCode OnInterrupt(MotionStateMachine machine, MotionState newState)
	{
		return InterruptCode.KillMe;
	}
}
