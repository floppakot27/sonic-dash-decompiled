using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dialog_PlayerReward : MonoBehaviour
{
	private class Content
	{
		public bool m_active;

		public string m_reasonTitle;

		public string m_reasonBody;

		public StoreContent.StoreEntry m_entry;

		public int m_quantity;
	}

	private enum RewardType
	{
		Star,
		MultipleStars,
		MultipleRings,
		RingsAndStars,
		SinglePowerUp,
		MultiplePowerUps,
		Upgrade,
		Character,
		Wallpaper
	}

	private const int ContentPoolSize = 10;

	private static string[] RewardTypeFormats = new string[9] { "REWARD_SINGLE_STAR", "REWARD_MULTIPLE_STAR", "REWARD_MULTIPLE_RING", "REWARD_RING_AND_STAR", "REWARD_SINGLE_POWERUP", "REWARD_MULTIPLE_POWERUP", "REWARD_UPGRADE", "REWARD_CHARACTER", "REWARD_WALLPAPER" };

	[SerializeField]
	private UILabel m_reasonObject;

	[SerializeField]
	private UILabel m_rewardObject;

	[SerializeField]
	private UILabel m_titleObject;

	[SerializeField]
	private GameObject m_meshObject;

	private Content[] m_contentPool;

	private Stack<Content> m_stackedContent = new Stack<Content>(10);

	public void SetContent(DialogContent_PlayerReward.Reason rewardReason, StoreContent.StoreEntry storeEntry, int quantity)
	{
		Content freeContent = GetFreeContent();
		DialogContent_PlayerReward.Content content = DialogContent_PlayerReward.GetContent(rewardReason);
		freeContent.m_reasonTitle = content.m_titleLoc;
		freeContent.m_reasonBody = content.m_infoLoc;
		freeContent.m_entry = storeEntry;
		freeContent.m_quantity = quantity;
		freeContent.m_active = true;
		m_stackedContent.Push(freeContent);
	}

	private void OnEnable()
	{
		StartCoroutine(StartPendingActivation());
	}

	private void UpdateContent()
	{
		if (base.gameObject.activeInHierarchy)
		{
			Content content = m_stackedContent.Peek();
			UpdateReasonString(content);
			UpdateRewardString(content);
			UpdateTitleString(content);
			UpdateDialogMesh(content.m_entry);
		}
	}

	private void UpdateReasonString(Content thisReward)
	{
		if (thisReward.m_reasonBody == null || thisReward.m_reasonBody.Length == 0)
		{
			m_reasonObject.gameObject.SetActive(value: false);
			return;
		}
		m_reasonObject.gameObject.SetActive(value: true);
		LocalisedStringProperties localisedStringProperties = m_reasonObject.GetComponentInChildren(typeof(LocalisedStringProperties)) as LocalisedStringProperties;
		localisedStringProperties.SetLocalisationID(thisReward.m_reasonBody);
		LocalisedStringStatic component = m_reasonObject.GetComponent<LocalisedStringStatic>();
		component.UpdateGuiText();
	}

	private void UpdateRewardString(Content thisReward)
	{
		string rewardString = GetRewardString(thisReward);
		m_rewardObject.text = rewardString;
	}

	private void UpdateTitleString(Content thisReward)
	{
		LocalisedStringProperties localisedStringProperties = m_titleObject.GetComponentInChildren(typeof(LocalisedStringProperties)) as LocalisedStringProperties;
		localisedStringProperties.SetLocalisationID(thisReward.m_reasonTitle);
		LocalisedStringStatic component = m_titleObject.GetComponent<LocalisedStringStatic>();
		component.UpdateGuiText();
	}

	private string GetRewardString(Content thisReward)
	{
		string result = null;
		if (thisReward.m_entry.m_type == StoreContent.EntryType.Currency)
		{
			int num = thisReward.m_entry.m_awards.m_playerRings * thisReward.m_quantity;
			int num2 = thisReward.m_entry.m_awards.m_playerStars * thisReward.m_quantity;
			string arg = LanguageUtils.FormatNumber(num);
			string text = LanguageUtils.FormatNumber(num2);
			if (num > 0 && num2 > 0)
			{
				string localisedFormatString = GetLocalisedFormatString(RewardType.RingsAndStars);
				result = string.Format(localisedFormatString, arg, text);
			}
			else if (num > 0)
			{
				string localisedFormatString2 = GetLocalisedFormatString(RewardType.MultipleRings);
				result = string.Format(localisedFormatString2, arg);
			}
			else if (num2 == 1)
			{
				result = GetLocalisedFormatString(RewardType.Star);
			}
			else
			{
				string localisedFormatString3 = GetLocalisedFormatString(RewardType.MultipleStars);
				result = string.Format(localisedFormatString3, text);
			}
		}
		else if (thisReward.m_entry.m_type == StoreContent.EntryType.PowerUp)
		{
			int num3 = thisReward.m_entry.m_powerUpCount * thisReward.m_quantity;
			if (num3 == 1)
			{
				string localisedFormatString4 = GetLocalisedFormatString(RewardType.SinglePowerUp);
				string localisedEntryName = GetLocalisedEntryName(thisReward.m_entry);
				result = string.Format(localisedFormatString4, localisedEntryName);
			}
			else
			{
				string localisedFormatString5 = GetLocalisedFormatString(RewardType.MultiplePowerUps);
				string localisedEntryName2 = GetLocalisedEntryName(thisReward.m_entry);
				result = string.Format(localisedFormatString5, num3, localisedEntryName2);
			}
		}
		else if (thisReward.m_entry.m_type == StoreContent.EntryType.Upgrade)
		{
			string localisedFormatString6 = GetLocalisedFormatString(RewardType.Upgrade);
			string localisedEntryName3 = GetLocalisedEntryName(thisReward.m_entry);
			result = string.Format(localisedFormatString6, localisedEntryName3);
		}
		else if (thisReward.m_entry.m_type == StoreContent.EntryType.Character)
		{
			string localisedFormatString7 = GetLocalisedFormatString(RewardType.Character);
			string localisedEntryName4 = GetLocalisedEntryName(thisReward.m_entry);
			result = string.Format(localisedFormatString7, localisedEntryName4);
		}
		else if (thisReward.m_entry.m_type == StoreContent.EntryType.Wallpaper)
		{
			result = GetLocalisedFormatString(RewardType.Wallpaper);
		}
		return result;
	}

	private string GetLocalisedFormatString(RewardType rewardType)
	{
		string id = RewardTypeFormats[(int)rewardType];
		return LanguageStrings.First.GetString(id);
	}

	private string GetLocalisedEntryName(StoreContent.StoreEntry storeEntry)
	{
		return LanguageStrings.First.GetString(storeEntry.m_title);
	}

	private Content GetFreeContent()
	{
		if (m_contentPool == null)
		{
			InitialiseContentPool();
		}
		for (int i = 0; i < m_contentPool.Length; i++)
		{
			if (!m_contentPool[i].m_active)
			{
				return m_contentPool[i];
			}
		}
		return null;
	}

	private void InitialiseContentPool()
	{
		m_contentPool = new Content[10];
		for (int i = 0; i < m_contentPool.Length; i++)
		{
			m_contentPool[i] = new Content();
		}
	}

	private void UpdateDialogMesh(StoreContent.StoreEntry storeEntry)
	{
		MeshFilter componentInChildren = m_meshObject.GetComponentInChildren<MeshFilter>();
		componentInChildren.sharedMesh = storeEntry.m_mesh;
	}

	private IEnumerator StartPendingActivation()
	{
		yield return null;
		UpdateContent();
	}

	private void DialogPopped()
	{
		Content content = m_stackedContent.Pop();
		content.m_active = false;
	}
}
