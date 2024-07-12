using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms;

public class LeaderboardPopulator
{
	private class CachedLeaderboard
	{
		public string m_boardID = string.Empty;

		public Leaderboards.Types m_type;

		public bool m_processed;

		public ILeaderboard m_leaderboard;

		public IUserProfile[] m_profiles;

		public Leaderboards.Entry[] m_entries;
	}

	private List<CachedLeaderboard> m_cachedLeaderboards;

	private object[] m_requestCompleteParams = new object[2];

	private object[] m_cacheCompleteParams = new object[2];

	public LeaderboardPopulator()
	{
		m_cachedLeaderboards = new List<CachedLeaderboard>(1);
		EventDispatch.RegisterInterest("CacheLeaderboard", this, EventDispatch.Priority.Highest);
		EventDispatch.RegisterInterest("RequestLeaderboard", this, EventDispatch.Priority.Highest);
	}

	private void CacheLeaderboardRequest(CachedLeaderboard leaderboardToCache, Leaderboards.Request request)
	{
		if (!Internet.ConnectionAvailable())
		{
			OnLeaderboardFailed(leaderboardToCache, request);
			return;
		}
		ILeaderboard leaderboard = leaderboardToCache.m_leaderboard;
		leaderboard.userScope = UserScope.Global;
		if (request.m_filter == Leaderboards.Request.Filter.Friends)
		{
			leaderboard.userScope = UserScope.FriendsOnly;
		}
		Range range = leaderboard.range;
		range.from = 1;
		range.count = request.m_entries;
		leaderboard.range = range;
		leaderboard.LoadScores(delegate(bool result)
		{
			if (!leaderboardToCache.m_processed)
			{
				if (result)
				{
					OnLeaderboardLoaded(leaderboardToCache, request);
				}
				else
				{
					OnLeaderboardFailed(leaderboardToCache, request);
				}
			}
		});
	}

	private void OnLeaderboardLoaded(CachedLeaderboard leaderboardToCache, Leaderboards.Request request)
	{
		ILeaderboard leaderboard = leaderboardToCache.m_leaderboard;
		if (leaderboard.scores == null || leaderboard.scores.Length == 0)
		{
			OnLeaderboardFailed(leaderboardToCache, request);
			GameAnalytics.LeaderboardFriends(0);
			return;
		}
		GameAnalytics.LeaderboardFriends(leaderboard.scores.Length);
		string[] array = new string[leaderboard.scores.Length];
		for (int i = 0; i < leaderboard.scores.Length; i++)
		{
			array[i] = leaderboard.scores[i].userID;
		}
		Social.LoadUsers(array, delegate(IUserProfile[] result)
		{
			OnUsersLoaded(result, leaderboardToCache, request);
		});
	}

	private void OnLeaderboardFailed(CachedLeaderboard leaderboardToCache, Leaderboards.Request request)
	{
		StoreLeaderboardEntries(leaderboardToCache, request);
	}

	private void OnUsersLoaded(IUserProfile[] userProfiles, CachedLeaderboard leaderboardToCache, Leaderboards.Request request)
	{
		if (userProfiles == null || userProfiles.Length == 0)
		{
			OnLeaderboardFailed(leaderboardToCache, request);
			return;
		}
		leaderboardToCache.m_profiles = userProfiles;
		StoreLeaderboardEntries(leaderboardToCache, request);
	}

	private void OnEqualiseScore(bool success)
	{
		if (success)
		{
			GameState.g_gameState.CacheLeaderboards();
		}
	}

	private void StoreLeaderboardEntries(CachedLeaderboard leaderboardToCache, Leaderboards.Request request)
	{
		int num = ((leaderboardToCache.m_leaderboard.scores != null && leaderboardToCache.m_leaderboard.scores.Length != 0) ? (leaderboardToCache.m_leaderboard.scores.Length + 1) : (request.m_entries + 1));
		leaderboardToCache.m_entries = new Leaderboards.Entry[num];
		for (int i = 0; i < num; i++)
		{
			leaderboardToCache.m_entries[i] = new Leaderboards.Entry();
		}
		Leaderboards.Entry[] entries = leaderboardToCache.m_entries;
		long num2 = 0L;
		bool flag = false;
		for (int j = 0; j < num - 1; j++)
		{
			Leaderboards.Entry entry = entries[j];
			entry.m_valid = true;
			if (leaderboardToCache.m_profiles == null || leaderboardToCache.m_profiles.Length <= j)
			{
				entry.m_user = string.Empty;
				entry.m_avatar = null;
				entry.m_valid = false;
			}
			else
			{
				HLUserProfile hLUserProfile = (HLUserProfile)leaderboardToCache.m_profiles[j];
				entry.m_user = hLUserProfile.userName;
				entry.m_avatar = hLUserProfile.image;
				entry.m_source = hLUserProfile.Source;
			}
			if (leaderboardToCache.m_leaderboard.scores == null || leaderboardToCache.m_leaderboard.scores.Length <= j || leaderboardToCache.m_leaderboard.scores[j].value == 0L)
			{
				entry.m_rank = 0;
				entry.m_score = 0L;
				entry.m_valid = false;
			}
			else
			{
				entry.m_rank = leaderboardToCache.m_leaderboard.scores[j].rank;
				entry.m_score = leaderboardToCache.m_leaderboard.scores[j].value;
			}
			entry.m_playersRank = false;
			if (entry.m_valid && Social.localUser != null && leaderboardToCache.m_leaderboard.scores.Length > j && ((HLLocalUser)Social.localUser).matchesUserID(leaderboardToCache.m_leaderboard.scores[j].userID))
			{
				flag = true;
				entry.m_playersRank = true;
				num2 = Math.Max(num2, entry.m_score);
			}
		}
		if (!flag)
		{
			Leaderboards.Entry entry2 = entries[num - 1];
			entry2.m_rank = leaderboardToCache.m_leaderboard.localUserScore.rank;
			entry2.m_score = leaderboardToCache.m_leaderboard.localUserScore.value;
			entry2.m_user = Social.localUser.userName;
			entry2.m_avatar = Social.localUser.image;
			if (leaderboardToCache.m_leaderboard.localUserScore.rank > 0)
			{
				entry2.m_playersRank = true;
				entry2.m_valid = true;
				num2 = Math.Max(num2, entry2.m_score);
				flag = true;
			}
		}
		PlayerStats.Stats currentStats = PlayerStats.GetCurrentStats();
		long num3 = currentStats.m_trackedLongStats[2];
		if (flag && num3 > num2)
		{
			Social.ReportScore(num3, leaderboardToCache.m_boardID, OnEqualiseScore);
		}
		leaderboardToCache.m_processed = true;
		NotifyBoardCacheComplete(leaderboardToCache);
	}

	private void NotifyBoardCacheComplete(CachedLeaderboard leaderboardToCache)
	{
		bool flag = leaderboardToCache != null && leaderboardToCache.m_leaderboard != null && leaderboardToCache.m_leaderboard.scores != null && leaderboardToCache.m_leaderboard.scores.Length > 0;
		m_cacheCompleteParams[0] = leaderboardToCache.m_boardID;
		m_cacheCompleteParams[1] = flag;
		EventDispatch.GenerateEvent("LeaderboardCacheComplete", m_cacheCompleteParams);
	}

	private void Event_CacheLeaderboard(Leaderboards.Request request)
	{
		for (int i = 0; i < m_cachedLeaderboards.Count; i++)
		{
			CachedLeaderboard cachedLeaderboard = m_cachedLeaderboards[i];
			if (cachedLeaderboard.m_boardID == request.m_leaderboardID)
			{
				cachedLeaderboard.m_processed = false;
				CacheLeaderboardRequest(cachedLeaderboard, request);
				return;
			}
		}
		CachedLeaderboard cachedLeaderboard2 = new CachedLeaderboard();
		cachedLeaderboard2.m_boardID = request.m_leaderboardID;
		cachedLeaderboard2.m_type = request.m_leaderboard;
		cachedLeaderboard2.m_leaderboard = Social.CreateLeaderboard();
		cachedLeaderboard2.m_leaderboard.id = request.m_leaderboard.ToString();
		cachedLeaderboard2.m_leaderboard.range = new Range(1, request.m_entries);
		m_cachedLeaderboards.Add(cachedLeaderboard2);
		CacheLeaderboardRequest(cachedLeaderboard2, request);
	}

	private void Event_RequestLeaderboard(string leaderboardID)
	{
		CachedLeaderboard cachedLeaderboard = null;
		for (int i = 0; i < m_cachedLeaderboards.Count; i++)
		{
			CachedLeaderboard cachedLeaderboard2 = m_cachedLeaderboards[i];
			if (cachedLeaderboard2.m_boardID == leaderboardID)
			{
				cachedLeaderboard = cachedLeaderboard2;
			}
		}
		bool authenticated = Social.localUser.authenticated;
		Leaderboards.Entry[] array = ((cachedLeaderboard != null && cachedLeaderboard.m_processed && authenticated) ? cachedLeaderboard.m_entries : null);
		if (array != null)
		{
			LeaderboardModifier.UpdateLeaderboardData(cachedLeaderboard.m_type, array);
		}
		m_requestCompleteParams[0] = leaderboardID;
		m_requestCompleteParams[1] = array;
		EventDispatch.GenerateEvent("LeaderboardRequestComplete", m_requestCompleteParams);
	}
}
