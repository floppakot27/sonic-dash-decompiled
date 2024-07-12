using UnityEngine;

public class ShieldMonitor : MonoBehaviour
{
	public float m_nearlyFinishedWarnDuration = 0.8f;

	public float[] m_shieldingLevels;

	public int m_initialChance = 30;

	public int m_minimumChance = 20;

	public int m_chanceReductionOnCollect = 10;

	private static ShieldMonitor s_singleton;

	private bool m_active;

	private bool m_shieldRequest;

	private bool m_isShielded;

	private bool m_isNearlyFinished;

	private bool m_sonicIsDead;

	private float m_timer;

	public int m_chance;

	private bool m_springPaused;

	public void EndShielding()
	{
		m_isShielded = false;
		m_isNearlyFinished = false;
		m_shieldRequest = false;
	}

	public bool isShielded()
	{
		return m_isShielded;
	}

	public bool isShieldNearlyFinished()
	{
		return m_isNearlyFinished;
	}

	public void requestShield()
	{
		m_shieldRequest = true;
	}

	public static ShieldMonitor instance()
	{
		return s_singleton;
	}

	public void notifyShieldPickup()
	{
		m_chance -= m_chanceReductionOnCollect;
		if (m_chance < m_minimumChance)
		{
			m_chance = m_minimumChance;
		}
	}

	public int getShieldChance()
	{
		return m_chance;
	}

	private void Awake()
	{
		s_singleton = this;
	}

	private void Start()
	{
		EventDispatch.RegisterInterest("OnNewGameStarted", this);
		EventDispatch.RegisterInterest("OnGameFinished", this);
		EventDispatch.RegisterInterest("OnSonicDeath", this);
		EventDispatch.RegisterInterest("OnSonicResurrection", this);
		EventDispatch.RegisterInterest("OnSonicStumble", this);
		EventDispatch.RegisterInterest("ResetGameState", this);
		EventDispatch.RegisterInterest("OnSpringStart", this);
		EventDispatch.RegisterInterest("OnSpringEnd", this);
	}

	private void Event_OnNewGameStarted()
	{
		m_active = true;
		m_isShielded = false;
		m_isNearlyFinished = false;
		m_sonicIsDead = false;
		m_shieldRequest = false;
		m_chance = m_initialChance;
	}

	private void Event_OnGameFinished()
	{
		m_active = false;
	}

	private void Event_OnSonicDeath()
	{
		m_sonicIsDead = true;
	}

	private void Event_OnSonicResurrection()
	{
		m_sonicIsDead = false;
	}

	private void Event_ResetGameState(GameState.Mode mode)
	{
		m_active = false;
		m_isShielded = false;
		m_isNearlyFinished = false;
	}

	private void Event_OnSonicStumble()
	{
		m_isShielded = false;
		m_isNearlyFinished = false;
		m_shieldRequest = false;
	}

	private void Event_OnSpringStart(SpringTV.Type springType)
	{
		if (!m_isNearlyFinished)
		{
			m_springPaused = true;
		}
	}

	private void Event_OnSpringEnd()
	{
		m_springPaused = false;
	}

	private void Update()
	{
		if (!m_active)
		{
			return;
		}
		if (m_isShielded)
		{
			if (m_springPaused)
			{
				return;
			}
			m_timer -= Time.deltaTime;
			if (m_timer < 0f || m_sonicIsDead)
			{
				m_isShielded = false;
				m_isNearlyFinished = false;
				m_shieldRequest = false;
			}
			else if (m_timer < m_nearlyFinishedWarnDuration)
			{
				m_isNearlyFinished = true;
			}
		}
		if (m_shieldRequest && !m_sonicIsDead && (bool)Sonic.Tracker && Sonic.Tracker.isReadyForShield())
		{
			int powerUpLevel = PowerUpsInventory.GetPowerUpLevel(PowerUps.Type.Shield);
			if (powerUpLevel < m_shieldingLevels.Length)
			{
				m_timer = m_shieldingLevels[powerUpLevel];
			}
			else
			{
				m_timer = 1f;
			}
			m_isShielded = true;
			m_isNearlyFinished = false;
			m_shieldRequest = false;
			PlayerStats.IncreaseStat(PlayerStats.StatNames.PowerShieldPicked_Run, 1);
		}
	}
}
