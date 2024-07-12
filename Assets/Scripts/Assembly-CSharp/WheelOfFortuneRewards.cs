using System;
using UnityEngine;

public class WheelOfFortuneRewards : MonoBehaviour
{
	public string BoosterBundleID = string.Empty;

	[SerializeField]
	private string[] m_normalRewardIDs = new string[1];

	private int[] m_normalRewardWheelCount = new int[8];

	[SerializeField]
	private int[] m_normalRewardQuantities = new int[1];

	[SerializeField]
	private bool[] m_normalRewardIncludedInBundle = new bool[1];

	[SerializeField]
	private int[] m_normalRewardBundleWinChances = new int[1];

	[SerializeField]
	private string[] m_jackpotRewardIDs = new string[1];

	[SerializeField]
	private int[] m_jackpotRewardQuantities = new int[1];

	[SerializeField]
	private int[] m_jackpotRewardChances = new int[1];

	[SerializeField]
	private int[] m_jackpotWinChances = new int[1];

	private int m_chanceToGetJackpot;

	private int m_chanceToGetJackpotBundle;

	private int[] m_chanceToAwardPrize = new int[8];

	private bool[] m_validJackpots;

	private int m_validJackpotCount;

	private string m_cachedJackpotID;

	private int m_cachedJackpotQuantity = 1;

	private System.Random m_rand = new System.Random();

	private bool m_isBundleJackpot;

	public static WheelOfFortuneRewards Instance { get; private set; }

	public bool[] ValidJackpots => m_validJackpots;

	public string[] JackpotRewardIDs
	{
		get
		{
			return m_jackpotRewardIDs;
		}
		set
		{
			m_jackpotRewardIDs = value;
		}
	}

	public string CachedJackpotID
	{
		get
		{
			return m_cachedJackpotID;
		}
		set
		{
			m_cachedJackpotID = value;
		}
	}

	public int CachedJackpotQuantity
	{
		get
		{
			return m_cachedJackpotQuantity;
		}
		set
		{
			m_cachedJackpotQuantity = value;
		}
	}

	public bool CachedIsBundleJackpot
	{
		get
		{
			return m_isBundleJackpot;
		}
		set
		{
			m_isBundleJackpot = value;
		}
	}

	public int ChanceToGetJackpot
	{
		get
		{
			return m_chanceToGetJackpot;
		}
		set
		{
			m_chanceToGetJackpot = value;
		}
	}

	public int ChanceToGetJackpotBundle
	{
		get
		{
			return m_chanceToGetJackpotBundle;
		}
		set
		{
			m_chanceToGetJackpotBundle = value;
		}
	}

	private void Awake()
	{
		if (Instance != null && Instance != this)
		{
			UnityEngine.Object.Destroy(this);
		}
		else
		{
			Instance = this;
		}
	}

	public void ValidateJackpotRewards()
	{
		m_validJackpots = new bool[m_jackpotRewardIDs.Length];
		for (int i = 0; i < m_jackpotRewardIDs.Length; i++)
		{
			if (m_jackpotRewardIDs[i] == BoosterBundleID)
			{
				m_validJackpots[i] = true;
				m_validJackpotCount++;
				continue;
			}
			StoreContent.StoreEntry storeEntry = StoreContent.GetStoreEntry(m_jackpotRewardIDs[i], StoreContent.Identifiers.Name);
			if (storeEntry.m_group == StoreContent.Group.Characters)
			{
				if (Characters.CharacterUnlocked(storeEntry.m_character))
				{
					m_validJackpots[i] = false;
					continue;
				}
				m_validJackpots[i] = true;
				m_validJackpotCount++;
			}
			else
			{
				m_validJackpots[i] = true;
				m_validJackpotCount++;
			}
		}
	}

	public void SetRandomSeed()
	{
		m_rand = new System.Random((int)DCTime.GetCurrentTime().Date.Ticks);
	}

	public StoreContent.StoreEntry[] GetAllOtherJackpotPrizes(StoreContent.StoreEntry current)
	{
		StoreContent.StoreEntry[] array = new StoreContent.StoreEntry[m_validJackpotCount];
		int num = 0;
		int num2 = 0;
		for (int i = 0; i < m_jackpotRewardIDs.Length; i++)
		{
			if (!(StoreContent.FormatIdentifier(m_jackpotRewardIDs[i]) != current.m_identifier))
			{
				continue;
			}
			if (m_jackpotRewardIDs[i] == BoosterBundleID)
			{
				do
				{
					num2 = m_rand.Next(m_normalRewardIDs.Length);
				}
				while (!m_normalRewardIncludedInBundle[num2]);
				array[num] = StoreContent.GetStoreEntry(m_normalRewardIDs[num2], StoreContent.Identifiers.Name);
				num++;
			}
			else if (m_validJackpots[i])
			{
				array[num] = StoreContent.GetStoreEntry(m_jackpotRewardIDs[i], StoreContent.Identifiers.Name);
				num++;
			}
		}
		return array;
	}

	public int GetQuantityForNewJackpot(StoreContent.StoreEntry entry)
	{
		int num = 0;
		for (int i = 0; i < m_jackpotRewardIDs.Length; i++)
		{
			if (m_jackpotRewardIDs[i] == BoosterBundleID)
			{
				num = i;
			}
			else if (StoreContent.FormatIdentifier(m_jackpotRewardIDs[i]) == entry.m_identifier)
			{
				return m_jackpotRewardQuantities[i] * StoreUtils.GetAwardedQuantity(entry);
			}
		}
		for (int j = 0; j < m_normalRewardIDs.Length; j++)
		{
			if (StoreContent.FormatIdentifier(m_normalRewardIDs[j]) == entry.m_identifier)
			{
				return m_jackpotRewardQuantities[num] * (m_normalRewardQuantities[j] * StoreUtils.GetAwardedQuantity(entry));
			}
		}
		return 1;
	}

	public StoreContent.StoreEntry GetRandomPrize(out int quantityOut, bool isJackpotPrize)
	{
		int num = 0;
		StoreContent.StoreEntry storeEntry = null;
		int num2 = -1;
		if (isJackpotPrize)
		{
			num = m_rand.Next(100);
			for (int i = 0; i < m_jackpotRewardIDs.Length; i++)
			{
				if (m_validJackpots[i])
				{
					num2 = i;
					if (num < m_jackpotRewardChances[i])
					{
						i = m_jackpotRewardIDs.Length;
					}
					else
					{
						num -= m_jackpotRewardChances[i];
					}
				}
				else
				{
					num -= m_jackpotRewardChances[i];
				}
			}
			if (m_jackpotRewardIDs[num2] != BoosterBundleID)
			{
				m_isBundleJackpot = false;
				storeEntry = StoreContent.GetStoreEntry(m_jackpotRewardIDs[num2], StoreContent.Identifiers.Name);
				quantityOut = m_jackpotRewardQuantities[num2] * StoreUtils.GetAwardedQuantity(storeEntry);
				m_chanceToGetJackpot = m_jackpotWinChances[num2];
				m_chanceToGetJackpotBundle = 0;
				return storeEntry;
			}
			m_isBundleJackpot = true;
			num = m_rand.Next(m_normalRewardIDs.Length);
			do
			{
				num = m_rand.Next(m_normalRewardIDs.Length);
			}
			while (!m_normalRewardIncludedInBundle[num]);
			storeEntry = StoreContent.GetStoreEntry(m_normalRewardIDs[num], StoreContent.Identifiers.Name);
			quantityOut = m_normalRewardQuantities[num] * m_jackpotRewardQuantities[num2] * StoreUtils.GetAwardedQuantity(storeEntry);
			m_chanceToGetJackpotBundle = m_normalRewardBundleWinChances[num];
			m_chanceToGetJackpot = 0;
		}
		else
		{
			num = m_rand.Next(m_normalRewardIDs.Length);
			storeEntry = RegulatedPrizeSelector(out num);
			quantityOut = m_normalRewardQuantities[num] * StoreUtils.GetAwardedQuantity(storeEntry);
		}
		return storeEntry;
	}

	public int PickWinningPrize()
	{
		int num = m_chanceToAwardPrize.Length;
		int num2 = num - 2;
		int num3 = UnityEngine.Random.Range(0, 100);
		if ((num3 < m_chanceToGetJackpot && !m_isBundleJackpot) || (num3 < m_chanceToGetJackpotBundle && m_isBundleJackpot))
		{
			return 0;
		}
		num3 = UnityEngine.Random.Range(0, 100);
		int num4 = -1;
		for (int i = 0; i < m_chanceToAwardPrize.Length; i++)
		{
			if (num3 < m_chanceToAwardPrize[i])
			{
				num4 = i;
				i = m_chanceToAwardPrize.Length;
			}
			else
			{
				num3 -= m_chanceToAwardPrize[i];
			}
		}
		if (num4 == 0)
		{
			num4 = m_rand.Next(1, num);
		}
		int num5 = Mathf.CeilToInt((float)m_chanceToAwardPrize[num4] * 0.5f);
		m_chanceToAwardPrize[num4] = Mathf.FloorToInt((float)m_chanceToAwardPrize[num4] * 0.5f);
		int num6 = Mathf.FloorToInt(num5 / num2);
		int num7 = num5 - num6 * num2;
		m_chanceToAwardPrize[0] += num7;
		bool flag = false;
		if (m_chanceToAwardPrize[0] >= num - 1)
		{
			flag = true;
			m_chanceToAwardPrize[0] -= num - 1;
		}
		for (int j = 1; j < 8; j++)
		{
			if (flag)
			{
				m_chanceToAwardPrize[j]++;
			}
			if (j != num4)
			{
				m_chanceToAwardPrize[j] += num6;
			}
		}
		return num4;
	}

	public void LoadPrizeWeightings(string savedWeights)
	{
		if (savedWeights != null)
		{
			string[] array = savedWeights.Split(',');
			for (int i = 0; i < array.Length; i++)
			{
				m_chanceToAwardPrize[i] = int.Parse(array[i]);
			}
		}
		else
		{
			ResetWeightings();
		}
	}

	public string SavePrizeWeightingsAsString()
	{
		string text = string.Empty;
		for (int i = 0; i < m_chanceToAwardPrize.Length; i++)
		{
			text += m_chanceToAwardPrize[i];
			if (i != m_chanceToAwardPrize.Length - 1)
			{
				text += ",";
			}
		}
		return text;
	}

	public void ResetWeightings()
	{
		m_chanceToAwardPrize[0] = 2;
		for (int i = 1; i < m_chanceToAwardPrize.Length; i++)
		{
			m_chanceToAwardPrize[i] = 14;
		}
	}

	public void LoadJackpot(string loadedValues)
	{
		if (loadedValues != null && loadedValues != string.Empty)
		{
			string[] array = loadedValues.Split(',');
			m_cachedJackpotID = array[0];
			m_cachedJackpotQuantity = int.Parse(array[1]);
			m_isBundleJackpot = bool.Parse(array[2]);
			m_chanceToGetJackpot = int.Parse(array[3]);
			m_chanceToGetJackpotBundle = int.Parse(array[4]);
		}
	}

	public string SaveJackpot()
	{
		return string.Format(m_cachedJackpotID + ",{0},{1},{2},{3}", m_cachedJackpotQuantity, m_isBundleJackpot, m_chanceToGetJackpot, m_chanceToGetJackpotBundle);
	}

	public void ClearPrizePopulationCount()
	{
		for (int i = 0; i < m_normalRewardWheelCount.Length; i++)
		{
			m_normalRewardWheelCount[i] = 0;
		}
	}

	public void BuyingRedStarRings()
	{
		WheelofFortuneAnalytics.Instance.BoughtRedStarRing(m_cachedJackpotID, GetJackpotType());
	}

	public static WheelofFortuneAnalytics.PrizeType GetJackpotType()
	{
		if (Instance.m_isBundleJackpot)
		{
			return WheelofFortuneAnalytics.PrizeType.FakeJackpot;
		}
		return WheelofFortuneAnalytics.PrizeType.Jackpot;
	}

	private StoreContent.StoreEntry RegulatedPrizeSelector(out int prizeIndex)
	{
		do
		{
			prizeIndex = m_rand.Next(m_normalRewardIDs.Length);
		}
		while (m_normalRewardWheelCount[prizeIndex] >= 2);
		m_normalRewardWheelCount[prizeIndex]++;
		return StoreContent.GetStoreEntry(m_normalRewardIDs[prizeIndex], StoreContent.Identifiers.Name);
	}
}
