using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

public class OfferState : MonoBehaviour
{
	private struct OfferEntry
	{
		public int m_offerType;

		public int m_campaignID;

		public DateTime m_lastDisplayTS;

		public int m_displayCount;

		public bool m_closed;

		public void Init()
		{
			m_offerType = 0;
			m_campaignID = -1;
			m_closed = false;
			m_displayCount = 0;
			m_lastDisplayTS = DateTime.Now;
		}
	}

	private OfferEntry m_currentOffer;

	private string m_stagedProductID;

	private string m_stagedBonusID;

	private int m_stagedBonusCount;

	private volatile bool m_needsSave;

	private int m_loadCount;

	private DateTime m_lastSynchTS;

	private static OfferState s_offerState;

	private static int s_saveCount;

	private void Start()
	{
		s_offerState = this;
		m_currentOffer = default(OfferEntry);
		m_currentOffer.Init();
		m_currentOffer.m_closed = true;
		EventDispatch.RegisterInterest("ProvideContent", this);
		EventDispatch.RegisterInterest("PaymentFailed", this);
		EventDispatch.RegisterInterest("OnGameDataLoaded", this);
		EventDispatch.RegisterInterest("OnGameDataSaveRequest", this);
		m_stagedProductID = "invalid";
		m_stagedBonusID = "none";
		m_stagedBonusCount = 0;
		m_needsSave = false;
		m_loadCount = 0;
		m_lastSynchTS = new DateTime(2001, 1, 1);
	}

	private void Update()
	{
		if (m_loadCount > 1)
		{
			if (m_needsSave)
			{
				m_needsSave = false;
				PropertyStore.Save();
			}
			if (UserIdentification.Current.Length > 0 && (DateTime.Now - m_lastSynchTS).TotalHours > 24.0)
			{
				m_lastSynchTS = DateTime.Now;
				StartCoroutine(DownloadServerFile());
			}
		}
	}

	private IEnumerator DownloadServerFile()
	{
		FileDownloader fDownloader = new FileDownloader(FileDownloader.Files.OfferState, keepAndUseLocalCopy: true);
		yield return fDownloader.Loading;
		if (fDownloader.Error == null)
		{
			JsonParser parser = new JsonParser(fDownloader.Text);
			Dictionary<string, object> jsonObject = parser.Parse();
			int status = -1;
			object valStatus = null;
			if (jsonObject.TryGetValue("status", out valStatus) && (int)((double)valStatus + 0.1) == 0)
			{
				OfferEntry newOffer = default(OfferEntry);
				newOffer.Init();
				object valOffer = null;
				if (jsonObject.TryGetValue("offer_type", out valOffer))
				{
					newOffer.m_offerType = (int)((double)valOffer + 0.1);
					object valCampaignID = null;
					if (jsonObject.TryGetValue("campaign_id", out valCampaignID))
					{
						newOffer.m_campaignID = (int)((double)valCampaignID + 0.1);
					}
				}
				if (newOffer.m_campaignID != m_currentOffer.m_campaignID)
				{
					m_currentOffer = newOffer;
					m_needsSave = true;
				}
			}
		}
		EventDispatch.GenerateEvent("OfferStateReady");
	}

	public static bool CanDisplay()
	{
		if (s_offerState.m_currentOffer.m_offerType == 0)
		{
			return false;
		}
		if (s_offerState.m_currentOffer.m_closed)
		{
			return false;
		}
		OfferContent.Offer offer = OfferContent.GetOffer(s_offerState.m_currentOffer.m_offerType);
		if (offer.m_nonPayingUsers)
		{
			PlayerStats.Stats currentStats = PlayerStats.GetCurrentStats();
			if (currentStats.m_trackedStats[73] > 0)
			{
				return false;
			}
		}
		if (!SLPlugin.IsNetworkConnected())
		{
			return false;
		}
		StoreContent.StoreEntry storeEntry = StoreContent.GetStoreEntry(offer.m_beforeProductID, StoreContent.Identifiers.OsStore);
		if (storeEntry.m_osStore.m_playerState != ProductStateCode.ProductInfoDone)
		{
			return false;
		}
		StoreContent.StoreEntry storeEntry2 = StoreContent.GetStoreEntry(offer.m_productID, StoreContent.Identifiers.OsStore);
		if (storeEntry2.m_osStore.m_playerState != ProductStateCode.ProductInfoDone)
		{
			return false;
		}
		if (s_offerState.m_currentOffer.m_displayCount == 0)
		{
			return true;
		}
		if ((DateTime.Now - s_offerState.m_currentOffer.m_lastDisplayTS).TotalHours < (double)offer.m_frequencyCap)
		{
			return false;
		}
		return true;
	}

	public static void RegisterDisplay()
	{
		OfferContent.Offer offer = OfferContent.GetOffer(s_offerState.m_currentOffer.m_offerType);
		s_offerState.m_currentOffer.m_displayCount++;
		s_offerState.m_currentOffer.m_lastDisplayTS = DateTime.Now;
		Dialog_TargetedOffer.Display(offer, s_offerState.m_currentOffer.m_displayCount >= offer.m_displayCap);
		if (s_offerState.m_currentOffer.m_displayCount >= offer.m_displayCap)
		{
			s_offerState.m_currentOffer.m_closed = true;
		}
		SLAnalytics.AddParameter("OfferID", s_offerState.m_currentOffer.m_offerType.ToString());
		SLAnalytics.AddParameter("BaseOffer", offer.m_productID);
		SLAnalytics.AddParameter("Bonus", offer.m_bonusType.ToString());
		SLAnalytics.LogEventWithParameters("TargetOfferImpression");
	}

	public static void StagePurchase(string productID, string bonusID, int bonusCount)
	{
		s_offerState.m_stagedProductID = productID;
		s_offerState.m_stagedBonusID = bonusID;
		s_offerState.m_stagedBonusCount = bonusCount;
		s_offerState.m_needsSave = true;
		SLAnalytics.AddParameter("OfferID", s_offerState.m_currentOffer.m_offerType.ToString());
		SLAnalytics.AddParameter("BaseOffer", s_offerState.m_stagedProductID);
		SLAnalytics.AddParameter("Bonus", s_offerState.m_stagedBonusID);
		SLAnalytics.AddParameter("Offer", s_offerState.m_stagedProductID + s_offerState.m_stagedBonusID);
		SLAnalytics.LogEventWithParameters("TargetOfferClicked");
	}

	private void Event_ProvideContent(string storeID, int quantity, ProvideContentSource contentSource, int contentRewardReason, StorePurchases.ShowDialog showRewardDialog)
	{
		if (contentSource == ProvideContentSource.ContentPurchase && storeID == s_offerState.m_stagedProductID)
		{
			object[] parameters = new object[5]
			{
				s_offerState.m_stagedBonusID,
				s_offerState.m_stagedBonusCount,
				ProvideContentSource.ContentReward,
				0,
				StorePurchases.ShowDialog.No
			};
			EventDispatch.GenerateEvent("ProvideContent", parameters);
			s_offerState.m_currentOffer.m_closed = true;
			SLAnalytics.AddParameter("OfferID", s_offerState.m_currentOffer.m_offerType.ToString());
			SLAnalytics.AddParameter("BaseOffer", s_offerState.m_stagedProductID);
			SLAnalytics.AddParameter("Bonus", s_offerState.m_stagedBonusID);
			SLAnalytics.AddParameter("Offer", s_offerState.m_stagedProductID + s_offerState.m_stagedBonusID);
			SLAnalytics.LogEventWithParameters("TargetOfferTaken");
			s_offerState.m_stagedProductID = "invalid";
			m_needsSave = true;
		}
	}

	private void Event_PaymentFailed(string storeID, PaymentErrorCode errorCode)
	{
		if (storeID == s_offerState.m_stagedProductID)
		{
			s_offerState.m_stagedProductID = "invalid";
			m_needsSave = true;
		}
	}

	private void Event_OnGameDataSaveRequest()
	{
		PropertyStore.Store("OfferState_OfferType", m_currentOffer.m_offerType);
		PropertyStore.Store("OfferState_CampaignID", m_currentOffer.m_campaignID);
		CultureInfo cultureInfo = new CultureInfo("en-US");
		string property = m_currentOffer.m_lastDisplayTS.ToString(cultureInfo.DateTimeFormat);
		PropertyStore.Store("OfferState_LastDisplayTS", property);
		PropertyStore.Store("OfferState_DisplayCount", m_currentOffer.m_displayCount);
		PropertyStore.Store("OfferState_OfferClosed", m_currentOffer.m_closed);
		if (m_stagedProductID != null)
		{
			PropertyStore.Store("OfferState_StagedProductID", m_stagedProductID);
			PropertyStore.Store("OfferState_StagedBonusID", m_stagedBonusID);
			PropertyStore.Store("OfferState_StagedBonusCount", m_stagedBonusCount);
		}
		s_saveCount++;
	}

	private void Event_OnGameDataLoaded(ActiveProperties activeProperties)
	{
		m_loadCount++;
		if (m_loadCount > 1)
		{
			m_stagedProductID = activeProperties.GetString("OfferState_StagedProductID");
			m_stagedBonusID = activeProperties.GetString("OfferState_StagedBonusID");
			m_stagedBonusCount = activeProperties.GetInt("OfferState_StagedBonusCount");
			m_currentOffer.m_offerType = activeProperties.GetInt("OfferState_OfferType");
			m_currentOffer.m_campaignID = activeProperties.GetInt("OfferState_CampaignID");
			string @string = activeProperties.GetString("OfferState_LastDisplayTS");
			if (@string != null && @string.Length > 4)
			{
				m_currentOffer.m_lastDisplayTS = DateTime.Parse(@string);
			}
			m_currentOffer.m_displayCount = activeProperties.GetInt("OfferState_DisplayCount");
			m_currentOffer.m_closed = activeProperties.GetBool("OfferState_OfferClosed");
		}
	}
}
