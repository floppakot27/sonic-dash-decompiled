using System;
using UnityEngine;

public class ObjectMonitor : MonoBehaviour
{
	[Flags]
	public enum PlugIns
	{
		MoreGames = 1,
		Social = 2,
		Offers = 4,
		Videos = 8,
		SegaNetwork = 0x10
	}

	public enum Region
	{
		Any,
		JapanOnly,
		ExcludeJapan
	}

	private delegate bool PlugInAvailable();

	[SerializeField]
	private PlugIns m_plugins;

	[SerializeField]
	private BuildConfiguration.Build m_buildConfigs = (BuildConfiguration.Build)(-1);

	[SerializeField]
	private SupportedDevices.iOS m_iOSDevices = (SupportedDevices.iOS)(-1);

	[SerializeField]
	private bool m_destoryIfNotNeeded;

	[SerializeField]
	private GameObject m_target;

	[SerializeField]
	private Region m_limitedRegion;

	[SerializeField]
	private bool m_enableInEditor = true;

	private static PlugInAvailable[] s_plugInDelegates = new PlugInAvailable[5]
	{
		SLCachedQueries.IsMoreGamesAvailable,
		SLCachedQueries.IsAvailable,
		SLCachedQueries.IsGameOffersAvailable,
		SLCachedQueries.IsVideoAvailable,
		IsSegaNetworksAvailable
	};

	public static bool CheckPlugInsAvailable(PlugIns requiredPlugIns)
	{
		for (int i = 0; i < s_plugInDelegates.Length; i++)
		{
			PlugIns plugIns = (PlugIns)(1 << i);
			PlugInAvailable plugInAvailable = s_plugInDelegates[i];
			if ((requiredPlugIns & plugIns) == plugIns && !plugInAvailable())
			{
				return false;
			}
		}
		return true;
	}

	public static bool CheckiOSPlatform(SupportedDevices.iOS requiredDevices)
	{
		return true;
	}

	public static bool CheckBuildConfig(BuildConfiguration.Build requiredConfigs)
	{
		if ((requiredConfigs & BuildConfiguration.Build.Distribution) != BuildConfiguration.Build.Distribution)
		{
			return false;
		}
		return true;
	}

	public static bool CheckRegion(Region expectedRegion)
	{
		if (expectedRegion == Region.Any)
		{
			return true;
		}
		Language.Locale locale = Language.GetLocale();
		Language.ID language = Language.GetLanguage();
		if (locale == Language.Locale.Japan || language == Language.ID.Japanese)
		{
			if (expectedRegion == Region.JapanOnly)
			{
				return true;
			}
		}
		else if (expectedRegion == Region.ExcludeJapan)
		{
			return true;
		}
		return false;
	}

	private void Start()
	{
		if (m_target == null)
		{
			m_target = base.gameObject;
		}
		if (m_destoryIfNotNeeded && !IsObjectNeeded())
		{
			UnityEngine.Object.Destroy(m_target);
			base.enabled = false;
		}
	}

	private void Update()
	{
		if (!GameState.IsAvailable || (GameState.GetMode() != GameState.Mode.Game && GameState.GetMode() != GameState.Mode.PauseMenu))
		{
			bool flag = IsObjectNeeded();
			if (flag != m_target.activeInHierarchy)
			{
				m_target.SetActive(flag);
			}
		}
	}

	private bool IsObjectNeeded()
	{
		bool flag = true;
		if (flag)
		{
			flag &= CheckBuildConfig(m_buildConfigs);
		}
		if (flag)
		{
			flag &= CheckiOSPlatform(m_iOSDevices);
		}
		if (flag)
		{
			flag &= CheckPlugInsAvailable(m_plugins);
		}
		if (flag)
		{
			flag &= CheckRegion(m_limitedRegion);
		}
		return flag;
	}

	private static bool IsSegaNetworksAvailable()
	{
		PlayerStats.Stats currentStats = PlayerStats.GetCurrentStats();
		if (currentStats.m_trackedStats[60] != 0)
		{
			return false;
		}
		return true;
	}
}
