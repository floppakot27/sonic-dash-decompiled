using UnityEngine;

public class DialogContent_AppUpdate : MonoBehaviour
{
	private const string VersionStateRoot = "version";

	private const string VersionNumberProperty = "versionnumber";

	private int m_timesShownThisUpdate;

	private bool m_hasBeenShownThisSession;

	private bool m_upToDateVersion;

	public static DialogContent_AppUpdate Instance { get; private set; }

	private void Awake()
	{
		if (Instance != null && Instance != this)
		{
			Object.Destroy(this);
		}
		else
		{
			Instance = this;
		}
	}

	private void Start()
	{
		EventDispatch.RegisterInterest("OnGameDataLoaded", this);
		EventDispatch.RegisterInterest("OnGameDataSaveRequest", this);
		EventDispatch.RegisterInterest("MainMenuActive", this);
	}

	public bool ValidToDisplay()
	{
		if (!m_hasBeenShownThisSession && Internet.ConnectionAvailable())
		{
			if (FeatureState.Valid)
			{
				LSON.Property stateProperty = FeatureState.GetStateProperty("version", "versionnumber");
				if (stateProperty != null)
				{
					string stringValue = string.Empty;
					bool flag = LSONProperties.AsString(stateProperty, out stringValue);
					if (VersionIdentifiers.CheckVersionNumbers(stringValue, "1.8.0") == VersionIdentifiers.VersionStatus.Higher)
					{
						m_upToDateVersion = false;
						m_hasBeenShownThisSession = true;
						m_timesShownThisUpdate++;
						return true;
					}
				}
			}
			m_upToDateVersion = true;
			m_timesShownThisUpdate = 0;
		}
		return false;
	}

	public void Trigger_UpdateToNewVersion()
	{
		GameAnalytics.AppUpdateDialogShown(GameAnalytics.AppUpdateChoice.Update, m_timesShownThisUpdate);
		Application.OpenURL("https://itunes.apple.com/app/sonic-dash/id582654048?mt=8");
	}

	public void Trigger_DoNotUpdate()
	{
		m_upToDateVersion = false;
		GameAnalytics.AppUpdateDialogShown(GameAnalytics.AppUpdateChoice.Cancel, m_timesShownThisUpdate);
	}

	private void Event_MainMenuActive()
	{
		if (ValidToDisplay())
		{
			Dialog_AppUpdate.Display();
		}
	}

	private void Event_OnGameDataSaveRequest()
	{
		PropertyStore.Store("AppUpdateDialog_VersionUpToDate", m_upToDateVersion);
		PropertyStore.Store("AppUpdateDialog_TimesShown", m_timesShownThisUpdate);
	}

	private void Event_OnGameDataLoaded(ActiveProperties activeProperties)
	{
		m_upToDateVersion = activeProperties.GetBool("AppUpdateDialog_VersionUpToDate");
		m_timesShownThisUpdate = activeProperties.GetInt("AppUpdateDialog_TimesShown");
	}
}
