using System;
using UnityEngine;

public class HudContent_Boosters
{
	[Flags]
	private enum State
	{
		None = 1,
		Visible = 2,
		HUD = 4
	}

	private static int[] s_selectedBoosters = new int[3] { -1, -1, -1 };

	private UISprite m_boosterTop;

	private UISprite m_boosterMid;

	private UISprite m_boosterBot;

	private Animation m_animationTop;

	private Animation m_animationMid;

	private Animation m_animationBot;

	private AudioClip m_enemyComboAudio;

	private AudioClip m_ringStreakAudio;

	private ParticleSystem m_boosterParticles;

	private GameObject m_boosterDisplayTrigger;

	private bool m_fightingBoss;

	private static string[] s_boosterSpriteNames = new string[5] { "icon-boostershud-enemycombo", "icon-boostershud-ringstreak", "icon-boostershud-springbonus", "icon-boostershud-goldnik", "icon-boostershud-scorebonus" };

	private State m_state = State.None;

	public HudContent_Boosters(UISprite boosterTop, UISprite boosterMid, UISprite boosterBot, Animation animationHUDTop, Animation animationHUDMid, Animation animationHUDBot, AudioClip enemyComboAudio, AudioClip ringStreakAudio, ParticleSystem boosterParticles, GameObject boosterTrigger)
	{
		m_boosterTop = boosterTop;
		m_boosterMid = boosterMid;
		m_boosterBot = boosterBot;
		m_animationTop = animationHUDTop;
		m_animationMid = animationHUDMid;
		m_animationBot = animationHUDBot;
		m_enemyComboAudio = enemyComboAudio;
		m_ringStreakAudio = ringStreakAudio;
		m_boosterParticles = boosterParticles;
		m_boosterDisplayTrigger = boosterTrigger;
		EventDispatch.RegisterInterest("OnBossBattlePhaseStart", this);
		Start();
	}

	public void HudVisible(bool visible)
	{
		if (visible)
		{
			m_state |= State.HUD;
		}
		if (TutorialSystem.instance().isTrackTutorialEnabled())
		{
			m_state &= ~State.HUD;
		}
	}

	public void Update()
	{
		if (!m_fightingBoss && (m_state & State.HUD) == State.HUD)
		{
			ShowBoosters();
		}
	}

	public void OnResetOnNewGame()
	{
		string empty = string.Empty;
		s_selectedBoosters = Boosters.GetBoostersSelected;
		empty = GetSpriteNameForBooster((PowerUps.Type)s_selectedBoosters[0]);
		if (empty != string.Empty)
		{
			m_boosterTop.enabled = true;
			m_boosterTop.spriteName = empty;
			m_boosterTop.MakePixelPerfect();
		}
		else
		{
			m_boosterTop.enabled = false;
		}
		empty = GetSpriteNameForBooster((PowerUps.Type)s_selectedBoosters[1]);
		if (empty != string.Empty)
		{
			m_boosterMid.enabled = true;
			m_boosterMid.spriteName = empty;
			m_boosterMid.MakePixelPerfect();
		}
		else
		{
			m_boosterMid.enabled = false;
		}
		empty = GetSpriteNameForBooster((PowerUps.Type)s_selectedBoosters[2]);
		if (empty != string.Empty)
		{
			m_boosterBot.enabled = true;
			m_boosterBot.spriteName = empty;
			m_boosterBot.MakePixelPerfect();
		}
		else
		{
			m_boosterBot.enabled = false;
		}
		HideBoosters();
		m_fightingBoss = false;
		m_state = State.None;
	}

	public void OnPauseStateChanged(bool paused)
	{
	}

	public void OnPlayerDeath()
	{
	}

	private void Start()
	{
		EventDispatch.RegisterInterest("OnBoosterActivated", this);
	}

	private void Event_OnBoosterActivated(PowerUps.Type boosterActivated)
	{
		switch (BoosterSlot(boosterActivated))
		{
		case -1:
			return;
		case 0:
			m_boosterParticles.transform.position = m_boosterTop.transform.position;
			m_animationTop.Play();
			break;
		case 1:
			m_boosterParticles.transform.position = m_boosterMid.transform.position;
			m_animationMid.Play();
			break;
		case 2:
			m_boosterParticles.transform.position = m_boosterBot.transform.position;
			m_animationBot.Play();
			break;
		}
		PlayAudio(boosterActivated);
	}

	private int BoosterSlot(PowerUps.Type booster)
	{
		for (int i = 0; i < s_selectedBoosters.Length; i++)
		{
			if (s_selectedBoosters[i] == (int)booster)
			{
				return i;
			}
		}
		return -1;
	}

	private string GetSpriteNameForBooster(PowerUps.Type booster)
	{
		return booster switch
		{
			PowerUps.Type.Booster_EnemyComboBonus => s_boosterSpriteNames[0], 
			PowerUps.Type.Booster_RingStreakBonus => s_boosterSpriteNames[1], 
			PowerUps.Type.Booster_SpringBonus => s_boosterSpriteNames[2], 
			PowerUps.Type.Booster_GoldenEnemy => s_boosterSpriteNames[3], 
			PowerUps.Type.Booster_ScoreMultiplier => s_boosterSpriteNames[4], 
			_ => string.Empty, 
		};
	}

	private void PlayAudio(PowerUps.Type booster)
	{
		switch (booster)
		{
		case PowerUps.Type.Booster_EnemyComboBonus:
			Audio.PlayClip(m_enemyComboAudio, loop: false);
			break;
		case PowerUps.Type.Booster_RingStreakBonus:
			Audio.PlayClip(m_ringStreakAudio, loop: false);
			break;
		}
	}

	private void HideBoosters()
	{
		if ((m_state & State.Visible) == State.Visible)
		{
			m_boosterDisplayTrigger.SendMessage("OnClick", SendMessageOptions.RequireReceiver);
			m_state &= ~State.Visible;
		}
	}

	private void ShowBoosters()
	{
		if (!m_fightingBoss && (m_state & State.Visible) != State.Visible)
		{
			m_boosterDisplayTrigger.SendMessage("OnClick", SendMessageOptions.RequireReceiver);
			m_state |= State.Visible;
		}
	}

	private void Event_OnBossBattlePhaseStart(int currentPhase)
	{
		BossBattleSystem.Phase phase = BossBattleSystem.Instance().GetPhase(currentPhase);
		switch (phase.Type)
		{
		case BossBattleSystem.Phase.Types.Arrive:
			m_fightingBoss = true;
			HideBoosters();
			break;
		case BossBattleSystem.Phase.Types.Finish:
			m_fightingBoss = false;
			ShowBoosters();
			break;
		}
	}
}
