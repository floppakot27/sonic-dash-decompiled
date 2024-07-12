using UnityEngine;

public class Leaderboards
{
	public enum Types
	{
		sdHighestScore
	}

	public class Entry
	{
		public bool m_valid;

		public bool m_playersRank;

		public int m_rank;

		public string m_user = string.Empty;

		public long m_score;

		public Texture2D m_avatar;

		public HLUserProfile.ProfileSource m_source = HLUserProfile.ProfileSource.Max;
	}

	public class Request
	{
		public enum Filter
		{
			All,
			Friends
		}

		public string m_leaderboardID = string.Empty;

		public Types m_leaderboard;

		public int m_entries;

		public Filter m_filter;

		public bool m_includePlayersRank;
	}
}
