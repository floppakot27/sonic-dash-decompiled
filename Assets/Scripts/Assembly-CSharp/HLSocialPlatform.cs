using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using UnityEngine.SocialPlatforms.Impl;

public class HLSocialPlatform : ISocialPlatform
{
	private static long m_facebookAppID = 301289123310261L;

	private ISocialPlatform m_gameCenter;

	private Dictionary<string, HLUserProfile> m_userProfiles = new Dictionary<string, HLUserProfile>();

	private HLLocalUser m_user = new HLLocalUser();

	private bool m_facebookScoreSentFinished;

	private bool m_gameCenterScoreSentFinished;

	private bool m_scoreSentResult;

	private Action<bool> m_sendScoreCallback;

	private Action<bool> m_loadScoresCallback;

	private Leaderboard m_loadScoresBoard;

	private bool m_facebookAuthenticationFinished;

	private bool m_gameCenterAuthenticationFinished;

	private Action<bool> m_authenticateCallback;

	public ILocalUser localUser => m_user;

	public HLSocialPlatform()
	{
		m_gameCenter = Social.Active;
		Social.Active = this;
		socialInitialise(m_facebookAppID);
	}

	private void socialInitialise(long appID)
	{
		HLSocialPluginAndroid.Initialise(appID);
	}

	public IAchievement CreateAchievement()
	{
		return m_gameCenter.CreateAchievement();
	}

	public ILeaderboard CreateLeaderboard()
	{
		return m_gameCenter.CreateLeaderboard();
	}

	public void LoadAchievementDescriptions(Action<IAchievementDescription[]> callback)
	{
		m_gameCenter.LoadAchievementDescriptions(callback);
	}

	public void ReportProgress(string id, double progress, Action<bool> callback)
	{
		HLSocialPluginAndroid.UpdateAchievement(id, progress);
		m_gameCenter.ReportProgress(id, progress, callback);
	}

	public void LoadAchievements(Action<IAchievement[]> callback)
	{
		HLSocialPluginAndroid.LoadAchievements();
	}

	private void socialSendScore(string leaderboardID, long score)
	{
		HLSocialPluginAndroid.SendScore(leaderboardID, score);
	}

	public void ReportScore(long score, string board, Action<bool> callback)
	{
		if (m_sendScoreCallback == null)
		{
			m_facebookScoreSentFinished = false;
			m_gameCenterScoreSentFinished = false;
			m_scoreSentResult = false;
			m_sendScoreCallback = callback;
			socialSendScore(board, score);
		}
	}

	private void checkScoreSentCallback()
	{
		if (m_sendScoreCallback != null && m_facebookScoreSentFinished && m_gameCenterScoreSentFinished)
		{
			m_sendScoreCallback(m_scoreSentResult);
			m_sendScoreCallback = null;
		}
	}

	public void OnFacebookSendScoreComplete(string Param)
	{
		bool flag = Param.ToLower().CompareTo("true") == 0;
		if (m_sendScoreCallback != null)
		{
			m_facebookScoreSentFinished = true;
			m_scoreSentResult |= flag;
			checkScoreSentCallback();
		}
	}

	public void OnGameCenterSendScoreComplete(string Param)
	{
		bool flag = Param.ToLower().CompareTo("true") == 0;
		if (m_sendScoreCallback != null)
		{
			m_gameCenterScoreSentFinished = true;
			m_scoreSentResult |= flag;
			checkScoreSentCallback();
		}
	}

	public void LoadScores(string leaderboardID, Action<IScore[]> callback)
	{
		m_gameCenter.LoadScores(leaderboardID, callback);
	}

	private void socialRefreshLeaderboard(string id, int maxEntries)
	{
		HLSocialPluginAndroid.RefreshLeaderboard(id, maxEntries);
	}

	private void socialRefreshLeaderboardFinished(string id)
	{
		HLSocialPluginAndroid.RefreshLeaderboardFinished(id);
	}

	public void LoadScores(ILeaderboard board, Action<bool> callback)
	{
		if (m_loadScoresCallback == null)
		{
			m_loadScoresBoard = (Leaderboard)board;
			m_loadScoresCallback = callback;
			socialRefreshLeaderboard(board.id, board.range.count);
		}
	}

	public IEnumerator OnRefreshSuccess(string param)
	{
		string[] tokens = param.Split('\n');
		int entryCount = Convert.ToInt32(tokens[1]);
		if (entryCount == -1)
		{
			m_loadScoresCallback(obj: false);
			m_loadScoresCallback = null;
			m_loadScoresBoard = null;
			socialRefreshLeaderboardFinished(tokens[0]);
			yield break;
		}
		List<Score> scores = new List<Score>(entryCount);
		DateTime totalStartTime = DateTime.Now;
		HudContent content = UnityEngine.Object.FindObjectOfType(typeof(HudContent)) as HudContent;
		Texture2D defaultTexture = ((!(content != null)) ? null : content.DefaultFriendImage);
		for (uint ii = 0u; ii < entryCount; ii++)
		{
			uint arrayIndexStart = ii * 5 + 2;
			string playerID = tokens[arrayIndexStart++];
			string playerName = tokens[arrayIndexStart++];
			string playerScoreString = tokens[arrayIndexStart++];
			string playerSource = tokens[arrayIndexStart++];
			string playerImageAddress = tokens[arrayIndexStart++];
			long playerScore = Convert.ToInt64(playerScoreString);
			Score score = new Score();
			score.SetUserID(playerID);
			score.value = playerScore;
			score.SetRank((int)(ii + 1));
			score.SetFormattedValue(playerScoreString);
			scores.Add(score);
			if (!m_userProfiles.ContainsKey(playerID))
			{
				HLUserProfile profile = new HLUserProfile();
				profile.SetUserID(playerID);
				profile.SetUserName(playerName);
				profile.SetIsFriend(value: true);
				if (playerSource.ToLower().CompareTo("fb") == 0)
				{
					profile.SetSource(HLUserProfile.ProfileSource.Facebook);
				}
				else if (playerSource.ToLower().CompareTo("gc") == 0)
				{
					profile.SetSource(HLUserProfile.ProfileSource.GameCenter);
				}
				else
				{
					profile.SetSource(HLUserProfile.ProfileSource.Multiple);
				}
				Texture2D imageTexture = null;
				if (playerImageAddress.Length > 5)
				{
					WWW www = new WWW(playerImageAddress);
					yield return www;
					imageTexture = www.texture;
				}
				if (imageTexture == null)
				{
					imageTexture = defaultTexture;
				}
				profile.SetImage(imageTexture);
				m_userProfiles.Add(playerID, profile);
				yield return null;
			}
		}
		DateTime totalStopTime = DateTime.Now;
		TimeSpan totalDuration = totalStopTime - totalStartTime;
		m_loadScoresBoard.SetScores(scores.ToArray());
		m_loadScoresCallback(obj: true);
		m_loadScoresCallback = null;
		m_loadScoresBoard = null;
		socialRefreshLeaderboardFinished(tokens[0]);
	}

	public void LoadUsers(string[] userIds, Action<IUserProfile[]> callback)
	{
		List<HLUserProfile> list = new List<HLUserProfile>();
		for (uint num = 0u; num < userIds.Length; num++)
		{
			if (m_userProfiles.ContainsKey(userIds[num]))
			{
				list.Add(m_userProfiles[userIds[num]]);
			}
		}
		callback(list.ToArray());
	}

	public void ShowAchievementsUI()
	{
		HLSocialPluginAndroid.ViewAchievements();
	}

	public void ShowLeaderboardUI()
	{
		m_gameCenter.ShowLeaderboardUI();
	}

	private void socialAuthenticate()
	{
		HLSocialPluginAndroid.Authenticate();
	}

	public void Authenticate(ILocalUser user, Action<bool> callback)
	{
		if (m_authenticateCallback == null)
		{
			m_facebookAuthenticationFinished = false;
			m_gameCenterAuthenticationFinished = false;
			m_authenticateCallback = callback;
			socialAuthenticate();
		}
	}

	public void OnAuthenticated(string Param)
	{
		string[] array = Param.Split('\n');
		bool flag = false;
		bool flag2 = false;
		string[] array2 = array;
		foreach (string text in array2)
		{
			if (text.ToLower().CompareTo("fbtrue") == 0)
			{
				flag = true;
			}
			else if (text.ToLower().CompareTo("fbfalse") == 0)
			{
				flag = false;
			}
			else if (text.ToLower().CompareTo("gctrue") == 0)
			{
				flag2 = true;
			}
			else if (text.ToLower().CompareTo("gcfalse") == 0)
			{
				flag2 = false;
			}
		}
		m_user.setFacebookAuthenticated(flag);
		m_user.setGameCenterAuthenticated(flag2);
		bool flag3 = flag || flag2;
		((LocalUser)localUser).SetAuthenticated(flag3);
		m_authenticateCallback(flag3);
		m_authenticateCallback = null;
	}

	private void checkAuthenticationCallback()
	{
		if (m_authenticateCallback != null && m_facebookAuthenticationFinished)
		{
			bool isFacebookAuthenticated = m_user.isFacebookAuthenticated;
			m_user.SetAuthenticated(isFacebookAuthenticated);
			m_authenticateCallback(isFacebookAuthenticated);
			m_authenticateCallback = null;
		}
	}

	public void OnFacebookAuthenticateComplete(string Param)
	{
		Debug.Log("OnFacebookAuthenticateComplete " + Param);
		string[] array = Param.Split('\n');
		bool flag = array[0].ToLower().CompareTo("true") == 0;
		m_user.setFacebookAuthenticated(flag);
		bool[] parameter = new bool[2] { flag, false };
		EventDispatch.GenerateEvent("OnFacebookAuthenticateComplete", parameter);
		if (flag)
		{
			m_user.setFacebookUserInfo(array[1], array[2]);
		}
		if (m_authenticateCallback != null)
		{
			m_facebookAuthenticationFinished = true;
			checkAuthenticationCallback();
		}
	}

	public void OnFacebookPublishAuthenticateComplete(string Param)
	{
		bool flag = Param.ToLower().CompareTo("true") == 0;
		bool[] parameter = new bool[2] { flag, true };
		EventDispatch.GenerateEvent("OnFacebookAuthenticateComplete", parameter);
	}

	public void OnGameCenterAuthenticateComplete(string Param)
	{
		string[] array = Param.Split('\n');
		bool flag = array[0].ToLower().CompareTo("true") == 0;
		m_user.setGameCenterAuthenticated(flag);
		if (flag)
		{
			m_user.setGameCenterUserInfo(array[1], array[2]);
		}
		if (m_authenticateCallback != null)
		{
			m_gameCenterAuthenticationFinished = true;
			checkAuthenticationCallback();
		}
	}

	private void socialLogout()
	{
		HLSocialPluginAndroid.Logout();
	}

	public void OnFacebookLogout(string param)
	{
		m_user.setFacebookAuthenticated(authenticated: false);
		SettingsFacebookLogInOutControl.ChangeFacebookConnectionOption(connected: false, showDialog: true);
	}

	public void FacebookLogout()
	{
		socialLogout();
	}

	private void socialRequestFacebookAccess()
	{
		HLSocialPluginAndroid.RequestFacebookAccess();
	}

	private bool socialLoginWillRequireAppSwitch()
	{
		return HLSocialPluginAndroid.LoginWillRequireAppSwitch();
	}

	public void RequestFacebookAccess()
	{
		if (socialLoginWillRequireAppSwitch())
		{
		}
		socialRequestFacebookAccess();
	}

	public void LoadFriends(ILocalUser user, Action<bool> callback)
	{
	}

	public bool GetLoading(ILeaderboard board)
	{
		return false;
	}

	private void socialOnAppGainedFocus()
	{
		HLSocialPluginAndroid.OnAppGainedFocus();
	}

	private void socialOnAppTerminate()
	{
		HLSocialPluginAndroid.OnAppTerminate();
	}

	public void OnApplicationTerminated()
	{
		socialOnAppTerminate();
	}

	public void OnApplicationGainedFocus()
	{
		socialOnAppGainedFocus();
	}

	private void socialInviteFriend(string user, string heading, string message)
	{
		HLSocialPluginAndroid.InviteFriend(user, heading, message);
	}

	public void InviteFriends()
	{
		socialInviteFriend(string.Empty, LanguageStrings.First.GetString("INVITE_HEADING"), LanguageStrings.First.GetString("INVITE_MESSAGE"));
	}
}
