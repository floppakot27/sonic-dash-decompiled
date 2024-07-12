using UnityEngine;

public class StoreVerification : MonoBehaviour
{
	private const string StateRoot = "store";

	private const string ValidatePurchaseProperty = "validateall_1.5+";

	private void Start()
	{
		EventDispatch.RegisterInterest("FeatureStateReady", this);
	}

	private void SetRecieptVerificationState(LSON.Property validateProperty)
	{
		bool verify = true;
		if (validateProperty != null)
		{
			bool boolValue = false;
			if (LSONProperties.AsBool(validateProperty, out boolValue))
			{
				verify = boolValue;
			}
		}
		SLStorePlugin.SetReceiptVerification(verify, metrics: false);
	}

	private void Event_FeatureStateReady()
	{
		LSON.Property stateProperty = FeatureState.GetStateProperty("store", "validateall_1.5+");
		SetRecieptVerificationState(stateProperty);
	}
}
