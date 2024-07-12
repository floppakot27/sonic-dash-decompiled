using UnityEngine;

public abstract class MotionSpringState : MotionState
{
	private Vector3 m_forwards;

	private SonicPhysics m_physics;

	protected SonicPhysics Physics => m_physics;

	protected Vector3 FlatForward => m_forwards;

	protected MotionSpringState(SonicPhysics physics, SonicHandling handling)
	{
		m_physics = physics;
		EventDispatch.RegisterInterest("OnTrackGenerationComplete", this);
	}

	public override void Enter()
	{
		m_forwards = Sonic.Transform.forward;
		m_forwards.y = 0f;
		m_forwards.Normalize();
	}

	public override void Exit()
	{
		EventDispatch.UnregisterInterest("OnTrackGenerationComplete", this);
	}

	public override bool IsSpringing()
	{
		return true;
	}
}
