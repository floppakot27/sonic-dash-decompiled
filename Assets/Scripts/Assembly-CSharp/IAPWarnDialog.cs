using UnityEngine;

public class IAPWarnDialog : MonoBehaviour
{
	private const string IAPWarnCanShowPropString = "IAPWarnCanShow";

	private bool m_canShow = true;

	private void Start()
	{
		EventDispatch.RegisterInterest("OnGameDataSaveRequest", this);
	}

	private void OnEnable()
	{
		ActiveProperties activeProperties = PropertyStore.ActiveProperties();
		if (m_canShow && activeProperties.DoesPropertyExist("IAPWarnCanShow"))
		{
			m_canShow = activeProperties.GetInt("IAPWarnCanShow") > 0;
		}
		if (m_canShow)
		{
			m_canShow = false;
		}
	}

	private void Event_OnGameDataSaveRequest()
	{
		PropertyStore.Store("IAPWarnCanShow", m_canShow ? 1 : 0);
	}
}
