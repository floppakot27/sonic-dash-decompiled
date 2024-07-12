using System;
using System.Collections;
using UnityEngine;

public class GC3Progress : MonoBehaviour
{
	[Flags]
	private enum State
	{
		Ready = 1
	}

	public const int GC3TiersAmount = 4;

	private const string PropertyGC3PageVisitedFromResult = "GC3PageVisitedFromResult";

	private const string PropertyGC3ActualPointsLocal = "GC3ActualPointsLocal";

	private const string PropertyGC3PreviousPointsLocal = "GC3PreviousPointsLocal";

	private const string PropertyGC3PreviousPointsGlobal = "GC3PreviousPointsGlobal";

	private const string PropertyGC3ActualPointsContributed = "GC3ActualPointsContributed";

	private const string PropertyGC3PreviousPointsContributed = "GC3PreviousPointsContributed";

	private const FileDownloader.Files FileLocation = FileDownloader.Files.GC3;

	private static GC3Progress instance;

	private int m_actualPointsGlobal;

	private int m_actualPointsLocal;

	private int m_actualPointsContributed;

	private int m_lastCheckPointsGlobal;

	private int m_lastCheckPointsLocal;

	private int m_lastCheckPointsContributed;

	private int m_collectedThisRun;

	[SerializeField]
	private int m_totalPointsNeededGlobal = 100000;

	[SerializeField]
	private int m_totalPointsNeededLocal = 10000;

	[SerializeField]
	private string[] m_tierRewards = new string[4];

	[SerializeField]
	private int[] m_tierRewardsAmounts = new int[4];

	private static State s_state;

	public static int ActualPointsLocal => instance.m_actualPointsLocal;

	public static int PreviousPointsLocal => instance.m_lastCheckPointsLocal;

	public static int ActualPointsGlobal => instance.m_actualPointsGlobal;

	public static int PreviousPointsGlobal => instance.m_lastCheckPointsGlobal;

	public static int ActualPointsContributed => instance.m_actualPointsContributed;

	public static int PreviousPointsContributed => instance.m_lastCheckPointsContributed;

	public static string[] TierRewards => instance.m_tierRewards;

	public static int CurrentCollectedThisRun => instance.m_collectedThisRun;

	public static int GetLocalTierSize => instance.m_totalPointsNeededLocal / 4;

	public static bool PageVisitedFromResult { get; set; }

	public static bool Ready => (s_state & State.Ready) == State.Ready;

	public static bool ChallengeFullycompleted()
	{
		bool flag = GetGC3LocalTierCurrent() == 4 && GetGC3GlobalTierCurrent() == 4;
		bool flag2 = IsRewardDue();
		return flag && !flag2;
	}

	public static void Restart()
	{
		s_state = (State)0;
		instance.StartCoroutine(instance.DownloadServerFile());
	}

	public static int GetGC3GlobalTierCurrent()
	{
		for (int num = 4; num >= 0; num--)
		{
			if ((float)instance.m_actualPointsGlobal >= (float)instance.m_totalPointsNeededGlobal * ((float)num / 4f))
			{
				return num;
			}
		}
		return 0;
	}

	public static int GetGC3GlobalTierLastCheck()
	{
		for (int num = 4; num >= 0; num--)
		{
			if ((float)instance.m_lastCheckPointsGlobal >= (float)instance.m_totalPointsNeededGlobal * ((float)num / 4f))
			{
				return num;
			}
		}
		return 0;
	}

	public static int GetGC3LocalTierCurrent()
	{
		for (int num = 4; num >= 0; num--)
		{
			if ((float)instance.m_actualPointsLocal >= (float)instance.m_totalPointsNeededLocal * ((float)num / 4f))
			{
				return num;
			}
		}
		return 0;
	}

	public static int GetGC3LocalTierLastCheck()
	{
		for (int num = 4; num >= 0; num--)
		{
			if ((float)instance.m_lastCheckPointsLocal >= (float)instance.m_totalPointsNeededLocal * ((float)num / 4f))
			{
				return num;
			}
		}
		return 0;
	}

	public static float CalculateLocalPercent(int localPoints)
	{
		return (float)localPoints / (float)instance.m_totalPointsNeededLocal;
	}

	public static float CalculateGlobalPercent(int globalPoints)
	{
		return (float)globalPoints / (float)instance.m_totalPointsNeededGlobal;
	}

	public static void GCPageVisited()
	{
		instance.m_lastCheckPointsGlobal = instance.m_actualPointsGlobal;
		instance.m_lastCheckPointsLocal = instance.m_actualPointsLocal;
		instance.m_lastCheckPointsContributed = instance.m_actualPointsContributed;
		PropertyStore.Save();
	}

	public static bool IsRewardDue()
	{
		int gC3LocalTierCurrent = GetGC3LocalTierCurrent();
		int gC3LocalTierLastCheck = GetGC3LocalTierLastCheck();
		int gC3GlobalTierCurrent = GetGC3GlobalTierCurrent();
		int gC3GlobalTierLastCheck = GetGC3GlobalTierLastCheck();
		return (gC3LocalTierCurrent > gC3LocalTierLastCheck && gC3GlobalTierCurrent >= gC3LocalTierCurrent) || (gC3GlobalTierCurrent > gC3GlobalTierLastCheck && gC3LocalTierCurrent > gC3GlobalTierLastCheck);
	}

	public static string GetRewardDue(out int amount, out bool finalReward)
	{
		if (!IsRewardDue())
		{
			amount = 0;
			finalReward = false;
			return null;
		}
		int num = GetGC3LocalTierCurrent() - 1;
		amount = instance.m_tierRewardsAmounts[num];
		finalReward = num + 1 == 4;
		return instance.m_tierRewards[num];
	}

	public static void GCCollectableCollected()
	{
		instance.m_collectedThisRun++;
	}

	public static int CollectedThisRun()
	{
		return instance.m_collectedThisRun;
	}

	public static void ContributeToChallenge()
	{
		int gC3GlobalTierCurrent = GetGC3GlobalTierCurrent();
		int gC3LocalTierCurrent = GetGC3LocalTierCurrent();
		if (!GCState.IsCurrentChallengeActive())
		{
			return;
		}
		instance.m_actualPointsContributed += instance.m_collectedThisRun;
		if (gC3LocalTierCurrent <= gC3GlobalTierCurrent)
		{
			instance.m_actualPointsLocal += instance.m_collectedThisRun;
			if (instance.m_actualPointsLocal > instance.m_totalPointsNeededLocal / 4 * (gC3LocalTierCurrent + 1))
			{
				instance.m_actualPointsLocal = instance.m_totalPointsNeededLocal / 4 * (gC3LocalTierCurrent + 1);
			}
		}
		GameAnalytics.GC3ChallengePoints(instance.m_collectedThisRun);
	}

	private void Start()
	{
		instance = this;
		EventDispatch.RegisterInterest("OnGameDataLoaded", this);
		EventDispatch.RegisterInterest("OnGameDataSaveRequest", this);
		EventDispatch.RegisterInterest("OnNewGameStarted", this);
	}

	private IEnumerator DownloadServerFile()
	{
		FileDownloader fDownloader = new FileDownloader(FileDownloader.Files.GC3, keepAndUseLocalCopy: true);
		yield return fDownloader.Loading;
		if (fDownloader.Error == null && !int.TryParse(fDownloader.Text, out m_actualPointsGlobal))
		{
			m_actualPointsGlobal = 0;
		}
		s_state |= State.Ready;
	}

	private void Event_OnNewGameStarted()
	{
		m_collectedThisRun = 0;
	}

	private void Event_OnGameDataSaveRequest()
	{
		PropertyStore.Store("GC3PageVisitedFromResult", PageVisitedFromResult);
		PropertyStore.Store("GC3ActualPointsLocal", m_actualPointsLocal);
		PropertyStore.Store("GC3PreviousPointsLocal", m_lastCheckPointsLocal);
		PropertyStore.Store("GC3PreviousPointsGlobal", m_lastCheckPointsGlobal);
		PropertyStore.Store("GC3ActualPointsContributed", m_actualPointsContributed);
		PropertyStore.Store("GC3PreviousPointsContributed", m_lastCheckPointsContributed);
	}

	private void Event_OnGameDataLoaded(ActiveProperties activeProperties)
	{
		PageVisitedFromResult = activeProperties.GetBool("GC3PageVisitedFromResult");
		m_actualPointsLocal = activeProperties.GetInt("GC3ActualPointsLocal");
		m_lastCheckPointsLocal = activeProperties.GetInt("GC3PreviousPointsLocal");
		m_actualPointsContributed = activeProperties.GetInt("GC3ActualPointsContributed");
		m_lastCheckPointsContributed = activeProperties.GetInt("GC3PreviousPointsContributed");
		if (m_actualPointsGlobal == 0)
		{
			m_lastCheckPointsGlobal = activeProperties.GetInt("GC3PreviousPointsGlobal");
		}
	}
}
