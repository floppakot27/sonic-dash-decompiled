using UnityEngine;

public class MotionSplatState : MotionDeathState
{
	private float m_startSpeed;

	private float m_splatDuration;

	private float m_splatTimer;

	private SplineTracker m_tracker;

	private SonicAnimationControl.SplatType m_splatType;

	public MotionSplatState(SonicHandling handling, SplineTracker tracker, SonicAnimationControl.SplatType splatType)
	{
		m_splatTimer = 0f;
		m_tracker = tracker;
		if (splatType == SonicAnimationControl.SplatType.Stationary)
		{
			float num = m_tracker.Target.CalculateCurvatureAt(tracker.CurrentDistance, 2f);
			splatType = ((!(num > 0.5f)) ? SonicAnimationControl.SplatType.Backwards : SonicAnimationControl.SplatType.Stationary);
		}
		m_splatType = splatType;
		m_startSpeed = 0.01f;
		m_startSpeed += ((m_splatType != 0) ? (m_tracker.TrackSpeed * Sonic.Handling.SplatRestitution) : 0f);
	}

	public override void Enter()
	{
		base.Enter();
		m_splatDuration = Sonic.AnimationControl.StartSplatAnim(m_splatType);
		if (m_splatType == SonicAnimationControl.SplatType.Forwards)
		{
			m_tracker.Start(m_startSpeed, m_tracker.CurrentDistance, Direction_1D.Forwards);
		}
		else
		{
			m_tracker.Start(m_startSpeed, m_tracker.CurrentDistance, Direction_1D.Backwards);
		}
		EventDispatch.GenerateEvent("OnSonicDeath");
		EventDispatch.GenerateEvent("OnSonicSplat");
		Sonic.AudioControl.PlayDeathSFX();
	}

	public override void Exit()
	{
	}

	public override LightweightTransform CalculateNewTransform(TransformParameters tParams)
	{
		float num = m_splatDuration + Sonic.Handling.PostDeathPoseDuration;
		m_splatTimer += Time.deltaTime;
		tParams.Physics.TargetSpeed = Utils.MapValue(m_splatTimer, 0f, m_splatDuration, m_startSpeed, 0f);
		LightweightTransform currentSplineTransform = tParams.Tracker.CurrentSplineTransform;
		if (m_splatTimer >= num && (tParams.CurrentTransform.Location - currentSplineTransform.Location).y < 1f)
		{
			DoDeath(bAllowRespawn: true, tParams.StateMachine);
		}
		float b = tParams.Tracker.UpdatePosition();
		b = Mathf.Max(5f * Time.deltaTime, b);
		Vector3 pos = Vector3.MoveTowards(tParams.CurrentTransform.Location, currentSplineTransform.Location, b * 1.1f);
		Quaternion rot = currentSplineTransform.Orientation;
		if (m_splatType != SonicAnimationControl.SplatType.Forwards)
		{
			rot = Quaternion.LookRotation(-currentSplineTransform.Forwards, currentSplineTransform.Up);
		}
		return new LightweightTransform(pos, rot);
	}

	public override MotionState OnSplat(SonicHandling handling, SplineTracker tracker, SonicAnimationControl.SplatType splatType, Hazard hazard)
	{
		return null;
	}
}
