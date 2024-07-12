using UnityEngine;

public class LocalisedStringProperties : MonoBehaviour
{
	[SerializeField]
	private string m_localisationID;

	[SerializeField]
	private LanguageStrings m_languageStrings;

	[SerializeField]
	private string m_sourceFile;

	[SerializeField]
	private Language.ID m_targetLanguage;

	private UILabel m_localisationTarget;

	public static void SetLocalisedString(GameObject target, string locId)
	{
		LocalisedStringProperties componentInChildren = Utils.GetComponentInChildren<LocalisedStringProperties>(target);
		string activeString = componentInChildren.SetLocalisationID(locId);
		componentInChildren.SetActiveString(activeString);
	}

	public string GetLocalisedString()
	{
		LanguageStrings languageStrings = m_languageStrings;
		if (languageStrings == null)
		{
			languageStrings = LanguageStrings.First;
		}
		if (languageStrings == null)
		{
			throw new UnityException($"LocalisedStringProperties::GetLocalisedString - Unable to find a Language Strings object to get the localised string from in '{base.name}'");
		}
		if (m_localisationID == null || m_localisationID.Length == 0)
		{
			return null;
		}
		return languageStrings.GetString(m_localisationID);
	}

	public string SetLocalisationID(string localisationID)
	{
		m_localisationID = localisationID;
		return GetLocalisedString();
	}

	public void SetActiveString(string localisedString)
	{
		if (!(m_localisationTarget == null) && localisedString != null)
		{
			m_localisationTarget.text = localisedString;
		}
	}

	private void Start()
	{
		StoreGuiText();
	}

	private void StoreGuiText()
	{
		m_localisationTarget = GetComponent<UILabel>();
		if (m_localisationTarget == null)
		{
			throw new UnityException($"LocalisedStringProperties::StoreGuiText - Unable to find a UILabel component on '{base.name}'");
		}
	}
}
