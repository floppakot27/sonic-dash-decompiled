using System;
using UnityEngine;

public class HudContent_BossAlerts
{
	[Flags]
	private enum State
	{
		None = 1,
		IntroVisible = 2,
		OutroVisible = 4,
		Visible = 8
	}

	private State m_state = State.None;

	private State m_prePauseState = State.None;

	private GameObject m_battleBeginTrigger;

	private GameObject m_battleEndTrigger;

	private GameObject m_bossDodgeHint;

	private GameObject m_bossAttackHint;

	private GameObject m_bossFailPrompt;

	private GameObject m_bossTargetReticuleTrigger;

	private BossTargetReticule m_bossTargetReticule;

	private UILabel m_attackOutcomeLabel;

	private UILabel m_scoreBonusLabel;

	private AudioClip m_alertEnterSound;

	private AudioClip m_alertExitSound;

	private int m_bossBonusScore;

	public HudContent_BossAlerts(GameObject battleBeginTrigger, GameObject battleEndTrigger, GameObject bossDodgeHint, GameObject bossAttackHint, GameObject bossFailPrompt, GameObject bossTargetTrigger, BossTargetReticule bossTarget, AudioClip alertInSound, AudioClip alertOutSound, UILabel attackOutcomeLabel, UILabel scoreBonusLabel)
	{
		m_battleBeginTrigger = battleBeginTrigger;
		m_battleEndTrigger = battleEndTrigger;
		m_bossDodgeHint = bossDodgeHint;
		m_bossAttackHint = bossAttackHint;
		m_bossFailPrompt = bossFailPrompt;
		m_alertEnterSound = alertInSound;
		m_alertExitSound = alertOutSound;
		m_attackOutcomeLabel = attackOutcomeLabel;
		m_scoreBonusLabel = scoreBonusLabel;
		m_bossTargetReticuleTrigger = bossTargetTrigger;
		m_bossTargetReticule = bossTarget;
		EventDispatch.RegisterInterest("OnBossBattlePhaseStart", this);
		EventDispatch.RegisterInterest("OnBossBattleIntroStart", this);
		EventDispatch.RegisterInterest("OnBossBattleIntroEnd", this);
		EventDispatch.RegisterInterest("OnBossBattleOutroStart", this);
		EventDispatch.RegisterInterest("OnBossBattleOutroEnd", this);
		EventDispatch.RegisterInterest("OnAttackTheBossHintPrompt", this);
		EventDispatch.RegisterInterest("OnAttackTheBossFailPrompt", this);
		EventDispatch.RegisterInterest("OnBossReticuleShow", this);
		EventDispatch.RegisterInterest("OnBossHitScoreAwarded", this);
	}

	public void Update()
	{
	}

	public void OnResetOnNewGame()
	{
		HideBattleBeginElement();
		HideBattleEndElement();
	}

	public void OnPauseStateChanged(bool paused)
	{
		if (paused)
		{
			m_prePauseState = m_state;
			HideBattleBeginElement();
			HideBattleEndElement();
			return;
		}
		if ((m_prePauseState & (State.IntroVisible | State.Visible)) == (State.IntroVisible | State.Visible))
		{
			ShowBattleBeginElement();
		}
		if ((m_prePauseState & (State.OutroVisible | State.Visible)) == (State.OutroVisible | State.Visible))
		{
			ShowBattleEndElement();
		}
	}

	public void OnPlayerDeath()
	{
	}

	public void HudVisible(bool visible)
	{
		if (visible)
		{
			m_state &= ~State.None;
			m_state |= State.Visible;
		}
	}

	private void ShowBattleBeginElement()
	{
		m_battleBeginTrigger.SendMessage("OnClick", SendMessageOptions.RequireReceiver);
		Audio.PlayClip(m_alertEnterSound, loop: false);
		m_bossBonusScore = BossBattleSystem.Instance().DefaultBossScore * (int)ScoreTracker.CurrentMultiplier;
		m_state |= State.IntroVisible;
	}

	private void HideBattleBeginElement()
	{
		if ((m_state & (State.IntroVisible | State.Visible)) == (State.IntroVisible | State.Visible))
		{
			m_battleBeginTrigger.SendMessage("OnClick", SendMessageOptions.RequireReceiver);
			Audio.PlayClip(m_alertExitSound, loop: false);
			m_state &= ~State.IntroVisible;
		}
	}

	private void ShowBattleEndElement()
	{
		m_battleEndTrigger.SendMessage("OnClick", SendMessageOptions.RequireReceiver);
		Audio.PlayClip(m_alertEnterSound, loop: false);
		m_state |= State.OutroVisible;
	}

	private void HideBattleEndElement()
	{
		if ((m_state & (State.OutroVisible | State.Visible)) == (State.OutroVisible | State.Visible))
		{
			m_battleEndTrigger.SendMessage("OnClick", SendMessageOptions.RequireReceiver);
			Audio.PlayClip(m_alertExitSound, loop: false);
			m_state &= ~State.OutroVisible;
		}
	}

	private void ShowDodgeHint()
	{
		m_bossDodgeHint.SendMessage("OnClick", SendMessageOptions.RequireReceiver);
	}

	private void ShowAttackHint()
	{
		m_bossAttackHint.SendMessage("OnClick", SendMessageOptions.RequireReceiver);
	}

	private void ShowFailPrompt()
	{
		m_bossFailPrompt.SendMessage("OnClick", SendMessageOptions.RequireReceiver);
	}

	private void ShowReticule(BossAttack.HitpointSetting hitPoint)
	{
		m_bossTargetReticuleTrigger.SendMessage("OnClick", SendMessageOptions.RequireReceiver);
		m_bossTargetReticule.HitBox = hitPoint;
	}

	private void Event_OnBossBattleIntroStart()
	{
		ShowBattleBeginElement();
	}

	private void Event_OnBossBattleIntroEnd()
	{
		HideBattleBeginElement();
	}

	private void Event_OnBossBattleOutroStart()
	{
		if (Boss.GetInstance().AttackPhase().AttackSuccess)
		{
			m_attackOutcomeLabel.enabled = true;
			m_attackOutcomeLabel.text = LanguageStrings.First.GetString("BOSS_WIN_TEXT");
		}
		else
		{
			m_attackOutcomeLabel.enabled = true;
			m_attackOutcomeLabel.text = LanguageStrings.First.GetString("BOSS_FAIL_TEXT");
		}
		string @string = LanguageStrings.First.GetString("BOSS_AWARD_PROMPT");
		m_scoreBonusLabel.text = string.Format(@string, LanguageUtils.FormatNumber(m_bossBonusScore));
		EventDispatch.GenerateEvent("OnBossBeat", BossBattleSystem.Instance().DefaultBossScore * (int)ScoreTracker.CurrentMultiplier);
		ShowBattleEndElement();
	}

	private void Event_OnBossBattleOutroEnd()
	{
		HideBattleEndElement();
	}

	private void Event_OnAttackTheBossHintPrompt()
	{
		ShowAttackHint();
	}

	private void Event_OnAttackTheBossFailPrompt()
	{
		ShowFailPrompt();
	}

	private void Event_OnBossReticuleShow(BossAttack.HitpointSetting hitPoint)
	{
		ShowReticule(hitPoint);
	}

	private void Event_OnBossHitScoreAwarded(int score)
	{
		m_bossBonusScore += score;
	}

	private void Event_OnBossBattlePhaseStart(int currentPhase)
	{
		BossBattleSystem.Phase phase = BossBattleSystem.Instance().GetPhase(currentPhase);
		if (phase.Type == BossBattleSystem.Phase.Types.Attack1)
		{
			ShowDodgeHint();
		}
	}
}
