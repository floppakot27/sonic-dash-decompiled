using System;
using UnityEngine;

public class Community : MonoBehaviour
{
	[Flags]
	public enum Components
	{
		Achievements = 1,
		Leaderboards = 2,
		Twitter = 4,
		Facebook = 8
	}

	[Flags]
	private enum State
	{
		None = 0,
		Active = 1
	}

	private const string m_PropertyfbPopupShowedTimes = "FBPopupShowedTimes";

	private const string m_PropertyfbPopupLasSessionShown = "FBPopupLastSessionShown";

	private const int m_timesChangeFrequencyPopup = 5;

	private const int m_popupFrequency1 = 3;

	private const int m_popupFrequency2 = 5;

	private State m_state = State.Active;

	private LeaderboardPopulator m_leaderboardPopulator;

	private LeaderboardContent m_leaderboardContent;

	private static HLSocialPlatform m_socialPlatform;

	private bool m_waitingForToInvite;

	private Achievements m_achievements;

	private static Components s_enabledComponents;

	private int m_fbPopupShowedTimes;

	private int m_fbPopupLasSessionShown;

	public static Community g_instance;

	public LeaderboardPopulator LeaderboardPopulator => m_leaderboardPopulator;

	public static bool Enabled(Components component)
	{
		return (s_enabledComponents & component) == component;
	}

	private void Awake()
	{
		g_instance = this;
		if (m_socialPlatform == null)
		{
			m_socialPlatform = new HLSocialPlatform();
		}
	}

	private void Start()
	{
		EventDispatch.RegisterInterest("OnGameDataLoaded", this);
		EventDispatch.RegisterInterest("OnGameDataSaveRequest", this);
		EventDispatch.RegisterInterest("MainMenuActive", this, EventDispatch.Priority.Lowest);
		m_leaderboardPopulator = new LeaderboardPopulator();
		m_leaderboardContent = new LeaderboardContent();
		m_achievements = new Achievements();
		Social.localUser.Authenticate(OnAuthenticationResult);
	}

	private void OnAuthenticationResult(bool success)
	{
		if (success)
		{
			m_achievements.LoadAchievements();
			s_enabledComponents |= Components.Leaderboards;
			s_enabledComponents |= Components.Achievements;
			GameState.g_gameState.CacheLeaderboards();
			m_state |= State.Active;
		}
		else
		{
			s_enabledComponents &= ~Components.Leaderboards;
			s_enabledComponents &= ~Components.Achievements;
			m_state &= ~State.Active;
		}
	}

	public void OnAuthenticated(string Param)
	{
		m_socialPlatform.OnAuthenticated(Param);
	}

	public void OnFacebookAuthenticateComplete(string Param)
	{
		if (m_socialPlatform != null)
		{
			m_socialPlatform.OnFacebookAuthenticateComplete(Param);
		}
	}

	public void OnFacebookLogout(string Param)
	{
		if (m_socialPlatform != null)
		{
			m_socialPlatform.OnFacebookLogout(Param);
		}
	}

	public void OnFacebookPublishAuthenticateComplete(string Param)
	{
		if (m_socialPlatform != null)
		{
			m_socialPlatform.OnFacebookPublishAuthenticateComplete(Param);
		}
		if (m_waitingForToInvite && ((HLLocalUser)Social.localUser).isFacebookAuthenticated)
		{
			((HLSocialPlatform)Social.Active).InviteFriends();
		}
		m_waitingForToInvite = false;
	}

	public void OnGameCenterAuthenticateComplete(string Param)
	{
		if (m_socialPlatform != null)
		{
			m_socialPlatform.OnGameCenterAuthenticateComplete(Param);
		}
	}

	public void OnRefreshSuccess(string param)
	{
		StartCoroutine(m_socialPlatform.OnRefreshSuccess(param));
	}

	public void OnFacebookSendScoreComplete(string param)
	{
		m_socialPlatform.OnFacebookSendScoreComplete(param);
	}

	public void OnGameCenterSendScoreComplete(string param)
	{
		m_socialPlatform.OnGameCenterSendScoreComplete(param);
	}

	public void OnFacebookInviteFriendsComplete(string param)
	{
	}

	private void OnApplicationFocus(bool focusState)
	{
		if (m_socialPlatform != null && focusState)
		{
			m_socialPlatform.OnApplicationGainedFocus();
		}
		SLAnalytics.OnFocus(focusState);
	}

	private void OnApplicationQuit()
	{
		if (m_socialPlatform != null)
		{
			m_socialPlatform.OnApplicationTerminated();
		}
	}

	private void Trigger_FBLogin()
	{
		if (!((HLLocalUser)Social.localUser).isFacebookAuthenticated)
		{
			PropertyStore.Save();
			if (m_socialPlatform != null)
			{
				m_socialPlatform.RequestFacebookAccess();
			}
		}
	}

	private void Trigger_FBLogInOut()
	{
		if (!((HLLocalUser)Social.localUser).isFacebookAuthenticated)
		{
			Trigger_FBLogin();
		}
		else if (m_socialPlatform != null)
		{
			m_socialPlatform.FacebookLogout();
		}
	}

	private void Trigger_FBInviteFriends()
	{
		if (!((HLLocalUser)Social.localUser).isFacebookAuthenticated)
		{
			m_waitingForToInvite = true;
			Trigger_FBLogin();
		}
		else
		{
			((HLSocialPlatform)Social.Active).InviteFriends();
		}
	}

	private void Event_OnGameDataSaveRequest()
	{
		PropertyStore.Store("FBPopupShowedTimes", m_fbPopupShowedTimes);
		PropertyStore.Store("FBPopupLastSessionShown", m_fbPopupLasSessionShown);
	}

	private void Event_OnGameDataLoaded(ActiveProperties activeProperties)
	{
		m_fbPopupShowedTimes = activeProperties.GetInt("FBPopupShowedTimes");
		m_fbPopupLasSessionShown = activeProperties.GetInt("FBPopupLastSessionShown");
	}

	private void Event_MainMenuActive()
	{
		bool isFacebookAuthenticated = ((HLLocalUser)Social.localUser).isFacebookAuthenticated;
		PlayerStats.Stats currentStats = PlayerStats.GetCurrentStats();
		int num = currentStats.m_trackedStats[79];
		if (currentStats.m_trackedStats[60] == 0 && !isFacebookAuthenticated && (m_fbPopupShowedTimes == 0 || (m_fbPopupShowedTimes <= 5 && num >= m_fbPopupLasSessionShown + 3) || (m_fbPopupShowedTimes <= 10 && num >= m_fbPopupLasSessionShown + 5)))
		{
			DialogStack.ShowDialog("Log Into Facebook Dialog");
			m_fbPopupShowedTimes++;
			m_fbPopupLasSessionShown = num;
		}
	}
}
