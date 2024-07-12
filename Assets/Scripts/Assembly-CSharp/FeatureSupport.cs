using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FeatureSupport : MonoBehaviour
{
	private static FeatureSupport s_featureSupport;

	[SerializeField]
	private List<SupportedDevices.Support> m_supportedFeatureList;

	public static bool IsSupported(string supportID)
	{
		if (s_featureSupport == null)
		{
			return ReturnUnknownIDSupport();
		}
		return s_featureSupport.IsFeatureSupported(supportID);
	}

	public static bool IsLowEndDevice()
	{
		return false;
	}

	private static bool ReturnUnknownIDSupport()
	{
		return false;
	}

	private void Awake()
	{
		s_featureSupport = this;
		EnsureSupportStateIsValid();
		GenerateFeatureCRCs();
	}

	private bool IsFeatureSupported(string supportID)
	{
		uint supportIdCRC = CRC32.Generate(supportID, CRC32.Case.Lower);
		SupportedDevices.Support supportState = m_supportedFeatureList.FirstOrDefault((SupportedDevices.Support thisEntry) => thisEntry.m_supportIdCRC == supportIdCRC);
		return CheckAndroidSupport(supportState);
	}

	private void EnsureSupportStateIsValid()
	{
		if (m_supportedFeatureList == null)
		{
			m_supportedFeatureList = new List<SupportedDevices.Support>();
		}
	}

	private void GenerateFeatureCRCs()
	{
		foreach (SupportedDevices.Support supportedFeature in m_supportedFeatureList)
		{
			uint supportIdCRC = CRC32.Generate(supportedFeature.m_supportID, CRC32.Case.Lower);
			supportedFeature.m_supportIdCRC = supportIdCRC;
		}
	}

	private bool CheckAndroidSupport(SupportedDevices.Support supportState)
	{
		return supportState.m_androidSupport;
	}
}
