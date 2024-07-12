using System;
using System.Globalization;
using UnityEngine;

public class WheelofFortuneAnalytics : MonoBehaviour
{
	public enum PrizeType
	{
		NONE,
		Normal,
		Jackpot,
		FakeJackpot
	}

	public enum Actions
	{
		FreeSpin,
		PaidSpin,
		Leave
	}

	private int m_timesNormalPrizeWonTotal;

	private int m_timesJackpotPrizeWonTotal;

	private int m_timesNormalPrizeWonToday;

	private int m_timesJackpotPrizeWonToday;

	private int m_paidSpinsTotal;

	private int m_paidSpinsToday;

	private int m_freeSpinsTakenTotal;

	private int m_freeSpinsMissedTotal;

	private string m_lastPrizeWon = "NO PRIZE SET";

	private PrizeType m_lastPrizeType;

	private DateTime m_analyticsLoggedDate;

	private bool m_attemptedToSendLoggedData;

	private bool m_firstActionDone;

	private int m_numberOfVisits;

	public static WheelofFortuneAnalytics Instance { get; private set; }

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
		ResetLastPrizeWon();
		EventDispatch.RegisterInterest("OnGameDataLoaded", this);
		EventDispatch.RegisterInterest("OnGameDataSaveRequest", this);
	}

	public void NormalPrizeWon(StoreContent.StoreEntry prize, PrizeType type)
	{
		m_timesNormalPrizeWonTotal++;
		m_timesNormalPrizeWonToday++;
		m_lastPrizeWon = prize.m_identifier;
		m_lastPrizeType = type;
	}

	public void JackpotWon(StoreContent.StoreEntry prize, PrizeType type)
	{
		m_timesJackpotPrizeWonTotal++;
		m_timesJackpotPrizeWonToday++;
		m_lastPrizeWon = prize.m_identifier;
		m_lastPrizeType = type;
	}

	public void PaidForSpin()
	{
		m_paidSpinsTotal++;
		m_paidSpinsToday++;
	}

	public void UsedFreeSpin()
	{
		m_freeSpinsTakenTotal++;
	}

	public void MissedFreeSpin()
	{
		m_freeSpinsMissedTotal++;
	}

	public void BoughtRedStarRing(string currentJackpot, PrizeType jackpotType)
	{
		GameAnalytics.WOFRedStarRingBought(m_lastPrizeWon, m_lastPrizeType.ToString("G"), currentJackpot, jackpotType.ToString("G"));
	}

	public void ResetLastPrizeWon()
	{
		m_lastPrizeWon = "NO PRIZE SET";
		m_lastPrizeType = PrizeType.NONE;
	}

	public void Visit()
	{
		m_firstActionDone = false;
		m_numberOfVisits++;
	}

	public void FirstAction(Actions action)
	{
		if (!m_firstActionDone || action != Actions.Leave)
		{
			GameAnalytics.WOFFirstDecission(action.ToString(), m_numberOfVisits);
			m_firstActionDone = true;
		}
	}

	public void SendCurrentLoggedAnalytics()
	{
		if (DCTimeValidation.TrustedTime)
		{
			GameAnalytics.WOFDailyStatsLogged(m_analyticsLoggedDate, m_timesNormalPrizeWonToday, m_timesJackpotPrizeWonToday, m_paidSpinsToday, m_timesNormalPrizeWonTotal, m_timesJackpotPrizeWonTotal, m_paidSpinsTotal, m_freeSpinsTakenTotal, m_freeSpinsMissedTotal);
		}
		GetToday();
		m_paidSpinsToday = 0;
		m_timesJackpotPrizeWonToday = 0;
		m_timesNormalPrizeWonToday = 0;
		ResetLastPrizeWon();
		m_attemptedToSendLoggedData = true;
	}

	private void GetToday()
	{
		m_analyticsLoggedDate = DCTime.GetCurrentTime().Date;
	}

	private void Event_OnGameDataSaveRequest()
	{
		PropertyStore.Store("WOFAmountOfNormalPrizesWonTotal", m_timesNormalPrizeWonTotal);
		PropertyStore.Store("WOFAmountOfNormalPrizesWonToday", m_timesNormalPrizeWonToday);
		PropertyStore.Store("WOFAmountOfJackpotPrizesWonTotal", m_timesJackpotPrizeWonTotal);
		PropertyStore.Store("WOFAmountOfJackpotPrizesWonToday", m_timesJackpotPrizeWonToday);
		PropertyStore.Store("WOFAmountOfPaidSpinsTotal", m_paidSpinsTotal);
		PropertyStore.Store("WOFAmountOfPaidSpinsToday", m_paidSpinsToday);
		PropertyStore.Store("WOFAmountOfFreeSpinsTaken", m_freeSpinsTakenTotal);
		PropertyStore.Store("WOFAmountOfFreeSpinsMissed", m_freeSpinsMissedTotal);
		PropertyStore.Store("WOFLastPrizeWon", m_lastPrizeWon);
		PropertyStore.Store("WOFLastPrizeWonType", m_lastPrizeType.ToString("D"));
		CultureInfo cultureInfo = new CultureInfo("en-US");
		if (m_attemptedToSendLoggedData && DCTimeValidation.TrustedTime)
		{
			GetToday();
		}
		PropertyStore.Store("WOFDateOfLoggedAnalaytics", m_analyticsLoggedDate.Date.ToString(cultureInfo.DateTimeFormat));
	}

	private void Event_OnGameDataLoaded(ActiveProperties ap)
	{
		m_timesNormalPrizeWonTotal = ap.GetInt("WOFAmountOfNormalPrizesWonTotal");
		m_timesNormalPrizeWonToday = ap.GetInt("WOFAmountOfNormalPrizesWonToday");
		m_timesJackpotPrizeWonTotal = ap.GetInt("WOFAmountOfJackpotPrizesWonTotal");
		m_timesJackpotPrizeWonToday = ap.GetInt("WOFAmountOfJackpotPrizesWonToday");
		m_paidSpinsTotal = ap.GetInt("WOFAmountOfPaidSpinsTotal");
		m_paidSpinsToday = ap.GetInt("WOFAmountOfPaidSpinsToday");
		m_freeSpinsTakenTotal = ap.GetInt("WOFAmountOfFreeSpinsTaken");
		m_freeSpinsMissedTotal = ap.GetInt("WOFAmountOfFreeSpinsMissed");
		m_lastPrizeWon = ap.GetString("WOFLastPrizeWon");
		m_lastPrizeType = (PrizeType)ap.GetInt("WOFLastPrizeWonType");
		if (m_lastPrizeWon == null)
		{
			ResetLastPrizeWon();
		}
		CultureInfo provider = new CultureInfo("en-US");
		if (!DateTime.TryParse(ap.GetString("WOFDateOfLoggedAnalaytics"), provider, DateTimeStyles.None, out m_analyticsLoggedDate))
		{
			GetToday();
		}
	}
}
