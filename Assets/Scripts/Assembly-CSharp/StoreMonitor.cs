using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoreMonitor : MonoBehaviour
{
	[Flags]
	private enum State
	{
		Displayed = 2
	}

	private State m_state;

	private StoreContent.StoreEntry m_currentEntry;

	[SerializeField]
	private string m_storeEntryID;

	[SerializeField]
	private bool m_randomiseEntry;

	public StoreContent.StoreEntry Entry => m_currentEntry;

	public string EntryID
	{
		set
		{
			m_storeEntryID = value;
			m_randomiseEntry = false;
			InitialiseEntry();
		}
	}

	private void Start()
	{
		EventDispatch.RegisterInterest("OnStorePurchaseStarted", this);
		EventDispatch.RegisterInterest("OnStorePurchaseCompleted", this);
		EventDispatch.RegisterInterest("OnStoreUpdateStarted", this);
		EventDispatch.RegisterInterest("OnStoreUpdateFinished", this);
		EventDispatch.RegisterInterest("OnStoreInitialised", this);
		InitialiseEntry();
	}

	private void OnEnable()
	{
		StartCoroutine(StartPendingActivation());
	}

	private void OnDisable()
	{
		m_state &= ~State.Displayed;
	}

	private IEnumerator StartPendingActivation()
	{
		yield return null;
		m_state |= State.Displayed;
		InitialiseEntry();
	}

	private void InitialiseEntry()
	{
		if (!StoreContent.StoreInitialised() || (m_state & State.Displayed) != State.Displayed)
		{
			return;
		}
		if (m_storeEntryID != null && m_storeEntryID.Length > 0)
		{
			m_currentEntry = StoreContent.GetStoreEntry(m_storeEntryID, StoreContent.Identifiers.Name);
		}
		else if (m_randomiseEntry)
		{
			bool flag = true;
			do
			{
				List<StoreContent.StoreEntry> stockList = StoreContent.StockList;
				int index = UnityEngine.Random.Range(0, stockList.Count);
				m_currentEntry = stockList[index];
				StoreContent.ValidateEntry(m_currentEntry);
				flag = (m_currentEntry.m_state & StoreContent.StoreEntry.State.Hidden) != StoreContent.StoreEntry.State.Hidden;
				flag &= (m_currentEntry.m_state & StoreContent.StoreEntry.State.Purchased) != StoreContent.StoreEntry.State.Purchased;
			}
			while (!(flag & (m_currentEntry.m_type != StoreContent.EntryType.Event)));
		}
		PopulateEntry();
	}

	private void PopulateEntry()
	{
		if (m_currentEntry != null)
		{
			StorePopulator.PopulateEntry(base.gameObject, m_currentEntry);
		}
	}

	private void Event_OnStoreInitialised()
	{
		InitialiseEntry();
	}

	private void Event_OnStorePurchaseCompleted(StoreContent.StoreEntry purchasedEntry, StorePurchases.Result result)
	{
		if (purchasedEntry != null)
		{
			PopulateEntry();
		}
	}

	private void Event_OnStorePurchaseStarted(StoreContent.StoreEntry purchasedEntry)
	{
		PopulateEntry();
	}

	private void Event_OnStoreUpdateStarted()
	{
		PopulateEntry();
	}

	private void Event_OnStoreUpdateFinished()
	{
		PopulateEntry();
	}
}
