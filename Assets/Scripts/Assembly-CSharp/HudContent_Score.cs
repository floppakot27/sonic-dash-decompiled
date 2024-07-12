using System;
using System.Text;
using UnityEngine;

public class HudContent_Score
{
	[Flags]
	private enum State
	{
		None = 0,
		SpecialMultiplier = 1
	}

	private const string EventBonusRoot = "events";

	private const string GrassEventProperty = "grassevent";

	private const string TempleEventProperty = "templeevent";

	private const string BeachEventProperty = "beachevent";

	private UILabel m_currentScore;

	private UILabel m_currentMultiplier;

	private ScoreTracker m_scoreTracker;

	private State m_state;

	private GameObject m_highScoreTrigger;

	private GameObject m_additionalMultiplierTrigger;

	private ScoreTracker.Event m_eventIconDisplayed;

	private ScoreTracker.Event m_eventActive;

	private GameObject m_grassEventIndicator;

	private GameObject m_templeEventIndicator;

	private GameObject m_beachEventIndicator;

	private GameObject m_displayTrigger;

	private bool m_highScoreDisplayed;

	private bool m_fightingBoss;

	private bool m_shown = true;

	private AudioClip m_highScoreAudioClip;

	public HudContent_Score(ScoreTracker scoreTracker, UILabel currentScore, UILabel currentMultiplier, GameObject highScoreTrigger, GameObject additionalMultiplierTrigger, GameObject grassEventIndicator, GameObject templeEventIndicator, GameObject beachEventIndicator, AudioClip highScoreAudioClip, GameObject displayTrigger)
	{
		m_displayTrigger = displayTrigger;
		m_currentScore = currentScore;
		m_currentMultiplier = currentMultiplier;
		m_scoreTracker = scoreTracker;
		m_highScoreAudioClip = highScoreAudioClip;
		m_highScoreTrigger = highScoreTrigger;
		m_additionalMultiplierTrigger = additionalMultiplierTrigger;
		m_eventIconDisplayed = ScoreTracker.Event.None;
		m_grassEventIndicator = grassEventIndicator;
		m_templeEventIndicator = templeEventIndicator;
		m_beachEventIndicator = beachEventIndicator;
		EventDispatch.RegisterInterest("OnDashMeterFilled", this);
		EventDispatch.RegisterInterest("OnDashMeterTriggered", this);
		EventDispatch.RegisterInterest("MultiplierUpdate", this);
		EventDispatch.RegisterInterest("RequestNewTrackMidGame", this);
		EventDispatch.RegisterInterest("OnNewGameStarted", this);
		EventDispatch.RegisterInterest("OnBossBattlePhaseStart", this);
		EventDispatch.RegisterInterest("OnAttackTheBossHintPrompt", this);
		EventDispatch.RegisterInterest("OnAttackTheBossFailPrompt", this);
		EventDispatch.RegisterInterest("OnBossBattleOutroEnd", this);
		EventDispatch.RegisterInterest("OnBossBattleIntroEnd", this);
	}

	public void Update()
	{
		long currentScore = ScoreTracker.CurrentScore;
		m_currentScore.text = LanguageUtils.FormatNumber(currentScore);
		uint currentMultiplier = ScoreTracker.CurrentMultiplier;
		StringBuilder stringBuilder = new StringBuilder(3);
		stringBuilder.Append("x");
		stringBuilder.Append(currentMultiplier);
		m_currentMultiplier.text = stringBuilder.ToString();
		if (!m_shown && !m_fightingBoss)
		{
			ShowScoreUI();
		}
		DisplayHighScoreIcon();
	}

	public void OnResetOnNewGame()
	{
		m_currentScore.text = "0";
		if (m_highScoreDisplayed)
		{
			m_highScoreTrigger.SendMessage("OnClick", SendMessageOptions.RequireReceiver);
			m_highScoreDisplayed = false;
		}
		if ((m_state & State.SpecialMultiplier) == State.SpecialMultiplier)
		{
			m_additionalMultiplierTrigger.SendMessage("OnClick", SendMessageOptions.DontRequireReceiver);
			m_state &= State.SpecialMultiplier;
		}
		CheckEvents();
	}

	public void NewGameActivateEvent()
	{
		ActivateEvent(ScoreTracker.Event.Grass);
	}

	public void OnPauseStateChanged(bool paused)
	{
	}

	public void OnPlayerDeath()
	{
	}

	private void DisplayHighScoreIcon()
	{
		if (!m_highScoreDisplayed && ScoreTracker.HighScoreAchived)
		{
			m_highScoreTrigger.SendMessage("OnClick", SendMessageOptions.RequireReceiver);
			m_highScoreDisplayed = true;
			Audio.PlayClip(m_highScoreAudioClip, loop: false);
		}
	}

	private void CheckEvents()
	{
		m_eventActive = ScoreTracker.Event.None;
		if (Internet.ConnectionAvailable() && FeatureState.Valid)
		{
			CheckEventState(ScoreTracker.Event.Grass, "grassevent");
			CheckEventState(ScoreTracker.Event.Temple, "templeevent");
			CheckEventState(ScoreTracker.Event.Beach, "beachevent");
		}
	}

	private void CheckEventState(ScoreTracker.Event eventType, string eventProperty)
	{
		LSON.Property stateProperty = FeatureState.GetStateProperty("events", eventProperty);
		if (stateProperty != null)
		{
			bool boolValue = false;
			if (LSONProperties.AsBool(stateProperty, out boolValue) && boolValue)
			{
				m_eventActive |= eventType;
			}
		}
	}

	private void Event_OnNewGameStarted()
	{
	}

	private void ActivateEvent(ScoreTracker.Event eventFlag)
	{
		if (ActivateEventIcon(ScoreTracker.Event.Grass, eventFlag == ScoreTracker.Event.Grass))
		{
			m_grassEventIndicator.SendMessage("OnClick", SendMessageOptions.RequireReceiver);
		}
		if (ActivateEventIcon(ScoreTracker.Event.Temple, eventFlag == ScoreTracker.Event.Temple))
		{
			m_templeEventIndicator.SendMessage("OnClick", SendMessageOptions.RequireReceiver);
		}
		if (ActivateEventIcon(ScoreTracker.Event.Beach, eventFlag == ScoreTracker.Event.Beach))
		{
			m_beachEventIndicator.SendMessage("OnClick", SendMessageOptions.RequireReceiver);
		}
	}

	private bool ActivateEventIcon(ScoreTracker.Event eventFlag, bool activate)
	{
		if (activate)
		{
			if ((m_eventIconDisplayed & eventFlag) != eventFlag && (m_eventActive & eventFlag) == eventFlag)
			{
				m_eventIconDisplayed |= eventFlag;
				return true;
			}
		}
		else if ((m_eventIconDisplayed & eventFlag) == eventFlag)
		{
			m_eventIconDisplayed &= ~eventFlag;
			return true;
		}
		return false;
	}

	private void ShowScoreUI()
	{
		if (!m_shown)
		{
			m_shown = true;
			m_displayTrigger.SendMessage("OnClick", SendMessageOptions.RequireReceiver);
		}
	}

	private void HideScoreUI()
	{
		if (m_shown)
		{
			m_shown = false;
			m_displayTrigger.SendMessage("OnClick", SendMessageOptions.RequireReceiver);
		}
	}

	private void Event_RequestNewTrackMidGame(TrackGenerator.MidGameNewTrackRequest request)
	{
		switch (request.Destination)
		{
		case SpringTV.Destination.Grass:
			ActivateEvent(ScoreTracker.Event.Grass);
			break;
		case SpringTV.Destination.Temple:
			ActivateEvent(ScoreTracker.Event.Temple);
			break;
		case SpringTV.Destination.Beach:
			ActivateEvent(ScoreTracker.Event.Beach);
			break;
		}
	}

	private void Event_OnDashMeterFilled()
	{
		m_state |= State.SpecialMultiplier;
		Event_MultiplierUpdate();
	}

	private void Event_MultiplierUpdate()
	{
		m_additionalMultiplierTrigger.SendMessage("OnClick", SendMessageOptions.DontRequireReceiver);
	}

	private void Event_OnDashMeterTriggered(bool enabled)
	{
		if ((m_state & State.SpecialMultiplier) == State.SpecialMultiplier)
		{
			Event_MultiplierUpdate();
			m_state &= ~State.SpecialMultiplier;
		}
	}

	private void Event_OnBossBattlePhaseStart(int currentPhase)
	{
		BossBattleSystem.Phase phase = BossBattleSystem.Instance().GetPhase(currentPhase);
		switch (phase.Type)
		{
		case BossBattleSystem.Phase.Types.Intro:
			m_fightingBoss = true;
			HideScoreUI();
			break;
		case BossBattleSystem.Phase.Types.Finish:
			m_fightingBoss = false;
			break;
		}
	}

	private void Event_OnAttackTheBossHintPrompt()
	{
		HideScoreUI();
	}

	private void Event_OnAttackTheBossFailPrompt()
	{
		HideScoreUI();
	}

	private void Event_OnBossBattleOutroEnd()
	{
		ShowScoreUI();
	}

	private void Event_OnBossBattleIntroEnd()
	{
		ShowScoreUI();
	}
}
