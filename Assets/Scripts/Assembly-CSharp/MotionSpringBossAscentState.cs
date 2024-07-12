using UnityEngine;

public class MotionSpringBossAscentState : MotionSpringState
{
	private float m_highestHeight = float.MinValue;

	private float m_previousHeight;

	public MotionSpringBossAscentState(SonicHandling handling, SonicPhysics physics)
		: base(physics, handling)
	{
	}

	public override void Enter()
	{
		base.Enter();
		JumpAnimationCurve newSpringJumpCurve = Sonic.Handling.GetNewSpringJumpCurve(Sonic.Tracker.HeightAboveLowGround);
		m_previousHeight = newSpringJumpCurve.CalculateHeight(0f);
		base.Physics.StartJump(newSpringJumpCurve);
		Sonic.AudioControl.PlaySpringSFX();
		EventDispatch.GenerateEvent("OnSpringStart", SpringTV.Type.Boss);
		Boss.GetInstance().AttackPhase().StartBehaviour();
	}

	public override LightweightTransform CalculateNewTransform(TransformParameters tParams)
	{
		float jumpHeight = tParams.Physics.JumpHeight;
		bool flag = m_highestHeight > jumpHeight;
		m_highestHeight = Mathf.Max(jumpHeight, m_highestHeight);
		tParams.Physics.TargetSpeed = 0f;
		Vector3 vector = Vector3.up * (jumpHeight - m_previousHeight);
		LightweightTransform result = new LightweightTransform(tParams.CurrentTransform.Location + vector, tParams.CurrentTransform.Orientation);
		tParams.Physics.UpdateJump(pauseHalfway: false);
		if (flag)
		{
			MoveToNextState();
		}
		m_previousHeight = jumpHeight;
		return result;
	}

	private void MoveToNextState()
	{
		MotionState newState = new MotionSpringBossAttackState(Sonic.Handling, base.Physics);
		MotionStateMachine internalMotionState = Sonic.Tracker.InternalMotionState;
		internalMotionState.RequestState(newState);
	}

	private void Event_OnTrackGenerationComplete()
	{
	}
}
