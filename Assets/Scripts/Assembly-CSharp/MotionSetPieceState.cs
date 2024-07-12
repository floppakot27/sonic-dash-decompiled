using UnityEngine;

public class MotionSetPieceState : MotionState
{
	private MotionStrafeState m_strafeHelper;

	private LightweightTransform m_lastGroundTransform;

	public override InterruptCode OnInterrupt(MotionStateMachine machine, MotionState newState)
	{
		return InterruptCode.SuspendMe;
	}

	public override void Enter()
	{
		Sonic.AnimationControl.SendMessage("StartRollAnim", new Pair<float, float>(-1f, -1f), SendMessageOptions.DontRequireReceiver);
	}

	public override void Exit()
	{
		Sonic.AnimationControl.SendMessage("StopRollAnim", SendMessageOptions.DontRequireReceiver);
	}

	public override LightweightTransform CalculateNewTransform(TransformParameters tParams)
	{
		tParams.Physics.TargetSpeed = tParams.Physics.IdealSpeed * Sonic.Handling.SetPieceDashPadSpeedMultiplier;
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
		return m_lastGroundTransform;
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

	public override void OnSetPieceEnd(MotionStateMachine stateMachine)
	{
		stateMachine.PopTopState();
	}
}
