using System;
using System.Linq;
using UnityEngine;

public class HudContent_FriendDisplay
{
	[Flags]
	private enum State
	{
		None = 1,
		ValidScores = 2,
		ValidDisplay = 4,
		Visible = 8,
		RequestedScores = 0x10
	}

	private static Leaderboards.Entry s_currentFriendDisplay;

	private static Leaderboards.Entry s_topFriendDisplay;

	private Leaderboards.Entry[] m_leaderboardEntries;

	private object[] m_populationRequestParams = new object[1];

	private State m_state = State.None;

	private int m_currentEntry;

	private GameObject m_displayTrigger;

	private float m_displayTimer;

	private ScoreTracker m_scoreTracker;

	private Texture2D m_defaultFriendImage;

	private float m_friendChangeDelay;

	private int m_friendScoreBoundary;

	private bool m_forceBossHiding;

	private UILabel m_score;

	private UILabel m_name;

	private UILabel m_rank;

	private UITexture m_avatar;

	public HudContent_FriendDisplay(GameObject displayRoot, GameObject displayTrigger, ScoreTracker scoreTracker, Texture2D defaultFriendImage, float friendChangeDelay, int friendScoreBoundary)
	{
		m_displayTrigger = displayTrigger;
		m_scoreTracker = scoreTracker;
		m_defaultFriendImage = defaultFriendImage;
		m_friendChangeDelay = friendChangeDelay;
		m_friendScoreBoundary = friendScoreBoundary;
		EventDispatch.RegisterInterest("LeaderboardRequestComplete", this);
		EventDispatch.RegisterInterest("OnBossBattlePhaseStart", this);
		EventDispatch.RegisterInterest("OnAttackTheBossHintPrompt", this);
		EventDispatch.RegisterInterest("OnAttackTheBossFailPrompt", this);
		EventDispatch.RegisterInterest("OnBossBattleOutroEnd", this);
		EventDispatch.RegisterInterest("OnBossBattleIntroEnd", this);
		CacheDisplayElements(displayRoot);
	}

	public static Leaderboards.Entry CurrentFriend()
	{
		return s_currentFriendDisplay;
	}

	public static Leaderboards.Entry TopFriend()
	{
		return s_topFriendDisplay;
	}

	public void Update()
	{
		if ((m_state & State.ValidScores) != State.ValidScores)
		{
			return;
		}
		if ((m_state & State.ValidDisplay) != State.ValidDisplay)
		{
			UpdateDisplayTimer();
			return;
		}
		if (ShowScore())
		{
			DisplayFriendScore();
		}
		UpdateCurrentFriend();
	}

	public void OnResetOnNewGame()
	{
		HideFriendScore();
		m_state = State.None;
		s_currentFriendDisplay = null;
		s_topFriendDisplay = null;
		if (Community.Enabled(Community.Components.Leaderboards))
		{
			PopulateLeaderboardEntries();
		}
	}

	public void OnPauseStateChanged(bool paused)
	{
	}

	public void OnPlayerDeath()
	{
	}

	public void HudVisible(bool visible)
	{
	}

	private void PopulateLeaderboardEntries()
	{
		m_state |= State.RequestedScores;
		if (m_populationRequestParams[0] == null)
		{
			m_populationRequestParams[0] = Leaderboards.Types.sdHighestScore.ToString();
		}
		EventDispatch.GenerateEvent("RequestLeaderboard", m_populationRequestParams);
	}

	private void CacheDisplayElements(GameObject displayRoot)
	{
		GameObject gameObject = Utils.FindTagInChildren(displayRoot, "FriendScore_Name");
		GameObject gameObject2 = Utils.FindTagInChildren(displayRoot, "FriendScore_Score");
		GameObject gameObject3 = Utils.FindTagInChildren(displayRoot, "FriendScore_Rank");
		GameObject gameObject4 = Utils.FindTagInChildren(displayRoot, "FriendScore_Avatar");
		if ((bool)gameObject)
		{
			m_name = gameObject.GetComponent<UILabel>();
		}
		if ((bool)gameObject2)
		{
			m_score = gameObject2.GetComponent<UILabel>();
		}
		if ((bool)gameObject3)
		{
			m_rank = gameObject3.GetComponent<UILabel>();
		}
		if ((bool)gameObject4)
		{
			m_avatar = gameObject4.GetComponent<UITexture>();
		}
	}

	private void DisplayFriendScore()
	{
		if (!ShowScore())
		{
			return;
		}
		Leaderboards.Entry entry = m_leaderboardEntries[m_currentEntry];
		s_currentFriendDisplay = m_leaderboardEntries[m_currentEntry];
		if ((bool)m_name)
		{
			m_name.text = entry.m_user;
		}
		if ((bool)m_score)
		{
			m_score.text = LanguageUtils.FormatNumber(entry.m_score);
		}
		if ((bool)m_rank)
		{
			m_rank.text = entry.m_rank.ToString();
		}
		if ((bool)m_avatar)
		{
			if ((bool)entry.m_avatar)
			{
				m_avatar.mainTexture = entry.m_avatar;
			}
			else
			{
				m_avatar.mainTexture = m_defaultFriendImage;
			}
		}
		m_displayTrigger.SendMessage("OnClick", SendMessageOptions.RequireReceiver);
		m_state |= State.Visible;
		m_state |= State.ValidDisplay;
	}

	private void HideFriendScore()
	{
		if ((m_state & State.Visible) == State.Visible)
		{
			m_displayTrigger.SendMessage("OnClick", SendMessageOptions.RequireReceiver);
			m_state &= ~State.Visible;
			m_state &= ~State.ValidDisplay;
			m_displayTimer = 0f;
		}
	}

	private void UpdateDisplayTimer()
	{
		m_displayTimer += Time.deltaTime;
		if (m_displayTimer > m_friendChangeDelay && !m_forceBossHiding)
		{
			DisplayFriendScore();
		}
	}

	private void UpdateCurrentFriend()
	{
		Leaderboards.Entry entry = m_leaderboardEntries[m_currentEntry];
		if (entry.m_score < ScoreTracker.CurrentScore)
		{
			HideFriendScore();
			FindNextFriendToDisplay();
			if (m_currentEntry < 0)
			{
				m_state &= ~State.ValidScores;
				s_currentFriendDisplay = null;
			}
		}
	}

	private void FindNextFriendToDisplay()
	{
		long num = 0L;
		Leaderboards.Entry entry = m_leaderboardEntries[m_currentEntry];
		do
		{
			m_currentEntry--;
			if (m_currentEntry >= 0)
			{
				Leaderboards.Entry entry2 = m_leaderboardEntries[m_currentEntry];
				num += entry2.m_score - entry.m_score;
				entry = entry2;
			}
		}
		while (m_currentEntry >= 0 && num < m_friendScoreBoundary);
		SkipPlayersWithSameScore();
	}

	private void SkipPlayersWithSameScore()
	{
		if (m_currentEntry <= 0)
		{
			return;
		}
		Leaderboards.Entry entry = m_leaderboardEntries[m_currentEntry];
		do
		{
			m_currentEntry--;
			if (m_currentEntry >= 0)
			{
				Leaderboards.Entry entry2 = m_leaderboardEntries[m_currentEntry];
				if (entry2.m_score != entry.m_score)
				{
					m_currentEntry++;
					break;
				}
				entry = entry2;
			}
		}
		while (m_currentEntry > 0);
	}

	private bool ShowScore()
	{
		if (TutorialSystem.instance().isTrackTutorialEnabled())
		{
			return false;
		}
		if (m_forceBossHiding)
		{
			return false;
		}
		if ((m_state & State.Visible) == State.Visible)
		{
			return false;
		}
		return true;
	}

	private void Event_LeaderboardRequestComplete(string LeaderboardID, Leaderboards.Entry[] entries)
	{
		if ((m_state & State.RequestedScores) != State.RequestedScores)
		{
			return;
		}
		m_state &= ~State.RequestedScores;
		if (entries == null)
		{
			m_leaderboardEntries = null;
			m_state &= ~State.ValidScores;
			return;
		}
		int num = entries.Count((Leaderboards.Entry entry) => entry != null && entry.m_valid);
		if (num == 0)
		{
			m_leaderboardEntries = null;
			m_state &= ~State.ValidScores;
			return;
		}
		m_leaderboardEntries = entries;
		m_state |= State.ValidScores;
		s_topFriendDisplay = m_leaderboardEntries[0];
		m_currentEntry = num - 1;
		SkipPlayersWithSameScore();
	}

	private void Event_OnBossBattlePhaseStart(int currentPhase)
	{
		BossBattleSystem.Phase phase = BossBattleSystem.Instance().GetPhase(currentPhase);
		switch (phase.Type)
		{
		case BossBattleSystem.Phase.Types.Intro:
			m_forceBossHiding = true;
			HideFriendScore();
			break;
		case BossBattleSystem.Phase.Types.Finish:
			m_forceBossHiding = false;
			break;
		}
	}

	private void Event_OnAttackTheBossHintPrompt()
	{
		m_forceBossHiding = true;
		HideFriendScore();
	}

	private void Event_OnAttackTheBossFailPrompt()
	{
		m_forceBossHiding = true;
		HideFriendScore();
	}

	private void Event_OnBossBattleOutroEnd()
	{
		m_forceBossHiding = false;
	}

	private void Event_OnBossBattleIntroEnd()
	{
		m_forceBossHiding = false;
	}
}
