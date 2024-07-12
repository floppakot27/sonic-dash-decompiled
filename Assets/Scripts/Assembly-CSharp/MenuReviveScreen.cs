using System;
using System.Collections;
using UnityEngine;

public class MenuReviveScreen : MonoBehaviour
{
	[Flags]
	private enum Shown
	{
		Friends = 1,
		Free = 2,
		Paid = 4,
		Ads = 8
	}

	[Flags]
	private enum State
	{
		AdShown = 1,
		FreeReviveUsed = 2,
		Active = 4,
		Delaying = 8,
		Finished = 0x10,
		ShowingAdRevive = 0x20
	}

	private static MenuReviveScreen s_instance;

	private Shown m_shown;

	private State m_state;

	private float m_timeOutTimer;

	private float m_showDelayTimer;

	[SerializeField]
	private float m_defaultTimeOut = 3f;

	[SerializeField]
	private float m_videoAdTimeOut = 5f;

	[SerializeField]
	private GuiTrigger m_purchasePage;

	[SerializeField]
	private ReviveMenuReviveAd m_reviveAd;

	[SerializeField]
	private ReviveMenuReviveButton m_reviveFree;

	[SerializeField]
	private ReviveMenuReviveButton m_revivePaid;

	[SerializeField]
	private GameObject m_showFriendScore;

	[SerializeField]
	private GameObject m_showReviveAd;

	[SerializeField]
	private GameObject m_showReviveFree;

	[SerializeField]
	private GameObject m_showRevivePaid;

	public static float TimeOut
	{
		get
		{
			if ((s_instance.m_state & State.ShowingAdRevive) == State.ShowingAdRevive)
			{
				return s_instance.m_videoAdTimeOut;
			}
			return s_instance.m_defaultTimeOut;
		}
	}

	public static GuiTrigger PurchasePage => s_instance.m_purchasePage;

	public static float CountdownTime => s_instance.m_timeOutTimer;

	public static bool Valid => s_instance != null;

	public static int RevivesRequired { get; set; }

	private void Start()
	{
		s_instance = this;
		EventDispatch.RegisterInterest("OnNewGameStarted", this);
		EventDispatch.RegisterInterest("OnContinueGameOk", this, EventDispatch.Priority.Highest);
		EventDispatch.RegisterInterest("OnContinueGameCancel", this, EventDispatch.Priority.Highest);
		EventDispatch.RegisterInterest("OnContinuePurchaseRequired", this, EventDispatch.Priority.Highest);
		EventDispatch.RegisterInterest("3rdPartyActive", this, EventDispatch.Priority.Highest);
		EventDispatch.RegisterInterest("3rdPartyInactive", this, EventDispatch.Priority.Highest);
	}

	private void Update()
	{
		if ((m_state & State.Delaying) == State.Delaying)
		{
			UpdateShowDelay();
		}
		else
		{
			UpdateTimeOut();
		}
	}

	private void OnEnable()
	{
		StartCoroutine(StartPendingActivation());
	}

	private void UpdateShowDelay()
	{
		m_showDelayTimer -= IndependantTimeDelta.Delta;
		if (m_showDelayTimer <= 0f)
		{
			m_state &= ~State.Delaying;
			ShowScreenContent();
		}
	}

	private void UpdateTimeOut()
	{
		if ((m_state & State.Active) == State.Active)
		{
			m_timeOutTimer -= IndependantTimeDelta.Delta;
			if (m_timeOutTimer < 0f)
			{
				m_timeOutTimer = 0f;
			}
			if (m_timeOutTimer <= 0f)
			{
				GameAnalytics.ContinueCancelled(GameAnalytics.CancelContinueReasons.Timeout);
				EventDispatch.GenerateEvent("OnContinueGameCancel");
			}
		}
	}

	private void ShowScreenContent()
	{
		if ((m_state & State.Finished) != State.Finished)
		{
			bool show = ShouldFriendScoreShow();
			ToggleComponentTrigger(show, Shown.Friends, m_showFriendScore);
			bool flag = ShouldFreeReviveShow();
			ToggleComponentTrigger(flag, Shown.Free, m_showReviveFree);
			if (flag)
			{
				m_reviveFree.Active(active: true);
			}
			bool flag2 = ShouldPaidReviveShow();
			ToggleComponentTrigger(flag2, Shown.Paid, m_showRevivePaid);
			if (flag2)
			{
				m_revivePaid.Active(active: true);
			}
			bool flag3 = ShouldAdReviveShow(flag);
			ToggleComponentTrigger(flag3, Shown.Ads, m_showReviveAd);
			if (flag3)
			{
				m_reviveAd.Active(active: true, endingProcess: false);
			}
			m_state |= State.Active;
			if (flag3)
			{
				m_state |= State.ShowingAdRevive;
			}
			else
			{
				m_state &= ~State.ShowingAdRevive;
			}
			m_timeOutTimer = TimeOut;
		}
	}

	private void HideScreenContent(bool endingProcess)
	{
		if ((m_shown & Shown.Friends) == Shown.Friends)
		{
			ToggleComponentTrigger(show: false, Shown.Friends, m_showFriendScore);
		}
		if ((m_shown & Shown.Free) == Shown.Free)
		{
			ToggleComponentTrigger(show: false, Shown.Free, m_showReviveFree);
		}
		if ((m_shown & Shown.Paid) == Shown.Paid)
		{
			ToggleComponentTrigger(show: false, Shown.Paid, m_showRevivePaid);
		}
		if ((m_shown & Shown.Ads) == Shown.Ads)
		{
			ToggleComponentTrigger(show: false, Shown.Ads, m_showReviveAd);
		}
		m_reviveAd.Active(active: false, endingProcess);
		m_reviveFree.Active(active: false);
		m_revivePaid.Active(active: false);
		m_state &= ~State.Active;
		if (endingProcess)
		{
			m_state |= State.Finished;
		}
	}

	private void ToggleComponentTrigger(bool show, Shown type, GameObject trigger)
	{
		bool flag = (m_shown & type) == type;
		if (flag != show)
		{
			trigger.SendMessage("OnClick", SendMessageOptions.DontRequireReceiver);
			if (show)
			{
				m_shown |= type;
			}
			else
			{
				m_shown &= ~type;
			}
		}
	}

	private bool ShouldFriendScoreShow()
	{
		Leaderboards.Entry entry = HudContent_FriendDisplay.CurrentFriend();
		return entry != null;
	}

	private bool ShouldFreeReviveShow()
	{
		bool flag = PowerUpsInventory.GetPowerUpCount(PowerUps.Type.FreeRevive) > 0;
		bool flag2 = (m_state & State.FreeReviveUsed) == State.FreeReviveUsed;
		return flag && !flag2;
	}

	private bool ShouldPaidReviveShow()
	{
		bool flag = PowerUpsInventory.GetPowerUpCount(PowerUps.Type.FreeRevive) > 0;
		bool flag2 = (m_state & State.FreeReviveUsed) == State.FreeReviveUsed;
		return !flag || flag2;
	}

	private bool ShouldAdReviveShow(bool freeReviveShown)
	{
		if (FeatureSupport.IsLowEndDevice())
		{
			return false;
		}
		if (freeReviveShown)
		{
			return false;
		}
		if ((m_state & State.AdShown) == State.AdShown)
		{
			return false;
		}
		m_state |= State.AdShown;
		if (ABTesting.GetTestValue(ABTesting.Tests.ADS_Revive) == 0)
		{
			return false;
		}
		SLAds.PrepareVideoAd();
		if (!SLAds.IsVideoAvailable() || !SLAds.IsVideoReady())
		{
			return false;
		}
		if (StoreUtils.IsStoreActive())
		{
			return false;
		}
		return true;
	}

	private IEnumerator StartPendingActivation()
	{
		yield return null;
		m_state &= ~State.Delaying;
		m_state &= ~State.Finished;
		ShowScreenContent();
	}

	private void Event_OnNewGameStarted()
	{
		m_state &= ~State.AdShown;
		m_state &= ~State.FreeReviveUsed;
	}

	private void Event_OnContinueGameCancel()
	{
		HideScreenContent(endingProcess: true);
	}

	private void Event_OnContinueGameOk(bool freeRevive)
	{
		HideScreenContent(endingProcess: true);
		if (freeRevive)
		{
			m_state |= State.FreeReviveUsed;
		}
	}

	private void Event_OnContinuePurchaseRequired()
	{
		MenuStack.MoveToPage(PurchasePage, replaceCurrent: true);
		HideScreenContent(endingProcess: true);
	}

	private void Event_3rdPartyActive()
	{
		if (base.gameObject.activeInHierarchy)
		{
			HideScreenContent(endingProcess: false);
		}
	}

	private void Event_3rdPartyInactive()
	{
		if (base.gameObject.activeInHierarchy)
		{
			m_showDelayTimer = 1f;
			m_state |= State.Delaying;
		}
	}
}
