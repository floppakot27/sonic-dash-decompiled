using UnityEngine;

public class SettingsFacebookLogInOutControl : MonoBehaviour
{
	private const string ToConnect = "OPTIONS_FACEBOOK_DESCRIPTION_LOGIN";

	private const string ToDisconnect = "OPTIONS_FACEBOOK_DESCRIPTION_LOGOUT";

	[SerializeField]
	private LocalisedStringProperties m_facebookConnectionOption;

	private static SettingsFacebookLogInOutControl instance;

	public static void ChangeFacebookConnectionOption(bool connected, bool showDialog)
	{
		if (showDialog && !connected)
		{
			DialogContent_GeneralInfo.Display(DialogContent_GeneralInfo.Type.LoggedOutOfFacebook);
		}
		if (connected)
		{
			instance.m_facebookConnectionOption.SetLocalisationID("OPTIONS_FACEBOOK_DESCRIPTION_LOGOUT");
		}
		else
		{
			instance.m_facebookConnectionOption.SetLocalisationID("OPTIONS_FACEBOOK_DESCRIPTION_LOGIN");
		}
		string localisedString = instance.m_facebookConnectionOption.GetLocalisedString();
		instance.m_facebookConnectionOption.SetActiveString(localisedString);
	}

	private void Awake()
	{
		instance = this;
		EventDispatch.RegisterInterest("OnFacebookLogin", this, EventDispatch.Priority.Lowest);
	}

	private void OnEnable()
	{
		ChangeFacebookConnectionOption(((HLLocalUser)Social.localUser).isFacebookAuthenticated, showDialog: false);
	}

	private void Event_OnFacebookLogin()
	{
		ChangeFacebookConnectionOption(((HLLocalUser)Social.localUser).isFacebookAuthenticated, showDialog: false);
	}
}
