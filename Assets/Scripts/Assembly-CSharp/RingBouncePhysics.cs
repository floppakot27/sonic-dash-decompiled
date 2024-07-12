using UnityEngine;

public class RingBouncePhysics : RingBounce
{
	private const float ForwardImpulse = 1.2f;

	private const float BounceImpulse = 8f;

	private const float BounceDecay = 0.3f;

	private static Vector3 s_minimumEjectionImpulse = new Vector3(0.2f, 5f, 0.2f);

	private static Vector3 s_maximumEjectionImpulse = new Vector3(3f, 8f, 3f);

	private Rigidbody m_rigidBody;

	private float m_currentUpwardImpulse;

	private bool m_bossBattleActive;

	public RingBouncePhysics(GameObject explosionRoot, bool replicateProperties)
	{
		m_rigidBody = explosionRoot.GetComponentInChildren<Rigidbody>();
		if (replicateProperties)
		{
			m_rigidBody = ReplicateRigidBody(m_rigidBody);
		}
		CollisionCallback component = m_rigidBody.GetComponent<CollisionCallback>();
		component.CollisionEnter = OnCollisionEnter;
		EventDispatch.RegisterInterest("OnBossBattlePhaseStart", this);
		EventDispatch.RegisterInterest("OnNewGameStarted", this);
		EventDispatch.RegisterInterest("OnSonicDeath", this);
	}

	protected override void InitialiseBounce()
	{
	}

	protected override void ResetBounce(float currentSpeed, Vector3 forward)
	{
		if (!(m_rigidBody == null) && Sonic.Bones != null && !(Sonic.Bones[m_source] == null))
		{
			m_rigidBody.position = Sonic.Bones[m_source].transform.position;
			m_rigidBody.transform.position = Sonic.Bones[m_source].position;
			m_rigidBody.transform.localPosition = Sonic.Bones[m_source].position;
			m_rigidBody.velocity = Vector3.zero;
			m_rigidBody.angularVelocity = Vector3.zero;
			float num = Random.Range(s_minimumEjectionImpulse.x, s_maximumEjectionImpulse.x);
			float num2 = Random.Range(s_minimumEjectionImpulse.y, s_maximumEjectionImpulse.y);
			float num3 = Random.Range(s_minimumEjectionImpulse.z, s_maximumEjectionImpulse.z);
			num *= ((!(Random.value <= 0.5f)) ? 1f : (-1f));
			num3 *= ((!(Random.value <= 0.5f)) ? 1f : (-1f));
			Vector3 vector = forward * (1.2f * currentSpeed);
			if (m_bossBattleActive)
			{
				vector *= -0.3f;
			}
			num += vector.x;
			num2 += vector.y;
			num3 += vector.z;
			m_rigidBody.AddForce(num, num2, num3, ForceMode.Impulse);
			m_currentUpwardImpulse = 8f;
		}
	}

	protected override void UpdateBounce()
	{
		m_rigidBody.AddForce(0f, -50f, 0f, ForceMode.Acceleration);
	}

	protected override void EnableBounce(bool enable)
	{
		if ((bool)m_rigidBody)
		{
			m_rigidBody.isKinematic = !enable;
			m_rigidBody.gameObject.SetActive(enable);
		}
	}

	protected override Vector3 GetBouncePosition()
	{
		return m_rigidBody.position;
	}

	private Rigidbody ReplicateRigidBody(Rigidbody originalBody)
	{
		GameObject gameObject = Object.Instantiate(originalBody.gameObject) as GameObject;
		gameObject.transform.parent = originalBody.transform.parent;
		m_rigidBody = gameObject.GetComponent<Rigidbody>();
		return m_rigidBody;
	}

	private void OnCollisionEnter(Collision other)
	{
		if (other.gameObject.layer == 19)
		{
			m_currentUpwardImpulse -= 0.3f;
			if (m_currentUpwardImpulse < 0f)
			{
				m_currentUpwardImpulse = 0f;
			}
			m_rigidBody.AddForce(0f, m_currentUpwardImpulse, 0f, ForceMode.Impulse);
		}
	}

	private void Event_OnBossBattlePhaseStart(int currentPhase)
	{
		BossBattleSystem.Phase phase = BossBattleSystem.Instance().GetPhase(currentPhase);
		if (phase.Type == BossBattleSystem.Phase.Types.Attack1)
		{
			m_bossBattleActive = true;
		}
		else if (phase.Type == BossBattleSystem.Phase.Types.Attack2)
		{
			m_bossBattleActive = false;
		}
	}

	private void Event_OnNewGameStarted()
	{
		m_bossBattleActive = false;
	}

	private void Event_OnSonicDeath()
	{
		m_bossBattleActive = false;
	}
}
