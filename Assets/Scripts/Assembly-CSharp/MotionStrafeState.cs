using System;
using UnityEngine;

public class MotionStrafeState : MotionState
{
	private SplineTracker m_originalSplineTracker;

	private float m_strafeTimer;

	private float m_strafeDuration;

	private float m_initialRight;

	protected SideDirection m_strafeDirection;

	private Func<MotionState> m_nextStateMaker;

	private Vector3 m_lastStrafePosition;

	private float m_strafeSmoothness;

	public bool IsFinished => m_strafeTimer >= m_strafeDuration;

	public Func<MotionState> QueuedState
	{
		get
		{
			return m_nextStateMaker;
		}
		protected set
		{
			m_nextStateMaker = value;
		}
	}

	protected float StrafeTimer => m_strafeTimer;

	protected float StrafeDuration => m_strafeDuration;

	protected SideDirection StrafeDirection => m_strafeDirection;

	private MotionStrafeState(SplineTracker originalTracker, float strafeDuration, Vector3 currentPosition, float strafeSmoothness, SideDirection strafeDirection)
	{
		m_originalSplineTracker = originalTracker;
		m_strafeTimer = 0f;
		m_strafeDuration = strafeDuration;
		m_strafeSmoothness = strafeSmoothness;
		m_strafeDirection = strafeDirection;
		Vector3 rhs = currentPosition - m_originalSplineTracker.CurrentSplineTransform.Location;
		m_initialRight = Vector3.Dot(m_originalSplineTracker.CurrentSplineTransform.Right, rhs);
	}

	protected MotionStrafeState(MotionStrafeState strafeToContinue, float timeRemaining)
		: this(strafeToContinue.m_originalSplineTracker, timeRemaining, strafeToContinue.m_lastStrafePosition, strafeToContinue.m_strafeSmoothness, strafeToContinue.m_strafeDirection)
	{
	}

	protected MotionStrafeState(SplineUtils.SplineParameters newSpline, SplineTracker mainTracker, GameObject animatingObject, float strafeDuration, SideDirection strafeDirection, Vector3 currentPosition, float strafeSmoothness)
		: this(mainTracker.Clone(), strafeDuration, currentPosition, strafeSmoothness, strafeDirection)
	{
		newSpline.ApplyTo(mainTracker);
	}

	public static MotionStrafeState CreateHelper(SplineTracker tracker, Track track, SideDirection direction, float strafeDuration, GameObject animatingObject, Vector3 currentPosition, float strafeSmoothness)
	{
		return Create(tracker, track, direction, strafeDuration, animatingObject, (SplineUtils.SplineParameters newSpline) => new MotionStrafeState(newSpline, tracker, animatingObject, strafeDuration, direction, currentPosition, strafeSmoothness));
	}

	public override LightweightTransform CalculateNewTransform(TransformParameters tParams)
	{
		m_originalSplineTracker.UpdatePosition();
		tParams.Tracker.UpdatePosition();
		return CalculateNewGroundTransform(tParams);
	}

	public LightweightTransform CalculateNewTransformPostUpdate(TransformParameters tParams)
	{
		m_originalSplineTracker.UpdatePositionByDelta(tParams.Tracker.PreviousDelta);
		return CalculateNewGroundTransform(tParams);
	}

	public override MotionState OnStrafe(SplineTracker tracker, Track track, SideDirection direction, float strafeDuration, GameObject animatingObject)
	{
		if (StrafeDirection != direction)
		{
			return Create(tracker, track, direction, strafeDuration, animatingObject, (SplineUtils.SplineParameters newSpline) => CreateNewStrafe(newSpline, tracker, animatingObject, strafeDuration, direction));
		}
		float newStrafeDuration = ((!(StrafeTimer <= Sonic.Handling.DoubleStrafeCutoffTime)) ? StrafeTimer : (StrafeDuration - StrafeTimer));
		return Create(tracker, track, direction, newStrafeDuration, animatingObject, delegate(SplineUtils.SplineParameters newSpline)
		{
			tracker.CopyFrom(m_originalSplineTracker);
			return CreateNewStrafe(newSpline, tracker, animatingObject, newStrafeDuration, direction);
		});
	}

	public MotionStrafeState ContinueInTime(float remainingTimeToCompletion)
	{
		return new MotionStrafeState(this, remainingTimeToCompletion);
	}

	private LightweightTransform CalculateNewGroundTransform(TransformParameters tParams)
	{
		LightweightTransform currentSplineTransform = tParams.Tracker.CurrentSplineTransform;
		LightweightTransform currentSplineTransform2 = m_originalSplineTracker.CurrentSplineTransform;
		currentSplineTransform2.Location += currentSplineTransform2.Right * m_initialRight;
		m_strafeTimer += Time.deltaTime;
		float num = Utils.SmoothlyApproach1(m_strafeTimer, 0f, m_strafeDuration, m_strafeSmoothness);
		LightweightTransform result = LightweightTransform.Lerp(currentSplineTransform2, currentSplineTransform, num);
		m_lastStrafePosition = result.Location;
		if (num >= 0.6f && (tParams.OverGap || num >= 0.8f))
		{
			OnStrafeCompletion(tParams, tParams.OverGap);
		}
		return result;
	}

	protected virtual MotionStrafeState CreateNewStrafe(SplineUtils.SplineParameters newSpline, SplineTracker mainTracker, GameObject animatingObject, float strafeDuration, SideDirection strafeDirection)
	{
		return new MotionStrafeState(newSpline, mainTracker, animatingObject, strafeDuration, strafeDirection, m_lastStrafePosition, m_strafeSmoothness);
	}

	protected static MotionStrafeState Create(SplineTracker tracker, Track track, SideDirection direction, float strafeDuration, GameObject animatingObject, Func<SplineUtils.SplineParameters, MotionStrafeState> factory)
	{
		SplineUtils.SplineParameters splineToSideOf = track.GetSplineToSideOf(tracker.Target, tracker.CurrentSplineTransform, direction);
		if (splineToSideOf.IsValid)
		{
			MotionStrafeState result = factory(splineToSideOf);
			animatingObject.SendMessage("OnStrafe", direction, SendMessageOptions.DontRequireReceiver);
			return result;
		}
		return null;
	}

	public override void Enter()
	{
	}

	public override void Exit()
	{
	}

	protected virtual void OnStrafeCompletion(TransformParameters tParams, bool isFalling)
	{
	}
}
