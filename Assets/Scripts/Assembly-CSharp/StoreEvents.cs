using UnityEngine;

public class StoreEvents : MonoBehaviour
{
	private StoreFront m_storeFront;

	private StoreMonitor m_storeMonitor;

	private void Start()
	{
		m_storeFront = Utils.GetComponentInParent<StoreFront>(base.gameObject);
		m_storeMonitor = Utils.GetComponentInParent<StoreMonitor>(base.gameObject);
	}

	private StoreContent.StoreEntry GetStoreEntry(GameObject callerObject)
	{
		StoreContent.StoreEntry result = null;
		if (m_storeFront != null)
		{
			UIDragPanelContents componentInParent = Utils.GetComponentInParent<UIDragPanelContents>(callerObject);
			result = m_storeFront.FindStoreEntry(componentInParent);
		}
		else if (m_storeMonitor != null)
		{
			result = m_storeMonitor.Entry;
		}
		return result;
	}

	private void Trigger_PurchaseRequested(GameObject callerObject)
	{
		StoreContent.StoreEntry storeEntry = GetStoreEntry(callerObject);
		StorePurchases.RequestPurchase(storeEntry.m_identifier, StorePurchases.LowCurrencyResponse.PurchaseCurrencyAndItem);
	}
}
