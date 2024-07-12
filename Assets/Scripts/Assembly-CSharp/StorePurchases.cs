using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class StorePurchases : MonoBehaviour
{
	public enum Result
	{
		Fail,
		Success,
		Reward
	}

	public enum LowCurrencyResponse
	{
		Fail,
		PurchaseCurrencyAndItem
	}

	public enum ShowDialog
	{
		Yes,
		No
	}

	private enum ResultParameters
	{
		Entry,
		Result
	}

	private enum RingResult
	{
		Enough,
		Purchasing,
		Failed
	}

	[Flags]
	private enum State
	{
		Blocked = 1,
		Restoring = 2
	}

	private class VideoRewards
	{
		public string m_storeEntry = string.Empty;

		public string m_rewardEntry = string.Empty;

		public VideoRewards(string storeEntry, string rewardEntry)
		{
			m_storeEntry = storeEntry;
			m_rewardEntry = rewardEntry;
		}
	}

	private const string SaveProperty_WebPurchases = "WebPurchases";

	private static readonly VideoRewards[] RewardEntries = new VideoRewards[2]
	{
		new VideoRewards("VideosForRings", "Single Ring Reward"),
		new VideoRewards("VideosForRevives", "VideosForRevives")
	};

	private static StorePurchases s_storePurchase = null;

	[SerializeField]
	private AudioSource m_purchaseCompleteAudio;

	[SerializeField]
	private float m_artificialOSDelay = 1f;

	[SerializeField]
	private OfferRegion_Timed m_canceledGamePromptRegion;

	[SerializeField]
	private OfferRegion_Timed m_cancelediOSPromptRegion;

	private StoreContent.StoreEntry m_pendingPurchase;

	private StoreContent.StoreEntry m_internalPurchase;

	private object[] m_purchaseResultParams;

	private float m_blockedTimer;

	private State m_state;

	private List<StoreContent.StoreEntry> m_restoredPurchases = new List<StoreContent.StoreEntry>(1);

	private string m_restorePurchaseResult;

	public static StoreContent.StoreEntry RequestPurchase(string entryID, LowCurrencyResponse lowCurrencyResponse)
	{
		if (StoreUtils.IsStoreActive())
		{
			return null;
		}
		if ((s_storePurchase.m_state & State.Blocked) == State.Blocked)
		{
			return null;
		}
		StoreContent.StoreEntry storeEntry = StoreContent.GetStoreEntry(entryID, StoreContent.Identifiers.Name);
		if ((storeEntry.m_state & StoreContent.StoreEntry.State.Downloading) == StoreContent.StoreEntry.State.Downloading)
		{
			return null;
		}
		return s_storePurchase.StartPurchase(storeEntry, lowCurrencyResponse);
	}

	public static void RequestReward(string entryID, int quantity, int reason, ShowDialog showRewardDialog)
	{
		s_storePurchase.Event_ProvideContent(entryID, quantity, ProvideContentSource.ContentReward, reason, showRewardDialog);
	}

	public static void RestorePurchases()
	{
		if (!StoreUtils.IsStoreActive())
		{
			s_storePurchase.StartPurchaseRestore();
		}
	}

	public static bool IsPurchaseInProgress()
	{
		bool result = false;
		if (s_storePurchase != null && s_storePurchase.m_pendingPurchase != null)
		{
			result = true;
		}
		return result;
	}

	public static bool IsRestoreInProgress()
	{
		return (s_storePurchase.m_state & State.Restoring) == State.Restoring;
	}

	public static void BuyBestRingBundle(StoreContent.PaymentMethod paymentMethod, int requiredRings)
	{
		s_storePurchase.BuyBestAvailableRingBundle(paymentMethod, requiredRings);
	}

	private void Start()
	{
		s_storePurchase = this;
		m_purchaseResultParams = new object[Utils.GetEnumCount<ResultParameters>()];
		EventDispatch.RegisterInterest("OnStoreInitialised", this);
		EventDispatch.RegisterInterest("PurchaseStarted", this);
		EventDispatch.RegisterInterest("ProvideContent", this);
		EventDispatch.RegisterInterest("PaymentFailed", this);
		EventDispatch.RegisterInterest("RestorePurchasesComplete", this, EventDispatch.Priority.Highest);
		EventDispatch.RegisterInterest("CompletePendingIAP", this);
		EventDispatch.RegisterInterest("CancelPendingIAP", this);
		EventDispatch.RegisterInterest("PurchaseUnableToVerifyReceipt", this);
	}

	private void Update()
	{
		UpdatePurchaseBlock();
		if (m_restorePurchaseResult != null)
		{
			UpdateRestorePurchasesComplete();
		}
	}

	private void StartPurchaseRestore()
	{
		m_restoredPurchases.Clear();
		SLStorePlugin.RestorePurchases();
		m_state |= State.Restoring;
	}

	private StoreContent.StoreEntry StartPurchase(StoreContent.StoreEntry entryToPurchase, LowCurrencyResponse lowCurrencyResponse)
	{
		m_pendingPurchase = entryToPurchase;
		EventDispatch.GenerateEvent("OnStorePurchaseStarted", m_pendingPurchase);
		if (m_pendingPurchase.m_payment == StoreContent.PaymentMethod.Rings || m_pendingPurchase.m_payment == StoreContent.PaymentMethod.StarRings)
		{
			StartRingBasedPurchase(m_pendingPurchase, lowCurrencyResponse);
		}
		else if (m_pendingPurchase.m_payment == StoreContent.PaymentMethod.Money)
		{
			StartMoneyPurchase(m_pendingPurchase);
		}
		else if (m_pendingPurchase.m_payment == StoreContent.PaymentMethod.Web)
		{
			StartWebPurchase(m_pendingPurchase);
		}
		else if (m_pendingPurchase.m_payment == StoreContent.PaymentMethod.External)
		{
			StartExternalPurchase(m_pendingPurchase);
		}
		else if (m_pendingPurchase.m_payment == StoreContent.PaymentMethod.Free)
		{
			StartFreePurchase(m_pendingPurchase);
		}
		return m_pendingPurchase;
	}

	private void StartRingBasedPurchase(StoreContent.StoreEntry storeOffer, LowCurrencyResponse lowCurrencyResponse)
	{
		int itemCost = StoreUtils.GetItemCost(storeOffer, StoreUtils.EntryType.Player);
		switch (CheckRingsAvailable(itemCost, storeOffer, lowCurrencyResponse))
		{
		case RingResult.Purchasing:
			break;
		case RingResult.Enough:
			GameAnalytics.ShopPurchaseCompleted(storeOffer.m_identifier);
			FinaliseStoreEntryPurchase(storeOffer, 1, applyCost: true, isConsideredRewardIfCurrency: false);
			PurchaseComplete(storeOffer, Result.Success);
			break;
		default:
			PurchaseComplete(storeOffer, Result.Fail);
			DialogContent_GeneralInfo.Display(DialogContent_GeneralInfo.Type.NotEnoughRings);
			break;
		}
	}

	private void StartMoneyPurchase(StoreContent.StoreEntry storeOffer)
	{
		StartCoroutine(InitiateDelayedMoneyPurchase(storeOffer));
	}

	private void StartWebPurchase(StoreContent.StoreEntry storeOffer)
	{
		FinaliseStoreEntryPurchase(storeOffer, 0, applyCost: false, isConsideredRewardIfCurrency: false);
		PurchaseComplete(storeOffer, Result.Success);
		RequestReward("Single Ring Reward", storeOffer.m_awards.m_playerRings, (int)storeOffer.m_rewardReason, ShowDialog.Yes);
		BlockFuturePurchases();
		bool flag = false;
		if (storeOffer.m_appLink != null && storeOffer.m_appLink.Length > 0)
		{
			flag = AppUrl.Open(storeOffer.m_appLink);
		}
		if (!flag)
		{
			Application.OpenURL(storeOffer.m_webLink);
		}
		if (storeOffer.m_identifier == "twitter")
		{
			GameAnalytics.TwitterFollow();
		}
		else if (storeOffer.m_identifier == "facebook")
		{
			GameAnalytics.FacebookLike();
		}
	}

	private void StartExternalPurchase(StoreContent.StoreEntry storeOffer)
	{
		switch (storeOffer.m_externalPlugIn)
		{
		case ObjectMonitor.PlugIns.Videos:
			AwardVideoCurrency(storeOffer);
			break;
		case ObjectMonitor.PlugIns.Offers:
			AwardOffersCurrency(storeOffer);
			break;
		}
	}

	private void StartFreePurchase(StoreContent.StoreEntry storeOffer)
	{
		FinaliseStoreEntryPurchase(storeOffer, 1, applyCost: false, isConsideredRewardIfCurrency: false);
		PurchaseComplete(storeOffer, Result.Success);
	}

	private void AwardVideoCurrency(StoreContent.StoreEntry storeOffer)
	{
		SLAds.PrepareVideoAd();
		string rewardId = SetCorrectAdReward(storeOffer);
		if (!SLAds.IsVideoAvailable() || !SLAds.IsVideoReady())
		{
			PurchaseComplete(storeOffer, Result.Fail);
			DialogContent_GeneralInfo.Display(DialogContent_GeneralInfo.Type.VideoAdsNotAvailable);
			GameAnalytics.WatchVideo(result: false, rewardId);
		}
		else
		{
			PurchaseComplete(storeOffer, Result.Reward);
			GameAnalytics.WatchVideo(result: true, rewardId);
			GameUnloader.DoWhileSafe(SLAds.ShowVideoAd, GameUnloader.ReloadTrigger.SDKInactive);
			BlockFuturePurchases();
		}
	}

	private void AwardOffersCurrency(StoreContent.StoreEntry storeOffer)
	{
		if (!SLAds.IsGameOffersAvailable())
		{
			PurchaseComplete(storeOffer, Result.Fail);
			DialogContent_GeneralInfo.Display(DialogContent_GeneralInfo.Type.GameOffersNotAvailable);
			return;
		}
		PurchaseComplete(storeOffer, Result.Reward);
		SLAds.SetVideoAdRewardID("Single Ring Reward");
		SLAds.ShowGameOffers();
		BlockFuturePurchases();
	}

	private void VerifyVideoRewards()
	{
		for (int i = 0; i < RewardEntries.Length; i++)
		{
			VideoRewards videoRewards = RewardEntries[i];
			videoRewards.m_storeEntry = StoreContent.FormatIdentifier(videoRewards.m_storeEntry);
			videoRewards.m_rewardEntry = StoreContent.FormatIdentifier(videoRewards.m_rewardEntry);
		}
	}

	private string SetCorrectAdReward(StoreContent.StoreEntry storeOffer)
	{
		VideoRewards videoRewards = RewardEntries.FirstOrDefault((VideoRewards entry) => entry.m_storeEntry == storeOffer.m_identifier);
		SLAds.SetVideoAdRewardID(videoRewards.m_rewardEntry);
		return videoRewards.m_rewardEntry;
	}

	private IEnumerator InitiateDelayedMoneyPurchase(StoreContent.StoreEntry storeOffer)
	{
		yield return null;
		string storeOfferID = StoreUtils.GetOsStoreId(storeOffer, StoreUtils.EntryType.Player);
		if (!SLStorePlugin.RequestPayment(storeOfferID, 1))
		{
			Event_PaymentFailed(storeOfferID, PaymentErrorCode.ErrorUnknown);
		}
	}

	private RingResult CheckRingsAvailable(int cost, StoreContent.StoreEntry storeEntry, LowCurrencyResponse lowCurrencyResponse)
	{
		int num = ((storeEntry.m_payment != 0) ? RingStorage.TotalStarRings : RingStorage.TotalBankedRings);
		if (num >= cost)
		{
			return RingResult.Enough;
		}
		RingResult ringResult = RingResult.Failed;
		if (lowCurrencyResponse != 0)
		{
			int requiredRings = cost - num;
			StoreContent.StoreEntry bestAvailableRingBundle = GetBestAvailableRingBundle(storeEntry.m_payment, requiredRings);
			if (bestAvailableRingBundle != null)
			{
			}
			if (bestAvailableRingBundle == null)
			{
				return RingResult.Failed;
			}
			m_pendingPurchase = storeEntry;
			m_internalPurchase = bestAvailableRingBundle;
			Dialog_IAPConfirm.Display(m_internalPurchase, m_pendingPurchase.m_payment);
			return RingResult.Purchasing;
		}
		m_pendingPurchase = null;
		m_internalPurchase = null;
		return RingResult.Failed;
	}

	private RingResult BuyBestAvailableRingBundle(StoreContent.PaymentMethod paymentMethod, int requiredRings)
	{
		RingResult result = RingResult.Failed;
		StoreContent.StoreEntry bestAvailableRingBundle = GetBestAvailableRingBundle(paymentMethod, requiredRings);
		if (bestAvailableRingBundle != null)
		{
			m_pendingPurchase = null;
			m_internalPurchase = bestAvailableRingBundle;
			Dialog_IAPConfirm.Display(m_internalPurchase, StoreContent.PaymentMethod.Rings);
			result = RingResult.Purchasing;
		}
		return result;
	}

	private StoreContent.StoreEntry GetBestAvailableRingBundle(StoreContent.PaymentMethod paymentMethod, int requiredRings)
	{
		if (paymentMethod != 0 && paymentMethod != StoreContent.PaymentMethod.StarRings)
		{
			return null;
		}
		List<StoreContent.StoreEntry> stockList = StoreContent.StockList;
		StoreContent.StoreEntry storeEntry = null;
		foreach (StoreContent.StoreEntry item in stockList)
		{
			if (item.m_type != 0 || item.m_payment != StoreContent.PaymentMethod.Money || (item.m_osStore.m_baseState != ProductStateCode.ProductInfoDone && item.m_osStore.m_baseState != ProductStateCode.ProductInfoDoneRrefershing) || (item.m_osStore.m_playerState != ProductStateCode.ProductInfoDone && item.m_osStore.m_playerState != ProductStateCode.ProductInfoDoneRrefershing) || (item.m_state & StoreContent.StoreEntry.State.Hidden) == StoreContent.StoreEntry.State.Hidden)
			{
				continue;
			}
			int num = ((paymentMethod != 0) ? item.m_awards.m_playerStars : item.m_awards.m_playerRings);
			if (num >= requiredRings)
			{
				if (storeEntry == null)
				{
					storeEntry = item;
				}
				int num2 = ((paymentMethod != 0) ? storeEntry.m_awards.m_playerStars : storeEntry.m_awards.m_playerRings);
				if (num < num2)
				{
					storeEntry = item;
				}
			}
		}
		return storeEntry;
	}

	private void FinaliseStoreEntryPurchase(StoreContent.StoreEntry storeOffer, int quantity, bool applyCost, bool isConsideredRewardIfCurrency)
	{
		bool flag = storeOffer.m_payment == StoreContent.PaymentMethod.Rings || storeOffer.m_payment == StoreContent.PaymentMethod.StarRings;
		if (applyCost && flag)
		{
			int itemCost = StoreUtils.GetItemCost(storeOffer, StoreUtils.EntryType.Player);
			if (storeOffer.m_payment == StoreContent.PaymentMethod.Rings)
			{
				GameAnalytics.RingsTaken(itemCost, RingStorage.TotalBankedRings);
				EventDispatch.GenerateEvent("OnRingsAwarded", -itemCost);
			}
			else
			{
				EventDispatch.GenerateEvent("OnStarRingsAwarded", new GameAnalytics.RSRAnalyticsParam(-itemCost, GameAnalytics.RingsRecievedReason.Puchased));
			}
		}
		StoreUtils.SaveOneShotPurchase(storeOffer);
		if (storeOffer.m_type == StoreContent.EntryType.Currency)
		{
			int num = storeOffer.m_awards.m_playerRings * quantity;
			int num2 = storeOffer.m_awards.m_playerStars * quantity;
			GameAnalytics.RingsRecievedReason reason = (isConsideredRewardIfCurrency ? GameAnalytics.RingsRecievedReason.Rewarded : GameAnalytics.RingsRecievedReason.Puchased);
			if (num > 0)
			{
				GameAnalytics.RingsGiven(num, RingStorage.TotalBankedRings, reason);
				EventDispatch.GenerateEvent("OnRingsAwarded", num);
			}
			if (num2 > 0)
			{
				EventDispatch.GenerateEvent("OnStarRingsAwarded", new GameAnalytics.RSRAnalyticsParam(num2, reason));
			}
		}
		else if (storeOffer.m_type == StoreContent.EntryType.PowerUp)
		{
			int modifyValue = storeOffer.m_powerUpCount * quantity;
			PowerUpsInventory.ModifyPowerUpStock(storeOffer.m_powerUpType, modifyValue);
		}
		else if (storeOffer.m_type == StoreContent.EntryType.Upgrade)
		{
			if (storeOffer.m_upgradeToMax)
			{
				PowerUpsInventory.ModifyPowerUpLevel(storeOffer.m_powerUpType, 7u);
			}
			else
			{
				PowerUpsInventory.ModifyPowerUpLevel(storeOffer.m_powerUpType, (uint)quantity);
			}
		}
		else if (storeOffer.m_type == StoreContent.EntryType.Character)
		{
			Characters.Type character = storeOffer.m_character;
			Characters.UnlockCharacter(character);
		}
		else if (storeOffer.m_type == StoreContent.EntryType.Wallpaper)
		{
			Wallpapers.Award(storeOffer);
		}
	}

	private void PurchaseComplete(StoreContent.StoreEntry thisEntry, Result purchaseResult)
	{
		bool flag = true;
		if (purchaseResult == Result.Success)
		{
			PropertyStore.Save();
			if (m_internalPurchase == thisEntry)
			{
				m_internalPurchase = null;
				if (m_pendingPurchase != null)
				{
					flag = false;
					StartPurchase(m_pendingPurchase, LowCurrencyResponse.Fail);
				}
			}
			else
			{
				Audio.PlayClip(m_purchaseCompleteAudio.clip, m_purchaseCompleteAudio.loop);
			}
		}
		if (flag)
		{
			m_purchaseResultParams[0] = m_pendingPurchase;
			m_purchaseResultParams[1] = purchaseResult;
			m_pendingPurchase = null;
			m_internalPurchase = null;
			EventDispatch.GenerateEvent("OnStorePurchaseCompleted", m_purchaseResultParams);
		}
	}

	private void BlockFuturePurchases()
	{
		s_storePurchase.m_state |= State.Blocked;
		s_storePurchase.m_blockedTimer = 1f;
	}

	private void UpdatePurchaseBlock()
	{
		if ((m_state & State.Blocked) == State.Blocked)
		{
			m_blockedTimer -= IndependantTimeDelta.Delta;
			if (m_blockedTimer <= 0f)
			{
				m_state &= ~State.Blocked;
			}
		}
	}

	private bool CheckCharacterPurchase(string id)
	{
		int num = Characters.WhatCharacterIs(id);
		if (num != -1 && Characters.CharacterUnlocked((Characters.Type)num))
		{
			return false;
		}
		return true;
	}

	private void Event_OnStoreInitialised()
	{
		VerifyVideoRewards();
	}

	private void Event_PurchaseStarted(string offerID)
	{
		if (m_pendingPurchase == null)
		{
			StoreContent.StoreEntry storeEntry = StoreContent.GetStoreEntry(offerID, StoreContent.Identifiers.Name);
			if (storeEntry == null)
			{
				storeEntry = StoreContent.GetStoreEntry(offerID, StoreContent.Identifiers.OsStore);
			}
			if (storeEntry != null)
			{
				m_pendingPurchase = storeEntry;
				m_internalPurchase = null;
				EventDispatch.GenerateEvent("OnStorePurchaseStarted", m_pendingPurchase);
			}
		}
	}

	private void Event_ProvideContent(string storeID, int quantity, ProvideContentSource contentSource, int contentRewardReason, ShowDialog showRewardDialog)
	{
		switch (contentSource)
		{
		case ProvideContentSource.ContentRestoredPurchase:
		{
			StoreContent.StoreEntry storeEntry2 = StoreContent.GetStoreEntry(storeID, StoreContent.Identifiers.OsStore);
			if (storeEntry2 != null && StoreUtils.IsStorePurchaseRequired(storeEntry2))
			{
				m_restoredPurchases.Add(storeEntry2);
				FinaliseStoreEntryPurchase(storeEntry2, quantity, applyCost: false, isConsideredRewardIfCurrency: false);
				if ((m_state & State.Restoring) != State.Restoring)
				{
					PurchaseComplete(storeEntry2, Result.Success);
					m_restoredPurchases.Clear();
				}
			}
			break;
		}
		case ProvideContentSource.ContentReward:
		{
			storeID = StoreContent.FormatIdentifier(storeID);
			if (quantity <= 0)
			{
				break;
			}
			StoreContent.StoreEntry storeEntry3 = StoreContent.GetStoreEntry(storeID, StoreContent.Identifiers.Name);
			if (storeEntry3 == null)
			{
				storeEntry3 = StoreContent.GetStoreEntry(storeID, StoreContent.Identifiers.OsStore);
			}
			if (storeEntry3 == null)
			{
				break;
			}
			if (storeEntry3.m_type == StoreContent.EntryType.Event)
			{
				EventDispatch.GenerateEvent("OnEventAwarded", storeEntry3);
				break;
			}
			if (CheckCharacterPurchase(storeEntry3.m_identifier))
			{
				FinaliseStoreEntryPurchase(storeEntry3, quantity, applyCost: false, isConsideredRewardIfCurrency: true);
				if (showRewardDialog == ShowDialog.Yes)
				{
					DialogContent_PlayerReward.Display((DialogContent_PlayerReward.Reason)contentRewardReason, storeEntry3, quantity);
				}
				int num = Characters.WhatCharacterIs(storeEntry3.m_identifier);
				if (num != -1)
				{
					CharacterManager.Singleton.SetPendingCharacterSelection((Characters.Type)num);
					GameAnalytics.CharacterRewarded((Characters.Type)num);
				}
				EventDispatch.GenerateEvent("OnPlayerRewarded");
			}
			PropertyStore.Save();
			break;
		}
		case ProvideContentSource.ContentPurchase:
		{
			storeID = StoreContent.FormatIdentifier(storeID);
			StoreContent.StoreEntry storeEntry = ((m_internalPurchase == null) ? m_pendingPurchase : m_internalPurchase);
			if (storeEntry == null)
			{
				storeEntry = (m_pendingPurchase = StoreContent.GetStoreEntry(storeID, StoreContent.Identifiers.OsStore));
				if (storeEntry == null)
				{
					break;
				}
			}
			if (storeEntry.m_payment == StoreContent.PaymentMethod.Rings || storeEntry.m_payment == StoreContent.PaymentMethod.StarRings)
			{
				GameAnalytics.ShopPurchaseCompleted(storeID);
			}
			else if (storeEntry.m_payment == StoreContent.PaymentMethod.Money)
			{
				GameAnalytics.InAppPurchaseCompleted(storeID, m_internalPurchase != null);
				if (GameAnalytics.GetPurchaseLocation() == GameAnalytics.PurchaseLocations.WOFScreen)
				{
					WheelOfFortuneRewards.Instance.BuyingRedStarRings();
				}
			}
			if (storeEntry == m_pendingPurchase && storeEntry.m_payment == StoreContent.PaymentMethod.Money)
			{
				Dialog_PurchaseComplete.Display(storeEntry);
			}
			if ((storeEntry.m_state & StoreContent.StoreEntry.State.RemoveAds) == StoreContent.StoreEntry.State.RemoveAds)
			{
				GameAnalytics.RemoveAdsPurchased(storeID);
				PaidUser.RemovedAds = true;
			}
			FinaliseStoreEntryPurchase(storeEntry, 1, applyCost: false, isConsideredRewardIfCurrency: false);
			PurchaseComplete(storeEntry, Result.Success);
			break;
		}
		}
	}

	private void Event_PaymentFailed(string storeID, PaymentErrorCode errorCode)
	{
		StoreContent.StoreEntry storeEntry = ((m_internalPurchase == null) ? m_pendingPurchase : m_internalPurchase);
		if (storeEntry != null)
		{
			DialogContent_GeneralInfo.Display(DialogContent_GeneralInfo.Type.FailedPurchase);
			bool internalPurchase = m_internalPurchase != null;
			GameAnalytics.InAppPurchaseFailed(storeID, internalPurchase, errorCode);
			PurchaseComplete(storeEntry, Result.Fail);
			if (errorCode == PaymentErrorCode.ErrorPaymentCancelled && m_cancelediOSPromptRegion != null)
			{
				m_cancelediOSPromptRegion.Visit();
			}
		}
	}

	private void Event_RestorePurchasesComplete(string results)
	{
		m_restorePurchaseResult = results;
	}

	private void UpdateRestorePurchasesComplete()
	{
		if (m_restorePurchaseResult == null)
		{
			return;
		}
		if (m_restorePurchaseResult.ToLower() == "true")
		{
			if (m_restoredPurchases.Count == 0)
			{
				DialogContent_GeneralInfo.Display(DialogContent_GeneralInfo.Type.RestorePurchaseNothing);
			}
			else
			{
				Dialog_RestorePurchase.Display(m_restoredPurchases);
			}
		}
		else
		{
			DialogContent_GeneralInfo.Display(DialogContent_GeneralInfo.Type.RestorePurchaseFailed);
		}
		m_restoredPurchases.Clear();
		m_state &= ~State.Restoring;
		m_restorePurchaseResult = null;
	}

	private void Event_CompletePendingIAP()
	{
		if (m_internalPurchase != null)
		{
			StartMoneyPurchase(m_internalPurchase);
		}
	}

	private void Event_CancelPendingIAP()
	{
		if (m_pendingPurchase != null)
		{
			GameAnalytics.ShopPurchaseFailed(m_pendingPurchase.m_identifier);
			PurchaseComplete(m_pendingPurchase, Result.Fail);
		}
		else
		{
			PurchaseComplete(m_internalPurchase, Result.Fail);
		}
		if (m_canceledGamePromptRegion != null)
		{
			m_canceledGamePromptRegion.Visit();
		}
	}

	private void Event_PurchaseUnableToVerifyReceipt(string param)
	{
		if (m_pendingPurchase != null)
		{
			PurchaseComplete(m_pendingPurchase, Result.Fail);
		}
		DialogContent_GeneralInfo.Display(DialogContent_GeneralInfo.Type.ReceiptVerificationFailed);
	}
}
