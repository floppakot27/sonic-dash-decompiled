using UnityEngine;

public class RingBounceFake : RingBounce
{
	private const float Gravity = 15f;

	private const float ForwardImpulse = 32f;

	private static Vector3 s_minimumEjectionImpulse = new Vector3(0.2f, 7f, 0.2f);

	private static Vector3 s_maximumEjectionImpulse = new Vector3(3f, 10f, 3f);

	private Vector3 m_position = Vector3.zero;

	private Vector3 m_localPosition = Vector3.zero;

	private Vector3 m_velocity = Vector3.zero;

	private GameObject m_explosionRoot;

	private bool m_bossBattleActive;

	public RingBounceFake(GameObject explosionRoot, bool replicateProperties)
	{
		m_explosionRoot = explosionRoot;
		EventDispatch.RegisterInterest("OnBossBattlePhaseStart", this);
		EventDispatch.RegisterInterest("OnNewGameStarted", this);
		EventDispatch.RegisterInterest("OnSonicDeath", this);
	}

	protected override void InitialiseBounce()
	{
	}

	protected override void ResetBounce(float currentSpeed, Vector3 forward)
	{
		if (Sonic.Bones != null && !(Sonic.Bones[m_source] == null))
		{
			m_localPosition = Sonic.Bones[m_source].transform.position;
			m_position = m_localPosition + m_explosionRoot.transform.position;
			float num = Random.Range(s_minimumEjectionImpulse.x, s_maximumEjectionImpulse.x);
			float new_y = Random.Range(s_minimumEjectionImpulse.y, s_maximumEjectionImpulse.y);
			float num2 = Random.Range(s_minimumEjectionImpulse.z, s_maximumEjectionImpulse.z);
			num *= ((!(Random.value <= 0.5f)) ? 1f : (-1f));
			num2 *= ((!(Random.value <= 0.5f)) ? 1f : (-1f));
			m_velocity.Set(num, new_y, num2);
			Vector3 vector = forward * 32f;
			if (m_bossBattleActive)
			{
				vector *= -0.3f;
			}
			m_velocity += vector;
		}
	}

	protected override void UpdateBounce()
	{
		m_velocity.y -= 15f * Time.deltaTime;
		Vector3 vector = m_velocity * Time.deltaTime;
		m_localPosition += vector;
		m_position = m_localPosition + m_explosionRoot.transform.position;
	}

	protected override void EnableBounce(bool enable)
	{
	}

	protected override Vector3 GetBouncePosition()
	{
		return m_position;
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
