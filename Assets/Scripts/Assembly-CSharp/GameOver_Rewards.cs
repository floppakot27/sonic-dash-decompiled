using UnityEngine;

public class GameOver_Rewards : GameOver_Component
{
	private static AudioClip s_audioPanelShown;

	private static bool s_dialogsHidden;

	public static void SetAudioProperties(AudioClip audioPanelShown)
	{
		s_audioPanelShown = audioPanelShown;
	}

	public static void DialogsHidden()
	{
		s_dialogsHidden = true;
	}

	public override void Reset()
	{
		base.Reset();
		s_dialogsHidden = false;
		bool flag = DisplayRewardDialog();
		if (flag)
		{
			SetStateDelegates(DisplayWaitForDialogs, null);
		}
		else
		{
			SetStateDelegates(null, null);
		}
		if (flag)
		{
			Audio.PlayClip(s_audioPanelShown, loop: false);
		}
	}

	private bool DisplayRewardDialog()
	{
		bool flag = false;
		if (ScoreTracker.HighScoreAchived)
		{
			flag = StarRingsRewards.Reward(StarRingsRewards.Reason.HighScore);
			if (!flag)
			{
			}
		}
		if (!flag)
		{
			Leaderboards.Entry entry = HudContent_FriendDisplay.TopFriend();
			if (entry != null && entry.m_score < ScoreTracker.CurrentScore)
			{
				flag = StarRingsRewards.Reward(StarRingsRewards.Reason.FirstLeaderboard);
				if (!flag)
				{
				}
			}
		}
		return flag;
	}

	private bool DisplayWaitForDialogs(float timeDelta)
	{
		if (s_dialogsHidden)
		{
			SetStateDelegates(DisplayEnd, null);
		}
		return false;
	}

	private bool DisplayEnd(float timeDelta)
	{
		return true;
	}
}
