using System;
using System.Globalization;
using UnityEngine;

public class DialogContent_GCPlayerInvolvement : MonoBehaviour
{
	[SerializeField]
	private UILabel m_descLabel;

	private void OnEnable()
	{
		UpdateDescription();
	}

	private void UpdateDescription()
	{
		LocalisedStringProperties component = m_descLabel.GetComponent<LocalisedStringProperties>();
		component.GetComponent<LocalisedStringStatic>().enabled = true;
		DateTime challengeDate = GCState.GetChallengeDate(GCState.Challenges.gc3);
		Language.ID language = Language.GetLanguage();
		Language.Locale locale = Language.GetLocale();
		string text;
		if (locale == Language.Locale.US && (language == Language.ID.English_UK || language == Language.ID.English_US))
		{
			challengeDate = challengeDate.AddHours(-8.0);
			text = string.Format(new CultureInfo("en-US"), "{0: dddd d MMMM, h:mm tt (PST)}", challengeDate);
		}
		else
		{
			text = string.Format(new CultureInfo(Language.CultureInfoIDs[(int)language]), "{0: dddd d MMMM, h:mm tt (UTC)}", challengeDate);
		}
		text = text.ToUpper();
		string text2 = string.Format(component.GetLocalisedString(), text);
		m_descLabel.text = text2;
		component.GetComponent<LocalisedStringStatic>().enabled = false;
	}
}
