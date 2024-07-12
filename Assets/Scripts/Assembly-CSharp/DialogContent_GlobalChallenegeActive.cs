using System;
using UnityEngine;

public class DialogContent_GlobalChallenegeActive : MonoBehaviour
{
	[SerializeField]
	private UILabel m_announceDescLabel;

	private void OnEnable()
	{
		UpdateDescription();
	}

	private void UpdateDescription()
	{
		LocalisedStringProperties component = m_announceDescLabel.GetComponent<LocalisedStringProperties>();
		DateTime challengeDate = GCState.GetChallengeDate(GCState.Challenges.gc3);
		string text = $"{challengeDate: h:mm tt (UTC), dddd d MMMM}";
		Language.Locale locale = Language.GetLocale();
		if (locale == Language.Locale.US)
		{
			challengeDate = challengeDate.AddHours(-8.0);
			text = $"{challengeDate: h:mm tt (PST), dddd d MMMM}";
		}
		text = text.ToUpper();
		string text2 = string.Format(component.GetLocalisedString(), text);
		m_announceDescLabel.text = text2;
	}
}
