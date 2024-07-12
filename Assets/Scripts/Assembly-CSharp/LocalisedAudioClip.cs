using System;
using UnityEngine;

[Serializable]
public class LocalisedAudioClip
{
	[Serializable]
	public class AudioOverride
	{
		[SerializeField]
		public bool m_enabled;

		[SerializeField]
		public string m_assetName;
	}

	[SerializeField]
	private string m_assetName;

	[SerializeField]
	private AudioOverride m_englishOverride;

	[SerializeField]
	private AudioOverride m_frenchOverride;

	[SerializeField]
	private AudioOverride m_italianOverride;

	[SerializeField]
	private AudioOverride m_germanOverride;

	[SerializeField]
	private AudioOverride m_spanishOverride;

	[SerializeField]
	private AudioOverride m_japaneseOverride;

	private AudioClip m_audioClip;

	public AudioClip GetAudioClip()
	{
		if (null == m_audioClip)
		{
			Language.ID language = Language.GetLanguage();
			string arg = "EN";
			string assetName = m_assetName;
			switch (language)
			{
			case Language.ID.English_UK:
			case Language.ID.English_US:
			case Language.ID.Portuguese_Brazil:
			case Language.ID.Russian:
			case Language.ID.Korean:
			case Language.ID.Chinese:
				arg = "EN";
				if (m_englishOverride.m_enabled)
				{
					assetName = m_englishOverride.m_assetName;
				}
				break;
			case Language.ID.French:
				arg = "FR";
				if (m_frenchOverride.m_enabled)
				{
					assetName = m_frenchOverride.m_assetName;
				}
				break;
			case Language.ID.Italian:
				arg = "IT";
				if (m_italianOverride.m_enabled)
				{
					assetName = m_italianOverride.m_assetName;
				}
				break;
			case Language.ID.German:
				arg = "DE";
				if (m_germanOverride.m_enabled)
				{
					assetName = m_germanOverride.m_assetName;
				}
				break;
			case Language.ID.Spanish:
				arg = "ES";
				if (m_spanishOverride.m_enabled)
				{
					assetName = m_spanishOverride.m_assetName;
				}
				break;
			case Language.ID.Japanese:
				arg = "JP";
				if (m_japaneseOverride.m_enabled)
				{
					assetName = m_japaneseOverride.m_assetName;
				}
				break;
			}
			if (m_assetName.Length > 0)
			{
				string path = $"Localisation/Audio/{arg}/{m_assetName}";
				m_audioClip = Resources.Load(path) as AudioClip;
			}
		}
		return m_audioClip;
	}
}
