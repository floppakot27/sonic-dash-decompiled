using UnityEngine;

public class HeadstartMonitor : MonoBehaviour
{
	private static HeadstartMonitor m_instance;

	private bool m_active;

	private bool m_headstartRequest;

	private bool m_superRequest;

	private bool m_isHeadstarting;

	private bool m_superHeadstart;

	private bool m_isNearlyFinished;

	private float m_timer;

	private bool m_sonicIsDead;

	public float[] m_headstartLevels;

	public float m_superHeadStartDuration;

	public float m_nearlyFinishedDuration = 0.6f;

	private bool m_exitPermitted = true;

	public float m_gapSafeDistance = 100f;

	private bool m_allowSafeZone = true;

	private void Awake()
	{
		m_instance = this;
	}

	public static HeadstartMonitor instance()
	{
		return m_instance;
	}

	private void Start()
	{
		EventDispatch.RegisterInterest("HeadStartActivated", this);
		EventDispatch.RegisterInterest("OnNewGameStarted", this);
		EventDispatch.RegisterInterest("OnGameFinished", this);
		EventDispatch.RegisterInterest("OnSonicDeath", this);
		EventDispatch.RegisterInterest("ResetGameState", this);
		EventDispatch.RegisterInterest("OnSpringStart", this);
		EventDispatch.RegisterInterest("OnSpringEnd", this);
	}

	public void Event_HeadStartActivated(bool super)
	{
		if (!m_isHeadstarting)
		{
			m_headstartRequest = true;
			m_superRequest = super;
		}
	}

	private void Event_OnNewGameStarted()
	{
		m_active = true;
		m_isHeadstarting = false;
		m_isNearlyFinished = false;
		m_sonicIsDead = false;
		m_headstartRequest = false;
		m_superRequest = false;
		m_superHeadstart = false;
		m_allowSafeZone = true;
	}

	private void Event_OnSpringStart(SpringTV.Type springType)
	{
		m_allowSafeZone = false;
	}

	private void Event_OnSpringEnd()
	{
		m_allowSafeZone = true;
	}

	private void Event_OnGameFinished()
	{
		m_active = false;
		m_isHeadstarting = false;
		m_isNearlyFinished = false;
		m_superHeadstart = false;
	}

	private void Event_OnSonicDeath()
	{
		m_sonicIsDead = true;
	}

	private void Event_ResetGameState(GameState.Mode mode)
	{
		m_active = false;
		m_isHeadstarting = false;
		m_isNearlyFinished = false;
		m_superHeadstart = false;
	}

	private void Update()
	{
		if (!m_active)
		{
			return;
		}
		if (m_isHeadstarting)
		{
			m_timer -= Time.deltaTime;
			if (m_timer < m_nearlyFinishedDuration)
			{
				m_isNearlyFinished = true;
			}
			if (!(m_timer < 0f) && !m_sonicIsDead)
			{
				return;
			}
			bool flag = true;
			if (!m_sonicIsDead)
			{
				bool flag2 = false;
				if (m_allowSafeZone && Sonic.Tracker.GetDistanceToNextGap() < m_gapSafeDistance)
				{
					flag2 = true;
				}
				if (!m_exitPermitted || flag2)
				{
					flag = false;
				}
			}
			if (flag)
			{
				m_isHeadstarting = false;
				m_isNearlyFinished = false;
				Sonic.Tracker.triggerRespawn(usedPowerUp: false);
			}
		}
		else
		{
			if (!m_headstartRequest || m_sonicIsDead || !Sonic.Tracker || !Sonic.Tracker.isReadyForDash())
			{
				return;
			}
			if (m_superRequest)
			{
				PowerUpsInventory.ModifyPowerUpStock(PowerUps.Type.SuperHeadStart, -1);
				m_timer = m_superHeadStartDuration;
				PlayerStats.IncreaseStat(PlayerStats.StatNames.SuperHeadstartsUsed_Total, 1);
				PlayerStats.IncreaseStat(PlayerStats.StatNames.SuperHeadstartsUsed_Session, 1);
				PlayerStats.IncreaseStat(PlayerStats.StatNames.SuperHeadstartsUsed_Run, 1);
			}
			else
			{
				int powerUpLevel = PowerUpsInventory.GetPowerUpLevel(PowerUps.Type.HeadStart);
				PowerUpsInventory.ModifyPowerUpStock(PowerUps.Type.HeadStart, -1);
				PlayerStats.IncreaseStat(PlayerStats.StatNames.HeadstartsUsed_Total, 1);
				PlayerStats.IncreaseStat(PlayerStats.StatNames.HeadstartsUsed_Session, 1);
				PlayerStats.IncreaseStat(PlayerStats.StatNames.HeadstartsUsed_Run, 1);
				if (powerUpLevel < m_headstartLevels.Length)
				{
					m_timer = m_headstartLevels[powerUpLevel];
				}
				else
				{
					m_timer = 1f;
				}
			}
			m_isHeadstarting = true;
			m_superHeadstart = m_superRequest;
			m_isNearlyFinished = false;
			m_headstartRequest = false;
			m_superRequest = false;
		}
	}

	public bool isHeadstarting()
	{
		return m_isHeadstarting;
	}

	public bool isSuperHeadstart()
	{
		return m_superHeadstart;
	}

	public bool isHeadstartNearlyFinished()
	{
		return m_isNearlyFinished;
	}

	public void preventExit()
	{
		m_exitPermitted = false;
	}

	public void permitExit()
	{
		m_exitPermitted = true;
	}
}
