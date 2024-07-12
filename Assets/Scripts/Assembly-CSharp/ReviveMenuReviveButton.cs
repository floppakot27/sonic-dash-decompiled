using System;
using System.Collections;
using UnityEngine;

public class ReviveMenuReviveButton : MonoBehaviour
{
	[Flags]
	private enum State
	{
		Active = 1,
		Delayed = 2
	}

	private State m_state;

	[SerializeField]
	private UILabel m_tokensDisplay;

	[SerializeField]
	private UISlider m_timeOutSlider;

	public void Active(bool active)
	{
		if (active)
		{
			m_state |= State.Active;
		}
		else
		{
			m_state &= ~State.Active;
		}
	}

	private void Start()
	{
	}

	private void Update()
	{
		if (MenuReviveScreen.Valid)
		{
			m_timeOutSlider.sliderValue = MenuReviveScreen.CountdownTime / MenuReviveScreen.TimeOut;
		}
	}

	private void OnEnable()
	{
		StartCoroutine(StartPendingActivation());
	}

	private IEnumerator StartPendingActivation()
	{
		while (!MenuReviveScreen.Valid)
		{
			yield return null;
		}
		m_tokensDisplay.text = MenuReviveScreen.RevivesRequired.ToString();
	}

	private void Trigger_ContinueGame_Paid()
	{
		if ((m_state & State.Active) == State.Active)
		{
			int powerUpCount = PowerUpsInventory.GetPowerUpCount(PowerUps.Type.Respawn);
			if (powerUpCount >= MenuReviveScreen.RevivesRequired)
			{
				GameAnalytics.ContinueUsed("NoPurchaseRequired");
				EventDispatch.GenerateEvent("OnContinueGameOk", false);
			}
			else
			{
				EventDispatch.GenerateEvent("OnContinuePurchaseRequired");
			}
		}
	}

	private void Trigger_ContinueGame_Free()
	{
		if ((m_state & State.Active) == State.Active)
		{
			GameAnalytics.ContinueUsed("FreeReviveUsed");
			EventDispatch.GenerateEvent("OnContinueGameOk", true);
		}
	}

	private void Trigger_ContinueGame_Cancel()
	{
		if ((m_state & State.Active) == State.Active)
		{
			GameAnalytics.ContinueCancelled(GameAnalytics.CancelContinueReasons.Skip);
			EventDispatch.GenerateEvent("OnContinueGameCancel");
		}
	}
}
