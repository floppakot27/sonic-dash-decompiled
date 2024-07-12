using System;
using UnityEngine;

public class MotionDiveState : MotionState
{
	private SonicPhysics m_physics;

	private GameObject m_animatingObject;

	private SplineTracker m_sourceSplineTracker;

	private float m_jumpHeight;

	private Func<MotionState> m_queuedState;

	private MotionStrafeState m_strafe;

	private float DiveTimeRemaining => m_jumpHeight / Sonic.Handling.DiveSpeed;

	public MotionDiveState(SonicPhysics physics, float initialJumpHeight, GameObject animatingObject, SonicHandling handling, MotionStrafeState currentStrafe, MotionState queuedState)
	{
		m_jumpHeight = initialJumpHeight;
		m_physics = physics;
		m_animatingObject = animatingObject;
		m_strafe = currentStrafe?.ContinueInTime(DiveTimeRemaining);
		if (queuedState != null)
		{
			m_queuedState = () => queuedState;
		}
	}

	public override void Enter()
	{
	}

	public override void Exit()
	{
	}

	public override LightweightTransform CalculateNewTransform(TransformParameters tParams)
	{
		LightweightTransform lightweightTransform;
		if (m_strafe == null)
		{
			tParams.Tracker.UpdatePosition();
			lightweightTransform = tParams.Tracker.CurrentSplineTransform;
		}
		else
		{
			lightweightTransform = m_strafe.CalculateNewTransform(tParams);
		}
		bool flag = false;
		m_jumpHeight -= Sonic.Handling.DiveSpeed * Time.deltaTime;
		if (m_jumpHeight < 0f)
		{
			m_jumpHeight = 0f;
			flag = true;
		}
		m_physics.JumpHeight = m_jumpHeight;
		Vector3 location = lightweightTransform.Location;
		location += lightweightTransform.Up * m_jumpHeight;
		if (flag)
		{
			tParams.StateMachine.PopTopState();
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
			else
			{
				MotionState newState = ((m_queuedState == null) ? new MotionRollState(m_animatingObject, Sonic.Handling) : m_queuedState());
				tParams.StateMachine.RequestState(newState);
				m_animatingObject.SendMessage("OnSlam", SendMessageOptions.DontRequireReceiver);
			}
		}
		return new LightweightTransform(location, lightweightTransform.Orientation);
	}

	public override bool IsFlying()
	{
		return true;
	}

	public override MotionState OnStrafe(SplineTracker tracker, Track track, SideDirection direction, float strafeDuration, GameObject animatingObject)
	{
		if (m_queuedState != null)
		{
			return null;
		}
		m_queuedState = () => MotionGroundStrafeState.Create(tracker, track, direction, strafeDuration, animatingObject);
		return null;
	}

	public override MotionState OnStumble(GameObject animatingObject, SonicHandling handling)
	{
		m_queuedState = () => new MotionStumbleState(animatingObject, handling);
		return null;
	}

	public override MotionState OnSetPiece()
	{
		m_queuedState = () => new MotionSetPieceState();
		return null;
	}
}
