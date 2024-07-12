using UnityEngine;

public class Dialog_TogglableEvent : MonoBehaviour
{
	[SerializeField]
	private string m_targetDialogTrigger;

	[SerializeField]
	private string m_serverEventRoot;

	[SerializeField]
	private string m_serverEventName;

	[SerializeField]
	private string m_eventSaveName;

	private bool m_activeLastCheck;

	private bool m_activeOnServer;

	private void OnEnable()
	{
		EventDispatch.RegisterInterest("MainMenuActive", this);
		EventDispatch.RegisterInterest("OnGameDataSaveRequest", this);
		EventDispatch.RegisterInterest("OnGameDataLoaded", this);
	}

	private void Event_MainMenuActive()
	{
		if (!FeatureState.Valid)
		{
			return;
		}
		LSON.Property stateProperty = FeatureState.GetStateProperty(m_serverEventRoot, m_serverEventName);
		if (stateProperty != null)
		{
			if (LSONProperties.AsBool(stateProperty, out m_activeOnServer))
			{
			}
		}
		else
		{
			m_activeOnServer = false;
		}
		if (!m_activeLastCheck && m_activeOnServer)
		{
			DialogStack.ShowDialog(m_targetDialogTrigger);
			m_activeLastCheck = m_activeOnServer;
		}
	}

	private void Event_OnGameDataSaveRequest()
	{
		PropertyStore.Store(m_eventSaveName, m_activeOnServer);
	}

	private void Event_OnGameDataLoaded(ActiveProperties activeProperties)
	{
		m_activeLastCheck = activeProperties.GetBool(m_eventSaveName);
	}
}
