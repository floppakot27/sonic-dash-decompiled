using System;
using System.Collections.Generic;
using UnityEngine;

public class BundleAnalytics : MonoBehaviour
{
	[Flags]
	private enum AnalyticsPosted
	{
		ConnectionNotAvailable = 1,
		ConnectionWarningShown = 2,
		ExitingBundlesCached = 4,
		OnlineBundlesParsed = 8,
		DownloadProcessStarted = 0x10,
		DownloadProcessFinished = 0x20
	}

	private AnalyticsPosted m_postedAnalytics;

	public void ConnectionNotAvaiilable()
	{
		if ((m_postedAnalytics & AnalyticsPosted.ConnectionNotAvailable) != AnalyticsPosted.ConnectionNotAvailable)
		{
			m_postedAnalytics |= AnalyticsPosted.ConnectionNotAvailable;
			SLAnalytics.LogEvent("Bundles_ConnectionNotAvaiilable");
		}
	}

	public void ConnectionWarningShown()
	{
		if ((m_postedAnalytics & AnalyticsPosted.ConnectionWarningShown) != AnalyticsPosted.ConnectionWarningShown)
		{
			m_postedAnalytics |= AnalyticsPosted.ConnectionWarningShown;
			SLAnalytics.LogEvent("Bundles_ConnectionWarningShown");
		}
	}

	public void DownloadProcessStarted()
	{
		if ((m_postedAnalytics & AnalyticsPosted.DownloadProcessStarted) != AnalyticsPosted.DownloadProcessStarted)
		{
			m_postedAnalytics |= AnalyticsPosted.DownloadProcessStarted;
			SLAnalytics.LogEvent("Bundles_DownloadProcessStarted");
		}
	}

	public void DownloadProcessFinished(bool completedSuccessfully)
	{
		if ((m_postedAnalytics & AnalyticsPosted.DownloadProcessFinished) != AnalyticsPosted.DownloadProcessFinished)
		{
			m_postedAnalytics |= AnalyticsPosted.DownloadProcessFinished;
			SLAnalytics.AddParameter("Success", completedSuccessfully.ToString());
			SLAnalytics.LogEventWithParameters("Bundles_DownloadProcessFinished");
		}
	}

	public void ExistingBundlesCached(List<BundleState.Bundle> existingBundles, string existingBundlesVersion)
	{
		if ((m_postedAnalytics & AnalyticsPosted.ExitingBundlesCached) != AnalyticsPosted.ExitingBundlesCached)
		{
			m_postedAnalytics |= AnalyticsPosted.ExitingBundlesCached;
			int num = existingBundles?.Count ?? 0;
			string value = ((!string.IsNullOrEmpty(existingBundlesVersion)) ? existingBundlesVersion : "Unknown");
			SLAnalytics.AddParameter("Cached Bundles", num.ToString());
			SLAnalytics.AddParameter("Bundle Version", value);
			SLAnalytics.LogEventWithParameters("Bundles_ExitingBundlesCached");
		}
	}

	public void OnlineBundlesParsed(List<BundleState.Bundle> bundleList, string onlineBundleVersion)
	{
		if ((m_postedAnalytics & AnalyticsPosted.OnlineBundlesParsed) != AnalyticsPosted.OnlineBundlesParsed)
		{
			m_postedAnalytics |= AnalyticsPosted.OnlineBundlesParsed;
			int num = bundleList?.Count ?? 0;
			string value = ((!string.IsNullOrEmpty(onlineBundleVersion)) ? onlineBundleVersion : "Unknown");
			SLAnalytics.AddParameter("Online Bundles", num.ToString());
			SLAnalytics.AddParameter("Bundle Version", value);
			SLAnalytics.LogEventWithParameters("Bundles_OnlineBundlesParsed");
		}
	}

	public void BundleDownloadStarted(string bundleName, bool cached)
	{
		SLAnalytics.AddParameter("Bundle Name", bundleName);
		SLAnalytics.AddParameter("Cached", cached.ToString());
		SLAnalytics.LogEventWithParameters("Bundles_BundleDownloadStarted");
	}

	public void BundleDownloadFinished(string bundleName, bool cached)
	{
		SLAnalytics.AddParameter("Bundle Name", bundleName);
		SLAnalytics.AddParameter("Cached", cached.ToString());
		SLAnalytics.LogEventWithParameters("Bundles_BundleDownloadFinished");
	}

	public void DownloadError(string errorType)
	{
		SLAnalytics.AddParameter("Error Region", errorType);
		SLAnalytics.LogEventWithParameters("Bundles_DownloadError");
	}
}
