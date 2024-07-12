using System.Collections.Generic;

public class StoreUtils
{
	public enum EntryType
	{
		Base,
		Player
	}

	private static string SaveProperty_OneShots = "One Shot Purchases";

	private static string SaveProperty_OneShotsPowerUpTag = "Purchased";

	public static bool IsStoreActive()
	{
		bool flag = StorePurchases.IsPurchaseInProgress();
		bool flag2 = StoreContent.StoreInitialised();
		bool flag3 = StoreContent.ProcessingStoreEntries();
		bool flag4 = StoreModifier.ModificationsComplete();
		bool flag5 = StorePurchases.IsRestoreInProgress();
		return flag || flag3 || flag5 || !flag2 || !flag4;
	}

	public static bool IsOsStoreIdValid(StoreContent.StoreEntry entry)
	{
		string osStoreId = GetOsStoreId(entry, EntryType.Base);
		string osStoreId2 = GetOsStoreId(entry, EntryType.Player);
		if (osStoreId == null || osStoreId2 == null)
		{
			return false;
		}
		if (osStoreId.Length == 0 || osStoreId2.Length == 0)
		{
			return false;
		}
		return true;
	}

	public static string GetOsStoreId(StoreContent.StoreEntry entry, EntryType entryType)
	{
		return ((entryType != EntryType.Player) ? entry.m_osStore.m_baseiTunesId : entry.m_osStore.m_playeriTunesId).ToLower();
	}

	public static bool IsStorePurchaseRequired(StoreContent.StoreEntry storeOffer)
	{
		bool result = true;
		if (storeOffer.m_type == StoreContent.EntryType.Character)
		{
			Characters.Type character = storeOffer.m_character;
			if (Characters.CharacterUnlocked(character))
			{
				result = false;
			}
		}
		else if (storeOffer.m_type == StoreContent.EntryType.PowerUp)
		{
			PowerUps.Type powerUpType = storeOffer.m_powerUpType;
			if (powerUpType == PowerUps.Type.DoubleRing)
			{
				result = PowerUpsInventory.GetPowerUpCount(PowerUps.Type.DoubleRing) == 0;
			}
			if (powerUpType == PowerUps.Type.FreeRevive)
			{
				result = PowerUpsInventory.GetPowerUpCount(PowerUps.Type.FreeRevive) == 0;
			}
		}
		return result;
	}

	public static int GetItemCost(StoreContent.StoreEntry storeOffer, EntryType entryType)
	{
		bool flag = storeOffer.m_payment == StoreContent.PaymentMethod.Rings || storeOffer.m_payment == StoreContent.PaymentMethod.StarRings;
		if (storeOffer.m_type != StoreContent.EntryType.Upgrade || storeOffer.m_upgradeToMax)
		{
			return (entryType != EntryType.Player) ? storeOffer.m_cost.m_baseCost[0] : storeOffer.m_cost.m_playerCost[0];
		}
		int powerUpLevel = PowerUpsInventory.GetPowerUpLevel(storeOffer.m_powerUpType);
		if (powerUpLevel >= 6)
		{
			return 0;
		}
		return (entryType != EntryType.Player) ? storeOffer.m_cost.m_baseCost[powerUpLevel + 1] : storeOffer.m_cost.m_playerCost[powerUpLevel + 1];
	}

	public static int GetMaximumRingPurchase(StoreContent.PaymentMethod paymentMethod)
	{
		if (paymentMethod != 0 && paymentMethod != StoreContent.PaymentMethod.StarRings)
		{
			return 0;
		}
		int num = 0;
		List<StoreContent.StoreEntry> stockList = StoreContent.StockList;
		foreach (StoreContent.StoreEntry item in stockList)
		{
			if (item.m_type == StoreContent.EntryType.Currency && (item.m_state & StoreContent.StoreEntry.State.Hidden) != StoreContent.StoreEntry.State.Hidden && (item.m_state & StoreContent.StoreEntry.State.Purchased) != StoreContent.StoreEntry.State.Purchased)
			{
				int num2 = item.m_awards.m_playerRings;
				if (paymentMethod == StoreContent.PaymentMethod.StarRings)
				{
					num2 = item.m_awards.m_playerStars;
				}
				if (num2 > num)
				{
					num = num2;
				}
			}
		}
		return num;
	}

	public static int GetAffordableItemCount(StoreContent.EntryType? entryType)
	{
		int num = 0;
		List<StoreContent.StoreEntry> list = StoreContent.StockList;
		if (entryType.HasValue)
		{
			list = list.FindAll((StoreContent.StoreEntry entry) => entry.m_type == entryType.GetValueOrDefault() && entryType.HasValue);
		}
		for (int i = 0; i < list.Count; i++)
		{
			StoreContent.StoreEntry storeEntry = list[i];
			if ((storeEntry.m_state & StoreContent.StoreEntry.State.Hidden) != StoreContent.StoreEntry.State.Hidden && (storeEntry.m_state & StoreContent.StoreEntry.State.Purchased) != StoreContent.StoreEntry.State.Purchased && (storeEntry.m_payment == StoreContent.PaymentMethod.Rings || storeEntry.m_payment == StoreContent.PaymentMethod.StarRings) && storeEntry.m_type != StoreContent.EntryType.Event)
			{
				int itemCost = GetItemCost(storeEntry, EntryType.Player);
				if (storeEntry.m_payment == StoreContent.PaymentMethod.Rings && RingStorage.TotalBankedRings >= itemCost)
				{
					num++;
				}
				else if (storeEntry.m_payment == StoreContent.PaymentMethod.StarRings && RingStorage.TotalStarRings >= itemCost)
				{
					num++;
				}
			}
		}
		return num;
	}

	public static void SaveOneShotPurchase(StoreContent.StoreEntry storeOffer)
	{
		if ((storeOffer.m_state & StoreContent.StoreEntry.State.OneShot) != StoreContent.StoreEntry.State.OneShot)
		{
			return;
		}
		storeOffer.m_state |= StoreContent.StoreEntry.State.Purchased;
		ActiveProperties activeProperties = PropertyStore.ActiveProperties();
		string text = activeProperties.GetString(SaveProperty_OneShots);
		string text2 = storeOffer.m_identifier;
		if (storeOffer.m_type == StoreContent.EntryType.PowerUp || storeOffer.m_type == StoreContent.EntryType.Upgrade)
		{
			text2 = SaveProperty_OneShotsPowerUpTag + storeOffer.m_powerUpType;
		}
		if (text == null || text.Length == 0)
		{
			text = text2;
		}
		else
		{
			string[] purchaseList = text.Split(',');
			if (!FindOneShotPurchaseInList(text2, purchaseList))
			{
				text = $"{text},{text2}";
			}
		}
		PropertyStore.Store(SaveProperty_OneShots, text);
	}

	public static bool OneShotItemPurchased(StoreContent.StoreEntry storeOffer, ActiveProperties currentProperties)
	{
		if ((storeOffer.m_state & StoreContent.StoreEntry.State.OneShot) != StoreContent.StoreEntry.State.OneShot)
		{
			return false;
		}
		if (currentProperties == null)
		{
			currentProperties = PropertyStore.ActiveProperties();
		}
		string @string = currentProperties.GetString(SaveProperty_OneShots);
		if (@string == null)
		{
			return false;
		}
		string itemName = storeOffer.m_identifier;
		if (storeOffer.m_type == StoreContent.EntryType.PowerUp || storeOffer.m_type == StoreContent.EntryType.Upgrade)
		{
			itemName = SaveProperty_OneShotsPowerUpTag + storeOffer.m_powerUpType;
		}
		string[] purchaseList = @string.Split(',');
		bool flag = FindOneShotPurchaseInList(itemName, purchaseList);
		if (flag)
		{
			storeOffer.m_state |= StoreContent.StoreEntry.State.Purchased;
			storeOffer.m_payment = StoreContent.PaymentMethod.Free;
		}
		return flag;
	}

	public static int GetAwardedQuantity(StoreContent.StoreEntry storeOffer)
	{
		int result = 0;
		if (storeOffer.m_type == StoreContent.EntryType.Currency)
		{
			int playerRings = storeOffer.m_awards.m_playerRings;
			int playerStars = storeOffer.m_awards.m_playerStars;
			if (playerRings > 0)
			{
				result = playerRings;
			}
			if (playerStars > 0)
			{
				result = playerStars;
			}
		}
		else if (storeOffer.m_type == StoreContent.EntryType.PowerUp)
		{
			result = storeOffer.m_powerUpCount;
		}
		else if (storeOffer.m_type == StoreContent.EntryType.Character)
		{
			result = 1;
		}
		else if (storeOffer.m_type == StoreContent.EntryType.Wallpaper)
		{
			result = 1;
		}
		return result;
	}

	private static bool FindOneShotPurchaseInList(string itemName, string[] purchaseList)
	{
		if (purchaseList == null || purchaseList.Length == 0)
		{
			return false;
		}
		for (int i = 0; i < purchaseList.Length; i++)
		{
			if (itemName == purchaseList[i])
			{
				return true;
			}
		}
		return false;
	}
}
