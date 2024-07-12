using System;
using System.Globalization;
using UnityEngine;

public class WheelOfFortuneSettings : MonoBehaviour
{
	private const int OneDayInSeconds = 86400;

	private DateTime m_lastFreeSpinTime;

	private bool m_logMissedSpins;

	[SerializeField]
	private int m_spinCost;

	[SerializeField]
	private UILabel m_spinCostLabel;

	[SerializeField]
	private GameObject[] m_freeSpinText = new GameObject[2];

	public static WheelOfFortuneSettings Instance { get; private set; }

	public bool HasFreeSpin { get; private set; }

	public bool KnowAboutFreeSpin { get; set; }

	public bool FirstFreeSpinAvailable { get; private set; }

	public bool FirstFreeSpinTrusted { get; private set; }

	public int SpinCost => m_spinCost;

	private void Awake()
	{
		if (Instance != null && Instance != this)
		{
			UnityEngine.Object.Destroy(this);
		}
		else
		{
			Instance = this;
		}
		EventDispatch.RegisterInterest("OnGameDataLoaded", this);
		EventDispatch.RegisterInterest("OnGameDataSaveRequest", this);
		EventDispatch.RegisterInterest("OnStoreInitialised", this);
		m_spinCostLabel.text = m_spinCost.ToString();
	}

	public void Reset()
	{
		m_lastFreeSpinTime = DCTime.GetCurrentTime().AddYears(-10).Date;
		HasFreeSpin = true;
		KnowAboutFreeSpin = false;
	}

	public float GetSecondsRemaining()
	{
		if (m_lastFreeSpinTime != default(DateTime))
		{
			if (DCTimeValidation.TrustedTime)
			{
				CheckFirstTimeSpinUsage();
				DateTime date = m_lastFreeSpinTime.AddDays(1.0).Date;
				TimeSpan timeSpan = date - DCTime.GetCurrentTime();
				if (timeSpan.TotalSeconds <= 0.0)
				{
					if (timeSpan.TotalSeconds <= -86400.0)
					{
						m_lastFreeSpinTime = date;
						if (m_logMissedSpins)
						{
							WheelofFortuneAnalytics.Instance.MissedFreeSpin();
						}
						if (!HasFreeSpin)
						{
							KnowAboutFreeSpin = false;
							HasFreeSpin = true;
							WheelOfFortuneRewards.Instance.ResetWeightings();
						}
						if (m_freeSpinText[0].activeSelf)
						{
							m_freeSpinText[0].SetActive(value: false);
							m_freeSpinText[1].SetActive(value: false);
						}
						return 0f;
					}
					if (!HasFreeSpin)
					{
						HasFreeSpin = true;
						KnowAboutFreeSpin = false;
						WheelOfFortuneRewards.Instance.ResetWeightings();
						WheelofFortuneAnalytics.Instance.SendCurrentLoggedAnalytics();
					}
					if (m_freeSpinText[0].activeSelf)
					{
						m_freeSpinText[0].SetActive(value: false);
						m_freeSpinText[1].SetActive(value: false);
					}
					return 0f;
				}
				if (HasFreeSpin)
				{
					HasFreeSpin = false;
				}
				if (!m_freeSpinText[0].activeSelf)
				{
					m_freeSpinText[0].SetActive(value: true);
					m_freeSpinText[1].SetActive(value: true);
				}
				return (float)timeSpan.TotalSeconds;
			}
			HasFreeSpin = false;
			KnowAboutFreeSpin = true;
			if (m_freeSpinText[0].activeSelf)
			{
				m_freeSpinText[0].SetActive(value: false);
				m_freeSpinText[1].SetActive(value: false);
			}
			return 1f;
		}
		m_lastFreeSpinTime = DCTime.GetCurrentTime().AddYears(-10).Date;
		HasFreeSpin = true;
		KnowAboutFreeSpin = false;
		return -86400f;
	}

	public void UpdateLastFreeSpinTime()
	{
		if (FirstFreeSpinAvailable)
		{
			FirstFreeSpinAvailable = false;
			FirstFreeSpinTrusted = DCTimeValidation.TrustedTime;
		}
		HasFreeSpin = false;
		if (DCTimeValidation.TrustedTime)
		{
			m_lastFreeSpinTime = DCTime.GetCurrentTime().Date;
		}
	}

	private void Event_OnGameDataSaveRequest()
	{
		CultureInfo cultureInfo = new CultureInfo("en-US");
		PropertyStore.Store("WheelOfFortuneLastFreeSpinDate", m_lastFreeSpinTime.Date.ToString(cultureInfo.DateTimeFormat));
		PropertyStore.Store("WheelOfFortuneKnowAboutYourSpin", KnowAboutFreeSpin);
		PropertyStore.Store("WheelOfFortuneSegmentWeightings", WheelOfFortuneRewards.Instance.SavePrizeWeightingsAsString());
		PropertyStore.Store("WheelOfFortuneCachedJackpotData", WheelOfFortuneRewards.Instance.SaveJackpot());
		PropertyStore.Store("WheelOfFortuneFirstTimeSpinAvailable", FirstFreeSpinAvailable);
		PropertyStore.Store("WheelOfFortuneFirstTimeSpinTrusted", FirstFreeSpinTrusted);
		PropertyStore.Store("WOFCanLogMissedFreeSpins", m_logMissedSpins);
	}

	private void Event_OnGameDataLoaded(ActiveProperties activeProperties)
	{
		FirstFreeSpinAvailable = activeProperties.GetBool("WheelOfFortuneFirstTimeSpinAvailable");
		FirstFreeSpinTrusted = activeProperties.GetBool("WheelOfFortuneFirstTimeSpinTrusted");
		CultureInfo provider = new CultureInfo("en-US");
		if (!DateTime.TryParse(activeProperties.GetString("WheelOfFortuneLastFreeSpinDate"), provider, DateTimeStyles.None, out m_lastFreeSpinTime))
		{
			m_logMissedSpins = false;
			m_lastFreeSpinTime = DCTime.GetCurrentTime().AddYears(-10).Date;
			HasFreeSpin = true;
			KnowAboutFreeSpin = false;
			WheelOfFortuneRewards.Instance.ResetWeightings();
			FirstFreeSpinAvailable = true;
		}
		else
		{
			KnowAboutFreeSpin = activeProperties.GetBool("WheelOfFortuneKnowAboutYourSpin");
			GetSecondsRemaining();
			CheckFirstTimeSpinUsage();
		}
		if (!HasFreeSpin)
		{
			WheelOfFortuneRewards.Instance.LoadPrizeWeightings(activeProperties.GetString("WheelOfFortuneSegmentWeightings"));
			WheelOfFortuneRewards.Instance.LoadJackpot(activeProperties.GetString("WheelOfFortuneCachedJackpotData"));
		}
		else
		{
			WheelOfFortuneRewards.Instance.ResetWeightings();
		}
		m_logMissedSpins = activeProperties.GetBool("WOFCanLogMissedFreeSpins");
	}

	private void CheckFirstTimeSpinUsage()
	{
		if (DCTimeValidation.TrustedTime && !FirstFreeSpinAvailable && !FirstFreeSpinTrusted)
		{
			FirstFreeSpinTrusted = true;
			m_logMissedSpins = true;
			m_lastFreeSpinTime = DCTime.GetCurrentTime().Date;
		}
	}

	private void Event_OnStoreInitialised()
	{
		WheelOfFortuneRewards.Instance.ValidateJackpotRewards();
	}
}
