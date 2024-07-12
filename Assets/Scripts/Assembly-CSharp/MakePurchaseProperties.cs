using UnityEngine;

public class MakePurchaseProperties : MonoBehaviour
{
	[SerializeField]
	private string m_storeOfferID;

	public string OfferID => m_storeOfferID;
}
