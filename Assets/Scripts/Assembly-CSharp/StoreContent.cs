using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class StoreContent : MonoBehaviour
{
	public enum EntryType
	{
		Currency,
		PowerUp,
		Upgrade,
		Event,
		Character,
		Wallpaper
	}

	public enum Group
	{
		Currency,
		PowerUps,
		Upgrades,
		Free,
		None,
		Characters,
		Wallpaper
	}

	public enum PaymentMethod
	{
		Rings,
		StarRings,
		Money,
		Web,
		Free,
		External
	}

	public enum Identifiers
	{
		Name,
		OsStore
	}

	[Serializable]
	public class StoreEntry
	{
		[Flags]
		public enum State
		{
			Hidden = 1,
			OneShot = 2,
			ShowInEditor = 4,
			OnSale = 8,
			Purchased = 0x10,
			RequiresConnection = 0x20,
			ShowIfPurchased = 0x40,
			Downloaded = 0x80,
			Downloading = 0x100,
			RemoveAds = 0x200
		}

		[Serializable]
		public class Cost
		{
			public int[] m_baseCost = new int[7];

			public int[] m_playerCost = new int[7];
		}

		[Serializable]
		public class Awards
		{
			public int m_baseRings = 100;

			public int m_baseStars;

			public int m_playerRings;

			public int m_playerStars;
		}

		[Serializable]
		public class OSStore
		{
			public string m_baseiTunesId = string.Empty;

			public string m_playeriTunesId = string.Empty;

			public ProductStateCode m_baseState;

			public ProductStateCode m_playerState;
		}

		public string m_identifier = string.Empty;

		public string m_title = string.Empty;

		public string m_description = string.Empty;

		public Mesh m_mesh;

		public EntryType m_type = EntryType.PowerUp;

		public PaymentMethod m_payment;

		public Group m_group = Group.None;

		public Cost m_cost = new Cost();

		public PowerUps.Type m_powerUpType = PowerUps.Type.Magnet;

		public int m_powerUpCount = 1;

		public bool m_upgradeToMax;

		public Characters.Type m_character;

		public Awards m_awards = new Awards();

		public string[] m_wallpaperResources = new string[10]
		{
			string.Empty,
			string.Empty,
			string.Empty,
			string.Empty,
			string.Empty,
			string.Empty,
			string.Empty,
			string.Empty,
			string.Empty,
			string.Empty
		};

		public DialogContent_PlayerReward.Reason m_rewardReason;

		public float m_downloadProgress;

		public ObjectMonitor.PlugIns m_externalPlugIn = ObjectMonitor.PlugIns.Videos;

		public string m_appLink = string.Empty;

		public string m_webLink = string.Empty;

		public OSStore m_osStore = new OSStore();

		public string m_buttonText = string.Empty;

		public string m_purchasedString = string.Empty;

		public string m_offerSellText = string.Empty;

		public State m_state;
	}

	[Flags]
	private enum State
	{
		StoreInitialised = 1,
		UpdateStoreState = 2,
		StoreUpdateRequested = 4,
		InitialUpdateDone = 8
	}

	public const string DefaultRingOffer = "Single Ring Reward";

	public const string DefaultStarOffer = "Single Star Reward";

	private static StoreContent s_storeContent;

	private static bool s_showHiddenStoreItems;

	private State m_state;

	[SerializeField]
	private List<StoreEntry> m_stockList;

	[SerializeField]
	private float m_forcedUpdateDelay = 3f;

	public static List<StoreEntry> StockList
	{
		get
		{
			s_storeContent.InitialiseStoreContent();
			return s_storeContent.m_stockList;
		}
	}

	public static int EntryTypeCount => Utils.GetEnumCount<EntryType>();

	public static bool ShowHiddenStoreItems
	{
		get
		{
			return s_showHiddenStoreItems;
		}
		set
		{
			s_showHiddenStoreItems = value;
		}
	}

	public static StoreEntry GetStoreEntry(string identifier, Identifiers idType)
	{
		StoreEntry storeEntry = null;
		if (idType == Identifiers.Name)
		{
			string idToFind = FormatIdentifier(identifier);
			return s_storeContent.m_stockList.FirstOrDefault((StoreEntry thisEntry) => thisEntry.m_identifier == idToFind);
		}
		return s_storeContent.m_stockList.FirstOrDefault(delegate(StoreEntry thisEntry)
		{
			string osStoreId = StoreUtils.GetOsStoreId(thisEntry, StoreUtils.EntryType.Player);
			return osStoreId.ToLower() == identifier.ToLower();
		});
	}

	public static void RefreshStoreState()
	{
		if (!StoreUtils.IsStoreActive())
		{
			s_storeContent.ResetOsStoreState(informTitle: true);
		}
	}

	public static bool ProcessingStoreEntries()
	{
		bool flag = (s_storeContent.m_state & State.UpdateStoreState) == State.UpdateStoreState;
		bool flag2 = (s_storeContent.m_state & State.InitialUpdateDone) == State.InitialUpdateDone;
		return flag && !flag2;
	}

	public static bool StoreInitialised()
	{
		return (s_storeContent.m_state & State.StoreInitialised) == State.StoreInitialised;
	}

	public static void ValidateEntry(StoreEntry storeEntry)
	{
		if (storeEntry.m_payment == PaymentMethod.External)
		{
			ObjectMonitor.PlugIns externalPlugIn = storeEntry.m_externalPlugIn;
			bool flag = ObjectMonitor.CheckPlugInsAvailable(externalPlugIn);
			if (externalPlugIn == ObjectMonitor.PlugIns.SegaNetwork)
			{
				if (flag)
				{
					storeEntry.m_state &= ~StoreEntry.State.Purchased;
				}
				else
				{
					storeEntry.m_state |= StoreEntry.State.Purchased;
				}
			}
			if (!flag)
			{
				bool flag2 = (storeEntry.m_state & StoreEntry.State.ShowInEditor) == StoreEntry.State.ShowInEditor;
				if (!Application.isEditor || !flag2)
				{
					storeEntry.m_state |= StoreEntry.State.Hidden;
				}
			}
			else
			{
				storeEntry.m_state &= ~StoreEntry.State.Hidden;
			}
		}
		else if (storeEntry.m_payment == PaymentMethod.Web)
		{
			if (Internet.ConnectionAvailable())
			{
				storeEntry.m_state &= ~StoreEntry.State.Hidden;
			}
			else
			{
				storeEntry.m_state |= StoreEntry.State.Hidden;
			}
		}
		else if (storeEntry.m_payment != PaymentMethod.Free && s_showHiddenStoreItems)
		{
			storeEntry.m_state &= ~StoreEntry.State.Hidden;
		}
		StoreUtils.OneShotItemPurchased(storeEntry, null);
	}

	public static string FormatIdentifier(string currentID)
	{
		string text = currentID.ToLower();
		return text.Replace(' ', '_');
	}

	private void Awake()
	{
		InitialiseStoreContent();
	}

	private void Start()
	{
		s_storeContent = this;
		UpdateStoreContent();
		EventDispatch.RegisterInterest("RequestStoreRefresh", this);
		EventDispatch.RegisterInterest("ProductStateChanged", this);
		EventDispatch.RegisterInterest("SoftLightPlugIns_InitialisedOnGame", this);
		EventDispatch.RegisterInterest("OnGameDataLoaded", this);
	}

	private void Update()
	{
		if ((m_state & State.UpdateStoreState) == State.UpdateStoreState)
		{
			UpdateOsStateState();
		}
	}

	private void InitialiseStoreContent()
	{
		if (m_stockList == null)
		{
			m_stockList = new List<StoreEntry>();
		}
		for (int i = 0; i < m_stockList.Count; i++)
		{
			if (m_stockList[i].m_type == EntryType.Upgrade && m_stockList[i].m_cost.m_baseCost.Length < 7)
			{
				int[] array = new int[7];
				int[] array2 = new int[7];
				for (int j = 0; j < m_stockList[i].m_cost.m_baseCost.Length; j++)
				{
					array[j] = m_stockList[i].m_cost.m_baseCost[j];
					array2[j] = m_stockList[i].m_cost.m_playerCost[j];
				}
				m_stockList[i].m_cost.m_baseCost = array;
				m_stockList[i].m_cost.m_playerCost = array2;
			}
		}
	}

	private void ValidateStoreContent()
	{
		for (int i = 0; i < m_stockList.Count; i++)
		{
			string identifier = m_stockList[i].m_identifier;
			if (identifier.Length == 0)
			{
			}
			for (int j = 0; j < m_stockList.Count; j++)
			{
				if (i == j || identifier == m_stockList[j].m_identifier)
				{
				}
			}
		}
	}

	private void ResetOsStoreState(bool informTitle)
	{
		foreach (StoreEntry stock in m_stockList)
		{
			stock.m_osStore.m_baseState = ProductStateCode.ProductInfoNone;
			stock.m_osStore.m_playerState = ProductStateCode.ProductInfoNone;
		}
		m_state |= State.UpdateStoreState;
		UpdateOsStateState();
		if (informTitle)
		{
			EventDispatch.GenerateEvent("OnStoreUpdateStarted");
		}
	}

	private void UpdateStoreContent()
	{
		foreach (StoreEntry stock in m_stockList)
		{
			stock.m_identifier = FormatIdentifier(stock.m_identifier);
			stock.m_awards.m_playerRings = stock.m_awards.m_baseRings;
			stock.m_awards.m_playerStars = stock.m_awards.m_baseStars;
			stock.m_osStore.m_playeriTunesId = stock.m_osStore.m_baseiTunesId;
			for (int i = 0; i < stock.m_cost.m_baseCost.Length; i++)
			{
				stock.m_cost.m_playerCost[i] = stock.m_cost.m_baseCost[i];
			}
			if (stock.m_type == EntryType.Event)
			{
				stock.m_group = Group.None;
			}
			for (int j = 0; j < stock.m_wallpaperResources.Length; j++)
			{
				if (!string.IsNullOrEmpty(stock.m_wallpaperResources[j]))
				{
					stock.m_wallpaperResources[j] = stock.m_wallpaperResources[j].ToLowerInvariant();
				}
			}
		}
	}

	private void UpdateOsStateState()
	{
		bool flag = false;
		for (int i = 0; i < m_stockList.Count; i++)
		{
			StoreEntry storeEntry = m_stockList[i];
			if (storeEntry.m_payment == PaymentMethod.Money)
			{
				if (storeEntry.m_osStore.m_baseState == ProductStateCode.ProductInfoNone)
				{
					string osStoreId = StoreUtils.GetOsStoreId(storeEntry, StoreUtils.EntryType.Base);
					storeEntry.m_osStore.m_baseState = SLStorePlugin.GetProductState(osStoreId);
				}
				if (storeEntry.m_osStore.m_playerState == ProductStateCode.ProductInfoNone)
				{
					string osStoreId2 = StoreUtils.GetOsStoreId(storeEntry, StoreUtils.EntryType.Player);
					storeEntry.m_osStore.m_playerState = SLStorePlugin.GetProductState(osStoreId2);
				}
				if (storeEntry.m_osStore.m_baseState == ProductStateCode.ProductInfoDoneRrefershing || storeEntry.m_osStore.m_baseState == ProductStateCode.ProductInfoFetching || storeEntry.m_osStore.m_baseState == ProductStateCode.ProductInfoNone || storeEntry.m_osStore.m_playerState == ProductStateCode.ProductInfoDoneRrefershing || storeEntry.m_osStore.m_playerState == ProductStateCode.ProductInfoFetching || storeEntry.m_osStore.m_playerState == ProductStateCode.ProductInfoNone)
				{
					flag = true;
				}
			}
		}
		if (!flag)
		{
			m_state |= State.InitialUpdateDone;
			if ((m_state & State.StoreUpdateRequested) == State.StoreUpdateRequested)
			{
				m_state &= ~State.StoreUpdateRequested;
				ResetOsStoreState(informTitle: false);
			}
			else
			{
				m_state &= ~State.UpdateStoreState;
				EventDispatch.GenerateEvent("OnStoreUpdateFinished");
			}
		}
	}

	private void OnApplicationPause(bool paused)
	{
		if (!paused && !(s_storeContent == null) && !StoreUtils.IsStoreActive())
		{
			SLStorePlugin.ResetProductInfo();
			bool informTitle = true;
			if ((m_state & State.UpdateStoreState) == State.UpdateStoreState)
			{
				informTitle = false;
			}
			ResetOsStoreState(informTitle);
		}
	}

	private void Event_OnGameDataLoaded(ActiveProperties loadedProperties)
	{
		List<StoreEntry> stockList = StockList;
		foreach (StoreEntry item in stockList)
		{
			if ((item.m_state & StoreEntry.State.OneShot) == StoreEntry.State.OneShot)
			{
				StoreUtils.OneShotItemPurchased(item, loadedProperties);
			}
		}
	}

	private void Event_RequestStoreRefresh()
	{
		if ((m_state & State.UpdateStoreState) == State.UpdateStoreState)
		{
			m_state |= State.StoreUpdateRequested;
		}
		else if ((m_state & State.StoreInitialised) == State.StoreInitialised)
		{
			ResetOsStoreState(informTitle: true);
		}
	}

	private void Event_ProductStateChanged(string productID)
	{
		for (int i = 0; i < m_stockList.Count; i++)
		{
			StoreEntry storeEntry = m_stockList[i];
			string osStoreId = StoreUtils.GetOsStoreId(storeEntry, StoreUtils.EntryType.Base);
			string osStoreId2 = StoreUtils.GetOsStoreId(storeEntry, StoreUtils.EntryType.Player);
			if (osStoreId == productID)
			{
				storeEntry.m_osStore.m_baseState = SLStorePlugin.GetProductState(osStoreId);
			}
			if (osStoreId2 == productID)
			{
				storeEntry.m_osStore.m_playerState = SLStorePlugin.GetProductState(osStoreId2);
			}
		}
	}

	private void Event_SoftLightPlugIns_InitialisedOnGame()
	{
		ResetOsStoreState(informTitle: true);
		m_state |= State.StoreInitialised;
		EventDispatch.GenerateEvent("OnStoreInitialised");
	}
}
