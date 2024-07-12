using UnityEngine;

public class GameOver_DailyChallenge : GameOver_Component
{
	private GameObject m_challengeTrigger;

	private GameObject m_continueButtonTrigger;

	private GameObject m_dailyChallengeRoot;

	private static AudioClip m_audioPanelShown;

	public static void SetAudioProperties(AudioClip audioPanelShown)
	{
		m_audioPanelShown = audioPanelShown;
	}

	public override void Reset()
	{
		base.Reset();
		if (DCs.AllPiecesCollected() && !DCs.ChallengeRewarded)
		{
			SetStateDelegates(DisplayInitialise, null);
		}
		else
		{
			SetStateDelegates(null, null);
		}
	}

	private bool DisplayInitialise(float timeDelta)
	{
		if (m_challengeTrigger == null)
		{
			m_challengeTrigger = GameObject.Find("Challenge Panel (Show) [Trigger]");
		}
		if (m_continueButtonTrigger == null)
		{
			m_continueButtonTrigger = GameObject.Find("Challenge Panel Continue (Show) [Trigger]");
		}
		SetStateDelegates(DisplayTrigger, null);
		return false;
	}

	private bool DisplayTrigger(float timeDelta)
	{
		m_challengeTrigger.SendMessage("OnClick", SendMessageOptions.RequireReceiver);
		Audio.PlayClip(m_audioPanelShown, loop: false);
		SetStateDelegates(DisplayDisableButtons, null);
		return false;
	}

	private bool DisplayDisableButtons(float timeDelta)
	{
		if (m_dailyChallengeRoot == null)
		{
			m_dailyChallengeRoot = GameObject.Find("Daily Challenge Game Over Position");
		}
		SetDelayTime(2f);
		SetStateDelegates(base.DelayUpdate, DisplayShowContinueButton);
		return false;
	}

	private bool DisplayShowContinueButton(float timeDelta)
	{
		m_continueButtonTrigger.SendMessage("OnClick", SendMessageOptions.RequireReceiver);
		SetStateDelegates(DisplayWaitForClosure, null);
		return false;
	}

	private bool DisplayWaitForClosure(float timeDelta)
	{
		if (base.Closed)
		{
			m_challengeTrigger.SendMessage("OnClick", SendMessageOptions.RequireReceiver);
			m_continueButtonTrigger.SendMessage("OnClick", SendMessageOptions.RequireReceiver);
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
