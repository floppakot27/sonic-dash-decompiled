using UnityEngine;

public class MagnetMonitor : MonoBehaviour
{
	private static MagnetMonitor m_instance;

	private bool m_active;

	private bool m_magnetiseRequest;

	private bool m_isMagnetised;

	private bool m_isNearlyFinished;

	public float m_nearlyFinishedWarnDuration = 0.6f;

	private bool m_sonicIsDead;

	private float m_timer;

	public float[] m_magnetisationLevels;

	public int m_RPMThreshold = 1;

	private int m_chance;

	public int m_initialChance = 30;

	public int m_minimumChance = 20;

	public int m_chanceReductionOnCollect = 10;

	private bool m_springPaused;

	public void notifyMagnetPickup()
	{
		m_chance -= m_chanceReductionOnCollect;
		if (m_chance < m_minimumChance)
		{
			m_chance = m_minimumChance;
		}
	}

	public int getMagnetChance()
	{
		return m_chance;
	}

	private void Awake()
	{
		m_instance = this;
	}

	public static MagnetMonitor instance()
	{
		return m_instance;
	}

	private void Start()
	{
		EventDispatch.RegisterInterest("OnNewGameStarted", this);
		EventDispatch.RegisterInterest("OnGameFinished", this);
		EventDispatch.RegisterInterest("OnSonicDeath", this);
		EventDispatch.RegisterInterest("OnSonicResurrection", this);
		EventDispatch.RegisterInterest("OnSonicStumble", this);
		EventDispatch.RegisterInterest("OnSonicRespawn", this);
		EventDispatch.RegisterInterest("ResetGameState", this);
		EventDispatch.RegisterInterest("OnSpringStart", this);
		EventDispatch.RegisterInterest("OnSpringEnd", this);
	}

	private void Event_OnNewGameStarted()
	{
		m_active = true;
		m_isMagnetised = false;
		m_isNearlyFinished = false;
		m_sonicIsDead = false;
		m_magnetiseRequest = false;
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

	public void Event_OnSonicResurrection()
	{
		m_sonicIsDead = false;
	}

	private void Event_ResetGameState(GameState.Mode mode)
	{
		m_active = false;
		m_isMagnetised = false;
		m_isNearlyFinished = false;
	}

	private void Event_OnSonicStumble()
	{
		m_isMagnetised = false;
		m_isNearlyFinished = false;
		m_magnetiseRequest = false;
	}

	private void Event_OnSonicRespawn()
	{
		bool flag = true;
		if (DashMonitor.instance().isDashing() || HeadstartMonitor.instance().isHeadstarting())
		{
			flag = false;
		}
		if (flag)
		{
			m_isMagnetised = false;
			m_isNearlyFinished = false;
			m_magnetiseRequest = false;
		}
	}

	private void Event_OnSpringStart(SpringTV.Type springType)
	{
		m_springPaused = true;
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
		if (m_isMagnetised)
		{
			if (m_springPaused)
			{
				return;
			}
			m_timer -= Time.deltaTime;
			if (m_timer < 0f || m_sonicIsDead)
			{
				m_isMagnetised = false;
				m_isNearlyFinished = false;
				m_magnetiseRequest = false;
			}
			else if (m_timer < m_nearlyFinishedWarnDuration)
			{
				m_isNearlyFinished = true;
			}
		}
		if (m_magnetiseRequest && !m_sonicIsDead && (bool)Sonic.Tracker && Sonic.Tracker.isReadyForMagnet())
		{
			int powerUpLevel = PowerUpsInventory.GetPowerUpLevel(PowerUps.Type.Magnet);
			if (powerUpLevel < m_magnetisationLevels.Length)
			{
				m_timer = m_magnetisationLevels[powerUpLevel];
			}
			else
			{
				m_timer = 1f;
			}
			m_isMagnetised = true;
			m_isNearlyFinished = false;
			m_magnetiseRequest = false;
			PlayerStats.IncreaseStat(PlayerStats.StatNames.PowerMagnetsPicked_Total, 1);
			PlayerStats.IncreaseStat(PlayerStats.StatNames.PowerMagnetsPicked_Session, 1);
			PlayerStats.IncreaseStat(PlayerStats.StatNames.PowerMagnetsPicked_Run, 1);
		}
	}

	public bool isMagnetised()
	{
		return m_isMagnetised;
	}

	public bool isMagnetNearlyFinished()
	{
		return m_isNearlyFinished;
	}

	public void requestMagnet()
	{
		m_magnetiseRequest = true;
	}

	public int GetRPM()
	{
		return m_RPMThreshold;
	}
}
