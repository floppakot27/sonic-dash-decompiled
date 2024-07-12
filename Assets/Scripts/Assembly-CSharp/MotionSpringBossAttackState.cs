using UnityEngine;

public class MotionSpringBossAttackState : MotionSpringState
{
	public MotionSpringBossAttackState(SonicHandling handling, SonicPhysics physics)
		: base(physics, handling)
	{
	}

	private void Awake()
	{
	}

	public override void Enter()
	{
		base.Enter();
		EventDispatch.RegisterInterest("BossSpringGestureSuccess", this);
		EventDispatch.RegisterInterest("BossSpringGestureFailure", this);
	}

	public override void Execute()
	{
	}

	public override void Exit()
	{
		EventDispatch.UnregisterInterest("BossSpringGestureSuccess", this);
		EventDispatch.UnregisterInterest("BossSpringGestureFailure", this);
	}

	public override LightweightTransform CalculateNewTransform(TransformParameters tParams)
	{
		return tParams.CurrentTransform;
	}

	public override MotionState OnStrafe(SplineTracker tracker, Track track, SideDirection direction, float strafeDuration, GameObject animatingObject)
	{
		BossBattleSystem.GestureSettings.Types gesture = ((direction != 0) ? BossBattleSystem.GestureSettings.Types.Right : BossBattleSystem.GestureSettings.Types.Left);
		OnGestureInput(gesture);
		return null;
	}

	public override MotionState OnJump(SonicPhysics physics, float initialGroundHeight, GameObject animatingObject, SonicHandling handling)
	{
		OnGestureInput(BossBattleSystem.GestureSettings.Types.Up);
		return null;
	}

	public override MotionState OnRoll(GameObject animatingObject, SonicPhysics physics, SonicHandling handling)
	{
		OnGestureInput(BossBattleSystem.GestureSettings.Types.Down);
		return null;
	}

	public override MotionState OnAttack(SonicPhysics physics, float initialJumpHeight, GameObject animatingObject, SonicHandling handling, float tapX, float tapY)
	{
		OnGestureInput(BossBattleSystem.GestureSettings.Types.TapOnScreen);
		return null;
	}

	private void OnGestureInput(BossBattleSystem.GestureSettings.Types gesture)
	{
		EventDispatch.GenerateEvent("OnGestureInput", gesture);
	}

	private void MoveToNextState()
	{
		MotionStateMachine internalMotionState = Sonic.Tracker.InternalMotionState;
		internalMotionState.RequestState(new MotionSpringBossFinishState(Sonic.Handling, base.Physics));
	}

	private void Event_BossSpringGestureSuccess()
	{
		if (BossBattleSystem.Instance() != null)
		{
			EventDispatch.GenerateEvent("OnBossAttackGestureEndSuccess");
			MoveToNextState();
		}
	}

	private void Event_BossSpringGestureFailure()
	{
		if (BossBattleSystem.Instance() != null)
		{
			EventDispatch.GenerateEvent("OnBossAttackGestureEndFailure", base.Physics.JumpTimeRemaining);
			MoveToNextState();
		}
	}

	private void Event_OnTrackGenerationComplete()
	{
	}
}
