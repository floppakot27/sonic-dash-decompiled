using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class StoreFront : MonoBehaviour
{
	private class StoreListing
	{
		public StoreContent.StoreEntry m_storeEntry;

		public UIDragPanelContents m_storePanel;

		public bool m_wasDownloading;
	}

	private static int s_entryCount;

	private UIGrid m_displayGrid;

	private UIDraggablePanel m_draggablePanel;

	private List<StoreListing> m_storeList;

	[SerializeField]
	private StoreContent m_storeContent;

	[SerializeField]
	private UIDragPanelContents m_headingTemplate;

	[SerializeField]
	private UIDragPanelContents m_entryTemplate;

	[SerializeField]
	private StoreContent.Group m_storeGroup = StoreContent.Group.None;

	[SerializeField]
	private GuiTrigger m_sourceTrigger;

	public StoreContent.StoreEntry FindStoreEntry(UIDragPanelContents storeEntry)
	{
		StoreListing storeListing = m_storeList.Find((StoreListing thisList) => thisList.m_storePanel == storeEntry);
		return storeListing.m_storeEntry;
	}

	private void Start()
	{
		m_displayGrid = Utils.FindBehaviourInTree(this, m_displayGrid);
		m_draggablePanel = Utils.FindBehaviourInTree(this, m_draggablePanel);
		EventDispatch.RegisterInterest("OnStorePurchaseStarted", this);
		EventDispatch.RegisterInterest("OnStorePurchaseCompleted", this);
		EventDispatch.RegisterInterest("OnStoreUpdateStarted", this);
		EventDispatch.RegisterInterest("OnStoreUpdateFinished", this);
		CreateStorePanelEntries();
	}

	private void Update()
	{
		if (m_storeList == null)
		{
			return;
		}
		for (int i = 0; i < m_storeList.Count; i++)
		{
			StoreListing storeListing = m_storeList[i];
			bool flag = (storeListing.m_storeEntry.m_state & StoreContent.StoreEntry.State.Downloading) == StoreContent.StoreEntry.State.Downloading;
			if (flag || storeListing.m_wasDownloading)
			{
				StorePopulator.PopulateMinimalEntry(storeListing.m_storePanel.gameObject, storeListing.m_storeEntry, StorePopulator.Display.UpdateDownload);
				storeListing.m_wasDownloading = flag;
				if (!storeListing.m_wasDownloading)
				{
					StorePopulator.PopulateEntry(storeListing.m_storePanel.gameObject, storeListing.m_storeEntry);
				}
			}
		}
	}

	private void OnEnable()
	{
		StartCoroutine(StartPendingActivation());
	}

	private IEnumerator StartPendingActivation()
	{
		yield return null;
		m_draggablePanel.ResetPosition();
		PopulatePanelEntries();
		m_draggablePanel.ResetPosition();
	}

	private void CreateStorePanelEntries()
	{
		m_draggablePanel.ResetPosition();
		List<StoreContent.StoreEntry> stockList = StoreContent.StockList;
		m_storeList = new List<StoreListing>();
		IEnumerable<StoreContent.StoreEntry> storeContent = stockList.Where((StoreContent.StoreEntry thisEntry) => thisEntry.m_group == m_storeGroup);
		CreateIndividualEntries(storeContent, m_entryTemplate, m_headingTemplate);
		m_displayGrid.sorted = true;
		m_displayGrid.Reposition();
		m_draggablePanel.ResetPosition();
	}

	private void PopulatePanelEntries()
	{
		for (int i = 0; i < m_storeList.Count; i++)
		{
			StoreListing storeListing = m_storeList[i];
			StoreContent.StoreEntry storeEntry = storeListing.m_storeEntry;
			UIDragPanelContents storePanel = storeListing.m_storePanel;
			StoreContent.ValidateEntry(storeEntry);
			bool flag = (storeEntry.m_state & StoreContent.StoreEntry.State.Hidden) == StoreContent.StoreEntry.State.Hidden;
			bool flag2 = (storeEntry.m_state & StoreContent.StoreEntry.State.Purchased) == StoreContent.StoreEntry.State.Purchased;
			bool flag3 = (storeEntry.m_state & StoreContent.StoreEntry.State.ShowIfPurchased) == StoreContent.StoreEntry.State.ShowIfPurchased;
			if (flag || (flag2 && !flag3))
			{
				storePanel.gameObject.SetActive(value: false);
				continue;
			}
			storePanel.gameObject.SetActive(value: true);
			StorePopulator.PopulateEntry(storePanel.gameObject, storeEntry);
		}
		m_displayGrid.Reposition();
	}

	private void CreateIndividualEntries(IEnumerable<StoreContent.StoreEntry> storeContent, UIDragPanelContents entryTemplate, UIDragPanelContents headerTemplate)
	{
		if (storeContent.Count() == 0)
		{
			return;
		}
		foreach (StoreContent.StoreEntry item in storeContent)
		{
			GameObject gameObject = NGUITools.AddChild(base.gameObject, entryTemplate.gameObject);
			gameObject.name = string.Format("{0} {1}", s_entryCount.ToString("D3"), item.m_type.ToString());
			s_entryCount++;
			UIDragPanelContents component = gameObject.GetComponent<UIDragPanelContents>();
			AddStoreListing(item, component);
		}
	}

	private void AddStoreListing(StoreContent.StoreEntry entry, UIDragPanelContents panel)
	{
		StoreListing storeListing = new StoreListing();
		storeListing.m_storeEntry = entry;
		storeListing.m_storePanel = panel;
		storeListing.m_wasDownloading = false;
		m_storeList.Add(storeListing);
	}

	private void Event_OnStorePurchaseCompleted(StoreContent.StoreEntry purchasedEntry, StorePurchases.Result result)
	{
		if (purchasedEntry != null)
		{
			PopulatePanelEntries();
		}
	}

	private void Event_OnStorePurchaseStarted(StoreContent.StoreEntry purchasedEntry)
	{
		PopulatePanelEntries();
	}

	private void Event_OnStoreUpdateStarted()
	{
		PopulatePanelEntries();
	}

	private void Event_OnStoreUpdateFinished()
	{
		PopulatePanelEntries();
	}
}
