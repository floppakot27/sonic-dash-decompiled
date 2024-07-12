using System;
using UnityEngine;

public class DialogContent_RateMe : MonoBehaviour
{
	private const string RateMeCanShowPropString = "RateMeCanShow";

	private const string RateMeVersion = "RateMeVersion";

	private const string RateMeTimeTriggeredPropString = "RateMeTimeTriggered";

	private const string RateMeTimesShown = "RateMeTimesShown";

	private const string RateMeIsFirst = "RateMeIsFirst";

	private static DialogContent_RateMe s_instance;

	public float m_initialPeriodInHours = 0.33f;

	public float m_remindPeriodInHours = 1f;

	public float m_minRunTimeInMinutes = 1f;

	private bool m_canShow = true;

	private float m_timeReminderTriggered;

	private int m_timesShown;

	private bool m_isFirst = true;

	public static DialogContent_RateMe Instance => s_instance;

	public static bool IsFirst => s_instance.m_isFirst;

	private void Awake()
	{
		s_instance = this;
	}

	private void Start()
	{
		EventDispatch.RegisterInterest("OnGameDataLoaded", this);
		EventDispatch.RegisterInterest("OnGameDataSaveRequest", this);
	}

	public bool validToDisplay()
	{
		if (Internet.ConnectionAvailable() && m_canShow && getMinutesLastRun() >= m_minRunTimeInMinutes)
		{
			if (m_timeReminderTriggered == 0f && getHoursPlayed() >= m_initialPeriodInHours)
			{
				return true;
			}
			if (getHoursPlayed() - m_timeReminderTriggered >= m_remindPeriodInHours)
			{
				return true;
			}
		}
		return false;
	}

	public void Trigger_NoThanks()
	{
		m_canShow = false;
		m_timesShown++;
		if (m_timesShown == 1)
		{
			GameAnalytics.RateMeDialogShownFirstTime(GameAnalytics.RateMeButtons.Never);
		}
		else
		{
			GameAnalytics.RateMeDialogShown(GameAnalytics.RateMeButtons.Never, m_timesShown);
		}
	}

	public void Trigger_Remind()
	{
		m_timeReminderTriggered = getHoursPlayed();
		m_timesShown++;
		if (m_timesShown == 1)
		{
			GameAnalytics.RateMeDialogShownFirstTime(GameAnalytics.RateMeButtons.Remember);
		}
		else
		{
			GameAnalytics.RateMeDialogShown(GameAnalytics.RateMeButtons.Remember, m_timesShown);
		}
	}

	public void Trigger_Yes()
	{
		m_canShow = false;
		SLPlugin.OpenRatePage();
		if (m_isFirst)
		{
			StorePurchases.RequestReward("Respawn", 5, 8, StorePurchases.ShowDialog.Yes);
		}
		else
		{
			StorePurchases.RequestReward("Respawn", 1, 8, StorePurchases.ShowDialog.Yes);
		}
		m_isFirst = false;
		m_timesShown++;
		if (m_timesShown == 1)
		{
			GameAnalytics.RateMeDialogShownFirstTime(GameAnalytics.RateMeButtons.Ok);
		}
		else
		{
			GameAnalytics.RateMeDialogShown(GameAnalytics.RateMeButtons.Ok, m_timesShown);
		}
	}

	private float getHoursStat(PlayerStats.StatNames statName)
	{
		double value = (double)PlayerStats.GetCurrentStats().m_trackedStats[(int)statName] * 100.0;
		return (float)TimeSpan.FromMilliseconds(value).TotalHours;
	}

	private float getMinutesLastRun()
	{
		return getHoursStat(PlayerStats.StatNames.TimePlayed_Run) * 60f;
	}

	private float getHoursPlayed()
	{
		float hoursStat = getHoursStat(PlayerStats.StatNames.TimePlayed_Total);
		return hoursStat + (float)TimeSpan.FromSeconds((int)PlayerStats.GetSecondsFromLastSaved()).TotalHours;
	}

	private void Event_OnGameDataSaveRequest()
	{
		PropertyStore.Store("RateMeCanShow", m_canShow ? 1 : 0);
		PropertyStore.Store("RateMeVersion", "1.8.0");
		PropertyStore.Store("RateMeTimeTriggered", m_timeReminderTriggered);
		PropertyStore.Store("RateMeTimesShown", m_timesShown);
		PropertyStore.Store("RateMeIsFirst", m_isFirst);
	}

	private void Event_OnGameDataLoaded(ActiveProperties activeProperties)
	{
		m_canShow = true;
		m_timeReminderTriggered = 0f;
		string text = string.Empty;
		if (activeProperties.DoesPropertyExist("RateMeVersion"))
		{
			text = activeProperties.GetString("RateMeVersion");
		}
		if (activeProperties.DoesPropertyExist("RateMeCanShow"))
		{
			m_canShow = activeProperties.GetInt("RateMeCanShow") > 0 || text != "1.8.0";
		}
		if (activeProperties.DoesPropertyExist("RateMeTimeTriggered"))
		{
			m_timeReminderTriggered = activeProperties.GetFloat("RateMeTimeTriggered");
		}
		if (activeProperties.DoesPropertyExist("RateMeTimesShown"))
		{
			m_timesShown = activeProperties.GetInt("RateMeTimesShown");
		}
		if (activeProperties.DoesPropertyExist("RateMeIsFirst"))
		{
			m_isFirst = activeProperties.GetBool("RateMeIsFirst");
		}
	}
}
