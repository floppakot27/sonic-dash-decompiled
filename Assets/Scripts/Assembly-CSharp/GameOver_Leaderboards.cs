using UnityEngine;

public class GameOver_Leaderboards : GameOver_Component
{
	private GameObject m_leaderboardTrigger;

	private static AudioClip m_audioPanelShown;

	public static void SetAudioProperties(AudioClip audioPanelShown)
	{
		m_audioPanelShown = audioPanelShown;
	}

	public override void Reset()
	{
		base.Reset();
		SetStateDelegates(DisplayInitialise, null);
	}

	public override void Hide()
	{
		m_leaderboardTrigger.SendMessage("OnClick", SendMessageOptions.RequireReceiver);
	}

	public override void Show()
	{
		m_leaderboardTrigger.SendMessage("OnClick", SendMessageOptions.RequireReceiver);
	}

	private bool DisplayInitialise(float timeDelta)
	{
		if (m_leaderboardTrigger == null)
		{
			m_leaderboardTrigger = GameObject.Find("Leaderboard Panel (Show) [Trigger]");
		}
		SetStateDelegates(DisplayTrigger, null);
		return false;
	}

	private bool DisplayTrigger(float timeDelta)
	{
		m_leaderboardTrigger.SendMessage("OnClick", SendMessageOptions.RequireReceiver);
		Audio.PlayClip(m_audioPanelShown, loop: false);
		return true;
	}
}
