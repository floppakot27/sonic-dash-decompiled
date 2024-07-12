using UnityEngine;

public class SLAnalytics
{
	private static AndroidJavaClass m_SLAnalytics;

	private static bool m_inSession;

	public static void Start()
	{
		m_SLAnalytics = new AndroidJavaClass("com.hardlightstudio.dev.sonicdash.plugin.SLAnalytics");
		m_SLAnalytics.CallStatic("StartSession");
		m_inSession = true;
	}

	public static void OnFocus(bool gainFocus)
	{
		if (m_SLAnalytics != null)
		{
			if (gainFocus)
			{
				m_SLAnalytics.CallStatic("StartSession");
				m_inSession = true;
			}
			else
			{
				m_SLAnalytics.CallStatic("EndSession");
				m_inSession = false;
			}
		}
	}

	public static void Update()
	{
	}

	public static void AddParameter(string Key, string Value)
	{
		if (m_SLAnalytics != null)
		{
			m_SLAnalytics.CallStatic("AnalyticsAddParameter", Key, Value);
		}
	}

	public static void LogTrackingEvent(string EventName, string Value)
	{
		if (m_SLAnalytics != null)
		{
			m_SLAnalytics.CallStatic("AnalyticsLogTrackEvent", EventName, Value);
		}
	}

	public static void LogEvent(string EventName)
	{
		if (m_SLAnalytics != null)
		{
			m_SLAnalytics.CallStatic("AnalyticsLogEvent", EventName);
		}
	}

	public static void LogEventWithParameters(string EventName)
	{
		if (m_SLAnalytics != null)
		{
			m_SLAnalytics.CallStatic("AnalyticsLogEventWithParameters", EventName);
		}
	}
}
