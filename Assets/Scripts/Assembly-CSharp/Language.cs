using UnityEngine;

public class Language
{
	public enum ID
	{
		English_UK,
		English_US,
		French,
		Italian,
		German,
		Spanish,
		Portuguese_Brazil,
		Russian,
		Korean,
		Chinese,
		Japanese
	}

	public enum Locale
	{
		Other,
		US,
		Brazil,
		Japan,
		Korea,
		China,
		Noof
	}

	public static string[] CultureInfoIDs = new string[11]
	{
		"en-GB", "en-US", "fr-FR", "it-IT", "de-DE", "es-ES", "pt-BR", "ru-RU", "ko-KR", "zh-CHS",
		"ja-JP"
	};

	private static readonly string[] LanguageExtensions = new string[11]
	{
		"uk", "us", "fr", "it", "ge", "sp", "br", "ru", "ko", "ch",
		"jp"
	};

	public static ID GetLanguage()
	{
		if ((bool)LanguageDebugging.Debugger && LanguageDebugging.Debugger.OverrideSystemLanguage)
		{
			return LanguageDebugging.Debugger.ForcedLanguage;
		}
		SystemLanguage[] array = new SystemLanguage[10]
		{
			SystemLanguage.English,
			SystemLanguage.French,
			SystemLanguage.Italian,
			SystemLanguage.German,
			SystemLanguage.Spanish,
			SystemLanguage.Portuguese,
			SystemLanguage.Russian,
			SystemLanguage.Chinese,
			SystemLanguage.Japanese,
			SystemLanguage.Korean
		};
		SystemLanguage systemLanguage = Application.systemLanguage;
		Locale locale = GetLocale();
		SystemLanguage systemLanguage2 = SystemLanguage.English;
		SystemLanguage[] array2 = array;
		foreach (SystemLanguage systemLanguage3 in array2)
		{
			if (systemLanguage3 == systemLanguage)
			{
				systemLanguage2 = systemLanguage3;
			}
		}
		if (systemLanguage2 == SystemLanguage.Chinese || systemLanguage2 == SystemLanguage.Japanese || systemLanguage2 == SystemLanguage.Korean)
		{
			systemLanguage2 = SystemLanguage.English;
		}
		return GetGameLanguage(systemLanguage2, locale);
	}

	public static Locale GetLocale()
	{
		int num = GetCurrentLocale();
		if (num == 5 || num == 3 || num == 4)
		{
			num = 0;
		}
		return (Locale)num;
	}

	public static string GetExtension()
	{
		ID language = GetLanguage();
		return LanguageExtensions[(int)language];
	}

	public static string GetExtensionGroup()
	{
		string result = string.Empty;
		switch (GetLanguage())
		{
		case ID.English_UK:
		case ID.English_US:
		case ID.French:
		case ID.Italian:
		case ID.German:
		case ID.Spanish:
		case ID.Portuguese_Brazil:
		case ID.Russian:
			result = "efigs";
			break;
		case ID.Korean:
		case ID.Chinese:
		case ID.Japanese:
			result = GetExtension();
			break;
		}
		return result;
	}

	private static ID GetGameLanguage(SystemLanguage unityLanguage, Locale currentLocale)
	{
		ID result = ID.English_UK;
		switch (unityLanguage)
		{
		case SystemLanguage.English:
			result = ID.English_US;
			break;
		case SystemLanguage.French:
			result = ID.French;
			break;
		case SystemLanguage.Italian:
			result = ID.Italian;
			break;
		case SystemLanguage.German:
			result = ID.German;
			break;
		case SystemLanguage.Spanish:
			result = ID.Spanish;
			break;
		case SystemLanguage.Portuguese:
			result = ID.Portuguese_Brazil;
			break;
		case SystemLanguage.Russian:
			result = ID.Russian;
			break;
		case SystemLanguage.Korean:
			result = ID.Korean;
			break;
		case SystemLanguage.Chinese:
			result = ID.Chinese;
			break;
		case SystemLanguage.Japanese:
			result = ID.Japanese;
			break;
		}
		return result;
	}

	private static int GetCurrentLocale()
	{
		AndroidJavaClass androidJavaClass = new AndroidJavaClass("com.hardlightstudio.dev.sonicdash.plugin.SLGlobal");
		return androidJavaClass.CallStatic<int>("GetCurrentLocale", new object[0]);
	}
}
