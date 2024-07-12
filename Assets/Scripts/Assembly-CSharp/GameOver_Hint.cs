using UnityEngine;

public class GameOver_Hint : GameOver_Component
{
	private static AudioClip s_audioPanelShown;

	private GameObject m_hintTrigger;

	public static void SetAudioProperties(AudioClip audioPanelShown)
	{
		s_audioPanelShown = audioPanelShown;
	}

	public static void SetDisplayTime(float displayTime)
	{
	}

	public override void Reset()
	{
		base.Reset();
		SetStateDelegates(DisplayInitialise, null);
	}

	private bool DisplayInitialise(float timeDelta)
	{
		DialogContent_Hints.Hint hint = DialogContent_Hints.GetHint();
		int storeEntry = DialogContent_Hints.GetStoreEntry(hint);
		Dialog_Hint.SetNextContent(hint, storeEntry);
		if ((hint.m_state & DialogContent_Hints.Hint.State.UseStore) == DialogContent_Hints.Hint.State.UseStore)
		{
			m_hintTrigger = GameObject.Find("Hint With Store (Show) [Trigger]");
		}
		else
		{
			m_hintTrigger = GameObject.Find("Hint No Store (Show) [Trigger]");
		}
		GameAnalytics.HintShown(hint.m_description);
		SetStateDelegates(DisplayTrigger, null);
		return false;
	}

	private bool DisplayTrigger(float timeDelta)
	{
		m_hintTrigger.SendMessage("OnClick", SendMessageOptions.RequireReceiver);
		Audio.PlayClip(s_audioPanelShown, loop: false);
		SetStateDelegates(DisplayWaitForTimeOut, null);
		if (ScoreTracker.CurrentScore >= ScoreTracker.HighScore || (!DialogContent_RateMe.IsFirst && (float)ScoreTracker.CurrentScore >= 0.7f * (float)ScoreTracker.HighScore))
		{
			Dialog_RateMe.Display();
		}
		return false;
	}

	private bool DisplayWaitForTimeOut(float timeDelta)
	{
		bool flag = false;
		if (base.Closed)
		{
			flag = true;
		}
		if (flag)
		{
			m_hintTrigger.SendMessage("OnClick", SendMessageOptions.RequireReceiver);
			Audio.PlayClip(s_audioPanelShown, loop: false);
			SetDelayTime(0.6f);
			SetStateDelegates(base.DelayUpdate, DisplayEnd);
		}
		return false;
	}

	private bool DisplayEnd(float timeDelta)
	{
		return true;
	}
}
