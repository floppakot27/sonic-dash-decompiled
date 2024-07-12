using System;
using System.Linq;
using UnityEngine;

public class LeaderboardModifier
{
	private const string PlayerName = "Mr. Test";

	private const int FirstScore = 5000;

	private const int ScoreDifference = 300;

	public static void UpdateLeaderboardData(Leaderboards.Types leaderboard, Leaderboards.Entry[] entries)
	{
		if (!Social.localUser.authenticated)
		{
			return;
		}
		LeaderboardContent.LastPostedData lastPostedData = LeaderboardContent.GetLastPostedData(leaderboard);
		if (!lastPostedData.m_valid)
		{
			return;
		}
		Leaderboards.Entry entry3 = entries.FirstOrDefault((Leaderboards.Entry thisEntry) => thisEntry != null && thisEntry.m_playersRank);
		if (entry3 == null || entry3.m_user != lastPostedData.m_user || lastPostedData.m_score <= entry3.m_score)
		{
			return;
		}
		entry3.m_score = lastPostedData.m_score;
		Array.Sort(entries, (Leaderboards.Entry entry1, Leaderboards.Entry entry2) => (entry1 == null) ? int.MaxValue : (entry2?.m_score.CompareTo(entry1.m_score) ?? int.MinValue));
		for (int i = 0; i < entries.Length; i++)
		{
			Leaderboards.Entry entry4 = entries[i];
			if (entry4 != null)
			{
				entry4.m_rank = i + 1;
			}
		}
	}
}
