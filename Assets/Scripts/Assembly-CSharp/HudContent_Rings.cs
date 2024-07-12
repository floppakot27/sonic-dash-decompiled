using System;
using UnityEngine;

public class HudContent_Rings
{
	public delegate void UpdateBanking();

	private UILabel m_currentRingCount;

	private UILabel m_bankedRingCount;

	private float m_bankingRate = 1f;

	private bool m_fightingBoss;

	private bool m_shown = true;

	private float m_bankingSpeed;

	private AudioClip m_bankingAudio;

	private AudioClip m_startBankingAudio;

	private GameObject m_bankingCountTrigger;

	private GameObject m_normalDisplayTrigger;

	private float m_currentBankingCount;

	private float m_currentHeldCount;

	private float m_lastCueBankingCount;

	private float m_targetBankingCount;

	private bool m_bankingTransitionDone;

	private bool m_paused;

	private bool m_bankedLabelVisible;

	private UpdateBanking m_updateBanking;

	public HudContent_Rings(UILabel currentRingCount, UILabel bankedRingCount, float bankingRate, GameObject bankingCountTrigger, AudioClip startBankingAudio, AudioClip bankingAudio, GameObject displayTrigger)
	{
		m_normalDisplayTrigger = displayTrigger;
		m_currentRingCount = currentRingCount;
		m_bankedRingCount = bankedRingCount;
		m_bankingRate = bankingRate;
		m_bankingAudio = bankingAudio;
		m_startBankingAudio = startBankingAudio;
		m_bankingCountTrigger = bankingCountTrigger;
		EventDispatch.RegisterInterest("OnBossBattlePhaseStart", this);
		EventDispatch.RegisterInterest("OnAttackTheBossHintPrompt", this);
		EventDispatch.RegisterInterest("OnAttackTheBossFailPrompt", this);
		EventDispatch.RegisterInterest("OnBossBattleOutroEnd", this);
		EventDispatch.RegisterInterest("OnBossBattleIntroEnd", this);
	}

	public void Update()
	{
		if (!(m_currentRingCount == null) && !(m_bankedRingCount == null))
		{
			if (!m_shown && !m_fightingBoss)
			{
				ShowNonBankedRingUI();
			}
			if (RingStorage.Banked && m_updateBanking == null)
			{
				InitialiseRingBanking();
			}
			if (m_updateBanking != null)
			{
				m_updateBanking();
			}
			else
			{
				UpdateRingPickUps();
			}
		}
	}

	public void BankingLabelTransitioned()
	{
		m_bankingTransitionDone = true;
	}

	public void OnResetOnNewGame()
	{
		m_currentBankingCount = 0f;
		m_currentHeldCount = 0f;
		m_lastCueBankingCount = 0f;
		m_targetBankingCount = 0f;
		m_bankingTransitionDone = false;
		m_updateBanking = null;
		m_paused = false;
		if (m_bankedLabelVisible)
		{
			m_bankingCountTrigger.SendMessage("OnClick", SendMessageOptions.RequireReceiver);
			m_bankedLabelVisible = false;
		}
		m_bankedRingCount.text = "0";
		m_currentRingCount.text = "0";
	}

	public void OnPauseStateChanged(bool paused)
	{
		m_paused = paused;
	}

	public void OnPlayerDeath()
	{
		m_paused = true;
	}

	public void OnSonicResurrection()
	{
		m_paused = false;
	}

	private void InitialiseRingBanking()
	{
		float num = RingStorage.RunBankedRings;
		float num2 = num - m_currentBankingCount;
		if (num2 != 0f)
		{
			m_bankingSpeed = num2 / m_bankingRate;
			Audio.PlayClip(m_startBankingAudio, loop: false);
			m_currentBankingCount = (float)RingStorage.RunBankedRings - num2;
			m_currentHeldCount = num2;
			m_lastCueBankingCount = m_currentBankingCount;
			m_targetBankingCount = num;
			m_bankingTransitionDone = false;
			m_bankingCountTrigger.SendMessage("OnClick", SendMessageOptions.RequireReceiver);
			m_bankedLabelVisible = true;
			m_updateBanking = UpdateBankingLabelDisplayed;
		}
	}

	private void UpdateRingPickUps()
	{
		int heldRings = RingStorage.HeldRings;
		m_currentRingCount.text = heldRings.ToString();
	}

	private bool UpdateIndividualRingCount(ref float current, ref float target)
	{
		bool result = false;
		if (target > current)
		{
			current += m_bankingSpeed * Time.deltaTime;
			if (current > target)
			{
				current = target;
			}
			result = true;
		}
		else if (target < current)
		{
			current -= m_bankingSpeed * Time.deltaTime;
			if (current < target)
			{
				current = target;
			}
			result = true;
		}
		return result;
	}

	private void UpdateBankingLabelDisplayed()
	{
		if (m_bankingTransitionDone)
		{
			m_updateBanking = UpdateRingBanking;
		}
	}

	private void UpdateRingBanking()
	{
		if (!m_paused)
		{
			float target = m_targetBankingCount;
			float target2 = RingStorage.HeldRings;
			bool flag = UpdateIndividualRingCount(ref m_currentBankingCount, ref target);
			bool flag2 = UpdateIndividualRingCount(ref m_currentHeldCount, ref target2);
			int num = (int)Math.Ceiling(m_currentBankingCount);
			int num2 = (int)Math.Floor(m_currentHeldCount);
			m_bankedRingCount.text = num.ToString();
			m_currentRingCount.text = num2.ToString();
			if (BankingCueNeeded())
			{
				TriggerBankingCue();
			}
			if (!flag && !flag2)
			{
				m_updateBanking = UpdateBankingLabelHidden;
				m_bankingTransitionDone = false;
				m_bankingCountTrigger.SendMessage("OnClick", SendMessageOptions.RequireReceiver);
				m_bankedLabelVisible = false;
			}
		}
	}

	private void UpdateBankingLabelHidden()
	{
		if (m_bankingTransitionDone)
		{
			m_updateBanking = null;
		}
	}

	private bool BankingCueNeeded()
	{
		int num = (int)Math.Ceiling(m_currentBankingCount);
		int num2 = (int)Math.Ceiling(m_lastCueBankingCount);
		if (num == num2)
		{
			return false;
		}
		return true;
	}

	private void TriggerBankingCue()
	{
		Audio.PlayClip(m_bankingAudio, loop: false);
		m_lastCueBankingCount = (float)Math.Ceiling(m_currentBankingCount);
	}

	private void ShowNonBankedRingUI()
	{
		if (!m_shown)
		{
			m_shown = true;
			m_normalDisplayTrigger.SendMessage("OnClick", SendMessageOptions.RequireReceiver);
		}
	}

	private void HideNonBankedRingUI()
	{
		if (m_shown)
		{
			m_shown = false;
			m_normalDisplayTrigger.SendMessage("OnClick", SendMessageOptions.RequireReceiver);
		}
	}

	private void Event_OnBossBattlePhaseStart(int currentPhase)
	{
		BossBattleSystem.Phase phase = BossBattleSystem.Instance().GetPhase(currentPhase);
		switch (phase.Type)
		{
		case BossBattleSystem.Phase.Types.Intro:
			m_fightingBoss = true;
			HideNonBankedRingUI();
			break;
		case BossBattleSystem.Phase.Types.Finish:
			m_fightingBoss = false;
			ShowNonBankedRingUI();
			break;
		}
	}

	private void Event_OnAttackTheBossHintPrompt()
	{
		HideNonBankedRingUI();
	}

	private void Event_OnAttackTheBossFailPrompt()
	{
		HideNonBankedRingUI();
	}

	private void Event_OnBossBattleOutroEnd()
	{
		ShowNonBankedRingUI();
	}

	private void Event_OnBossBattleIntroEnd()
	{
		ShowNonBankedRingUI();
	}
}
