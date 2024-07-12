using System;
using System.Globalization;
using UnityEngine;

public class AdsDisplay : MonoBehaviour
{
	public enum Regions
	{
		MainMenu,
		ResultScreen
	}

	[Flags]
	private enum State
	{
		Active = 1,
		Initialised = 2
	}

	private const string AdsPeriodStartProperty = "AdsPeriodStart";

	private const string AdsInPerdiodProperty = "AdsInPeriod";

	private const string AdsCountMMProperty = "AdsCountMM";

	private const string AdsCountResultProperty = "AdsCountResult";

	private State m_state;

	[SerializeField]
	private Regions m_region;

	[SerializeField]
	private bool m_countsForLimit = true;

	private static DateTime s_periodStart;

	private static int s_asdInPeriod;

	private static int s_mmCount = 1;

	private static int s_resultCount = 1;

	public bool Active => (m_state & State.Active) == State.Active;

	public static void Start(Regions region, bool countForLimit)
	{
		string placement = "AdSpace" + region;
		SLAds.ShowIntersitialAd(placement);
		if (countForLimit)
		{
			s_asdInPeriod++;
		}
	}

	public static void End(Regions region)
	{
	}

	public void Visit()
	{
		InitialiseRegion();
		bool flag = !PaidUser.Paid && !PaidUser.RemovedAds;
		if ((flag & AdvertStates.AdsEnabled) && ShowAdd(m_region))
		{
			m_state |= State.Active;
			OnRegionActivated();
		}
	}

	public void Leave()
	{
		if ((m_state & State.Active) == State.Active)
		{
			End(m_region);
			m_state &= ~State.Active;
			OnRegionDectivated();
		}
	}

	private void OnEnable()
	{
		Visit();
	}

	private void OnDisable()
	{
		Leave();
	}

	protected virtual void OnRegionActivated()
	{
	}

	protected virtual void OnRegionDectivated()
	{
	}

	private void InitialiseRegion()
	{
		if ((m_state & State.Initialised) != State.Initialised)
		{
			EventDispatch.RegisterInterest("OnGameDataSaveRequest", this);
			EventDispatch.RegisterInterest("OnGameDataLoaded", this);
			m_state |= State.Initialised;
		}
	}

	private bool ShowAdd(Regions region)
	{
		if ((DCTime.GetCurrentTime() - s_periodStart).TotalHours > (double)AdvertStates.AdsLimitPeriod)
		{
			ChangePeriod();
		}
		switch (region)
		{
		case Regions.MainMenu:
			if (AdvertStates.AdsMainMenuRate == 0)
			{
				return false;
			}
			if (s_mmCount % AdvertStates.AdsMainMenuRate == 0)
			{
				Start(region, m_countsForLimit);
			}
			s_mmCount++;
			return true;
		case Regions.ResultScreen:
			if (AdvertStates.AdsResultScreenRate == 0)
			{
				return false;
			}
			if (s_resultCount % AdvertStates.AdsResultScreenRate == 0)
			{
				Start(region, m_countsForLimit);
			}
			s_resultCount++;
			return true;
		default:
			return false;
		}
	}

	private void ChangePeriod()
	{
		s_periodStart = DCTime.GetCurrentTime();
		s_asdInPeriod = 0;
		s_mmCount = 1;
		s_resultCount = 1;
	}

	private void Event_OnGameDataSaveRequest()
	{
		CultureInfo cultureInfo = new CultureInfo("en-US");
		PropertyStore.Store("AdsPeriodStart", s_periodStart.ToString(cultureInfo.DateTimeFormat));
		PropertyStore.Store("AdsInPeriod", s_asdInPeriod);
		PropertyStore.Store("AdsCountMM", s_mmCount);
		PropertyStore.Store("AdsCountResult", s_resultCount);
	}

	private void Event_OnGameDataLoaded(ActiveProperties activeProperties)
	{
		CultureInfo provider = new CultureInfo("en-US");
		if (!DateTime.TryParse(activeProperties.GetString("AdsPeriodStart"), provider, DateTimeStyles.None, out s_periodStart))
		{
			s_periodStart = DCTime.GetCurrentTime().AddDays(-1.0);
		}
		s_asdInPeriod = activeProperties.GetInt("AdsInPeriod");
		s_mmCount = activeProperties.GetInt("AdsCountMM");
		s_resultCount = activeProperties.GetInt("AdsCountResult");
	}
}
