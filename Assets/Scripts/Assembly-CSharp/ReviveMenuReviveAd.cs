using System;
using UnityEngine;

public class ReviveMenuReviveAd : MonoBehaviour
{
	[Flags]
	private enum State
	{
		Active = 1,
		WaitingForConfirmation = 2
	}

	private readonly string FreeReviveEvent = StoreContent.FormatIdentifier("VideosForRevives");

	private State m_state;

	public void Active(bool active, bool endingProcess)
	{
		if (active)
		{
			m_state |= State.Active;
			m_state &= ~State.WaitingForConfirmation;
			return;
		}
		m_state &= ~State.Active;
		if (endingProcess)
		{
			m_state &= ~State.WaitingForConfirmation;
		}
	}

	private void Start()
	{
		EventDispatch.RegisterInterest("OnEventAwarded", this, EventDispatch.Priority.Highest);
	}

	private void Trigger_StartRevive()
	{
		if ((m_state & State.Active) == State.Active)
		{
			m_state |= State.WaitingForConfirmation;
			StorePurchases.RequestPurchase(FreeReviveEvent, StorePurchases.LowCurrencyResponse.Fail);
		}
	}

	private void Event_OnEventAwarded(StoreContent.StoreEntry awardedEntry)
	{
		if ((m_state & State.WaitingForConfirmation) == State.WaitingForConfirmation && !(awardedEntry.m_identifier != FreeReviveEvent))
		{
			m_state &= ~State.WaitingForConfirmation;
			GameAnalytics.ContinueUsed("VideoWatched");
			EventDispatch.GenerateEvent("OnContinueGameOk", true);
		}
	}
}
