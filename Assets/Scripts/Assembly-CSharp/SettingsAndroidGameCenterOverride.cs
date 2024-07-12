using UnityEngine;

public class SettingsAndroidGameCenterOverride : MonoBehaviour
{
	private const string m_googlePlayTitle = "OPTIONS_GOOGLE_PLAY_TITLE";

	private const string m_googlePlaySignIn = "OPTIONS_GOOGLE_PLAY_SIGNIN";

	private const string m_googlePlayAchieve = "OPTIONS_GOOGLE_PLAY_ACHIEVEMENTS";

	private const string m_googlePlusLogo = "button_googleplus";

	private const string m_googlePlayLogo = "button_googleplaygames";

	[SerializeField]
	private LocalisedStringProperties m_gameCenterTitle;

	[SerializeField]
	private LocalisedStringProperties m_gameCenterText;

	[SerializeField]
	private UISprite m_gameCenterLogo;

	private bool m_signedIn = true;

	public void OnEnable()
	{
		m_signedIn = HLSocialPluginAndroid.IsGooglePlusSignedIn();
		UpdateButtons();
	}

	public void Update()
	{
		bool flag = HLSocialPluginAndroid.IsGooglePlusSignedIn();
		if (m_signedIn != flag)
		{
			m_signedIn = flag;
			UpdateButtons();
		}
	}

	private void UpdateButtons()
	{
		if (m_gameCenterTitle != null)
		{
			m_gameCenterTitle.SetLocalisationID("OPTIONS_GOOGLE_PLAY_TITLE");
		}
		if (m_gameCenterLogo != null)
		{
			if (m_signedIn)
			{
				m_gameCenterLogo.spriteName = "button_googleplaygames";
			}
			else
			{
				m_gameCenterLogo.spriteName = "button_googleplus";
			}
		}
		if (m_gameCenterText != null)
		{
			if (m_signedIn)
			{
				m_gameCenterText.SetLocalisationID("OPTIONS_GOOGLE_PLAY_ACHIEVEMENTS");
			}
			else
			{
				m_gameCenterText.SetLocalisationID("OPTIONS_GOOGLE_PLAY_SIGNIN");
			}
			string localisedString = m_gameCenterText.GetLocalisedString();
			m_gameCenterText.SetActiveString(localisedString);
		}
	}
}
