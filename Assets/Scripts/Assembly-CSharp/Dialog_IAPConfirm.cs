using System.Collections;
using UnityEngine;

public class Dialog_IAPConfirm : MonoBehaviour
{
	private const string RingsRequestedString = "STORE_PURCHASE_NO_RINGS_02";

	private const string StarsRequestedString = "STORE_PURCHASE_NO_STAR_RINGS_02";

	private static StoreContent.StoreEntry s_entryToDisplay;

	private static StoreContent.PaymentMethod s_paymentMethod;

	[SerializeField]
	private UILabel m_description;

	[SerializeField]
	private GameObject m_storeEntry;

	public static void Display(StoreContent.StoreEntry entry, StoreContent.PaymentMethod paymentMethod)
	{
		s_entryToDisplay = entry;
		s_paymentMethod = paymentMethod;
		DialogStack.ShowDialog("IAP Confirm Dialog");
	}

	private void Start()
	{
	}

	private void OnEnable()
	{
		StartCoroutine(StartPendingActivation());
	}

	private IEnumerator StartPendingActivation()
	{
		yield return null;
		UpdateContent();
	}

	private void UpdateContent()
	{
		string id = "STORE_PURCHASE_NO_RINGS_02";
		if (s_paymentMethod == StoreContent.PaymentMethod.StarRings)
		{
			id = "STORE_PURCHASE_NO_STAR_RINGS_02";
		}
		string @string = LanguageStrings.First.GetString(id);
		string string2 = LanguageStrings.First.GetString(s_entryToDisplay.m_title);
		string text = string.Format(@string, string2);
		m_description.text = text;
		StorePopulator.PopulateEntry(m_storeEntry, s_entryToDisplay, StorePopulator.Display.IgnoreTimeOut);
	}

	private void Trigger_CompleteIAP()
	{
		EventDispatch.GenerateEvent("CompletePendingIAP");
	}

	private void Trigger_CancelIAP()
	{
		EventDispatch.GenerateEvent("CancelPendingIAP");
	}
}
