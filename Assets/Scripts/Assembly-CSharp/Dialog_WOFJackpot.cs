using System.Collections;
using UnityEngine;

public class Dialog_WOFJackpot : MonoBehaviour
{
	private enum RewardType
	{
		Star,
		MultipleStars,
		MultipleRings,
		RingsAndStars,
		SinglePowerUp,
		MultiplePowerUps,
		SingleBooster,
		MultipleBooster,
		Upgrade,
		Character,
		Wallpaper
	}

	private const string JackpotTitle = "DIALOG_WHEEL_JACKPOT_PRIZE_TITLE";

	private const string JackpotReason = "DIALOG_WHEEL_JACKPOT_PRIZE_BODY";

	private const string BoosterPrefix = "booster_";

	private static StoreContent.StoreEntry s_prize = null;

	private static int s_quantity = 0;

	[SerializeField]
	private UILabel m_reasonObject;

	[SerializeField]
	private UILabel m_rewardObject;

	[SerializeField]
	private UILabel m_titleObject;

	[SerializeField]
	private MeshFilter m_meshObject;

	private static string[] RewardTypeFormats = new string[11]
	{
		"REWARD_SINGLE_STAR", "REWARD_MULTIPLE_STAR", "REWARD_MULTIPLE_RING", "REWARD_RING_AND_STAR", "REWARD_SINGLE_POWERUP", "REWARD_MULTIPLE_POWERUP", "REWARD_BOOSTER", "REWARD_BOOSTERS", "REWARD_UPGRADE", "REWARD_CHARACTER",
		"REWARD_WALLPAPER"
	};

	public static void Display(StoreContent.StoreEntry prize, int quantity)
	{
		s_prize = prize;
		s_quantity = quantity;
		DialogStack.ShowDialog("Jackpot Dialog");
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

	private void UpdateContent()
	{
		if (base.gameObject.activeInHierarchy)
		{
			m_titleObject.text = LanguageStrings.First.GetString("DIALOG_WHEEL_JACKPOT_PRIZE_TITLE");
			m_reasonObject.text = LanguageStrings.First.GetString("DIALOG_WHEEL_JACKPOT_PRIZE_BODY");
			m_rewardObject.text = GetRewardString();
			m_meshObject.mesh = s_prize.m_mesh;
		}
	}

	private string GetRewardString()
	{
		string result = null;
		if (s_prize.m_type == StoreContent.EntryType.Currency)
		{
			int num = s_prize.m_awards.m_playerRings * s_quantity;
			int num2 = s_prize.m_awards.m_playerStars * s_quantity;
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
		else if (s_prize.m_type == StoreContent.EntryType.PowerUp)
		{
			int num3 = s_prize.m_powerUpCount * s_quantity;
			if (num3 == 1)
			{
				string empty = string.Empty;
				empty = ((!s_prize.m_identifier.StartsWith("booster_")) ? GetLocalisedFormatString(RewardType.SinglePowerUp) : GetLocalisedFormatString(RewardType.SingleBooster));
				string localisedEntryName = GetLocalisedEntryName(s_prize);
				result = string.Format(empty, localisedEntryName);
			}
			else
			{
				string empty2 = string.Empty;
				empty2 = ((!s_prize.m_identifier.StartsWith("booster_")) ? GetLocalisedFormatString(RewardType.MultiplePowerUps) : GetLocalisedFormatString(RewardType.MultipleBooster));
				string localisedEntryName2 = GetLocalisedEntryName(s_prize);
				result = string.Format(empty2, num3, localisedEntryName2);
			}
		}
		else if (s_prize.m_type == StoreContent.EntryType.Upgrade)
		{
			string localisedFormatString4 = GetLocalisedFormatString(RewardType.Upgrade);
			string localisedEntryName3 = GetLocalisedEntryName(s_prize);
			result = string.Format(localisedFormatString4, localisedEntryName3);
		}
		else if (s_prize.m_type == StoreContent.EntryType.Character)
		{
			string localisedFormatString5 = GetLocalisedFormatString(RewardType.Character);
			string localisedEntryName4 = GetLocalisedEntryName(s_prize);
			result = string.Format(localisedFormatString5, localisedEntryName4);
		}
		else if (s_prize.m_type == StoreContent.EntryType.Wallpaper)
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
}
