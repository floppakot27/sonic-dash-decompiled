using UnityEngine;

public class LeaderboardContent
{
	public struct LastPostedData
	{
		public bool m_valid;

		public string m_user;

		public long m_score;
	}

	private static LastPostedData[] s_lastPostedData;

	public LeaderboardContent()
	{
		PrepareLastPostedData();
		EventDispatch.RegisterInterest("RequestLeaderboardDisplay", this, EventDispatch.Priority.Highest);
		EventDispatch.RegisterInterest("PostLeaderboardScore", this, EventDispatch.Priority.Highest);
	}

	public static LastPostedData GetLastPostedData(Leaderboards.Types leaderboard)
	{
		return s_lastPostedData[(int)leaderboard];
	}

	private void Event_RequestLeaderboardDisplay()
	{
		Social.ShowLeaderboardUI();
	}

	private void Event_PostLeaderboardScore(Leaderboards.Types leaderboard, long score)
	{
		if (Social.localUser.authenticated)
		{
			s_lastPostedData[(int)leaderboard].m_score = score;
			s_lastPostedData[(int)leaderboard].m_user = Social.localUser.userName;
			s_lastPostedData[(int)leaderboard].m_valid = true;
			string board = leaderboard.ToString();
			Social.ReportScore(score, board, null);
		}
	}

	private void PrepareLastPostedData()
	{
		if (s_lastPostedData == null)
		{
			int enumCount = Utils.GetEnumCount<Leaderboards.Types>();
			s_lastPostedData = new LastPostedData[enumCount];
			for (int i = 0; i < enumCount; i++)
			{
				s_lastPostedData[i].m_valid = false;
			}
		}
	}
}
