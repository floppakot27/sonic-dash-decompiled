using System;
using UnityEngine;

public class OfferRegion_MainMenu : MonoBehaviour
{
	[Flags]
	private enum State
	{
		InitialDone = 1
	}

	private State m_state;

	[SerializeField]
	private OfferRegion_Timed m_onBoot;

	[SerializeField]
	private OfferRegion_Timed m_onPushNotification;

	[SerializeField]
	private OfferRegion_Timed m_onGameFlow;

	private void OnEnable()
	{
		if ((m_state & State.InitialDone) != State.InitialDone)
		{
			if (m_onBoot != null)
			{
				m_onBoot.Visit();
			}
		}
		else if (m_onGameFlow != null)
		{
			m_onGameFlow.Visit();
		}
		m_state |= State.InitialDone;
	}
}
