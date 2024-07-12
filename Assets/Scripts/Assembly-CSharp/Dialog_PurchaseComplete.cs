using System.Collections;
using UnityEngine;

public class Dialog_PurchaseComplete : MonoBehaviour
{
	private StoreContent.StoreEntry m_storeEntry;

	private GameObject m_entryRoot;

	public static void Display(StoreContent.StoreEntry storeEntry)
	{
		GuiTrigger guiTrigger = DialogStack.ShowDialog("Purchase Complete Dialog");
		Dialog_PurchaseComplete componentInChildren = Utils.GetComponentInChildren<Dialog_PurchaseComplete>(guiTrigger.Trigger.gameObject);
		componentInChildren.SetContent(storeEntry);
	}

	private void SetContent(StoreContent.StoreEntry storeEntry)
	{
		m_storeEntry = storeEntry;
	}

	private void OnEnable()
	{
		StartCoroutine(StartPendingActivation());
	}

	private IEnumerator StartPendingActivation()
	{
		yield return null;
		string itemName = "PurchaseComplete_Entry";
		m_entryRoot = GameObject.FindGameObjectWithTag(itemName);
		UILabel labelItem = m_entryRoot.GetComponentInChildren<UILabel>();
		MeshFilter meshItem = m_entryRoot.GetComponentInChildren<MeshFilter>();
		LocalisedStringProperties.SetLocalisedString(labelItem.gameObject, m_storeEntry.m_title);
		meshItem.mesh = m_storeEntry.m_mesh;
	}
}
