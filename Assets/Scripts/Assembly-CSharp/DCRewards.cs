using System;
using System.Collections.Generic;
using UnityEngine;

public class DCRewards : MonoBehaviour
{
	public const int FinalDayRewardCount = 5;

	private static DCRewards s_rewards;

	[SerializeField]
	private string[] m_dailyRewardIDs = new string[4];

	[SerializeField]
	private int[] m_dailyRewardQuantity = new int[4];

	[SerializeField]
	private string[] m_finalRewardID = new string[5];

	[SerializeField]
	private int[] m_finalRewardQuantity = new int[5];

	[SerializeField]
	private Characters.Type[] m_characterOrder = new Characters.Type[Enum.GetNames(typeof(Characters.Type)).Length];

	[SerializeField]
	private float[] m_finalRewardChance = new float[5];

	public static StoreContent.StoreEntry GetDailyReward(int index, out int quantity, bool getFinalQuantity)
	{
		StoreContent.StoreEntry storeEntry = StoreContent.GetStoreEntry(s_rewards.m_dailyRewardIDs[index], StoreContent.Identifiers.Name);
		quantity = s_rewards.m_dailyRewardQuantity[index];
		if (getFinalQuantity)
		{
			quantity *= StoreUtils.GetAwardedQuantity(storeEntry);
		}
		return storeEntry;
	}

	public static StoreContent.StoreEntry GetFinalDayReward(int index, out int quantity, bool getFinalQuantity)
	{
		StoreContent.StoreEntry storeEntry = null;
		if (index >= 4)
		{
			storeEntry = s_rewards.GetLastRewardOnFinalDay(out quantity);
		}
		else
		{
			storeEntry = StoreContent.GetStoreEntry(s_rewards.m_finalRewardID[index], StoreContent.Identifiers.Name);
			quantity = s_rewards.m_finalRewardQuantity[index];
			if (getFinalQuantity)
			{
				quantity *= StoreUtils.GetAwardedQuantity(storeEntry);
			}
		}
		return storeEntry;
	}

	public static StoreContent.StoreEntry GetFinalDayReward(out int quantity, bool getFinalQuantity)
	{
		int num = 0;
		float value = UnityEngine.Random.value;
		int num2 = 0;
		while (num2 < s_rewards.m_finalRewardChance.Length - 1 && (!(value >= s_rewards.m_finalRewardChance[num2]) || !(value < s_rewards.m_finalRewardChance[num2 + 1])))
		{
			num2++;
			num++;
		}
		return GetFinalDayReward(num, out quantity, getFinalQuantity);
	}

	private void Start()
	{
		s_rewards = this;
		SetFinalRewardChanceRange();
	}

	private StoreContent.StoreEntry GetLastRewardOnFinalDay(out int quantity)
	{
		StoreContent.StoreEntry storeEntry = null;
		Characters.Type type = Characters.Type.Sonic;
		for (int i = 0; i < m_characterOrder.Length; i++)
		{
			Characters.Type type2 = m_characterOrder[i];
			if (type2 != 0 && !Characters.CharacterUnlocked(type2))
			{
				type = type2;
				break;
			}
		}
		if (type != 0)
		{
			storeEntry = GetCharacterStoreEntry(type);
			quantity = 1;
		}
		else
		{
			storeEntry = StoreContent.GetStoreEntry(m_finalRewardID[4], StoreContent.Identifiers.Name);
			quantity = m_finalRewardQuantity[4] * StoreUtils.GetAwardedQuantity(storeEntry);
		}
		return storeEntry;
	}

	private StoreContent.StoreEntry GetCharacterStoreEntry(Characters.Type character)
	{
		List<StoreContent.StoreEntry> stockList = StoreContent.StockList;
		for (int i = 0; i < stockList.Count; i++)
		{
			StoreContent.StoreEntry storeEntry = stockList[i];
			if (storeEntry.m_type == StoreContent.EntryType.Character && storeEntry.m_character == character)
			{
				return storeEntry;
			}
		}
		return null;
	}

	private void SetFinalRewardChanceRange()
	{
		float num = 0f;
		for (int i = 0; i < m_finalRewardChance.Length; i++)
		{
			num += m_finalRewardChance[i];
		}
		float num2 = 1f / num;
		for (int j = 0; j < m_finalRewardChance.Length; j++)
		{
			m_finalRewardChance[j] *= num2;
		}
		for (int num3 = m_finalRewardChance.Length - 1; num3 >= 0; num3--)
		{
			if (num3 == m_finalRewardChance.Length - 1)
			{
				m_finalRewardChance[num3] = 1f - m_finalRewardChance[num3];
			}
			else
			{
				m_finalRewardChance[num3] = m_finalRewardChance[num3 + 1] - m_finalRewardChance[num3];
				if (m_finalRewardChance[num3] < 0.0001f)
				{
					m_finalRewardChance[num3] = 0f;
				}
			}
		}
	}
}
