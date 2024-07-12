using UnityEngine;

public class LeaderboardBragger : MonoBehaviour
{
	private void Trigger_BragScore()
	{
	}

	private void Trigger_InviteFriends()
	{
		if (SLSocial.IsAvailable())
		{
			PlayerStats.IncreaseStat(PlayerStats.StatNames.TimesBragged_Total, 1);
			StarRingsRewards.Reward(StarRingsRewards.Reason.Bragging);
			string text = null;
			if (ScoreTracker.HighScore > 0)
			{
				string @string = LanguageStrings.First.GetString("SOCIAL_INVITE_WITH_SCORE");
				text = string.Format(@string, LanguageUtils.FormatNumber(ScoreTracker.HighScore));
			}
			else
			{
				text = LanguageStrings.First.GetString("SOCIAL_INVITE_NO_SCORE");
			}
			if (text != null)
			{
				SLSocial.ShareMessage(text);
			}
		}
	}
}
