using System.Collections.Generic;
using Localisation;
using UnityEngine;

public class LanguageStrings : MonoBehaviour
{
	private const string LocalisationResourcePath = "Localisation/";

	[SerializeField]
	private string m_stringFile;

	private Strings m_strings = new Strings();

	public static List<LanguageStrings> StringResources { get; private set; }

	public static LanguageStrings First => (StringResources != null) ? StringResources[0] : null;

	public string GetString(string id)
	{
		return m_strings.GetString(id);
	}

	public string[] GetAllStrings()
	{
		TextAsset textAsset = LoadLanguageFile(loadPlatformText: false);
		string systemLanguage = GetSystemLanguage();
		string[] strings = null;
		string[] identifiers = null;
		bool stringEntries = m_strings.GetStringEntries(textAsset, systemLanguage, ref identifiers, ref strings);
		UnloadLanguageFile(textAsset);
		return (!stringEntries) ? null : strings;
	}

	public void ReloadStringFile()
	{
		string systemLanguage = GetSystemLanguage();
		TextAsset textAsset = LoadLanguageFile(loadPlatformText: false);
		if (textAsset != null)
		{
			m_strings.LoadXMLStringsFile(textAsset, systemLanguage, Strings.Type.Primary);
			UnloadLanguageFile(textAsset);
		}
		TextAsset textAsset2 = LoadLanguageFile(loadPlatformText: true);
		if (textAsset2 != null)
		{
			m_strings.LoadXMLStringsFile(textAsset2, systemLanguage, Strings.Type.Platform);
			UnloadLanguageFile(textAsset2);
		}
	}

	private void Start()
	{
		Object.DontDestroyOnLoad(this);
		StoreThisInList();
		ReloadStringFile();
	}

	private void OnNukingLevel()
	{
		StringResources = null;
	}

	private TextAsset LoadLanguageFile(bool loadPlatformText)
	{
		TextAsset textAsset = null;
		if (loadPlatformText)
		{
			string path = "Localisation/" + m_stringFile + Platform.PlatformPostFix;
			return Resources.Load(path) as TextAsset;
		}
		string path2 = "Localisation/" + m_stringFile + Platform.CommonPostFix;
		return Resources.Load(path2) as TextAsset;
	}

	private void UnloadLanguageFile(TextAsset languageFile)
	{
		if (languageFile != null)
		{
			Resources.UnloadAsset(languageFile);
		}
	}

	private string GetSystemLanguage()
	{
		return Language.GetLanguage().ToString();
	}

	private void StoreThisInList()
	{
		if (StringResources == null)
		{
			StringResources = new List<LanguageStrings>();
		}
		if (!StringResources.Contains(this))
		{
			StringResources.Add(this);
		}
	}
}
