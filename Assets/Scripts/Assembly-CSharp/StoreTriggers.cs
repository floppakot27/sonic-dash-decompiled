using UnityEngine;

public class StoreTriggers : MonoBehaviour
{
	private void Trigger_MakePurchase(GameObject callerObject)
	{
		if (callerObject != null)
		{
			MakePurchaseProperties component = callerObject.GetComponent<MakePurchaseProperties>();
			if (component != null)
			{
				StorePurchases.RequestPurchase(component.OfferID, StorePurchases.LowCurrencyResponse.PurchaseCurrencyAndItem);
			}
		}
	}
}
