using System.Collections.Generic;
using UnityEngine;

public class QualityLevels : MonoBehaviour
{
	[SerializeField]
	private List<SupportedDevices.Support> m_qualityLevels;

	private void Awake()
	{
		SetQualityLevel();
	}

	private void SetQualityLevel()
	{
		int androidQualityLevel = GetAndroidQualityLevel();
		QualitySettings.SetQualityLevel(androidQualityLevel, applyExpensiveChanges: true);
	}

	private void EnsureSupportStateIsValid()
	{
		if (m_qualityLevels == null)
		{
			int num = QualitySettings.names.Length;
			m_qualityLevels = new List<SupportedDevices.Support>(num);
			for (int i = 0; i < num; i++)
			{
				m_qualityLevels[i] = new SupportedDevices.Support();
			}
		}
	}

	private int GetAndroidQualityLevel()
	{
		for (int i = 0; i < m_qualityLevels.Count; i++)
		{
			SupportedDevices.Support support = m_qualityLevels[i];
			if (support.m_androidSupport)
			{
				return i;
			}
		}
		return m_qualityLevels.Count - 1;
	}
}
