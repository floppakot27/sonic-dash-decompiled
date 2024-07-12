using UnityEngine;

public class MotionFallState : MotionDeathState
{
	private Rigidbody m_rigidBody;

	private Vector3 m_initialVelocity;

	private bool m_sploshPlayed;

	private float m_deathCountdown = 1f;

	private float m_deathTimeout = 2.5f;

	private float m_deathTimer;

	private static GameObject m_worldGameObject;

	public MotionFallState(TransformParameters tParams)
	{
		if (m_worldGameObject == null)
		{
			m_worldGameObject = GameObject.Find("World");
		}
		m_rigidBody = Sonic.AnimationControl.gameObject.rigidbody;
		m_initialVelocity = Sonic.Tracker.CurrentVelocity * 0.5f;
	}

	public override void Enter()
	{
		base.Enter();
		m_sploshPlayed = false;
		m_deathTimer = m_deathTimeout;
		SetSonicKinematic(bKinematic: false);
		Sonic.AnimationControl.StartFallAnim();
		EventDispatch.GenerateEvent("OnSonicFall");
		EventDispatch.GenerateEvent("OnSonicDeath");
		Physics.IgnoreLayerCollision(16, 24, ignore: false);
	}

	public override void Exit()
	{
		SetSonicKinematic(bKinematic: true);
		Physics.IgnoreLayerCollision(16, 24, ignore: true);
	}

	public override bool IsFalling()
	{
		return true;
	}

	public override InterruptCode OnInterrupt(MotionStateMachine machine, MotionState newState)
	{
		SetSonicKinematic(bKinematic: true);
		return InterruptCode.KillMe;
	}

	public override LightweightTransform CalculateNewTransform(TransformParameters tParams)
	{
		m_deathTimer -= Time.deltaTime;
		if (!m_sploshPlayed && m_rigidBody.transform.position.y < -9f)
		{
			m_sploshPlayed = true;
			EventDispatch.GenerateEvent("OnSplosh");
			SetSonicKinematic(bKinematic: true);
			m_deathTimer = m_deathCountdown;
		}
		if (m_deathTimer <= 0f)
		{
			if (!m_sploshPlayed)
			{
				SetSonicKinematic(bKinematic: true);
			}
			bool bAllowRespawn = ((Sonic.Tracker.GetTrackSegmentForRevive_Simple() != null) ? true : false);
			DoDeath(bAllowRespawn, tParams.StateMachine);
		}
		return tParams.Tracker.CurrentSplineTransform;
	}

	public override MotionState OnFall()
	{
		return null;
	}

	private void SetSonicKinematic(bool bKinematic)
	{
		if (bKinematic)
		{
			m_rigidBody.AddForce(-m_rigidBody.velocity, ForceMode.VelocityChange);
			m_rigidBody.isKinematic = true;
			m_rigidBody.useGravity = false;
		}
		else
		{
			m_rigidBody.isKinematic = false;
			m_rigidBody.useGravity = true;
			m_rigidBody.AddForce(m_initialVelocity, ForceMode.VelocityChange);
		}
	}
}
