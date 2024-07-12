using System;
using UnityEngine;

public class HudContent_DashMeter
{
	[Flags]
	private enum State
	{
		None = 1,
		Filled = 2,
		Visible = 4,
		Hud = 8
	}

	private DashMeter m_dashMeter;

	private UISprite m_filledMeterSprite;

	private GameObject m_dashModeEnabledTrigger;

	private GameObject m_dashModeShowTrigger;

	private UITexture m_dashMeterBar;

	private float m_fullBarScale;

	private AudioClip m_dashMeterBlockedAudio;

	private State m_state = State.None;

	private bool m_fightingBoss;

	public HudContent_DashMeter(DashMeter dashMeter, UISprite filledMeterSprite, GameObject dashModeShowTrigger, GameObject dashModeEnabledTrigger, AudioClip dashMeterBlockedAudio, UITexture dashMeterBar)
	{
		m_dashMeter = dashMeter;
		m_dashMeterBlockedAudio = dashMeterBlockedAudio;
		m_filledMeterSprite = filledMeterSprite;
		m_dashModeEnabledTrigger = dashModeEnabledTrigger;
		m_dashModeShowTrigger = dashModeShowTrigger;
		m_dashMeterBar = dashMeterBar;
		m_fullBarScale = m_dashMeterBar.transform.localScale.y;
		EventDispatch.RegisterInterest("OnDashMeterTriggered", this);
		EventDispatch.RegisterInterest("OnDashMeterFilled", this);
		EventDispatch.RegisterInterest("OnBossBattlePhaseStart", this);
	}

	public void Update()
	{
		if (ShouldShowDashMeter())
		{
			ShowDashMeter();
		}
		UpdateMeterBar(m_dashMeter.Value);
		if (m_fightingBoss)
		{
			m_dashMeter.ForceFinishBurnDown();
		}
	}

	public void OnResetOnNewGame()
	{
		UpdateMeterBar(0f);
		if ((m_state & State.Filled) == State.Filled)
		{
			m_dashModeEnabledTrigger.SendMessage("OnClick", SendMessageOptions.RequireReceiver);
		}
		m_dashMeterBar.gameObject.SetActive(value: true);
		m_filledMeterSprite.gameObject.SetActive(value: false);
		HideDashMeter();
		m_fightingBoss = false;
		m_state = State.None;
	}

	public void OnPauseStateChanged(bool paused)
	{
	}

	public void OnPlayerDeath()
	{
	}

	public void HudVisible(bool visible)
	{
		if (visible)
		{
			m_state |= State.Hud;
		}
	}

	private void UpdateMeterBar(float progress)
	{
		Vector3 localScale = m_dashMeterBar.transform.localScale;
		localScale.y = m_fullBarScale * progress;
		m_dashMeterBar.transform.localScale = localScale;
	}

	private bool ShouldShowDashMeter()
	{
		if ((m_state & State.Visible) == State.Visible)
		{
			return false;
		}
		if ((m_state & State.Hud) != State.Hud)
		{
			return false;
		}
		if (m_fightingBoss)
		{
			return false;
		}
		return true;
	}

	private void HideDashMeter()
	{
		if ((m_state & State.Visible) == State.Visible)
		{
			m_dashModeShowTrigger.SendMessage("OnClick", SendMessageOptions.RequireReceiver);
			m_state &= ~State.Visible;
		}
	}

	private void ShowDashMeter()
	{
		if (ShouldShowDashMeter())
		{
			m_dashModeShowTrigger.SendMessage("OnClick", SendMessageOptions.RequireReceiver);
			m_state |= State.Visible;
			if (m_dashMeter.Value >= 1f)
			{
				m_dashMeterBar.gameObject.SetActive(value: false);
				m_filledMeterSprite.gameObject.SetActive(value: true);
			}
			else
			{
				m_dashMeterBar.gameObject.SetActive(value: true);
				m_filledMeterSprite.gameObject.SetActive(value: false);
			}
		}
	}

	private void Event_OnDashMeterTriggered(bool enabled)
	{
		if (!enabled)
		{
			Audio.PlayClip(m_dashMeterBlockedAudio, loop: false);
			return;
		}
		if ((m_state & State.Filled) == State.Filled)
		{
			m_dashModeEnabledTrigger.SendMessage("OnClick", SendMessageOptions.RequireReceiver);
		}
		m_dashMeterBar.gameObject.SetActive(value: true);
		m_filledMeterSprite.gameObject.SetActive(value: false);
		m_state &= ~State.Filled;
	}

	private void Event_OnDashMeterFilled()
	{
		if ((m_state & State.Filled) != State.Filled)
		{
			m_state |= State.Filled;
			m_dashMeterBar.gameObject.SetActive(value: false);
			m_filledMeterSprite.gameObject.SetActive(value: true);
			m_dashModeEnabledTrigger.SendMessage("OnClick", SendMessageOptions.RequireReceiver);
		}
	}

	private void Event_OnBossBattlePhaseStart(int currentPhase)
	{
		BossBattleSystem.Phase phase = BossBattleSystem.Instance().GetPhase(currentPhase);
		switch (phase.Type)
		{
		case BossBattleSystem.Phase.Types.Arrive:
			m_fightingBoss = true;
			HideDashMeter();
			break;
		case BossBattleSystem.Phase.Types.Finish:
			m_fightingBoss = false;
			break;
		}
	}
}
