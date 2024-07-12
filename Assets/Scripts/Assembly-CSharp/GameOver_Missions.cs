using UnityEngine;

public class GameOver_Missions : GameOver_Component
{
	private GameObject m_missionTrigger;

	private static AudioClip m_audioPanelShown;

	public static void SetAudioProperties(AudioClip audioPanelShown)
	{
		m_audioPanelShown = audioPanelShown;
	}

	public override void Reset()
	{
		base.Reset();
		if (MissionTracker.AllMissionSetComplete() && !MissionTracker.AllMissionsComplete())
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
		m_missionTrigger = GameObject.Find("Missions Panel (Show) [Trigger]");
		SetStateDelegates(DisplayTrigger, null);
		return false;
	}

	private bool DisplayTrigger(float timeDelta)
	{
		m_missionTrigger.SendMessage("OnClick", SendMessageOptions.RequireReceiver);
		Audio.PlayClip(m_audioPanelShown, loop: false);
		SetStateDelegates(DisplayWaitForClosure, null);
		return false;
	}

	private bool DisplayWaitForClosure(float timeDelta)
	{
		if (base.Closed)
		{
			m_missionTrigger.SendMessage("OnClick", SendMessageOptions.RequireReceiver);
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
