using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dialog_RestorePurchase : MonoBehaviour
{
	private List<StoreContent.StoreEntry> m_restoredPurchases = new List<StoreContent.StoreEntry>(1);

	private GameObject[] m_itemList;

	public static void Display(List<StoreContent.StoreEntry> restoredPurchases)
	{
		string dialogName = $"Restore Purchase x{restoredPurchases.Count}";
		GuiTrigger guiTrigger = DialogStack.ShowDialog(dialogName);
		Dialog_RestorePurchase componentInChildren = Utils.GetComponentInChildren<Dialog_RestorePurchase>(guiTrigger.Trigger.gameObject);
		componentInChildren.SetContent(restoredPurchases);
	}

	private void SetContent(List<StoreContent.StoreEntry> restoredPurchases)
	{
		m_restoredPurchases.Clear();
		for (int i = 0; i < restoredPurchases.Count; i++)
		{
			m_restoredPurchases.Add(restoredPurchases[i]);
		}
		string text = "RestorePurchase_Entry";
		m_itemList = GameObject.FindGameObjectsWithTag(text);
		int num = m_itemList.Length;
		int count = m_restoredPurchases.Count;
	}

	private void OnEnable()
	{
		StartCoroutine(StartPendingActivation());
	}

	private IEnumerator StartPendingActivation()
	{
		yield return null;
		for (int i = 0; i < m_restoredPurchases.Count; i++)
		{
			GameObject objectToFill = m_itemList[i];
			StoreContent.StoreEntry storeEntry = m_restoredPurchases[i];
			FillEntry(objectToFill, storeEntry);
		}
	}

	private void FillEntry(GameObject item, StoreContent.StoreEntry storeItem)
	{
		UILabel componentInChildren = item.GetComponentInChildren<UILabel>();
		MeshFilter componentInChildren2 = item.GetComponentInChildren<MeshFilter>();
		LocalisedStringProperties.SetLocalisedString(componentInChildren.gameObject, storeItem.m_title);
		componentInChildren2.mesh = storeItem.m_mesh;
	}
}
