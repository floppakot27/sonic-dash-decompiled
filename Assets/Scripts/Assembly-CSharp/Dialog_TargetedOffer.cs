using System.Collections;
using UnityEngine;

public class Dialog_TargetedOffer : MonoBehaviour
{
	private static OfferContent.Offer s_offer;

	private static bool s_lastChance;

	[SerializeField]
	private UILabel m_title;

	[SerializeField]
	private UILabel m_description;

	[SerializeField]
	private UILabel m_itemOffer;

	[SerializeField]
	private UILabel m_itemOfferDiscount;

	[SerializeField]
	private UILabel m_itemBonus;

	[SerializeField]
	private UILabel m_costBefore;

	[SerializeField]
	private UILabel m_costNow;

	private string m_bonusID;

	public static void Display(OfferContent.Offer offer, bool LastChance)
	{
		s_offer = offer;
		s_lastChance = LastChance;
		DialogStack.ShowDialog("Targeted Offer Dialog");
	}

	private void Start()
	{
	}

	private void OnEnable()
	{
		StartCoroutine(StartPendingActivation());
	}

	private IEnumerator StartPendingActivation()
	{
		yield return null;
		UpdateContent();
	}

	private string SelectCharacterReward()
	{
		string result = string.Empty;
		int num = 10000;
		for (int i = 1; i < Utils.GetEnumCount<Characters.Type>(); i++)
		{
			Characters.Type type = (Characters.Type)i;
			if (Characters.CharacterUnlocked(type))
			{
				continue;
			}
			StoreContent.StoreEntry storeEntry = StoreContent.GetStoreEntry(CharacterSelection.m_storeEntries[(int)type], StoreContent.Identifiers.Name);
			if ((storeEntry.m_state & StoreContent.StoreEntry.State.Hidden) == 0)
			{
				int itemCost = StoreUtils.GetItemCost(storeEntry, StoreUtils.EntryType.Player);
				if (itemCost < num)
				{
					result = CharacterSelection.m_storeEntries[(int)type];
					num = itemCost;
				}
			}
		}
		return result;
	}

	private void Update()
	{
	}

	private void UpdateContent()
	{
		string text = "TARGETED_OFFERS_SPECIAL_1";
		string text2 = string.Empty;
		string bonusID = SelectCharacterReward();
		if (s_offer.m_bonusType == OfferContent.Bonus.Character && text2.Length == 0)
		{
			s_offer.m_bonusType = OfferContent.Bonus.Revives;
		}
		switch (s_offer.m_bonusType)
		{
		case OfferContent.Bonus.Character:
			text2 = "TARGETED_OFFERS_GIFT_1";
			m_bonusID = bonusID;
			break;
		case OfferContent.Bonus.Revives:
			text2 = "TARGETED_OFFERS_GIFT_2";
			m_bonusID = "Respawn Multiple 25";
			break;
		case OfferContent.Bonus.Headstarts:
			text2 = "TARGETED_OFFERS_GIFT_3";
			m_bonusID = "Headstart Multiple";
			break;
		}
		if (s_offer.m_displayCap == 1)
		{
			text = "TARGETED_OFFERS_ONE_TIME";
		}
		else if (s_lastChance)
		{
			text = "TARGETED_OFFERS_LAST_CHANCE";
		}
		SLAnalytics.AddParameter("BaseOffer", s_offer.m_productID);
		SLAnalytics.AddParameter("Bonus", m_bonusID);
		SLAnalytics.AddParameter("Offer", s_offer.m_productID + m_bonusID);
		SLAnalytics.LogEventWithParameters("TargetOfferShown");
		string @string = LanguageStrings.First.GetString(text);
		string string2 = LanguageStrings.First.GetString(text + "_BODY");
		string string3 = LanguageStrings.First.GetString(text2);
		m_costBefore.text = string.Empty;
		m_costNow.text = string.Empty;
		string text3 = string.Format(string2, s_offer.m_percentOff, string3);
		m_title.text = @string;
		m_description.text = text3;
		StoreContent.StoreEntry storeEntry = StoreContent.GetStoreEntry(m_bonusID, StoreContent.Identifiers.Name);
		GameObject gameObject = Utils.FindTagInChildren(base.gameObject, "RewardDialog_Reward");
		if (gameObject != null)
		{
			MeshFilter[] componentsInChildren = gameObject.GetComponentsInChildren<MeshFilter>(includeInactive: true);
			if (componentsInChildren != null && componentsInChildren.Length != 0)
			{
				componentsInChildren[0].mesh = storeEntry.m_mesh;
				gameObject.SetActive(value: true);
			}
		}
		m_itemBonus.text = LanguageStrings.First.GetString(storeEntry.m_title);
		StoreContent.StoreEntry storeEntry2 = StoreContent.GetStoreEntry(s_offer.m_productID, StoreContent.Identifiers.OsStore);
		string2 = LanguageStrings.First.GetString("TARGETED_OFFERS_RED_STAR_RINGS");
		m_itemOffer.text = string.Format(string2, storeEntry2.m_awards.m_playerStars);
		string2 = LanguageStrings.First.GetString("TARGETED_OFFERS_OFF");
		m_itemOfferDiscount.text = string.Format(string2, s_offer.m_percentOff.ToString());
		StoreContent.StoreEntry storeEntry3 = StoreContent.GetStoreEntry(s_offer.m_beforeProductID, StoreContent.Identifiers.OsStore);
		if (storeEntry3.m_osStore.m_playerState == ProductStateCode.ProductInfoDone)
		{
			string productCost = SLStorePlugin.GetProductCost(s_offer.m_beforeProductID);
			string string4 = LanguageStrings.First.GetString("STORE_SALE_PRICE_WAS");
			m_costBefore.text = string.Format(string4, productCost);
		}
		StoreContent.StoreEntry storeEntry4 = StoreContent.GetStoreEntry(s_offer.m_productID, StoreContent.Identifiers.OsStore);
		if (storeEntry4.m_osStore.m_playerState == ProductStateCode.ProductInfoDone)
		{
			string productCost2 = SLStorePlugin.GetProductCost(s_offer.m_productID);
			string string5 = LanguageStrings.First.GetString("STORE_SALE_PRICE_NOW");
			m_costNow.text = string.Format(string5, productCost2);
		}
	}

	private void Trigger_CompleteIAP()
	{
		OfferState.StagePurchase(s_offer.m_productID, m_bonusID, s_offer.m_bonusCount);
		SLStorePlugin.RequestPaymentDirect(s_offer.m_productID, 1);
	}

	private void Trigger_CancelIAP()
	{
	}
}
