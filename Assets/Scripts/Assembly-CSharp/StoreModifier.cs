using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class StoreModifier : MonoBehaviour
{
	[Flags]
	private enum State
	{
		Ready = 1,
		Error = 2,
		ReadyToModify = 4,
		ItemsModified = 8
	}

	private enum ModificationType
	{
		Global,
		Specific
	}

	private struct Modifier
	{
		[Flags]
		public enum Valid
		{
			Name = 1,
			Hidden = 2,
			OnSale = 4,
			RingAwardedModifier = 8,
			StarAwardedModifier = 0x10,
			CostModifier = 0x20,
			RingsToAward = 0x40,
			StarsToAward = 0x80,
			AmountToAward = 0x100,
			Cost = 0x200,
			SellText = 0x400,
			iTunesID = 0x800,
			RemoveAds = 0x1000
		}

		public Valid m_validity;

		public uint m_entryNameCRC;

		public bool m_hidden;

		public bool m_onSale;

		public bool m_removeAds;

		public float m_ringAwardedModifier;

		public float m_starAwardedModifier;

		public float m_costModifier;

		public int m_ringsToAward;

		public int m_starsToAward;

		public int m_amountToAdd;

		public int[] m_cost;

		public string m_sellText;

		public string m_iTunesID;
	}

	private const FileDownloader.Files FileLocation = FileDownloader.Files.StoreModifier;

	private const int InvalidModifier = -1;

	private static StoreModifier s_modifier;

	private State m_state;

	private Modifier[] m_modifiers;

	private Modifier m_globalModifier;

	public static bool Ready => (s_modifier.m_state & State.Ready) == State.Ready || (s_modifier.m_state & State.Error) == State.Error;

	public static bool URLReady { get; set; }

	public static bool ModificationsComplete()
	{
		bool flag = (s_modifier.m_state & State.ItemsModified) == State.ItemsModified;
		bool flag2 = (s_modifier.m_state & State.Error) == State.Error;
		return flag || flag2;
	}

	public static void Restart()
	{
		s_modifier.StartCoroutine(s_modifier.DownloadServerFile());
	}

	private void Start()
	{
		s_modifier = this;
		EventDispatch.RegisterInterest("OnStoreInitialised", this, EventDispatch.Priority.Highest);
	}

	private void ModifyStockList()
	{
		bool flag = false;
		m_state &= ~State.ReadyToModify;
		List<StoreContent.StoreEntry> stockList = StoreContent.StockList;
		foreach (StoreContent.StoreEntry item in stockList)
		{
			flag |= ModifyStoreEntry(item);
		}
		if (flag)
		{
			EventDispatch.GenerateEvent("RequestStoreRefresh");
		}
		m_state |= State.ItemsModified;
	}

	private bool ModifyStoreEntry(StoreContent.StoreEntry entry)
	{
		bool flag = false;
		bool flag2 = false;
		if (m_modifiers != null)
		{
			uint storeEntryCRC = CRC32.Generate(entry.m_identifier, CRC32.Case.Lower);
			int num = FindModifier(storeEntryCRC);
			if (num != -1)
			{
				flag2 |= UpdateStoreEntry(entry, ref m_modifiers[num], ModificationType.Specific);
				flag = true;
			}
		}
		if (!flag)
		{
			flag2 |= UpdateStoreEntry(entry, ref m_globalModifier, ModificationType.Global);
		}
		return flag2;
	}

	private bool UpdateStoreEntry(StoreContent.StoreEntry entry, ref Modifier modifier, ModificationType modificationType)
	{
		bool result = false;
		if (modificationType == ModificationType.Global)
		{
			if ((modifier.m_validity & Modifier.Valid.RingAwardedModifier) == Modifier.Valid.RingAwardedModifier)
			{
				entry.m_awards.m_playerRings = Mathf.CeilToInt((float)entry.m_awards.m_playerRings * modifier.m_ringAwardedModifier);
			}
			if ((modifier.m_validity & Modifier.Valid.StarAwardedModifier) == Modifier.Valid.StarAwardedModifier)
			{
				entry.m_awards.m_playerStars = Mathf.CeilToInt((float)entry.m_awards.m_playerStars * modifier.m_starAwardedModifier);
			}
			if ((modifier.m_validity & Modifier.Valid.CostModifier) == Modifier.Valid.CostModifier)
			{
				for (int i = 0; i < entry.m_cost.m_playerCost.Length; i++)
				{
					entry.m_cost.m_playerCost[i] = Mathf.CeilToInt((float)entry.m_cost.m_playerCost[i] * modifier.m_costModifier);
				}
			}
		}
		else
		{
			bool flag = false;
			if ((modifier.m_validity & Modifier.Valid.OnSale) == Modifier.Valid.OnSale)
			{
				if (modifier.m_onSale)
				{
					flag = true;
					entry.m_state |= StoreContent.StoreEntry.State.OnSale;
				}
				else
				{
					flag = false;
					entry.m_state &= ~StoreContent.StoreEntry.State.OnSale;
				}
			}
			if ((modifier.m_validity & Modifier.Valid.Hidden) == Modifier.Valid.Hidden)
			{
				if (modifier.m_hidden)
				{
					entry.m_state |= StoreContent.StoreEntry.State.Hidden;
				}
				else
				{
					entry.m_state &= ~StoreContent.StoreEntry.State.Hidden;
				}
			}
			if ((modifier.m_validity & Modifier.Valid.SellText) == Modifier.Valid.SellText)
			{
				entry.m_offerSellText = modifier.m_sellText;
			}
			if ((modifier.m_validity & Modifier.Valid.RemoveAds) == Modifier.Valid.RemoveAds)
			{
				if (modifier.m_removeAds)
				{
					entry.m_state |= StoreContent.StoreEntry.State.RemoveAds;
				}
				else
				{
					entry.m_state &= ~StoreContent.StoreEntry.State.RemoveAds;
				}
			}
			if ((modifier.m_validity & Modifier.Valid.RingsToAward) == Modifier.Valid.RingsToAward)
			{
				entry.m_awards.m_playerRings = modifier.m_ringsToAward;
				if (!flag)
				{
					entry.m_awards.m_baseRings = modifier.m_ringsToAward;
				}
			}
			if ((modifier.m_validity & Modifier.Valid.StarsToAward) == Modifier.Valid.StarsToAward)
			{
				entry.m_awards.m_playerStars = modifier.m_starsToAward;
				if (!flag)
				{
					entry.m_awards.m_baseStars = modifier.m_starsToAward;
				}
			}
			if ((modifier.m_validity & Modifier.Valid.AmountToAward) == Modifier.Valid.AmountToAward)
			{
				entry.m_powerUpCount = modifier.m_amountToAdd;
				if (!flag)
				{
					entry.m_powerUpCount = modifier.m_amountToAdd;
				}
			}
			if ((modifier.m_validity & Modifier.Valid.Cost) == Modifier.Valid.Cost)
			{
				for (int j = 0; j < entry.m_cost.m_playerCost.Length; j++)
				{
					entry.m_cost.m_playerCost[j] = modifier.m_cost[j];
					if (!flag)
					{
						entry.m_cost.m_baseCost[j] = modifier.m_cost[j];
					}
				}
			}
			if ((modifier.m_validity & Modifier.Valid.iTunesID) == Modifier.Valid.iTunesID)
			{
				entry.m_osStore.m_playeriTunesId = modifier.m_iTunesID;
				if (!flag)
				{
					entry.m_osStore.m_baseiTunesId = modifier.m_iTunesID;
				}
				result = true;
			}
		}
		return result;
	}

	private int FindModifier(uint storeEntryCRC)
	{
		for (int i = 0; i < m_modifiers.Length; i++)
		{
			if ((m_modifiers[i].m_validity & Modifier.Valid.Name) == Modifier.Valid.Name && m_modifiers[i].m_entryNameCRC == storeEntryCRC)
			{
				return i;
			}
		}
		return -1;
	}

	private void CacheStoreModifiers(LSON.Root[] lsonRoot)
	{
		if (lsonRoot != null)
		{
			int modifierCount = GetModifierCount(lsonRoot, null);
			if (modifierCount > 0)
			{
				m_modifiers = new Modifier[modifierCount];
				GetModifierCount(lsonRoot, m_modifiers);
			}
			LSON.Root root2 = lsonRoot.FirstOrDefault((LSON.Root root) => root.m_name == "global settings");
			if (root2 != null)
			{
				PopulateModifier(root2, ref m_globalModifier);
			}
		}
	}

	private int GetModifierCount(LSON.Root[] lsonRoot, Modifier[] modifiers)
	{
		int num = 0;
		foreach (LSON.Root root in lsonRoot)
		{
			if (root.m_name != null && root.m_properties != null && root.m_name == "store entry")
			{
				if (modifiers != null)
				{
					PopulateModifier(root, ref modifiers[num]);
				}
				num++;
			}
		}
		return num;
	}

	private void PopulateModifier(LSON.Root root, ref Modifier modifier)
	{
		modifier.m_validity = (Modifier.Valid)0;
		if (root.m_properties == null)
		{
			return;
		}
		LSON.Property[] properties = root.m_properties;
		foreach (LSON.Property property in properties)
		{
			if (property.m_name == null)
			{
				continue;
			}
			switch (property.m_name.ToLower())
			{
			case "name":
			{
				string stringValue = null;
				if (LSONProperties.AsString(property, out stringValue))
				{
					stringValue = StoreContent.FormatIdentifier(stringValue);
					modifier.m_entryNameCRC = CRC32.Generate(stringValue, CRC32.Case.AsIs);
					modifier.m_validity |= Modifier.Valid.Name;
				}
				break;
			}
			case "hidden":
				if (LSONProperties.AsBool(property, out modifier.m_hidden))
				{
					modifier.m_validity |= Modifier.Valid.Hidden;
				}
				break;
			case "on sale":
				if (LSONProperties.AsBool(property, out modifier.m_onSale))
				{
					modifier.m_validity |= Modifier.Valid.OnSale;
				}
				break;
			case "remove ads":
				if (LSONProperties.AsBool(property, out modifier.m_removeAds))
				{
					modifier.m_validity |= Modifier.Valid.RemoveAds;
				}
				break;
			case "cost modifier":
				if (LSONProperties.AsFloat(property, out modifier.m_costModifier))
				{
					modifier.m_validity |= Modifier.Valid.CostModifier;
				}
				break;
			case "rings to award modifier":
				if (LSONProperties.AsFloat(property, out modifier.m_ringAwardedModifier))
				{
					modifier.m_validity |= Modifier.Valid.RingAwardedModifier;
				}
				break;
			case "stars to award modifier":
				if (LSONProperties.AsFloat(property, out modifier.m_starAwardedModifier))
				{
					modifier.m_validity |= Modifier.Valid.StarAwardedModifier;
				}
				break;
			case "rings to award":
				if (LSONProperties.AsInt(property, out modifier.m_ringsToAward))
				{
					modifier.m_validity |= Modifier.Valid.RingsToAward;
				}
				break;
			case "stars to award":
				if (LSONProperties.AsInt(property, out modifier.m_starsToAward))
				{
					modifier.m_validity |= Modifier.Valid.StarsToAward;
				}
				break;
			case "amount to add":
				if (LSONProperties.AsInt(property, out modifier.m_amountToAdd))
				{
					modifier.m_validity |= Modifier.Valid.AmountToAward;
				}
				break;
			case "cost":
				ValidateCostArray(ref modifier);
				if (LSONProperties.AsInt(property, out modifier.m_cost[0]))
				{
					modifier.m_validity |= Modifier.Valid.Cost;
				}
				break;
			case "cost 01":
				ValidateCostArray(ref modifier);
				if (LSONProperties.AsInt(property, out modifier.m_cost[1]))
				{
					modifier.m_validity |= Modifier.Valid.Cost;
				}
				break;
			case "cost 02":
				ValidateCostArray(ref modifier);
				if (LSONProperties.AsInt(property, out modifier.m_cost[2]))
				{
					modifier.m_validity |= Modifier.Valid.Cost;
				}
				break;
			case "cost 03":
				ValidateCostArray(ref modifier);
				if (LSONProperties.AsInt(property, out modifier.m_cost[3]))
				{
					modifier.m_validity |= Modifier.Valid.Cost;
				}
				break;
			case "cost 04":
				ValidateCostArray(ref modifier);
				if (LSONProperties.AsInt(property, out modifier.m_cost[4]))
				{
					modifier.m_validity |= Modifier.Valid.Cost;
				}
				break;
			case "cost 05":
				ValidateCostArray(ref modifier);
				if (LSONProperties.AsInt(property, out modifier.m_cost[5]))
				{
					modifier.m_validity |= Modifier.Valid.Cost;
				}
				break;
			case "cost 06":
				ValidateCostArray(ref modifier);
				if (LSONProperties.AsInt(property, out modifier.m_cost[6]))
				{
					modifier.m_validity |= Modifier.Valid.Cost;
				}
				break;
			case "offer sell text":
				if (LSONProperties.AsString(property, out modifier.m_sellText))
				{
					modifier.m_validity |= Modifier.Valid.SellText;
				}
				break;
			case "itunesid":
				if (LSONProperties.AsString(property, out modifier.m_iTunesID))
				{
					modifier.m_validity |= Modifier.Valid.iTunesID;
				}
				break;
			}
		}
	}

	private void ValidateCostArray(ref Modifier modifier)
	{
		if (modifier.m_cost == null)
		{
			modifier.m_cost = new int[7];
		}
	}

	private IEnumerator DownloadServerFile()
	{
		while (!URLReady)
		{
			yield return null;
		}
		FileDownloader fDownloader = new FileDownloader(FileDownloader.Files.StoreModifier, keepAndUseLocalCopy: true);
		yield return fDownloader.Loading;
		if (fDownloader.Error == null)
		{
			LSON.Root[] lsonRoot = LSONReader.Parse(fDownloader.Text);
			if (lsonRoot != null)
			{
				CacheStoreModifiers(lsonRoot);
			}
			m_state |= State.Ready;
			if ((m_state & State.ReadyToModify) == State.ReadyToModify)
			{
				ModifyStockList();
			}
		}
		else
		{
			m_state |= State.Error;
		}
	}

	private void Event_OnStoreInitialised()
	{
		if ((m_state & State.Ready) == State.Ready)
		{
			ModifyStockList();
		}
		else
		{
			m_state |= State.ReadyToModify;
		}
	}
}
