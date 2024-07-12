using System;
using System.Globalization;
using UnityEngine;

public class RSRGenerator : MonoBehaviour
{
	private const string PropertyNumberOfRSRCollectedToday = "RSR_NumberOfRSRCollectedToday";

	private const string PropertyDailyRuns = "RSR_DailyRuns";

	private const string PropertyRunLastCollectedRSR = "RSR_RunLastCollectedRSR";

	private const string PropertyNextRunToSpawn = "RSR_NextRunToSpawn";

	private const string PropertyToday = "RSR_Today";

	private static RSRGenerator s_singleton;

	[SerializeField]
	private int m_runsBeforeFirstRSR;

	[SerializeField]
	private int m_spreadOfRunsToAppearFirst;

	[SerializeField]
	private int m_runsBetweenRSR;

	[SerializeField]
	private int m_spreadOfRunsToAppear;

	[SerializeField]
	private int m_maxDailyRSR;

	[SerializeField]
	private AnimationCurve m_chanceToSpawnByDistance;

	private bool m_spawnedRSRThisTrack;

	private bool m_collectedRSRThisRun;

	private int m_numberOfRSRCollectedToday;

	private int m_dailyRuns;

	private int m_runLastCollectedRSR = -1;

	private int m_nextRunToSpawn = -1;

	private DateTime m_today;

	public static bool RSRSpawned
	{
		get
		{
			return s_singleton.m_spawnedRSRThisTrack;
		}
		set
		{
			s_singleton.m_spawnedRSRThisTrack = value;
		}
	}

	public static bool CanSpawnRSR()
	{
		if (s_singleton.m_spawnedRSRThisTrack || s_singleton.m_collectedRSRThisRun || s_singleton.m_numberOfRSRCollectedToday >= s_singleton.m_maxDailyRSR)
		{
			return false;
		}
		if (s_singleton.m_dailyRuns >= s_singleton.m_nextRunToSpawn)
		{
			float distance = PlayerStats.GetCurrentStats().m_trackedDistances[7];
			float spawnChance = s_singleton.GetSpawnChance(distance);
			float value = UnityEngine.Random.value;
			return value <= spawnChance;
		}
		return false;
	}

	public static void RSRCollected()
	{
		s_singleton.m_collectedRSRThisRun = true;
		s_singleton.m_runLastCollectedRSR = s_singleton.m_dailyRuns;
		s_singleton.m_nextRunToSpawn = s_singleton.m_runLastCollectedRSR + 1 + s_singleton.m_runsBetweenRSR + UnityEngine.Random.Range(0, s_singleton.m_spreadOfRunsToAppear);
		s_singleton.m_numberOfRSRCollectedToday++;
	}

	private void Start()
	{
		s_singleton = this;
		EventDispatch.RegisterInterest("OnGameDataLoaded", this);
		EventDispatch.RegisterInterest("OnGameDataSaveRequest", this);
		EventDispatch.RegisterInterest("OnGameFinished", this);
		EventDispatch.RegisterInterest("ABTestingReady", this);
	}

	private void Event_ABTestingReady()
	{
		int testValue = ABTesting.GetTestValue(ABTesting.Tests.RSR_RunsBefore1);
		if (testValue != -1)
		{
			m_runsBeforeFirstRSR = testValue;
		}
		testValue = ABTesting.GetTestValue(ABTesting.Tests.RSR_Spread1);
		if (testValue != -1)
		{
			m_spreadOfRunsToAppearFirst = testValue;
		}
		testValue = ABTesting.GetTestValue(ABTesting.Tests.RSR_RunsBeforeNext);
		if (testValue != -1)
		{
			m_runsBetweenRSR = testValue;
		}
		testValue = ABTesting.GetTestValue(ABTesting.Tests.RSR_SpreadNext);
		if (testValue != -1)
		{
			m_spreadOfRunsToAppear = testValue;
		}
		testValue = ABTesting.GetTestValue(ABTesting.Tests.RSR_MaxDaily);
		if (testValue != -1)
		{
			m_maxDailyRSR = testValue;
		}
	}

	private void Event_OnGameDataSaveRequest()
	{
		PropertyStore.Store("RSR_NumberOfRSRCollectedToday", m_numberOfRSRCollectedToday);
		PropertyStore.Store("RSR_DailyRuns", m_dailyRuns);
		PropertyStore.Store("RSR_RunLastCollectedRSR", m_runLastCollectedRSR);
		PropertyStore.Store("RSR_NextRunToSpawn", m_nextRunToSpawn);
		CultureInfo cultureInfo = new CultureInfo("en-US");
		PropertyStore.Store("RSR_Today", m_today.Date.ToString(cultureInfo.DateTimeFormat));
	}

	private void Event_OnGameDataLoaded(ActiveProperties activeProperties)
	{
		m_numberOfRSRCollectedToday = activeProperties.GetInt("RSR_NumberOfRSRCollectedToday");
		m_dailyRuns = activeProperties.GetInt("RSR_DailyRuns");
		if (activeProperties.DoesPropertyExist("RSR_RunLastCollectedRSR"))
		{
			m_runLastCollectedRSR = activeProperties.GetInt("RSR_RunLastCollectedRSR");
		}
		if (activeProperties.DoesPropertyExist("RSR_NextRunToSpawn"))
		{
			m_nextRunToSpawn = activeProperties.GetInt("RSR_NextRunToSpawn");
		}
		CultureInfo provider = new CultureInfo("en-US");
		if (!DateTime.TryParse(activeProperties.GetString("RSR_Today"), provider, DateTimeStyles.None, out m_today))
		{
			m_today = DateTime.UtcNow.AddYears(-10).Date;
		}
		if (NeedToChangeDay())
		{
			ChangeDay();
		}
		else if (m_dailyRuns == 0)
		{
			m_nextRunToSpawn = m_runsBeforeFirstRSR + UnityEngine.Random.Range(0, m_spreadOfRunsToAppearFirst);
		}
	}

	private void Event_OnGameFinished()
	{
		m_dailyRuns++;
		s_singleton.m_collectedRSRThisRun = false;
		if (NeedToChangeDay())
		{
			ChangeDay();
		}
	}

	private bool NeedToChangeDay()
	{
		if (!DCTimeValidation.TrustedTime)
		{
			return false;
		}
		DateTime date = m_today.AddDays(1.0).Date;
		return (date - DCTime.GetCurrentTime()).TotalSeconds <= 0.0;
	}

	private void ChangeDay()
	{
		if (DCTimeValidation.TrustedTime)
		{
			m_today = DCTime.GetCurrentTime().Date;
			m_spawnedRSRThisTrack = false;
			m_collectedRSRThisRun = false;
			m_numberOfRSRCollectedToday = 0;
			m_dailyRuns = 0;
			m_runLastCollectedRSR = -1;
			m_nextRunToSpawn = m_runsBeforeFirstRSR + UnityEngine.Random.Range(0, m_spreadOfRunsToAppearFirst);
		}
	}

	private float GetSpawnChance(float distance)
	{
		float time = distance / 10000f;
		return m_chanceToSpawnByDistance.Evaluate(time);
	}
}
