using UnityEngine;

public class StarRingsRewards : MonoBehaviour
{
	public enum Reason
	{
		Bragging,
		Returning,
		FirstLeaderboard,
		RegisterSegaID,
		HighScore
	}

	public static bool Reward(Reason reason)
	{
		bool result = false;
		int amount = 1;
		PlayerStats.Stats currentStats = PlayerStats.GetCurrentStats();
		switch (reason)
		{
		case Reason.Bragging:
			if (ABTesting.GetTestValue(ABTesting.Tests.RSR_Brag) > 0 && currentStats.m_trackedStats[77] == 1)
			{
				Dialog_StarRingReward.Display(Reason.Bragging);
				PlayerStats.IncreaseStat(PlayerStats.StatNames.StarRingsEarned_Total, 1);
				EventDispatch.GenerateEvent("OnStarRingsAwarded", new GameAnalytics.RSRAnalyticsParam(amount, GameAnalytics.RingsRecievedReason.Brag));
				result = true;
			}
			break;
		case Reason.Returning:
			if (ABTesting.GetTestValue(ABTesting.Tests.RSR_Return) > 0 && currentStats.m_trackedStats[79] == 2)
			{
				Dialog_StarRingReward.Display(Reason.Returning);
				PlayerStats.IncreaseStat(PlayerStats.StatNames.StarRingsEarned_Total, 1);
				EventDispatch.GenerateEvent("OnStarRingsAwarded", new GameAnalytics.RSRAnalyticsParam(amount, GameAnalytics.RingsRecievedReason.Return));
				result = true;
			}
			break;
		case Reason.FirstLeaderboard:
			if (ABTesting.GetTestValue(ABTesting.Tests.RSR_Leader) > 0 && currentStats.m_trackedStats[78] == 0)
			{
				currentStats.m_trackedStats[78] = 1;
				Dialog_StarRingReward.Display(Reason.FirstLeaderboard);
				PlayerStats.IncreaseStat(PlayerStats.StatNames.StarRingsEarned_Total, 1);
				EventDispatch.GenerateEvent("OnStarRingsAwarded", new GameAnalytics.RSRAnalyticsParam(amount, GameAnalytics.RingsRecievedReason.Leaderboard));
				result = true;
			}
			break;
		case Reason.HighScore:
			if (ABTesting.GetTestValue(ABTesting.Tests.RSR_Highscore) > 0 && currentStats.m_trackedStats[80] == 0)
			{
				currentStats.m_trackedStats[80] = 1;
				Dialog_StarRingReward.Display(Reason.HighScore);
				PlayerStats.IncreaseStat(PlayerStats.StatNames.StarRingsEarned_Total, 1);
				EventDispatch.GenerateEvent("OnStarRingsAwarded", new GameAnalytics.RSRAnalyticsParam(amount, GameAnalytics.RingsRecievedReason.Highscore));
				result = true;
			}
			break;
		}
		return result;
	}
}
