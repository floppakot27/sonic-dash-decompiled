using System;
using UnityEngine;

public class MissionTracker : MonoBehaviour
{
	public enum MissionLevel1
	{
		AvoidRings_Distance,
		BankInOneRun_Rings,
		VisitsTemple_Times,
		CollectInOneRun_Rings,
		SpendInStore_Rings,
		CollectMoreInOneRun_Rings,
		TripOnMines_Times,
		UseRevive_Times,
		UseHeadstarts_Times,
		RunInOneRun_Distance,
		_2_CompleteDC_Times,
		_2_PickUpInOneRun_Shields,
		_2_BankInOneRun_Rings,
		_2_UseSuperHeadstarts_Times,
		_2_SpendInStore_Rings,
		_2_InOneRun_EnemyStreaks,
		_2_RunInOneRun_Distance,
		_2_CollectInOneRun_Rings,
		_2_DashBoostThroughTotemsInOneRun_Times,
		_2_UseReviveInOneRun_Times
	}

	public enum MissionLevel2
	{
		RollUnderBridge_Times,
		InOneRun_EnemyStreaks,
		ScoreInOneRun_Points,
		Run_Distance,
		ScoreWithoutDroppingRingsInOneRun_Points,
		RollMiddleLane_Times,
		InOneRun_RingStreaks,
		DropWithoutDying_Rings,
		RunWithoutBankingInOneRun_Distance,
		PickUpInOneRun_PowerUps,
		_2_RunWithFullDashMeterInOneRun_Distance,
		_2_SpendAirbourneInOneRun_Seconds,
		_2_JumpOverCrabmeatsInOneRun_Times,
		_2_ScoreInOneRun_Points,
		_2_RemainInSameLineInOneRun_Distance,
		_2_TripOnMinesInOneRun_Times,
		_2_UseDashInOneRun_Times,
		_2_RollUnderBridgeInOneRun_Times,
		_2_RunWithoutBankingInOneRun_Distance,
		_2_ScoreWithoutDroppingRingsInOneRun_Points
	}

	public enum MissionLevel3
	{
		UseDash_Times,
		PlayForConsecutive_Days,
		RunCorkscrews_Times,
		DefeatNoHomingNoSpinRoll_Enemies,
		RunLoops_Times,
		DefeatUsingHoming_Enemies,
		Total_EnemyStreaks,
		Total_RingStreaks,
		DefeatInAir_Enemies,
		UpgradeToMax_PowerUps,
		_2_Run_Distance,
		_2_DashBoostThroughTotems_Times,
		_2_Total_RingStreaks,
		_2_JumpOverPlantPots_Times,
		_2_UpgradeToMax_PowerUps,
		_2_PickUp_PowerUps,
		_2_RunWithFullDashMeter_Distance,
		_2_SpendAirbourne_Seconds,
		_2_JumpOverSpikys_Times,
		_2_CompleteDCConsecutiveDays_Times
	}

	public struct Mission
	{
		[Flags]
		public enum State
		{
			Completed = 1,
			JustCompleted = 2,
			SpecialCheck = 4,
			ResetEachRun = 8,
			Initialized = 0x10
		}

		public int m_id;

		public int m_level;

		public PlayerStats.StatTypes m_trackedType;

		public int m_trackedValue;

		public long m_amountStart;

		public long m_amountCurrent;

		public long m_amountTarget;

		public long m_amountNeeded;

		public State m_state;
	}

	public const float ToFloatDivision = 10000f;

	public const float ToTenthsOfSecond = 10f;

	public const int SetLength = 3;

	public const int NumberOfSets = 19;

	public const int NumberOfMissions = 57;

	private const string MissionSetProperty = "Current Mission Set";

	private const string Mission1StartProperty = "Mission 1 Start";

	private const string Mission1CurrentProperty = "Mission 1 Current";

	private const string Mission1TargetProperty = "Mission 1 Target";

	private const string Mission1StateProperty = "Mission 1 State";

	private const string Mission2StartProperty = "Mission 2 Start";

	private const string Mission2CurrentProperty = "Mission 2 Current";

	private const string Mission2TargetProperty = "Mission 2 Target";

	private const string Mission2StateProperty = "Mission 2 State";

	private const string Mission3StartProperty = "Mission 3 Start";

	private const string Mission3CurrentProperty = "Mission 3 Current";

	private const string Mission3TargetProperty = "Mission 3 Target";

	private const string Mission3StateProperty = "Mission 3 State";

	private const string MissionWrappedStateProperty = "Missions Wrapped";

	private static MissionTracker s_missionTracker;

	private int m_currentMissionSet;

	private bool m_looped;

	private bool m_trackMissions = true;

	private object[] m_completeEventParameters = new object[2];

	[SerializeField]
	private int[] m_missionSetsLevel1;

	[SerializeField]
	private int[] m_missionSetsLevel2;

	[SerializeField]
	private int[] m_missionSetsLevel3;

	[SerializeField]
	private int[] m_missionValuesLevel1;

	[SerializeField]
	private int[] m_missionValuesLevel2;

	[SerializeField]
	private int[] m_missionValuesLevel3;

	private int[][] m_missionSets = new int[3][];

	private Mission[][] m_missions = new Mission[3][]
	{
		new Mission[Enum.GetNames(typeof(MissionLevel1)).Length],
		new Mission[Enum.GetNames(typeof(MissionLevel2)).Length],
		new Mission[Enum.GetNames(typeof(MissionLevel3)).Length]
	};

	public static int RSRRewardPerSet { get; set; }

	public static int GetMissionSet()
	{
		return s_missionTracker.m_currentMissionSet;
	}

	public static int GetTranslatedIndex(int missionGroup)
	{
		return s_missionTracker.m_missionSets[missionGroup][s_missionTracker.m_currentMissionSet];
	}

	public static void CompleteActiveMission(int missionGroup)
	{
		s_missionTracker.CompleteMission(ref s_missionTracker.m_missions[missionGroup][s_missionTracker.m_missionSets[missionGroup][s_missionTracker.m_currentMissionSet]], purchased: true);
	}

	public static bool AllMissionSetComplete()
	{
		bool flag = true;
		for (int i = 0; i < 3; i++)
		{
			flag &= s_missionTracker.MissionComplete(i);
		}
		return flag;
	}

	public static bool AllMissionsComplete()
	{
		return s_missionTracker.m_looped;
	}

	public static Mission GetActiveMission(int missionGroup)
	{
		return s_missionTracker.m_missions[missionGroup][s_missionTracker.m_missionSets[missionGroup][s_missionTracker.m_currentMissionSet]];
	}

	public static void MoveToNextSet()
	{
		s_missionTracker.ChangeSet();
	}

	public static void Track(bool trackMissions)
	{
		s_missionTracker.m_trackMissions = trackMissions;
	}

	public void Initialize()
	{
		int num = Enum.GetNames(typeof(MissionLevel1)).Length;
		int num2 = Enum.GetNames(typeof(MissionLevel2)).Length;
		int num3 = Enum.GetNames(typeof(MissionLevel3)).Length;
		if (m_missionSetsLevel1 == null)
		{
			m_missionSetsLevel1 = new int[num];
		}
		else if (m_missionSetsLevel1.Length != num)
		{
			m_missionSetsLevel1 = new int[num];
		}
		if (m_missionSetsLevel2 == null)
		{
			m_missionSetsLevel2 = new int[num2];
		}
		else if (m_missionSetsLevel2.Length != num2)
		{
			m_missionSetsLevel2 = new int[num2];
		}
		if (m_missionSetsLevel3 == null)
		{
			m_missionSetsLevel3 = new int[num3];
		}
		else if (m_missionSetsLevel3.Length != num3)
		{
			m_missionSetsLevel3 = new int[num3];
		}
		if (m_missionValuesLevel1 == null)
		{
			m_missionValuesLevel1 = new int[num];
		}
		else if (m_missionValuesLevel1.Length != num)
		{
			m_missionValuesLevel1 = new int[num];
		}
		if (m_missionValuesLevel2 == null)
		{
			m_missionValuesLevel2 = new int[num2];
		}
		else if (m_missionValuesLevel2.Length != num2)
		{
			m_missionValuesLevel2 = new int[num2];
		}
		if (m_missionValuesLevel3 == null)
		{
			m_missionValuesLevel3 = new int[num3];
		}
		else if (m_missionValuesLevel3.Length != num3)
		{
			m_missionValuesLevel3 = new int[num3];
		}
	}

	public void Awake()
	{
		Initialize();
	}

	private void Update()
	{
		if (!m_trackMissions || AllMissionsComplete())
		{
			return;
		}
		for (int i = 0; i < m_missions.Length; i++)
		{
			Mission currentMission = m_missions[i][m_missionSets[i][m_currentMissionSet]];
			if ((currentMission.m_id == 9 && currentMission.m_level == 3) || (currentMission.m_id == 14 && currentMission.m_level == 3) || (currentMission.m_id == 1 && currentMission.m_level == 3) || (currentMission.m_id == 11 && currentMission.m_level == 3) || (currentMission.m_id == 4 && currentMission.m_level == 1) || (currentMission.m_id == 14 && currentMission.m_level == 1))
			{
				if ((currentMission.m_state & Mission.State.Completed) != Mission.State.Completed)
				{
					CheckMission(ref currentMission);
				}
			}
			else if (GameState.GetMode() != 0 && (currentMission.m_state & Mission.State.Completed) != Mission.State.Completed)
			{
				CheckMission(ref currentMission);
			}
			m_missions[i][m_missionSets[i][m_currentMissionSet]] = currentMission;
		}
	}

	private void CheckMission(ref Mission currentMission)
	{
		if (AllMissionsComplete())
		{
			return;
		}
		PlayerStats.Stats currentStats = PlayerStats.GetCurrentStats();
		if ((currentMission.m_state & Mission.State.Completed) == Mission.State.Completed)
		{
			return;
		}
		switch (currentMission.m_trackedType)
		{
		case PlayerStats.StatTypes.Int:
			currentMission.m_amountCurrent = currentStats.m_trackedStats[currentMission.m_trackedValue];
			if (currentMission.m_amountCurrent >= currentMission.m_amountTarget)
			{
				CompleteMission(ref currentMission, purchased: false);
			}
			break;
		case PlayerStats.StatTypes.Long:
			if ((currentMission.m_state & Mission.State.SpecialCheck) == Mission.State.SpecialCheck)
			{
				currentMission.m_amountCurrent = currentStats.m_trackedLongStats[0];
				currentMission.m_amountStart = currentStats.m_trackedLongStats[currentMission.m_trackedValue];
				currentMission.m_amountTarget = currentMission.m_amountStart + currentMission.m_amountNeeded;
			}
			else
			{
				currentMission.m_amountCurrent = currentStats.m_trackedLongStats[currentMission.m_trackedValue];
			}
			if (currentMission.m_amountCurrent >= currentMission.m_amountTarget)
			{
				CompleteMission(ref currentMission, purchased: false);
			}
			break;
		case PlayerStats.StatTypes.Float:
			if ((currentMission.m_id == 10 && currentMission.m_level == 2) || (currentMission.m_id == 16 && currentMission.m_level == 3))
			{
				currentMission.m_amountCurrent = (long)(currentStats.m_trackedDistances[currentMission.m_trackedValue] * 10000f);
			}
			else
			{
				currentMission.m_amountCurrent = (long)(currentStats.m_trackedDistances[0] * 10000f);
				if ((currentMission.m_state & Mission.State.SpecialCheck) == Mission.State.SpecialCheck)
				{
					currentMission.m_amountStart = (long)(currentStats.m_trackedDistances[currentMission.m_trackedValue] * 10000f);
					currentMission.m_amountTarget = currentMission.m_amountStart + (long)((float)currentMission.m_amountNeeded * 10000f);
				}
			}
			if (currentMission.m_amountCurrent >= currentMission.m_amountTarget)
			{
				CompleteMission(ref currentMission, purchased: false);
			}
			break;
		case PlayerStats.StatTypes.Date:
			currentMission.m_amountCurrent = currentStats.m_trackedDates[0].Ticks;
			currentMission.m_amountStart = currentStats.m_trackedDates[currentMission.m_trackedValue].Ticks;
			currentMission.m_amountTarget = currentMission.m_amountStart + new DateTime(0L).AddDays(currentMission.m_amountNeeded).Ticks;
			if (currentMission.m_amountCurrent >= currentMission.m_amountTarget)
			{
				CompleteMission(ref currentMission, purchased: false);
			}
			break;
		}
	}

	private void SetTarget(ref Mission currentMission)
	{
		if ((currentMission.m_state & Mission.State.Completed) == Mission.State.Completed)
		{
			return;
		}
		PlayerStats.Stats currentStats = PlayerStats.GetCurrentStats();
		switch (currentMission.m_trackedType)
		{
		case PlayerStats.StatTypes.Int:
			if ((currentMission.m_id == 11 && currentMission.m_level == 2) || (currentMission.m_id == 17 && currentMission.m_level == 3))
			{
				currentMission.m_amountStart = currentStats.m_trackedStats[currentMission.m_trackedValue];
				currentMission.m_amountTarget = (long)((float)currentMission.m_amountStart + (float)currentMission.m_amountNeeded * 10f);
				currentMission.m_amountCurrent = currentMission.m_amountStart;
			}
			else if ((currentMission.m_id == 9 && currentMission.m_level == 3) || (currentMission.m_id == 14 && currentMission.m_level == 3))
			{
				currentMission.m_amountStart = 0L;
				currentMission.m_amountTarget = currentMission.m_amountNeeded;
				currentMission.m_amountCurrent = currentStats.m_trackedStats[currentMission.m_trackedValue];
			}
			else
			{
				currentMission.m_amountStart = currentStats.m_trackedStats[currentMission.m_trackedValue];
				currentMission.m_amountTarget = currentMission.m_amountStart + currentMission.m_amountNeeded;
				currentMission.m_amountCurrent = currentMission.m_amountStart;
			}
			break;
		case PlayerStats.StatTypes.Long:
			currentMission.m_amountStart = currentStats.m_trackedLongStats[currentMission.m_trackedValue];
			currentMission.m_amountTarget = currentMission.m_amountStart + currentMission.m_amountNeeded;
			currentMission.m_amountCurrent = currentMission.m_amountStart;
			break;
		case PlayerStats.StatTypes.Float:
			currentMission.m_amountStart = (long)(currentStats.m_trackedDistances[currentMission.m_trackedValue] * 10000f);
			currentMission.m_amountTarget = (long)((float)currentMission.m_amountStart + (float)currentMission.m_amountNeeded * 10000f);
			currentMission.m_amountCurrent = currentMission.m_amountStart;
			break;
		case PlayerStats.StatTypes.Date:
			currentMission.m_amountStart = currentStats.m_trackedDates[currentMission.m_trackedValue].Ticks;
			currentMission.m_amountTarget = currentMission.m_amountStart + new DateTime(0L).AddDays(currentMission.m_amountNeeded).Ticks;
			currentMission.m_amountCurrent = currentMission.m_amountStart;
			break;
		}
		currentMission.m_state |= Mission.State.Initialized;
	}

	private void Start()
	{
		s_missionTracker = this;
		RSRRewardPerSet = 3;
		EventDispatch.RegisterInterest("OnNewGameStarted", this, EventDispatch.Priority.Lowest);
		EventDispatch.RegisterInterest("OnGameFinished", this);
		EventDispatch.RegisterInterest("OnGameDataLoaded", this, EventDispatch.Priority.Lowest);
		EventDispatch.RegisterInterest("OnGameDataSaveRequest", this);
		EventDispatch.RegisterInterest("ABTestingReady", this);
		m_missionSets[0] = m_missionSetsLevel1;
		m_missionSets[1] = m_missionSetsLevel2;
		m_missionSets[2] = m_missionSetsLevel3;
		CreateMissions();
	}

	private void CreateMissions()
	{
		m_missions[0][0].m_id = 0;
		m_missions[0][0].m_level = 1;
		m_missions[0][0].m_trackedType = PlayerStats.StatTypes.Float;
		m_missions[0][0].m_trackedValue = 2;
		m_missions[0][0].m_amountNeeded = m_missionValuesLevel1[0];
		m_missions[0][0].m_state = Mission.State.SpecialCheck | Mission.State.ResetEachRun;
		m_missions[0][1].m_id = 1;
		m_missions[0][1].m_level = 1;
		m_missions[0][1].m_trackedType = PlayerStats.StatTypes.Int;
		m_missions[0][1].m_trackedValue = 16;
		m_missions[0][1].m_amountNeeded = m_missionValuesLevel1[1];
		m_missions[0][1].m_state = Mission.State.ResetEachRun;
		m_missions[0][2].m_id = 2;
		m_missions[0][2].m_level = 1;
		m_missions[0][2].m_trackedType = PlayerStats.StatTypes.Int;
		m_missions[0][2].m_trackedValue = 35;
		m_missions[0][2].m_amountNeeded = m_missionValuesLevel1[2];
		m_missions[0][2].m_state = (Mission.State)0;
		m_missions[0][3].m_id = 3;
		m_missions[0][3].m_level = 1;
		m_missions[0][3].m_trackedType = PlayerStats.StatTypes.Int;
		m_missions[0][3].m_trackedValue = 11;
		m_missions[0][3].m_amountNeeded = m_missionValuesLevel1[3];
		m_missions[0][3].m_state = Mission.State.ResetEachRun;
		m_missions[0][4].m_id = 4;
		m_missions[0][4].m_level = 1;
		m_missions[0][4].m_trackedType = PlayerStats.StatTypes.Int;
		m_missions[0][4].m_trackedValue = 21;
		m_missions[0][4].m_amountNeeded = m_missionValuesLevel1[4];
		m_missions[0][4].m_state = (Mission.State)0;
		m_missions[0][5].m_id = 5;
		m_missions[0][5].m_level = 1;
		m_missions[0][5].m_trackedType = PlayerStats.StatTypes.Int;
		m_missions[0][5].m_trackedValue = 11;
		m_missions[0][5].m_amountNeeded = m_missionValuesLevel1[5];
		m_missions[0][5].m_state = Mission.State.ResetEachRun;
		m_missions[0][6].m_id = 6;
		m_missions[0][6].m_level = 1;
		m_missions[0][6].m_trackedType = PlayerStats.StatTypes.Int;
		m_missions[0][6].m_trackedValue = 25;
		m_missions[0][6].m_amountNeeded = m_missionValuesLevel1[6];
		m_missions[0][6].m_state = (Mission.State)0;
		m_missions[0][7].m_id = 7;
		m_missions[0][7].m_level = 1;
		m_missions[0][7].m_trackedType = PlayerStats.StatTypes.Int;
		m_missions[0][7].m_trackedValue = 47;
		m_missions[0][7].m_amountNeeded = m_missionValuesLevel1[7];
		m_missions[0][7].m_state = (Mission.State)0;
		m_missions[0][8].m_id = 8;
		m_missions[0][8].m_level = 1;
		m_missions[0][8].m_trackedType = PlayerStats.StatTypes.Int;
		m_missions[0][8].m_trackedValue = 50;
		m_missions[0][8].m_amountNeeded = m_missionValuesLevel1[8];
		m_missions[0][8].m_state = (Mission.State)0;
		m_missions[0][9].m_id = 9;
		m_missions[0][9].m_level = 1;
		m_missions[0][9].m_trackedType = PlayerStats.StatTypes.Float;
		m_missions[0][9].m_trackedValue = 0;
		m_missions[0][9].m_amountNeeded = m_missionValuesLevel1[9];
		m_missions[0][9].m_state = Mission.State.ResetEachRun;
		m_missions[0][10].m_id = 10;
		m_missions[0][10].m_level = 1;
		m_missions[0][10].m_trackedType = PlayerStats.StatTypes.Int;
		m_missions[0][10].m_trackedValue = 81;
		m_missions[0][10].m_amountNeeded = m_missionValuesLevel1[10];
		m_missions[0][10].m_state = (Mission.State)0;
		m_missions[0][11].m_id = 11;
		m_missions[0][11].m_level = 1;
		m_missions[0][11].m_trackedType = PlayerStats.StatTypes.Int;
		m_missions[0][11].m_trackedValue = 41;
		m_missions[0][11].m_amountNeeded = m_missionValuesLevel1[11];
		m_missions[0][11].m_state = Mission.State.ResetEachRun;
		m_missions[0][12].m_id = 12;
		m_missions[0][12].m_level = 1;
		m_missions[0][12].m_trackedType = PlayerStats.StatTypes.Int;
		m_missions[0][12].m_trackedValue = 16;
		m_missions[0][12].m_amountNeeded = m_missionValuesLevel1[12];
		m_missions[0][12].m_state = Mission.State.ResetEachRun;
		m_missions[0][13].m_id = 13;
		m_missions[0][13].m_level = 1;
		m_missions[0][13].m_trackedType = PlayerStats.StatTypes.Int;
		m_missions[0][13].m_trackedValue = 53;
		m_missions[0][13].m_amountNeeded = m_missionValuesLevel1[13];
		m_missions[0][13].m_state = (Mission.State)0;
		m_missions[0][14].m_id = 14;
		m_missions[0][14].m_level = 1;
		m_missions[0][14].m_trackedType = PlayerStats.StatTypes.Int;
		m_missions[0][14].m_trackedValue = 21;
		m_missions[0][14].m_amountNeeded = m_missionValuesLevel1[14];
		m_missions[0][14].m_state = (Mission.State)0;
		m_missions[0][15].m_id = 15;
		m_missions[0][15].m_level = 1;
		m_missions[0][15].m_trackedType = PlayerStats.StatTypes.Int;
		m_missions[0][15].m_trackedValue = 10;
		m_missions[0][15].m_amountNeeded = m_missionValuesLevel1[15];
		m_missions[0][15].m_state = Mission.State.ResetEachRun;
		m_missions[0][16].m_id = 16;
		m_missions[0][16].m_level = 1;
		m_missions[0][16].m_trackedType = PlayerStats.StatTypes.Float;
		m_missions[0][16].m_trackedValue = 0;
		m_missions[0][16].m_amountNeeded = m_missionValuesLevel1[16];
		m_missions[0][16].m_state = Mission.State.ResetEachRun;
		m_missions[0][17].m_id = 17;
		m_missions[0][17].m_level = 1;
		m_missions[0][17].m_trackedType = PlayerStats.StatTypes.Int;
		m_missions[0][17].m_trackedValue = 11;
		m_missions[0][17].m_amountNeeded = m_missionValuesLevel1[17];
		m_missions[0][17].m_state = Mission.State.ResetEachRun;
		m_missions[0][18].m_id = 18;
		m_missions[0][18].m_level = 1;
		m_missions[0][18].m_trackedType = PlayerStats.StatTypes.Int;
		m_missions[0][18].m_trackedValue = 87;
		m_missions[0][18].m_amountNeeded = m_missionValuesLevel1[18];
		m_missions[0][18].m_state = Mission.State.ResetEachRun;
		m_missions[0][19].m_id = 19;
		m_missions[0][19].m_level = 1;
		m_missions[0][19].m_trackedType = PlayerStats.StatTypes.Int;
		m_missions[0][19].m_trackedValue = 49;
		m_missions[0][19].m_amountNeeded = m_missionValuesLevel1[19];
		m_missions[0][19].m_state = Mission.State.ResetEachRun;
		m_missions[1][0].m_id = 0;
		m_missions[1][0].m_level = 2;
		m_missions[1][0].m_trackedType = PlayerStats.StatTypes.Int;
		m_missions[1][0].m_trackedValue = 28;
		m_missions[1][0].m_amountNeeded = m_missionValuesLevel2[0];
		m_missions[1][0].m_state = (Mission.State)0;
		m_missions[1][1].m_id = 1;
		m_missions[1][1].m_level = 2;
		m_missions[1][1].m_trackedType = PlayerStats.StatTypes.Int;
		m_missions[1][1].m_trackedValue = 10;
		m_missions[1][1].m_amountNeeded = m_missionValuesLevel2[1];
		m_missions[1][1].m_state = Mission.State.ResetEachRun;
		m_missions[1][2].m_id = 2;
		m_missions[1][2].m_level = 2;
		m_missions[1][2].m_trackedType = PlayerStats.StatTypes.Long;
		m_missions[1][2].m_trackedValue = 0;
		m_missions[1][2].m_amountNeeded = m_missionValuesLevel2[2];
		m_missions[1][2].m_state = Mission.State.ResetEachRun;
		m_missions[1][3].m_id = 3;
		m_missions[1][3].m_level = 2;
		m_missions[1][3].m_trackedType = PlayerStats.StatTypes.Float;
		m_missions[1][3].m_trackedValue = 0;
		m_missions[1][3].m_amountNeeded = m_missionValuesLevel2[3];
		m_missions[1][3].m_state = (Mission.State)0;
		m_missions[1][4].m_id = 4;
		m_missions[1][4].m_level = 2;
		m_missions[1][4].m_trackedType = PlayerStats.StatTypes.Long;
		m_missions[1][4].m_trackedValue = 1;
		m_missions[1][4].m_amountNeeded = m_missionValuesLevel2[4];
		m_missions[1][4].m_state = Mission.State.SpecialCheck | Mission.State.ResetEachRun;
		m_missions[1][5].m_id = 5;
		m_missions[1][5].m_level = 2;
		m_missions[1][5].m_trackedType = PlayerStats.StatTypes.Int;
		m_missions[1][5].m_trackedValue = 27;
		m_missions[1][5].m_amountNeeded = m_missionValuesLevel2[5];
		m_missions[1][5].m_state = (Mission.State)0;
		m_missions[1][6].m_id = 6;
		m_missions[1][6].m_level = 2;
		m_missions[1][6].m_trackedType = PlayerStats.StatTypes.Int;
		m_missions[1][6].m_trackedValue = 19;
		m_missions[1][6].m_amountNeeded = m_missionValuesLevel2[6];
		m_missions[1][6].m_state = Mission.State.ResetEachRun;
		m_missions[1][7].m_id = 7;
		m_missions[1][7].m_level = 2;
		m_missions[1][7].m_trackedType = PlayerStats.StatTypes.Int;
		m_missions[1][7].m_trackedValue = 22;
		m_missions[1][7].m_amountNeeded = m_missionValuesLevel2[7];
		m_missions[1][7].m_state = Mission.State.ResetEachRun;
		m_missions[1][8].m_id = 8;
		m_missions[1][8].m_level = 2;
		m_missions[1][8].m_trackedType = PlayerStats.StatTypes.Float;
		m_missions[1][8].m_trackedValue = 3;
		m_missions[1][8].m_amountNeeded = m_missionValuesLevel2[8];
		m_missions[1][8].m_state = Mission.State.SpecialCheck | Mission.State.ResetEachRun;
		m_missions[1][9].m_id = 9;
		m_missions[1][9].m_level = 2;
		m_missions[1][9].m_trackedType = PlayerStats.StatTypes.Int;
		m_missions[1][9].m_trackedValue = 42;
		m_missions[1][9].m_amountNeeded = m_missionValuesLevel2[9];
		m_missions[1][9].m_state = Mission.State.ResetEachRun;
		m_missions[1][10].m_id = 10;
		m_missions[1][10].m_level = 2;
		m_missions[1][10].m_trackedType = PlayerStats.StatTypes.Float;
		m_missions[1][10].m_trackedValue = 9;
		m_missions[1][10].m_amountNeeded = m_missionValuesLevel2[10];
		m_missions[1][10].m_state = Mission.State.ResetEachRun;
		m_missions[1][11].m_id = 11;
		m_missions[1][11].m_level = 2;
		m_missions[1][11].m_trackedType = PlayerStats.StatTypes.Int;
		m_missions[1][11].m_trackedValue = 89;
		m_missions[1][11].m_amountNeeded = m_missionValuesLevel2[11];
		m_missions[1][11].m_state = Mission.State.ResetEachRun;
		m_missions[1][12].m_id = 12;
		m_missions[1][12].m_level = 2;
		m_missions[1][12].m_trackedType = PlayerStats.StatTypes.Int;
		m_missions[1][12].m_trackedValue = 83;
		m_missions[1][12].m_amountNeeded = m_missionValuesLevel2[12];
		m_missions[1][12].m_state = Mission.State.ResetEachRun;
		m_missions[1][13].m_id = 13;
		m_missions[1][13].m_level = 2;
		m_missions[1][13].m_trackedType = PlayerStats.StatTypes.Long;
		m_missions[1][13].m_trackedValue = 0;
		m_missions[1][13].m_amountNeeded = m_missionValuesLevel2[13];
		m_missions[1][13].m_state = Mission.State.ResetEachRun;
		m_missions[1][14].m_id = 14;
		m_missions[1][14].m_level = 2;
		m_missions[1][14].m_trackedType = PlayerStats.StatTypes.Float;
		m_missions[1][14].m_trackedValue = 8;
		m_missions[1][14].m_amountNeeded = m_missionValuesLevel2[14];
		m_missions[1][14].m_state = Mission.State.SpecialCheck | Mission.State.ResetEachRun;
		m_missions[1][15].m_id = 15;
		m_missions[1][15].m_level = 2;
		m_missions[1][15].m_trackedType = PlayerStats.StatTypes.Int;
		m_missions[1][15].m_trackedValue = 26;
		m_missions[1][15].m_amountNeeded = m_missionValuesLevel2[15];
		m_missions[1][15].m_state = Mission.State.ResetEachRun;
		m_missions[1][16].m_id = 16;
		m_missions[1][16].m_level = 2;
		m_missions[1][16].m_trackedType = PlayerStats.StatTypes.Int;
		m_missions[1][16].m_trackedValue = 34;
		m_missions[1][16].m_amountNeeded = m_missionValuesLevel2[16];
		m_missions[1][16].m_state = Mission.State.ResetEachRun;
		m_missions[1][17].m_id = 17;
		m_missions[1][17].m_level = 2;
		m_missions[1][17].m_trackedType = PlayerStats.StatTypes.Int;
		m_missions[1][17].m_trackedValue = 29;
		m_missions[1][17].m_amountNeeded = m_missionValuesLevel2[17];
		m_missions[1][17].m_state = Mission.State.ResetEachRun;
		m_missions[1][18].m_id = 18;
		m_missions[1][18].m_level = 2;
		m_missions[1][18].m_trackedType = PlayerStats.StatTypes.Float;
		m_missions[1][18].m_trackedValue = 3;
		m_missions[1][18].m_amountNeeded = m_missionValuesLevel2[18];
		m_missions[1][18].m_state = Mission.State.SpecialCheck | Mission.State.ResetEachRun;
		m_missions[1][19].m_id = 19;
		m_missions[1][19].m_level = 2;
		m_missions[1][19].m_trackedType = PlayerStats.StatTypes.Long;
		m_missions[1][19].m_trackedValue = 1;
		m_missions[1][19].m_amountNeeded = m_missionValuesLevel2[19];
		m_missions[1][19].m_state = Mission.State.SpecialCheck | Mission.State.ResetEachRun;
		m_missions[2][0].m_id = 0;
		m_missions[2][0].m_level = 3;
		m_missions[2][0].m_trackedType = PlayerStats.StatTypes.Int;
		m_missions[2][0].m_trackedValue = 32;
		m_missions[2][0].m_amountNeeded = m_missionValuesLevel3[0];
		m_missions[2][0].m_state = (Mission.State)0;
		m_missions[2][1].m_id = 1;
		m_missions[2][1].m_level = 3;
		m_missions[2][1].m_trackedType = PlayerStats.StatTypes.Date;
		m_missions[2][1].m_trackedValue = 1;
		m_missions[2][1].m_amountNeeded = m_missionValuesLevel3[1];
		m_missions[2][1].m_state = Mission.State.SpecialCheck;
		m_missions[2][2].m_id = 2;
		m_missions[2][2].m_level = 3;
		m_missions[2][2].m_trackedType = PlayerStats.StatTypes.Int;
		m_missions[2][2].m_trackedValue = 30;
		m_missions[2][2].m_amountNeeded = m_missionValuesLevel3[2];
		m_missions[2][2].m_state = (Mission.State)0;
		m_missions[2][3].m_id = 3;
		m_missions[2][3].m_level = 3;
		m_missions[2][3].m_trackedType = PlayerStats.StatTypes.Int;
		m_missions[2][3].m_trackedValue = 7;
		m_missions[2][3].m_amountNeeded = m_missionValuesLevel3[3];
		m_missions[2][3].m_state = (Mission.State)0;
		m_missions[2][4].m_id = 4;
		m_missions[2][4].m_level = 3;
		m_missions[2][4].m_trackedType = PlayerStats.StatTypes.Int;
		m_missions[2][4].m_trackedValue = 31;
		m_missions[2][4].m_amountNeeded = m_missionValuesLevel3[4];
		m_missions[2][4].m_state = (Mission.State)0;
		m_missions[2][5].m_id = 5;
		m_missions[2][5].m_level = 3;
		m_missions[2][5].m_trackedType = PlayerStats.StatTypes.Int;
		m_missions[2][5].m_trackedValue = 5;
		m_missions[2][5].m_amountNeeded = m_missionValuesLevel3[5];
		m_missions[2][5].m_state = (Mission.State)0;
		m_missions[2][6].m_id = 6;
		m_missions[2][6].m_level = 3;
		m_missions[2][6].m_trackedType = PlayerStats.StatTypes.Int;
		m_missions[2][6].m_trackedValue = 9;
		m_missions[2][6].m_amountNeeded = m_missionValuesLevel3[6];
		m_missions[2][6].m_state = (Mission.State)0;
		m_missions[2][7].m_id = 7;
		m_missions[2][7].m_level = 3;
		m_missions[2][7].m_trackedType = PlayerStats.StatTypes.Int;
		m_missions[2][7].m_trackedValue = 18;
		m_missions[2][7].m_amountNeeded = m_missionValuesLevel3[7];
		m_missions[2][7].m_state = (Mission.State)0;
		m_missions[2][8].m_id = 8;
		m_missions[2][8].m_level = 3;
		m_missions[2][8].m_trackedType = PlayerStats.StatTypes.Int;
		m_missions[2][8].m_trackedValue = 8;
		m_missions[2][8].m_amountNeeded = m_missionValuesLevel3[8];
		m_missions[2][8].m_state = (Mission.State)0;
		m_missions[2][9].m_id = 9;
		m_missions[2][9].m_level = 3;
		m_missions[2][9].m_trackedType = PlayerStats.StatTypes.Int;
		m_missions[2][9].m_trackedValue = 46;
		m_missions[2][9].m_amountNeeded = m_missionValuesLevel3[9];
		m_missions[2][9].m_state = (Mission.State)0;
		m_missions[2][10].m_id = 10;
		m_missions[2][10].m_level = 3;
		m_missions[2][10].m_trackedType = PlayerStats.StatTypes.Float;
		m_missions[2][10].m_trackedValue = 0;
		m_missions[2][10].m_amountNeeded = m_missionValuesLevel3[10];
		m_missions[2][10].m_state = (Mission.State)0;
		m_missions[2][11].m_id = 11;
		m_missions[2][11].m_level = 3;
		m_missions[2][11].m_trackedType = PlayerStats.StatTypes.Int;
		m_missions[2][11].m_trackedValue = 86;
		m_missions[2][11].m_amountNeeded = m_missionValuesLevel3[11];
		m_missions[2][11].m_state = (Mission.State)0;
		m_missions[2][12].m_id = 12;
		m_missions[2][12].m_level = 3;
		m_missions[2][12].m_trackedType = PlayerStats.StatTypes.Int;
		m_missions[2][12].m_trackedValue = 18;
		m_missions[2][12].m_amountNeeded = m_missionValuesLevel3[12];
		m_missions[2][12].m_state = (Mission.State)0;
		m_missions[2][13].m_id = 13;
		m_missions[2][13].m_level = 3;
		m_missions[2][13].m_trackedType = PlayerStats.StatTypes.Int;
		m_missions[2][13].m_trackedValue = 85;
		m_missions[2][13].m_amountNeeded = m_missionValuesLevel3[13];
		m_missions[2][13].m_state = (Mission.State)0;
		m_missions[2][14].m_id = 14;
		m_missions[2][14].m_level = 3;
		m_missions[2][14].m_trackedType = PlayerStats.StatTypes.Int;
		m_missions[2][14].m_trackedValue = 46;
		m_missions[2][14].m_amountNeeded = m_missionValuesLevel3[14];
		m_missions[2][14].m_state = (Mission.State)0;
		m_missions[2][15].m_id = 15;
		m_missions[2][15].m_level = 3;
		m_missions[2][15].m_trackedType = PlayerStats.StatTypes.Int;
		m_missions[2][15].m_trackedValue = 42;
		m_missions[2][15].m_amountNeeded = m_missionValuesLevel3[15];
		m_missions[2][15].m_state = (Mission.State)0;
		m_missions[2][16].m_id = 16;
		m_missions[2][16].m_level = 3;
		m_missions[2][16].m_trackedType = PlayerStats.StatTypes.Float;
		m_missions[2][16].m_trackedValue = 9;
		m_missions[2][16].m_amountNeeded = m_missionValuesLevel3[16];
		m_missions[2][16].m_state = (Mission.State)0;
		m_missions[2][17].m_id = 17;
		m_missions[2][17].m_level = 3;
		m_missions[2][17].m_trackedType = PlayerStats.StatTypes.Int;
		m_missions[2][17].m_trackedValue = 88;
		m_missions[2][17].m_amountNeeded = m_missionValuesLevel3[17];
		m_missions[2][17].m_state = (Mission.State)0;
		m_missions[2][18].m_id = 18;
		m_missions[2][18].m_level = 3;
		m_missions[2][18].m_trackedType = PlayerStats.StatTypes.Int;
		m_missions[2][18].m_trackedValue = 84;
		m_missions[2][18].m_amountNeeded = m_missionValuesLevel3[18];
		m_missions[2][18].m_state = (Mission.State)0;
		m_missions[2][19].m_id = 19;
		m_missions[2][19].m_level = 3;
		m_missions[2][19].m_trackedType = PlayerStats.StatTypes.Int;
		m_missions[2][19].m_trackedValue = 82;
		m_missions[2][19].m_amountNeeded = m_missionValuesLevel3[19];
		m_missions[2][19].m_state = (Mission.State)0;
		m_looped = false;
	}

	private void InitializeMissions()
	{
		for (int i = 0; i < m_missions.Length; i++)
		{
			Mission currentMission = m_missions[i][m_missionSets[i][m_currentMissionSet]];
			SetTarget(ref currentMission);
			CheckMission(ref currentMission);
			m_missions[i][m_missionSets[i][m_currentMissionSet]] = currentMission;
		}
	}

	private void Event_OnGameDataSaveRequest()
	{
		PropertyStore.Store("Current Mission Set", m_currentMissionSet);
		Mission mission = m_missions[0][m_missionSets[0][m_currentMissionSet]];
		PropertyStore.Store("Mission 1 Start", mission.m_amountStart);
		PropertyStore.Store("Mission 1 Current", mission.m_amountCurrent);
		PropertyStore.Store("Mission 1 Target", mission.m_amountTarget);
		PropertyStore.Store("Mission 1 State", (int)mission.m_state);
		mission = m_missions[1][m_missionSets[1][m_currentMissionSet]];
		PropertyStore.Store("Mission 2 Start", mission.m_amountStart);
		PropertyStore.Store("Mission 2 Current", mission.m_amountCurrent);
		PropertyStore.Store("Mission 2 Target", mission.m_amountTarget);
		PropertyStore.Store("Mission 2 State", (int)mission.m_state);
		mission = m_missions[2][m_missionSets[2][m_currentMissionSet]];
		PropertyStore.Store("Mission 3 Start", mission.m_amountStart);
		PropertyStore.Store("Mission 3 Current", mission.m_amountCurrent);
		PropertyStore.Store("Mission 3 Target", mission.m_amountTarget);
		PropertyStore.Store("Mission 3 State", (int)mission.m_state);
		PropertyStore.Store("Missions Wrapped", m_looped);
	}

	private void Event_OnGameDataLoaded(ActiveProperties activeProperties)
	{
		if (!PropertyStore.ActiveProperties().DoesPropertyExist("Current Mission Set"))
		{
			m_currentMissionSet = 0;
			CreateMissions();
			InitializeMissions();
			return;
		}
		m_currentMissionSet = activeProperties.GetInt("Current Mission Set");
		Mission mission = m_missions[0][m_missionSets[0][m_currentMissionSet]];
		mission.m_amountStart = activeProperties.GetLong("Mission 1 Start");
		mission.m_amountCurrent = activeProperties.GetLong("Mission 1 Current");
		mission.m_amountTarget = activeProperties.GetLong("Mission 1 Target");
		mission.m_state = (Mission.State)activeProperties.GetInt("Mission 1 State");
		m_missions[0][m_missionSets[0][m_currentMissionSet]] = mission;
		mission = m_missions[1][m_missionSets[1][m_currentMissionSet]];
		mission.m_amountStart = activeProperties.GetLong("Mission 2 Start");
		mission.m_amountCurrent = activeProperties.GetLong("Mission 2 Current");
		mission.m_amountTarget = activeProperties.GetLong("Mission 2 Target");
		mission.m_state = (Mission.State)activeProperties.GetInt("Mission 2 State");
		m_missions[1][m_missionSets[1][m_currentMissionSet]] = mission;
		mission = m_missions[2][m_missionSets[2][m_currentMissionSet]];
		mission.m_amountStart = activeProperties.GetLong("Mission 3 Start");
		mission.m_amountCurrent = activeProperties.GetLong("Mission 3 Current");
		mission.m_amountTarget = activeProperties.GetLong("Mission 3 Target");
		mission.m_state = (Mission.State)activeProperties.GetInt("Mission 3 State");
		m_missions[2][m_missionSets[2][m_currentMissionSet]] = mission;
		m_looped = activeProperties.GetBool("Missions Wrapped");
		if (m_looped && m_currentMissionSet == 0)
		{
			m_currentMissionSet = 10;
			m_looped = false;
			for (int i = 0; i < m_missions.Length; i++)
			{
				Mission currentMission = m_missions[i][m_missionSets[i][m_currentMissionSet]];
				SetTarget(ref currentMission);
				m_missions[i][m_missionSets[i][m_currentMissionSet]] = currentMission;
			}
		}
		else if (m_looped && m_currentMissionSet < 19)
		{
			m_looped = false;
			for (int j = 0; j < m_missions.Length; j++)
			{
				Mission currentMission2 = m_missions[j][m_missionSets[j][m_currentMissionSet]];
				SetTarget(ref currentMission2);
				m_missions[j][m_missionSets[j][m_currentMissionSet]] = currentMission2;
			}
		}
	}

	private void Event_OnNewGameStarted()
	{
		for (int i = 0; i < m_missions.Length; i++)
		{
			Mission currentMission = m_missions[i][m_missionSets[i][m_currentMissionSet]];
			if ((currentMission.m_state & Mission.State.ResetEachRun) == Mission.State.ResetEachRun || (currentMission.m_state & Mission.State.Initialized) != Mission.State.Initialized)
			{
				SetTarget(ref currentMission);
			}
			currentMission.m_state &= ~Mission.State.JustCompleted;
			m_missions[i][m_missionSets[i][m_currentMissionSet]] = currentMission;
		}
	}

	private void Event_OnGameFinished()
	{
		for (int i = 0; i < m_missions.Length; i++)
		{
			Mission currentMission = m_missions[i][m_missionSets[i][m_currentMissionSet]];
			CheckMission(ref currentMission);
			m_missions[i][m_missionSets[i][m_currentMissionSet]] = currentMission;
		}
	}

	private void CompleteMission(ref Mission currentMission, bool purchased)
	{
		currentMission.m_state = currentMission.m_state | Mission.State.Completed | Mission.State.JustCompleted;
		currentMission.m_amountCurrent = currentMission.m_amountTarget;
		PlayerStats.IncreaseStat(PlayerStats.StatNames.MissionsCompleted_Total, 1);
		bool flag = GetCompleteMissionCount() == 3;
		m_completeEventParameters[0] = currentMission.m_level - 1;
		m_completeEventParameters[1] = flag;
		EventDispatch.GenerateEvent("OnMissionComplete", m_completeEventParameters);
		string missionName = "NULL";
		if (currentMission.m_level == 1)
		{
			missionName = ((MissionLevel1)currentMission.m_id).ToString();
		}
		else if (currentMission.m_level == 2)
		{
			missionName = ((MissionLevel2)currentMission.m_id).ToString();
		}
		else if (currentMission.m_level == 3)
		{
			missionName = ((MissionLevel3)currentMission.m_id).ToString();
		}
		GameAnalytics.MissionComplete(missionName, purchased);
		PropertyStore.Save();
	}

	private Mission[] GetCurrentSet()
	{
		Mission[] array = new Mission[3];
		for (int i = 0; i < m_missions.Length; i++)
		{
			ref Mission reference = ref array[i];
			reference = m_missions[i][m_missionSets[i][m_currentMissionSet]];
		}
		return array;
	}

	private Mission[] ChangeSet()
	{
		if (++m_currentMissionSet >= 19)
		{
			m_looped = true;
		}
		else
		{
			for (int i = 0; i < m_missions.Length; i++)
			{
				Mission currentMission = m_missions[i][m_missionSets[i][m_currentMissionSet]];
				SetTarget(ref currentMission);
				m_missions[i][m_missionSets[i][m_currentMissionSet]] = currentMission;
			}
		}
		EventDispatch.GenerateEvent("OnMissionSetChanged");
		PropertyStore.Save();
		return GetCurrentSet();
	}

	private bool MissionComplete(int missionGroup)
	{
		bool flag = (m_missions[missionGroup][m_missionSets[missionGroup][m_currentMissionSet]].m_state & Mission.State.JustCompleted) == Mission.State.JustCompleted;
		bool flag2 = (m_missions[missionGroup][m_missionSets[missionGroup][m_currentMissionSet]].m_state & Mission.State.Completed) == Mission.State.Completed;
		return flag || flag2;
	}

	private int GetCompleteMissionCount()
	{
		int num = 0;
		for (int i = 0; i < 3; i++)
		{
			if (MissionComplete(i))
			{
				num++;
			}
		}
		return num;
	}

	private void Event_ABTestingReady()
	{
		int testValue = ABTesting.GetTestValue(ABTesting.Tests.RSR_MissionSetAmount);
		if (testValue != -1)
		{
			RSRRewardPerSet = testValue;
		}
	}
}
