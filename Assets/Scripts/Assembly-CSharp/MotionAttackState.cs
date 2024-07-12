using UnityEngine;

public class MotionAttackState : MotionState
{
	private SonicPhysics m_physics;

	private GameObject m_animatingObject;

	private float m_jumpHeight;

	private float m_timer;

	private float m_attackDuration = 1f;

	private Enemy m_target;

	private Vector3 m_initialPosition;

	private Vector3 m_targetPosition;

	private static Vector3 m_frameMovement = Vector3.zero;

	private float m_distanceToTravel = 1f;

	private float m_speed;

	public MotionAttackState(SonicPhysics physics, float initialJumpHeight, GameObject animatingObject, SonicHandling handling, Enemy target)
	{
		m_target = target;
		m_physics = physics;
		m_animatingObject = animatingObject;
		m_initialPosition = Sonic.Transform.position;
		m_targetPosition = m_target.gameObject.transform.position;
	}

	public override void Enter()
	{
		m_timer = 0f;
		m_distanceToTravel = (m_targetPosition - m_initialPosition).magnitude;
		m_attackDuration = m_distanceToTravel / Sonic.Handling.AttackSpeed;
		m_speed = Sonic.Handling.AttackSpeed;
		Trail.m_instance.activate();
		Sonic.Tracker.disableCollisions();
		EventDispatch.GenerateEvent("OnSonicAttack");
		m_animatingObject.SendMessage("StartAttackAnim", new Pair<float, float>(0f, 0f), SendMessageOptions.DontRequireReceiver);
	}

	public override InterruptCode OnInterrupt(MotionStateMachine machine, MotionState newState)
	{
		return InterruptCode.KillMe;
	}

	public override void Execute()
	{
	}

	public override void Exit()
	{
	}

	public override LightweightTransform CalculateNewTransform(TransformParameters tParams)
	{
		bool flag = false;
		Vector3 zero = Vector3.zero;
		if (Sonic.Handling.AttackAccelerationEnabled)
		{
			Vector3 vector = m_targetPosition - m_initialPosition;
			vector.Normalize();
			m_speed += Sonic.Handling.AttackAcceleration * Time.deltaTime;
			float num = Time.deltaTime * m_speed;
			float magnitude = (Sonic.Transform.position - m_initialPosition).magnitude;
			float num2 = m_distanceToTravel - magnitude;
			if (num >= num2)
			{
				num = num2;
				flag = true;
			}
			zero = (m_frameMovement = vector * num);
		}
		else
		{
			Vector3 vector2 = m_targetPosition - m_initialPosition;
			float num3 = Time.deltaTime / m_attackDuration;
			zero = (m_frameMovement = vector2 * num3);
			m_timer += Time.deltaTime;
			if (m_timer > m_attackDuration)
			{
				flag = true;
			}
		}
		if (flag)
		{
			if (m_target == null || m_target.isAttackableFromAir())
			{
				if ((bool)m_target)
				{
					object[] value = new object[2]
					{
						Sonic.Tracker,
						false
					};
					m_target.SendMessage("OnDeath", value);
					object[] parameters = new object[2]
					{
						m_target,
						Enemy.Kill.Homing
					};
					EventDispatch.GenerateEvent("OnEnemyKilled", parameters);
				}
				tParams.StateMachine.RequestState(new MotionPostAttackState(m_physics, m_jumpHeight, m_animatingObject, Sonic.Handling, m_target));
			}
			else if (DashMonitor.instance().isDashing() || HeadstartMonitor.instance().isHeadstarting())
			{
				Sonic.Tracker.triggerRespawn(!DashMonitor.instance().isDashing() && !HeadstartMonitor.instance().isHeadstarting());
				if ((bool)m_target)
				{
					object[] value2 = new object[2]
					{
						Sonic.Tracker,
						true
					};
					m_target.SendMessage("OnDeath", value2);
					object[] parameters2 = new object[2]
					{
						m_target,
						Enemy.Kill.Homing
					};
					EventDispatch.GenerateEvent("OnEnemyKilled", parameters2);
				}
				tParams.StateMachine.RequestState(new MotionPostAttackState(m_physics, m_jumpHeight, m_animatingObject, Sonic.Handling, m_target));
			}
			else
			{
				m_target.SendMessage("OnSonicKill", Sonic.Tracker);
				tParams.StateMachine.ForceState(tParams.StateMachine.CurrentState.OnSplat(Sonic.Handling, Sonic.Tracker.InternalTracker, SonicAnimationControl.SplatType.Stationary, null));
			}
			Trail.m_instance.deactivate();
			Sonic.Tracker.enableCollisions();
		}
		return new LightweightTransform(tParams.CurrentTransform.Location + zero, tParams.CurrentTransform.Orientation);
	}

	public override bool IsFlying()
	{
		return true;
	}

	public override MotionState OnAttack(SonicPhysics physics, float initialJumpHeight, GameObject animatingObject, SonicHandling handling, float tapX, float tapY)
	{
		return null;
	}

	public override MotionState OnRoll(GameObject animatingObject, SonicPhysics physics, SonicHandling handling)
	{
		return null;
	}

	public static Vector3 getFrameMovement()
	{
		return m_frameMovement;
	}
}
