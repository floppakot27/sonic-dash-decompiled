public class MotionRewindState : MotionState
{
	private float m_distanceToRewindTo;

	public MotionRewindState(float distanceToRewindTo)
	{
		m_distanceToRewindTo = distanceToRewindTo;
	}

	public override void Enter()
	{
		Sonic.Tracker.ResetOnTrack();
		Sonic.Tracker.InternalTracker.Reverse();
		Sonic.Tracker.InternalTracker.RunBackwards = true;
		Sonic.AnimationControl.OnRewind();
		TutorialSystem.instance().onRewindStart();
		BehindCamera instance = BehindCamera.Instance;
		if (instance != null)
		{
			instance.ResetToGameCamera(2f);
		}
	}

	public override void Exit()
	{
		Sonic.Tracker.InternalTracker.RunBackwards = false;
		TutorialSystem.instance().onRewindFinished();
	}

	public override MotionState OnSplat(SonicHandling handling, SplineTracker tracker, SonicAnimationControl.SplatType splatType, Hazard hazard)
	{
		return null;
	}

	public override MotionState OnFall()
	{
		return null;
	}

	public override LightweightTransform CalculateNewTransform(TransformParameters tParams)
	{
		LightweightTransform result = tParams.Tracker.Update(tParams.CurrentTransform);
		if (m_distanceToRewindTo > Sonic.Tracker.TrackPosition)
		{
			Sonic.Tracker.Resurrect(freeRevive: true);
		}
		return result;
	}
}
