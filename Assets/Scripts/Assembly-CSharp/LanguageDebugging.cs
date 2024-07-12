using System.Text.RegularExpressions;
using UnityEngine;

public class LanguageDebugging : MonoBehaviour
{
	private static LanguageDebugging s_debugger;

	private Color m_erroredLabel = new Color(1f, 0f, 1f, 1f);

	[SerializeField]
	private bool m_highlightUnlocalisedString;

	[SerializeField]
	private bool m_overrideSystemLanguage;

	[SerializeField]
	private Language.ID m_forcedLanguage;

	public bool HighlightUnlocalisedString
	{
		get
		{
			return m_highlightUnlocalisedString;
		}
		set
		{
			m_highlightUnlocalisedString = value;
		}
	}

	public bool OverrideSystemLanguage
	{
		get
		{
			return m_overrideSystemLanguage;
		}
		set
		{
			m_overrideSystemLanguage = value;
		}
	}

	public Language.ID ForcedLanguage
	{
		get
		{
			return m_forcedLanguage;
		}
		set
		{
			m_forcedLanguage = value;
		}
	}

	public static LanguageDebugging Debugger
	{
		get
		{
			if (s_debugger == null)
			{
				s_debugger = Object.FindObjectOfType(typeof(LanguageDebugging)) as LanguageDebugging;
			}
			return s_debugger;
		}
	}

	public void ReloadLanguageAssets()
	{
		LanguageStrings[] array = Object.FindObjectsOfType(typeof(LanguageStrings)) as LanguageStrings[];
		LanguageStrings[] array2 = array;
		foreach (LanguageStrings languageStrings in array2)
		{
			languageStrings.ReloadStringFile();
		}
		AtlasLoader[] array3 = Object.FindObjectsOfType(typeof(AtlasLoader)) as AtlasLoader[];
		AtlasLoader[] array4 = array3;
		foreach (AtlasLoader atlasLoader in array4)
		{
			atlasLoader.UpdateAllAtlases();
		}
		LocalisedStringStatic[] array5 = Resources.FindObjectsOfTypeAll(typeof(LocalisedStringStatic)) as LocalisedStringStatic[];
		LocalisedStringStatic[] array6 = array5;
		foreach (LocalisedStringStatic localisedStringStatic in array6)
		{
			localisedStringStatic.ForceStringUpdate();
		}
	}

	public void ForceLanguage(Language.ID languageToUse)
	{
		OverrideSystemLanguage = true;
		ForcedLanguage = languageToUse;
		ReloadLanguageAssets();
	}

	private void Start()
	{
		EventDispatch.RegisterInterest("EnableUnlocalisedTextHighlight", this);
	}

	private void Update()
	{
		if (m_highlightUnlocalisedString)
		{
			ColourNonLocalisedUILabels();
		}
	}

	private void ColourNonLocalisedUILabels()
	{
		UILabel[] array = Object.FindObjectsOfType(typeof(UILabel)) as UILabel[];
		UILabel[] array2 = array;
		foreach (UILabel uILabel in array2)
		{
			LocalisedStringProperties component = uILabel.GetComponent<LocalisedStringProperties>();
			if (component == null && IsStringValidForLocalisation(uILabel.text))
			{
				ColourLabel(uILabel);
			}
		}
	}

	private bool IsStringValidForLocalisation(string thisString)
	{
		bool flag = Regex.IsMatch(thisString, "^\\d+$");
		return !flag;
	}

	private void ColourLabel(UILabel thisLabel)
	{
		Color erroredLabel = m_erroredLabel;
		erroredLabel.a = thisLabel.color.a;
		thisLabel.color = erroredLabel;
	}

	private void Event_EnableUnlocalisedTextHighlight(bool enable)
	{
		m_highlightUnlocalisedString = enable;
	}
}
