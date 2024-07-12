using System;
using UnityEngine;

[RequireComponent(typeof(GuiTrigger))]
public class HudDisplay : MonoBehaviour
{
	[Flags]
	private enum State
	{
		None = 0,
		RequireCountDown = 1,
		CountingDown = 2
	}

	private static bool s_displayHud = true;

	private State m_state;

	private float m_displayTimer;

	[SerializeField]
	private float m_startGameDelay = 5f;

	private GuiTrigger m_guiTrigger;

	private HudContent m_hudContent;

	public static bool DisplayHud
	{
		get
		{
			return s_displayHud;
		}
		set
		{
			s_displayHud = value;
		}
	}

	private void Start()
	{
		EventDispatch.RegisterInterest("DisableGameState", this);
		EventDispatch.RegisterInterest("ResetGameState", this);
		EventDispatch.RegisterInterest("StartGameState", this);
		EventDispatch.RegisterInterest("OnSonicDeath", this);
		EventDispatch.RegisterInterest("OnSonicResurrection", this);
		EventDispatch.RegisterInterest("OnBossBattleIntroStart", this);
		EventDispatch.RegisterInterest("OnBossBattleIntroEnd", this);
		EventDispatch.RegisterInterest("OnBossReticuleShow", this);
		EventDispatch.RegisterInterest("OnBossBattleOutroEnd", this);
		m_guiTrigger = GetComponent<GuiTrigger>();
		m_hudContent = GetComponent<HudContent>();
		m_state = State.None;
	}

	private void Update()
	{
		if (!m_guiTrigger.Visible && s_displayHud && (m_state & State.CountingDown) == State.CountingDown)
		{
			m_displayTimer += Time.deltaTime;
			if (m_displayTimer >= m_startGameDelay)
			{
				m_state &= ~State.RequireCountDown;
				m_state &= ~State.CountingDown;
				SetHudVisibility(visible: true);
				m_hudContent.NewGameHudVisible();
			}
		}
	}

	private void SetHudVisibility(bool visible)
	{
		if (visible)
		{
			m_guiTrigger.Show();
		}
		else
		{
			m_guiTrigger.Hide();
		}
		m_hudContent.HudVisible(visible);
	}

	private void Event_ResetGameState(GameState.Mode state)
	{
		m_state |= State.RequireCountDown;
		m_state &= ~State.CountingDown;
	}

	private void Event_StartGameState(GameState.Mode state)
	{
		if (state == GameState.Mode.Menu || state == GameState.Mode.PauseMenu)
		{
			SetHudVisibility(visible: false);
			return;
		}
		if ((m_state & State.RequireCountDown) != State.RequireCountDown)
		{
			SetHudVisibility(visible: true);
			return;
		}
		m_state |= State.CountingDown;
		m_displayTimer = 0f;
	}

	private void Event_DisableGameState(GameState.Mode state)
	{
		SetHudVisibility(visible: false);
	}

	private void Event_OnSonicDeath()
	{
		SetHudVisibility(visible: false);
	}

	private void Event_OnSonicResurrection()
	{
		SetHudVisibility(visible: true);
	}

	private void Event_OnBossBattleIntroStart()
	{
		SetHudVisibility(visible: false);
	}

	private void Event_OnBossBattleIntroEnd()
	{
		SetHudVisibility(visible: true);
	}

	private void Event_OnBossReticuleShow(BossAttack.HitpointSetting hitPoint)
	{
		SetHudVisibility(visible: false);
	}

	private void Event_OnBossBattleOutroEnd()
	{
		SetHudVisibility(visible: true);
	}
}
