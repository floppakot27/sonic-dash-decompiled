using System.Collections.Generic;
using UnityEngine;

public class MenuRewardsDialogsPanel : MonoBehaviour
{
	private int m_currentReason;

	private int m_currentOffer;

	private int m_multipleCount;

	private int[] m_starQuantity = new int[3] { 1, 2, 14 };

	private int[] m_ringQuantity = new int[3] { 1000, 6000, 3000000 };

	private int[] m_powerUpQuanity = new int[3] { 1, 4, 10 };

	private void Start()
	{
		m_multipleCount = 0;
		m_currentReason = 0;
		m_currentOffer = 0;
	}

	private bool ValidateStoreEntry(StoreContent.StoreEntry storeEntry)
	{
		if (storeEntry.m_type == StoreContent.EntryType.Event)
		{
			return false;
		}
		if (storeEntry.m_type == StoreContent.EntryType.PowerUp && storeEntry.m_powerUpCount != 1)
		{
			return false;
		}
		if (storeEntry.m_identifier == "DoubleRings")
		{
			return false;
		}
		if (storeEntry.m_payment == StoreContent.PaymentMethod.External)
		{
			return false;
		}
		if (storeEntry.m_payment == StoreContent.PaymentMethod.Web)
		{
			return false;
		}
		if (storeEntry.m_payment == StoreContent.PaymentMethod.Free)
		{
			return false;
		}
		return true;
	}

	private int ValidateStoreEntry(int current, List<StoreContent.StoreEntry> stockList)
	{
		while (!ValidateStoreEntry(stockList[current]))
		{
			current++;
			if (current >= stockList.Count)
			{
				current = 0;
			}
		}
		return current;
	}

	private void Trigger_ShowNextReason()
	{
		DialogContent_PlayerReward.Reason currentReason = (DialogContent_PlayerReward.Reason)m_currentReason;
		StoreContent.StoreEntry storeEntry = StoreContent.GetStoreEntry("Rings 01", StoreContent.Identifiers.Name);
		DialogContent_PlayerReward.Display(currentReason, storeEntry, 1);
		m_currentReason++;
		if (m_currentReason >= Utils.GetEnumCount<DialogContent_PlayerReward.Reason>())
		{
			m_currentReason = 0;
		}
	}

	private void Trigger_ShowNextReward()
	{
		bool flag = false;
		int quantity = 1;
		List<StoreContent.StoreEntry> stockList = StoreContent.StockList;
		m_currentOffer = ValidateStoreEntry(m_currentOffer, stockList);
		StoreContent.StoreEntry storeEntry = stockList[m_currentOffer];
		if (storeEntry.m_type == StoreContent.EntryType.Currency || storeEntry.m_type == StoreContent.EntryType.PowerUp)
		{
			flag = true;
			if (storeEntry.m_type == StoreContent.EntryType.Currency)
			{
				int playerRings = storeEntry.m_awards.m_playerRings;
				int playerStars = storeEntry.m_awards.m_playerStars;
				if (playerRings > 0 && playerStars > 0)
				{
					flag = false;
				}
				if (playerRings > 1 || playerStars > 1)
				{
					flag = false;
				}
			}
			if (flag && m_multipleCount < m_starQuantity.Length)
			{
				if (storeEntry.m_type == StoreContent.EntryType.PowerUp)
				{
					quantity = m_powerUpQuanity[m_multipleCount];
				}
				else
				{
					int playerRings2 = storeEntry.m_awards.m_playerRings;
					quantity = ((playerRings2 <= 0) ? m_starQuantity[m_multipleCount] : m_ringQuantity[m_multipleCount]);
				}
				m_multipleCount++;
				if (m_multipleCount >= m_starQuantity.Length)
				{
					flag = false;
					m_multipleCount = 0;
				}
			}
		}
		DialogContent_PlayerReward.Display(DialogContent_PlayerReward.Reason.BeatHighScore, stockList[m_currentOffer], quantity);
		if (!flag)
		{
			m_currentOffer++;
			if (m_currentOffer >= stockList.Count)
			{
				m_currentOffer = 0;
			}
		}
	}
}
