using System;
using UnityEngine;

public class AchievementTracker : MonoBehaviour
{
	public struct Achievement
	{
		[Flags]
		public enum State
		{
			Completed = 1,
			SpecialCheck = 2,
			ResetEachRun = 4,
			Pending = 0x10
		}

		public Achievements.Types m_id;

		public string m_name;

		public PlayerStats.StatTypes m_trackedType;

		public int m_trackedValue;

		public long m_amountStart;

		public long m_amountCurrent;

		public long m_amountTarget;

		public long m_amountNeeded;

		public State m_state;
	}

	private const string AchievementStatePropertyPrefix = "AchievementState_";

	public static float toFloatDivision = 10000f;

	[SerializeField]
	private int[] m_achivementeNeededValues;

	private Achievement[] m_achievements = new Achievement[Enum.GetNames(typeof(Achievements.Types)).Length];

	public void Initialize()
	{
		if (m_achivementeNeededValues == null)
		{
			m_achivementeNeededValues = new int[Enum.GetNames(typeof(Achievements.Types)).Length];
		}
		else if (m_achivementeNeededValues.Length != Enum.GetNames(typeof(Achievements.Types)).Length)
		{
			m_achivementeNeededValues = new int[Enum.GetNames(typeof(Achievements.Types)).Length];
		}
	}

	public void Awake()
	{
		Initialize();
	}

	private void Start()
	{
		EventDispatch.RegisterInterest("OnNewGameStarted", this);
		EventDispatch.RegisterInterest("OnGameFinished", this, EventDispatch.Priority.Lowest);
		EventDispatch.RegisterInterest("PowerUpLeveledUp", this, EventDispatch.Priority.Lowest);
		EventDispatch.RegisterInterest("OnGameDataLoaded", this);
		EventDispatch.RegisterInterest("OnGameDataSaveRequest", this);
		CreateAchievements();
	}

	private void CreateAchievements()
	{
		m_achievements[0].m_id = Achievements.Types.SonicRampage;
		m_achievements[0].m_trackedType = PlayerStats.StatTypes.Float;
		m_achievements[0].m_trackedValue = 0;
		m_achievements[0].m_amountNeeded = m_achivementeNeededValues[0];
		m_achievements[0].m_amountTarget = m_achievements[0].m_amountNeeded * (long)toFloatDivision;
		m_achievements[0].m_state = (Achievement.State)0;
		m_achievements[1].m_id = Achievements.Types.RingHoarder;
		m_achievements[1].m_trackedType = PlayerStats.StatTypes.Int;
		m_achievements[1].m_trackedValue = 11;
		m_achievements[1].m_amountNeeded = m_achivementeNeededValues[1];
		m_achievements[1].m_amountTarget = m_achievements[1].m_amountNeeded;
		m_achievements[1].m_state = (Achievement.State)0;
		m_achievements[2].m_id = Achievements.Types.PowerOverload;
		m_achievements[2].m_trackedType = PlayerStats.StatTypes.Int;
		m_achievements[2].m_trackedValue = 45;
		m_achievements[2].m_amountNeeded = m_achivementeNeededValues[2];
		m_achievements[2].m_amountTarget = m_achievements[2].m_amountNeeded;
		m_achievements[2].m_state = (Achievement.State)0;
		m_achievements[3].m_id = Achievements.Types.KnuclesOnTheMove;
		m_achievements[3].m_trackedType = PlayerStats.StatTypes.Float;
		m_achievements[3].m_trackedValue = 5;
		m_achievements[3].m_amountNeeded = m_achivementeNeededValues[3];
		m_achievements[3].m_amountTarget = m_achievements[3].m_amountNeeded * (long)toFloatDivision;
		m_achievements[3].m_state = (Achievement.State)0;
		m_achievements[4].m_id = Achievements.Types.CaChing;
		m_achievements[4].m_trackedType = PlayerStats.StatTypes.Int;
		m_achievements[4].m_trackedValue = 61;
		m_achievements[4].m_amountNeeded = m_achivementeNeededValues[4];
		m_achievements[4].m_amountTarget = m_achievements[4].m_amountNeeded;
		m_achievements[4].m_state = (Achievement.State)0;
		m_achievements[5].m_id = Achievements.Types.SkyIsTheLimit;
		m_achievements[5].m_trackedType = PlayerStats.StatTypes.Long;
		m_achievements[5].m_trackedValue = 0;
		m_achievements[5].m_amountNeeded = m_achivementeNeededValues[5];
		m_achievements[5].m_amountTarget = m_achievements[5].m_amountNeeded;
		m_achievements[5].m_state = Achievement.State.ResetEachRun;
		m_achievements[6].m_id = Achievements.Types.Ringmaster;
		m_achievements[6].m_trackedType = PlayerStats.StatTypes.Int;
		m_achievements[6].m_trackedValue = 11;
		m_achievements[6].m_amountNeeded = m_achivementeNeededValues[6];
		m_achievements[6].m_amountTarget = m_achievements[6].m_amountNeeded;
		m_achievements[6].m_state = Achievement.State.ResetEachRun;
		m_achievements[7].m_id = Achievements.Types.SuperSonic;
		m_achievements[7].m_trackedType = PlayerStats.StatTypes.Float;
		m_achievements[7].m_trackedValue = 0;
		m_achievements[7].m_amountNeeded = m_achivementeNeededValues[7];
		m_achievements[7].m_amountTarget = m_achievements[7].m_amountNeeded * (long)toFloatDivision;
		m_achievements[7].m_state = Achievement.State.ResetEachRun;
		m_achievements[8].m_id = Achievements.Types.MissionMaster;
		m_achievements[8].m_trackedType = PlayerStats.StatTypes.Int;
		m_achievements[8].m_trackedValue = 62;
		m_achievements[8].m_amountNeeded = m_achivementeNeededValues[8];
		m_achievements[8].m_amountTarget = m_achievements[8].m_amountNeeded;
		m_achievements[8].m_state = (Achievement.State)0;
		m_achievements[9].m_id = Achievements.Types.OnARoll;
		m_achievements[9].m_trackedType = PlayerStats.StatTypes.Int;
		m_achievements[9].m_trackedValue = 62;
		m_achievements[9].m_amountNeeded = m_achivementeNeededValues[9];
		m_achievements[9].m_amountTarget = m_achievements[9].m_amountNeeded;
		m_achievements[9].m_state = (Achievement.State)0;
		m_achievements[10].m_id = Achievements.Types.ActionPacked;
		m_achievements[10].m_trackedType = PlayerStats.StatTypes.Int;
		m_achievements[10].m_trackedValue = 62;
		m_achievements[10].m_amountNeeded = m_achivementeNeededValues[10];
		m_achievements[10].m_amountTarget = m_achievements[10].m_amountNeeded;
		m_achievements[10].m_state = (Achievement.State)0;
		m_achievements[11].m_id = Achievements.Types.HotHeels;
		m_achievements[11].m_trackedType = PlayerStats.StatTypes.Float;
		m_achievements[11].m_trackedValue = 0;
		m_achievements[11].m_amountNeeded = m_achivementeNeededValues[11];
		m_achievements[11].m_amountTarget = m_achievements[11].m_amountNeeded * (long)toFloatDivision;
		m_achievements[11].m_state = Achievement.State.ResetEachRun;
		m_achievements[12].m_id = Achievements.Types.SEGAMember;
		m_achievements[12].m_trackedType = PlayerStats.StatTypes.Int;
		m_achievements[12].m_trackedValue = 60;
		m_achievements[12].m_amountNeeded = m_achivementeNeededValues[12];
		m_achievements[12].m_amountTarget = m_achievements[12].m_amountNeeded;
		m_achievements[12].m_state = Achievement.State.SpecialCheck;
		m_achievements[13].m_id = Achievements.Types.EasyTarget;
		m_achievements[13].m_trackedType = PlayerStats.StatTypes.Int;
		m_achievements[13].m_trackedValue = 11;
		m_achievements[13].m_amountNeeded = m_achivementeNeededValues[13];
		m_achievements[13].m_amountTarget = m_achievements[13].m_amountNeeded;
		m_achievements[13].m_state = Achievement.State.ResetEachRun;
		m_achievements[14].m_id = Achievements.Types.WarmUp;
		m_achievements[14].m_trackedType = PlayerStats.StatTypes.Float;
		m_achievements[14].m_trackedValue = 0;
		m_achievements[14].m_amountNeeded = m_achivementeNeededValues[14];
		m_achievements[14].m_amountTarget = m_achievements[14].m_amountNeeded * (long)toFloatDivision;
		m_achievements[14].m_state = Achievement.State.ResetEachRun;
	}

	private void Update()
	{
		if ((m_achievements[12].m_state & Achievement.State.Completed) != Achievement.State.Completed)
		{
			CheckAchievement(ref m_achievements[12]);
		}
		if ((m_achievements[2].m_state & Achievement.State.Completed) != Achievement.State.Completed)
		{
			CheckAchievement(ref m_achievements[2]);
		}
		if ((m_achievements[10].m_state & Achievement.State.Completed) != Achievement.State.Completed)
		{
			CheckAchievement(ref m_achievements[10]);
		}
		if ((m_achievements[9].m_state & Achievement.State.Completed) != Achievement.State.Completed)
		{
			CheckAchievement(ref m_achievements[9]);
		}
		if ((m_achievements[8].m_state & Achievement.State.Completed) != Achievement.State.Completed)
		{
			CheckAchievement(ref m_achievements[8]);
		}
		if (GameState.GetMode() == GameState.Mode.Menu)
		{
			return;
		}
		for (int i = 0; i < m_achievements.Length; i++)
		{
			if ((m_achievements[i].m_state & Achievement.State.Completed) != Achievement.State.Completed)
			{
				CheckAchievement(ref m_achievements[i]);
			}
		}
	}

	private void CheckAchievement(ref Achievement currentAchievement)
	{
		PlayerStats.Stats currentStats = PlayerStats.GetCurrentStats();
		if (currentAchievement.m_id == Achievements.Types.MissionMaster && MissionTracker.AllMissionsComplete() && currentStats.m_trackedStats[currentAchievement.m_trackedValue] < currentAchievement.m_amountTarget)
		{
			currentStats.m_trackedStats[currentAchievement.m_trackedValue] = (int)currentAchievement.m_amountTarget;
		}
		switch (currentAchievement.m_trackedType)
		{
		case PlayerStats.StatTypes.Int:
			currentAchievement.m_amountCurrent = currentStats.m_trackedStats[currentAchievement.m_trackedValue];
			break;
		case PlayerStats.StatTypes.Long:
			currentAchievement.m_amountCurrent = currentStats.m_trackedLongStats[currentAchievement.m_trackedValue];
			break;
		case PlayerStats.StatTypes.Float:
			currentAchievement.m_amountCurrent = (long)(currentStats.m_trackedDistances[currentAchievement.m_trackedValue] * toFloatDivision);
			break;
		}
		if (currentAchievement.m_amountCurrent >= currentAchievement.m_amountTarget || (currentAchievement.m_state & Achievement.State.Pending) == Achievement.State.Pending)
		{
			currentAchievement.m_state |= Achievement.State.Pending;
			Achievements.AwardAchievement(currentAchievement.m_id, 100f, ref currentAchievement);
		}
	}

	private void SetTarget(ref Achievement currentAchievement)
	{
		if ((currentAchievement.m_state & Achievement.State.Completed) != Achievement.State.Completed)
		{
			PlayerStats.Stats currentStats = PlayerStats.GetCurrentStats();
			switch (currentAchievement.m_trackedType)
			{
			case PlayerStats.StatTypes.Int:
				currentAchievement.m_amountStart = currentStats.m_trackedStats[currentAchievement.m_trackedValue];
				currentAchievement.m_amountTarget = currentAchievement.m_amountStart + currentAchievement.m_amountNeeded;
				break;
			case PlayerStats.StatTypes.Long:
				currentAchievement.m_amountStart = currentStats.m_trackedLongStats[currentAchievement.m_trackedValue];
				currentAchievement.m_amountTarget = currentAchievement.m_amountStart + currentAchievement.m_amountNeeded;
				break;
			case PlayerStats.StatTypes.Float:
				currentAchievement.m_amountStart = (long)(currentStats.m_trackedDistances[currentAchievement.m_trackedValue] * toFloatDivision);
				currentAchievement.m_amountTarget = (long)((float)currentAchievement.m_amountStart + (float)currentAchievement.m_amountNeeded * toFloatDivision);
				break;
			}
			currentAchievement.m_amountCurrent = currentAchievement.m_amountStart;
		}
	}

	private void Event_OnNewGameStarted()
	{
		for (int i = 0; i < m_achievements.Length; i++)
		{
			if ((m_achievements[i].m_state & Achievement.State.Completed) != Achievement.State.Completed && (m_achievements[i].m_state & Achievement.State.ResetEachRun) == Achievement.State.ResetEachRun)
			{
				SetTarget(ref m_achievements[i]);
			}
		}
	}

	private void Event_OnGameFinished()
	{
		for (int i = 0; i < m_achievements.Length; i++)
		{
			CheckAchievement(ref m_achievements[i]);
			float progress = (float)(m_achievements[i].m_amountCurrent - m_achievements[i].m_amountStart) / (float)(m_achievements[i].m_amountTarget - m_achievements[i].m_amountStart) * 100f;
			Achievements.AwardAchievement(m_achievements[i].m_id, progress, ref m_achievements[i]);
		}
	}

	private void Event_PowerUpLeveledUp(PowerUps.Type pUp)
	{
		CheckAchievement(ref m_achievements[2]);
		float progress = (float)(m_achievements[2].m_amountCurrent - m_achievements[2].m_amountStart) / (float)(m_achievements[2].m_amountTarget - m_achievements[2].m_amountStart) * 100f;
		Achievements.AwardAchievement(m_achievements[2].m_id, progress, ref m_achievements[2]);
	}

	private void Event_OnGameDataSaveRequest()
	{
		string[] names = Enum.GetNames(typeof(Achievements.Types));
		for (int i = 0; i < names.Length; i++)
		{
			PropertyStore.Store("AchievementState_" + names[i], (int)m_achievements[i].m_state);
		}
	}

	private void Event_OnGameDataLoaded(ActiveProperties activeProperties)
	{
		string[] names = Enum.GetNames(typeof(Achievements.Types));
		for (int i = 0; i < names.Length; i++)
		{
			m_achievements[i].m_state |= (Achievement.State)activeProperties.GetInt("AchievementState_" + names[i]);
		}
	}
}
