using UnityEngine;

public class DashMonitor : MonoBehaviour
{
	private static DashMonitor m_instance;

	private bool m_active;

	private bool m_dashEnableRequest;

	private bool m_dashDisableRequest;

	private bool m_isDashing;

	private bool m_isNearlyFinished;

	public GameObject m_forceField;

	private bool m_sonicIsDead;

	private bool m_exitPermitted = true;

	public float m_gapSafeDistance = 100f;

	private bool m_allowSafeZone = true;

	private void Awake()
	{
		m_instance = this;
	}

	public static DashMonitor instance()
	{
		return m_instance;
	}

	private void Start()
	{
		EventDispatch.RegisterInterest("OnDashStarted", this);
		EventDispatch.RegisterInterest("OnDashStop", this);
		EventDispatch.RegisterInterest("OnDashFinishingSoon", this);
		EventDispatch.RegisterInterest("OnDashFinished", this);
		EventDispatch.RegisterInterest("OnNewGameStarted", this);
		EventDispatch.RegisterInterest("OnGameFinished", this);
		EventDispatch.RegisterInterest("OnSonicDeath", this);
		EventDispatch.RegisterInterest("ResetGameState", this);
		EventDispatch.RegisterInterest("OnSonicResurrection", this);
		EventDispatch.RegisterInterest("OnSpringStart", this);
		EventDispatch.RegisterInterest("OnSpringEnd", this);
	}

	public void Event_OnDashStarted(float f)
	{
		m_dashEnableRequest = true;
	}

	public void Event_OnDashAutoTrigger()
	{
		m_dashEnableRequest = true;
	}

	public void Event_OnDashStop()
	{
		m_dashDisableRequest = true;
	}

	public void Event_OnDashFinishingSoon()
	{
		m_isNearlyFinished = true;
	}

	public void Event_OnDashFinished()
	{
		m_dashDisableRequest = true;
	}

	private void resetAfterRestart()
	{
		m_active = true;
		m_isDashing = false;
		m_isNearlyFinished = false;
		m_sonicIsDead = false;
		m_dashEnableRequest = false;
		m_dashDisableRequest = false;
	}

	private void Event_OnNewGameStarted()
	{
		resetAfterRestart();
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

	private void Event_OnSonicResurrection()
	{
		resetAfterRestart();
	}

	private void Event_OnGameFinished()
	{
		m_active = false;
		m_isDashing = false;
		m_isNearlyFinished = false;
	}

	private void Event_OnSonicDeath()
	{
		m_sonicIsDead = true;
	}

	private void Event_ResetGameState(GameState.Mode mode)
	{
		m_active = false;
		m_isDashing = false;
		m_isNearlyFinished = false;
	}

	private void Update()
	{
		if (!m_active)
		{
			return;
		}
		if (m_isDashing)
		{
			if (!m_dashDisableRequest && !m_sonicIsDead)
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
				m_isDashing = false;
				m_isNearlyFinished = false;
				m_dashDisableRequest = false;
				if (!TutorialSystem.instance().isTrackTutorialEnabled())
				{
					Sonic.Tracker.triggerRespawn(usedPowerUp: false);
				}
			}
		}
		else if (m_dashEnableRequest && !m_sonicIsDead && (bool)Sonic.Tracker && Sonic.Tracker.isReadyForDash())
		{
			m_isDashing = true;
			m_isNearlyFinished = false;
			m_dashEnableRequest = false;
			m_dashDisableRequest = false;
			PlayerStats.IncreaseStat(PlayerStats.StatNames.DashUses_Total, 1);
			PlayerStats.IncreaseStat(PlayerStats.StatNames.DashUses_Session, 1);
			PlayerStats.IncreaseStat(PlayerStats.StatNames.DashUses_Run, 1);
			PlayerStats.DashMeterFilled = false;
		}
	}

	public bool isDashing()
	{
		return m_isDashing;
	}

	public bool isDashNearlyFinished()
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
