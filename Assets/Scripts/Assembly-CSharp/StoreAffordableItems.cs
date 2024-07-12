using UnityEngine;

public class StoreAffordableItems : MonoBehaviour
{
	private UILabel m_countLabel;

	private bool m_visible = true;

	private int m_previousCount;

	[SerializeField]
	private bool m_includeAllStoreItems = true;

	[SerializeField]
	private StoreContent.EntryType m_storeType = StoreContent.EntryType.Character;

	private void Start()
	{
		m_countLabel = Utils.GetComponentInChildren<UILabel>(base.gameObject);
		ShowChildren(show: false);
	}

	private void Update()
	{
		if (m_countLabel == null)
		{
			return;
		}
		int affordableItemCount = StoreUtils.GetAffordableItemCount(GetStoreEntryType());
		if (m_previousCount != affordableItemCount)
		{
			if (affordableItemCount > 0)
			{
				ShowChildren(show: true);
				m_countLabel.text = affordableItemCount.ToString();
			}
			else
			{
				ShowChildren(show: false);
			}
			m_previousCount = affordableItemCount;
		}
	}

	private StoreContent.EntryType? GetStoreEntryType()
	{
		if (m_includeAllStoreItems)
		{
			return null;
		}
		return m_storeType;
	}

	private void ShowChildren(bool show)
	{
		UIWidget[] componentsInChildren = GetComponentsInChildren<UIWidget>(includeInactive: true);
		if (componentsInChildren != null && show != m_visible)
		{
			m_visible = show;
			UIWidget[] array = componentsInChildren;
			foreach (UIWidget uIWidget in array)
			{
				uIWidget.gameObject.SetActive(show);
			}
		}
	}
}
