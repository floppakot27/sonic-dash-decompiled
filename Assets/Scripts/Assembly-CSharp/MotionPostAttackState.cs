using System;
using UnityEngine;

public class MotionPostAttackState : MotionState
{
	private GameObject m_animatingObject;

	private TargetManager m_targetManager;

	private Func<MotionState> m_queuedState;

	private bool m_resetTracker;

	private SonicPhysics m_physics;

	private Enemy m_target;

	private MotionStrafeState m_strafeHelper;

	private Vector3 m_lastGroundPosition;

	public MotionPostAttackState(SonicPhysics physics, float initialJumpHeight, GameObject animatingObject, SonicHandling handling, Enemy target)
	{
		m_target = target;
		m_physics = physics;
		m_targetManager = TargetManager.instance();
		m_animatingObject = animatingObject;
		m_targetManager.notifyAttack();
		m_resetTracker = true;
	}

	public override InterruptCode OnInterrupt(MotionStateMachine machine, MotionState newState)
	{
		return InterruptCode.KillMe;
	}

	public override void Enter()
	{
		m_physics.StartJump(0f);
		Sonic.MotionMonitor.setStatic();
	}

	public override void Exit()
	{
	}

	public override LightweightTransform CalculateNewTransform(TransformParameters tParams)
	{
		if (m_resetTracker)
		{
			Spline spline = null;
			float num = 10000f;
			float initialPosition = 0f;
			m_resetTracker = false;
			Enemy target = m_target;
			TargetManager.instance().removeTarget(target);
			Spline spline2 = target.getSpline();
			Spline[] componentsInChildren = spline2.transform.parent.GetComponentsInChildren<Spline>();
			Spline[] array = componentsInChildren;
			foreach (Spline spline3 in array)
			{
				Utils.ClosestPoint closestPoint = spline3.EstimateDistanceAlongSpline(tParams.CurrentTransform.Location);
				if (spline == null || closestPoint.SqrError < num)
				{
					num = closestPoint.SqrError;
					spline = spline3;
					initialPosition = closestPoint.LineDistance;
				}
			}
			tParams.Tracker.Target = spline;
			tParams.Tracker.Start(tParams.Tracker.TrackSpeed, initialPosition, Direction_1D.Forwards);
			m_animatingObject.SendMessage("OnStrafe", SideDirection.Left, SendMessageOptions.DontRequireReceiver);
		}
		LightweightTransform currentTransform = tParams.CurrentTransform;
		currentTransform.Location -= new Vector3(0f, tParams.Physics.JumpHeight * 3f, 0f);
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
			LightweightTransform lightweightTransform2 = tParams.Tracker.SmoothTowardsTargetPosition(currentTransform, 10f, 5f, snapRotation: false);
			lightweightTransform = lightweightTransform2;
		}
		m_lastGroundPosition = lightweightTransform.Location;
		bool flag = tParams.Physics.UpdateJump(pauseHalfway: false);
		float y = tParams.Physics.JumpHeight * 3f;
		Vector3 pos = lightweightTransform.Location + new Vector3(0f, y, 0f);
		bool flag2 = false;
		if (TargetManager.instance().m_queueAttacks && TargetManager.instance().isAttackQueued() && tParams.Physics.JumpProgress >= 0.5f)
		{
			TargetManager.instance().consumeAttack();
			Enemy closestTarget = m_targetManager.getClosestTarget(TargetManager.instance().getQueuedAttackX(), TargetManager.instance().getQueuedAttackY());
			if (closestTarget != null)
			{
				Sonic.MotionMonitor.setMoving();
				closestTarget.beginAttack();
				tParams.StateMachine.RequestState(new MotionAttackState(m_physics, 0f, m_animatingObject, Sonic.Handling, closestTarget));
				flag2 = true;
			}
		}
		if (flag && !flag2)
		{
			m_targetManager.notifyEndOfAttacking();
			if (!m_targetManager.isFloorTargetingEnabled())
			{
				m_targetManager.deactivate();
			}
			Sonic.MotionMonitor.setMoving();
			EventDispatch.GenerateEvent("OnSonicAttackEnd");
			m_animatingObject.SendMessage("StopRollAnim", SendMessageOptions.DontRequireReceiver);
			if (m_queuedState != null)
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

	public override MotionState OnDive(SonicPhysics physics, float initialJumpHeight, GameObject animatingObject, SonicHandling handling)
	{
		if (TutorialSystem.instance().isTrackTutorialEnabled())
		{
			return null;
		}
		m_targetManager.deactivate();
		return new MotionDiveState(physics, physics.JumpHeight, animatingObject, handling, m_strafeHelper, null);
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
			}
		}
		else
		{
			m_strafeHelper = MotionStrafeState.CreateHelper(tracker, track, direction, strafeDuration, animatingObject, m_lastGroundPosition, Sonic.Handling.AirStrafeSmoothness);
		}
		return null;
	}

	public override MotionState OnAttack(SonicPhysics physics, float initialJumpHeight, GameObject animatingObject, SonicHandling handling, float tapX, float tapY)
	{
		Enemy closestTarget = m_targetManager.getClosestTarget(tapX, tapY);
		if (closestTarget != null)
		{
			Sonic.MotionMonitor.setMoving();
			closestTarget.beginAttack();
			return new MotionAttackState(physics, 0f, animatingObject, handling, closestTarget);
		}
		return null;
	}
}
