using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
	public class Stats
	{
		public int[] m_trackedStats = new int[Enum.GetNames(typeof(StatNames)).Length];

		public long[] m_trackedLongStats = new long[Enum.GetNames(typeof(LongStatNames)).Length];

		public float[] m_trackedDistances = new float[Enum.GetNames(typeof(DistanceNames)).Length];

		public DateTime[] m_trackedDates = new DateTime[Enum.GetNames(typeof(DateNames)).Length];
	}

	public enum StatTypes
	{
		Int,
		Long,
		Float,
		Date
	}

	public enum StatNames
	{
		NumberOfRuns_Total,
		NumberOfRuns_Session,
		Enemies_Total,
		Enemies_Session,
		Enemies_Run,
		EnemiesHoming_Total,
		EnemiesRolling_Total,
		EnemiesDiving_Total,
		EnemiesAir_Total,
		EnemyStreaks_Total,
		EnemyStreaks_Run,
		RingsCollected_Total,
		RingsCollected_Session,
		RingsCollected_Run,
		RingsBanked_Total,
		RingsBanked_Session,
		RingsBanked_Run,
		RingsHeld,
		RingStreaks_Total,
		RingStreaks_Run,
		RingsPurchased_Total,
		RingsSpent_Total,
		TimesDropRings_Total,
		RingsDropped_Total,
		VasesDestroyed_Total,
		MinesTriped_Total,
		MinesTriped_Run,
		RollsMiddle_Total,
		BridgesRolled_Total,
		BridgesRolled_Run,
		CorkscrewsRan_Total,
		LoopsBoosted_Total,
		DashUses_Total,
		DashUses_Session,
		DashUses_Run,
		TempleVisits_Total,
		GrassVisits_Total,
		BeachVisits_Total,
		PowerMagnetsPicked_Total,
		PowerMagnetsPicked_Session,
		PowerMagnetsPicked_Run,
		PowerShieldPicked_Run,
		PowerupsPicked_Total,
		PowerupsPicked_Session,
		PowerupsPicked_Run,
		MaxedPowerUps_Total,
		Level5PowerUps_Total,
		RevivesUsed_Total,
		RevivesUsed_Session,
		RevivesUsed_Run,
		HeadstartsUsed_Total,
		HeadstartsUsed_Session,
		HeadstartsUsed_Run,
		SuperHeadstartsUsed_Total,
		SuperHeadstartsUsed_Session,
		SuperHeadstartsUsed_Run,
		RingsBanked_Run_Best,
		RingStreaks_Run_Best,
		Enemies_Run_Best,
		EnemyStreaks_Run_Best,
		RegisteredFacebook,
		RingsAsAmy_Total,
		MissionsCompleted_Total,
		TimePlayed_Total,
		TimePlayed_Session,
		TimePlayed_Run,
		MaxMultiplier_Total,
		MaxMultiplier_Session,
		MaxMultiplier_Run,
		StarRingsEarned_Total,
		ShopPurchases_Total,
		ShopPurchases_Session,
		ShopPurchases_Run,
		InAppPurchases_Total,
		InAppPurchases_Session,
		InAppPurchases_Run,
		NumberOfGamingSessions_Total,
		TimesBragged_Total,
		FirstLeaderboardRewarded,
		NumberOfSessions_Total,
		HighScoreRewarded,
		DCsCompleted_Total,
		DCsCompletedConsecutive_Total,
		CrabmeatJumpedOver_Run,
		SpikysJumpedOver_Total,
		PlantPotsJumpedOver_Total,
		TotemsDashedThrough_Total,
		TotemsDashedThrough_Run,
		TimeAirbourne_Total,
		TimeAirbourne_Run,
		SpringsPassed_Run,
		BossBattles_Total,
		BossBattlesEasy_Total,
		BossBattlesHard_Total
	}

	public enum LongStatNames
	{
		Score,
		ScoreLastDroppedRings,
		Score_Run_Best
	}

	public enum DistanceNames
	{
		DistanceRun_Total,
		DistanceLastMissedRing,
		DistanceLastPickedRing,
		DistanceLastBanked,
		Distance_Run_Best,
		DistanceAsKnuckles,
		DistanceRun_Session,
		DistanceRun_Run,
		DistanceChangedLane,
		DistanceDashMeterFilled
	}

	public enum DateNames
	{
		LastDayPlayed,
		LastDayNotPlayed
	}

	[SerializeField]
	private List<string> m_plantpotObjectNames;

	[SerializeField]
	private List<string> m_totemObjectNames;

	private static PlayerStats m_singleton;

	private static Stats s_currentStats = new Stats();

	private CharacterManager m_characterManager;

	private int m_previousRingsBanked;

	private float m_previousDistance;

	private int m_lastSessionTimeSaved;

	private float m_sessionTime;

	private float m_runTime;

	private Track.Lane m_previousLane;

	private float m_partialTime;

	private bool m_needsSave;

	private int m_loadCount;

	private bool m_displayFacebookReward;

	public static bool DashMeterFilled { get; set; }

	public static bool Airbourne { get; set; }

	public static Stats GetCurrentStats()
	{
		return s_currentStats;
	}

	public static int GetStat(StatNames statName)
	{
		return GetCurrentStats().m_trackedStats[(int)statName];
	}

	public static void IncreaseStat(StatNames name, int amount)
	{
		s_currentStats.m_trackedStats[(int)name] += amount;
		if (m_singleton != null && m_singleton.m_characterManager.GetCurrentCharacter() == Characters.Type.Amy && name == StatNames.RingsCollected_Total)
		{
			s_currentStats.m_trackedStats[61] += amount;
		}
	}

	public static void IncreaseLongStat(LongStatNames name, long amount)
	{
		s_currentStats.m_trackedLongStats[(int)name] += amount;
	}

	public static void IncreaseDistance(DistanceNames name, float amount)
	{
		if (!(amount < 0f))
		{
			s_currentStats.m_trackedDistances[(int)name] += amount;
		}
	}

	public static void UpdateDistanceToCurrent(DistanceNames name)
	{
		s_currentStats.m_trackedDistances[(int)name] = s_currentStats.m_trackedDistances[0];
	}

	public static float GetSecondsFromLastSaved()
	{
		return (m_singleton.m_sessionTime * 10f - (float)m_singleton.m_lastSessionTimeSaved) / 10f;
	}

	public static void UpdateMultiplier(uint multiplierValue)
	{
		if (multiplierValue > s_currentStats.m_trackedStats[68])
		{
			s_currentStats.m_trackedStats[68] = (int)multiplierValue;
		}
		if (multiplierValue > s_currentStats.m_trackedStats[67])
		{
			s_currentStats.m_trackedStats[67] = (int)multiplierValue;
		}
		if (multiplierValue > s_currentStats.m_trackedStats[66])
		{
			s_currentStats.m_trackedStats[66] = (int)multiplierValue;
		}
	}

	private static string ScoreToTrackEvent(long score)
	{
		if (score >= 1000000)
		{
			return "Score_1m";
		}
		if (score >= 500000)
		{
			return "Score_500K";
		}
		if (score >= 200000)
		{
			return "Score_200K";
		}
		if (score >= 100000)
		{
			return "Score_100K";
		}
		if (score >= 50000)
		{
			return "Score_50K";
		}
		if (score >= 20000)
		{
			return "Score_20K";
		}
		if (score > 10000)
		{
			return "Score_10K";
		}
		return string.Empty;
	}

	public static void UpdateFinalScore(long score)
	{
		s_currentStats.m_trackedLongStats[0] = score;
		if (s_currentStats.m_trackedLongStats[0] > s_currentStats.m_trackedLongStats[2])
		{
			string text = ScoreToTrackEvent(s_currentStats.m_trackedLongStats[2]);
			s_currentStats.m_trackedLongStats[2] = s_currentStats.m_trackedLongStats[0];
			string text2 = ScoreToTrackEvent(s_currentStats.m_trackedLongStats[2]);
			if (text2 != text)
			{
				SLAnalytics.LogTrackingEvent(text2, string.Empty);
			}
		}
	}

	public static void EnterSetPiece(TrackDatabase.PieceType type)
	{
		switch (type)
		{
		case TrackDatabase.PieceType.SetPieceLoop:
			IncreaseStat(StatNames.LoopsBoosted_Total, 1);
			break;
		case TrackDatabase.PieceType.SetPieceCorkscrew:
			IncreaseStat(StatNames.CorkscrewsRan_Total, 1);
			break;
		}
	}

	public static void JumpOverObstacle(UnityEngine.Object obstacle)
	{
		for (int i = 0; i < m_singleton.m_plantpotObjectNames.Count; i++)
		{
			if (obstacle.name.StartsWith(m_singleton.m_plantpotObjectNames[i]))
			{
				IncreaseStat(StatNames.PlantPotsJumpedOver_Total, 1);
				break;
			}
		}
	}

	public static void DashThroughObstacle(UnityEngine.Object obstacle)
	{
		for (int i = 0; i < m_singleton.m_totemObjectNames.Count; i++)
		{
			if (obstacle.name.StartsWith(m_singleton.m_totemObjectNames[i]))
			{
				IncreaseStat(StatNames.TotemsDashedThrough_Run, 1);
				IncreaseStat(StatNames.TotemsDashedThrough_Total, 1);
				break;
			}
		}
	}

	public static PlayerStats instance()
	{
		return m_singleton;
	}

	private void Start()
	{
		m_singleton = this;
		EventDispatch.RegisterInterest("OnNewGameAboutToStart", this, EventDispatch.Priority.High);
		EventDispatch.RegisterInterest("OnGameFinished", this, EventDispatch.Priority.Low);
		EventDispatch.RegisterInterest("OnGameDataLoaded", this);
		EventDispatch.RegisterInterest("OnGameDataSaveRequest", this);
		EventDispatch.RegisterInterest("MainMenuActive", this);
		EventDispatch.RegisterInterest("OnSonicStumble", this, EventDispatch.Priority.Highest);
		EventDispatch.RegisterInterest("OnRingStreakCompleted", this);
		EventDispatch.RegisterInterest("OnRingBankRequest", this, EventDispatch.Priority.Lowest);
		EventDispatch.RegisterInterest("OnRingsAwarded", this);
		EventDispatch.RegisterInterest("OnDashMeterFilled", this);
		EventDispatch.RegisterInterest("OnEnemyKilled", this);
		EventDispatch.RegisterInterest("PowerUpLeveledUp", this);
		EventDispatch.RegisterInterest("OnFacebookLogin", this);
		DashMeterFilled = false;
		m_characterManager = UnityEngine.Object.FindObjectOfType(typeof(CharacterManager)) as CharacterManager;
		m_needsSave = false;
	}

	private void OnApplicationQuit()
	{
	}

	private void Event_OnGameDataSaveRequest()
	{
		s_currentStats.m_trackedStats[63] += (int)(m_sessionTime * 10f) - m_lastSessionTimeSaved;
		s_currentStats.m_trackedStats[64] = (int)(m_sessionTime * 10f);
		m_lastSessionTimeSaved = (int)(m_sessionTime * 10f);
		string[] names = Enum.GetNames(typeof(StatNames));
		for (int i = 0; i < names.Length; i++)
		{
			if (!names[i].EndsWith("_Run"))
			{
				PropertyStore.Store(names[i], s_currentStats.m_trackedStats[i]);
			}
		}
		names = Enum.GetNames(typeof(LongStatNames));
		for (int j = 0; j < names.Length; j++)
		{
			PropertyStore.Store(names[j], s_currentStats.m_trackedLongStats[j]);
		}
		names = Enum.GetNames(typeof(DistanceNames));
		for (int k = 0; k < names.Length; k++)
		{
			if (!names[k].EndsWith("_Run"))
			{
				PropertyStore.Store(names[k], s_currentStats.m_trackedDistances[k]);
			}
		}
		CultureInfo cultureInfo = new CultureInfo("en-US");
		names = Enum.GetNames(typeof(DateNames));
		for (int l = 0; l < names.Length; l++)
		{
			PropertyStore.Store(names[l], s_currentStats.m_trackedDates[l].Date.ToString(cultureInfo.DateTimeFormat));
		}
		PropertyStore.Store("VersionID", "1.8.0");
		m_needsSave = false;
	}

	private void Event_OnGameDataLoaded(ActiveProperties activeProperties)
	{
		string[] names = Enum.GetNames(typeof(StatNames));
		for (int i = 0; i < names.Length; i++)
		{
			if (!names[i].EndsWith("_Run"))
			{
				s_currentStats.m_trackedStats[i] = activeProperties.GetInt(names[i]);
			}
		}
		if (s_currentStats.m_trackedStats[46] < s_currentStats.m_trackedStats[45])
		{
			s_currentStats.m_trackedStats[46] = s_currentStats.m_trackedStats[45];
		}
		names = Enum.GetNames(typeof(LongStatNames));
		for (int j = 0; j < names.Length; j++)
		{
			s_currentStats.m_trackedLongStats[j] = activeProperties.GetLong(names[j]);
		}
		names = Enum.GetNames(typeof(DistanceNames));
		for (int k = 0; k < names.Length; k++)
		{
			if (!names[k].EndsWith("_Run"))
			{
				s_currentStats.m_trackedDistances[k] = activeProperties.GetFloat(names[k]);
			}
		}
		CultureInfo provider = new CultureInfo("en-US");
		names = Enum.GetNames(typeof(DateNames));
		for (int l = 0; l < names.Length; l++)
		{
			if (!DateTime.TryParse(activeProperties.GetString(names[l]), provider, DateTimeStyles.None, out s_currentStats.m_trackedDates[l]))
			{
				ref DateTime reference = ref s_currentStats.m_trackedDates[l];
				reference = DateTime.Now.AddDays(-1.0).Date;
			}
		}
		names = Enum.GetNames(typeof(StatNames));
		for (int m = 0; m < names.Length; m++)
		{
			if (names[m].EndsWith("_Session"))
			{
				s_currentStats.m_trackedStats[m] = 0;
			}
		}
		names = Enum.GetNames(typeof(DistanceNames));
		for (int n = 0; n < names.Length; n++)
		{
			if (names[n].EndsWith("_Session"))
			{
				s_currentStats.m_trackedDistances[n] = 0f;
			}
		}
		IncreaseStat(StatNames.NumberOfSessions_Total, 1);
		m_loadCount++;
		if (m_loadCount != 2)
		{
			return;
		}
		if (activeProperties.GetPropertyCount() > 0)
		{
			string @string = activeProperties.GetString("VersionID");
			if ("1.8.0" != @string)
			{
				SLAnalytics.LogTrackingEvent("Update", "1.8.0");
				m_needsSave = true;
			}
		}
		else
		{
			SLAnalytics.LogTrackingEvent("Open", "Install");
			m_needsSave = true;
		}
		int num = s_currentStats.m_trackedStats[79];
		if (num >= 2)
		{
			SLAnalytics.LogTrackingEvent("RepeatOpen", num.ToString());
		}
	}

	private void Event_MainMenuActive()
	{
		if (m_needsSave)
		{
			m_needsSave = false;
			PropertyStore.Save();
		}
	}

	private void Event_OnNewGameAboutToStart()
	{
		IncreaseStat(StatNames.NumberOfRuns_Total, 1);
		IncreaseStat(StatNames.NumberOfRuns_Session, 1);
		int num = s_currentStats.m_trackedStats[1];
		if (num >= 2)
		{
			SLAnalytics.LogTrackingEvent("RepeatPlay", num.ToString());
		}
		if (!s_currentStats.m_trackedDates[0].Date.Equals(DateTime.Now.Date))
		{
			if (!s_currentStats.m_trackedDates[0].Date.Equals(DateTime.Now.AddDays(-1.0).Date))
			{
				ref DateTime reference = ref s_currentStats.m_trackedDates[1];
				reference = DateTime.Now.AddDays(-1.0).Date;
			}
			ref DateTime reference2 = ref s_currentStats.m_trackedDates[0];
			reference2 = DateTime.Now.Date;
		}
		string[] names = Enum.GetNames(typeof(StatNames));
		for (int i = 0; i < names.Length; i++)
		{
			if (names[i].EndsWith("_Run"))
			{
				s_currentStats.m_trackedStats[i] = 0;
			}
		}
		names = Enum.GetNames(typeof(DistanceNames));
		for (int j = 0; j < names.Length; j++)
		{
			if (names[j].EndsWith("_Run"))
			{
				s_currentStats.m_trackedDistances[j] = 0f;
			}
		}
		s_currentStats.m_trackedLongStats[0] = 0L;
		s_currentStats.m_trackedLongStats[1] = 0L;
		UpdateDistanceToCurrent(DistanceNames.DistanceLastMissedRing);
		UpdateDistanceToCurrent(DistanceNames.DistanceLastPickedRing);
		UpdateDistanceToCurrent(DistanceNames.DistanceChangedLane);
		UpdateDistanceToCurrent(DistanceNames.DistanceLastBanked);
		m_previousRingsBanked = 0;
		m_previousDistance = 0f;
		DashMeterFilled = false;
		Airbourne = false;
		m_partialTime = 0f;
		m_runTime = 0f;
		GameAnalytics.RunStart(m_singleton.m_characterManager.GetCurrentCharacter());
	}

	private void Event_OnGameFinished()
	{
		s_currentStats.m_trackedStats[17] = RingStorage.HeldRings;
		s_currentStats.m_trackedStats[14] = RingStorage.TotalBankedRings;
		IncreaseStat(StatNames.RingsBanked_Session, RingStorage.RunBankedRings - m_previousRingsBanked);
		s_currentStats.m_trackedStats[16] = RingStorage.RunBankedRings;
		s_currentStats.m_trackedStats[65] = (int)(m_runTime * 10f);
		if (s_currentStats.m_trackedDistances[7] > s_currentStats.m_trackedDistances[4])
		{
			s_currentStats.m_trackedDistances[4] = s_currentStats.m_trackedDistances[7];
		}
		if (s_currentStats.m_trackedLongStats[0] > s_currentStats.m_trackedLongStats[2])
		{
			string text = ScoreToTrackEvent(s_currentStats.m_trackedLongStats[2]);
			s_currentStats.m_trackedLongStats[2] = s_currentStats.m_trackedLongStats[0];
			string text2 = ScoreToTrackEvent(s_currentStats.m_trackedLongStats[2]);
			if (text2 != text)
			{
				SLAnalytics.LogTrackingEvent(text2, string.Empty);
			}
		}
		if (s_currentStats.m_trackedStats[16] > s_currentStats.m_trackedStats[56])
		{
			s_currentStats.m_trackedStats[56] = s_currentStats.m_trackedStats[16];
		}
		if (s_currentStats.m_trackedStats[19] > s_currentStats.m_trackedStats[57])
		{
			s_currentStats.m_trackedStats[57] = s_currentStats.m_trackedStats[19];
		}
		if (s_currentStats.m_trackedStats[4] > s_currentStats.m_trackedStats[58])
		{
			s_currentStats.m_trackedStats[58] = s_currentStats.m_trackedStats[4];
		}
		if (s_currentStats.m_trackedStats[10] > s_currentStats.m_trackedStats[59])
		{
			s_currentStats.m_trackedStats[59] = s_currentStats.m_trackedStats[10];
		}
	}

	private void Event_OnFacebookLogin()
	{
		if (s_currentStats.m_trackedStats[60] == 0)
		{
			if (SLSegaID.IsLoggedIn())
			{
			}
			s_currentStats.m_trackedStats[60] = 1;
			if (ABTesting.GetTestValue(ABTesting.Tests.RSR_Facebook) > 0)
			{
				m_displayFacebookReward = true;
			}
		}
	}

	private void Update()
	{
		if (m_displayFacebookReward)
		{
			StorePurchases.RequestReward("single_star_reward", 1, 5, StorePurchases.ShowDialog.Yes);
			m_displayFacebookReward = false;
		}
		m_sessionTime += IndependantTimeDelta.Delta;
		if (GameState.GetMode() == GameState.Mode.Menu)
		{
			return;
		}
		m_runTime += Time.deltaTime;
		if (Sonic.Tracker.InternalMotionState != null && Sonic.Tracker.InternalMotionState.HasActiveState && (Sonic.Tracker.InternalMotionState.CurrentState.GetType() == typeof(MotionDiveState) || Sonic.Tracker.InternalMotionState.CurrentState.GetType() == typeof(MotionFallState) || Sonic.Tracker.InternalMotionState.CurrentState.GetType() == typeof(MotionGlideState) || Sonic.Tracker.InternalMotionState.CurrentState.GetType() == typeof(MotionJumpState) || Sonic.Tracker.InternalMotionState.CurrentState.GetType() == typeof(MotionSpringState) || Sonic.Tracker.InternalMotionState.CurrentState.GetType() == typeof(MotionSpringAscentState) || Sonic.Tracker.InternalMotionState.CurrentState.GetType() == typeof(MotionSpringDescentState) || Sonic.Tracker.InternalMotionState.CurrentState.GetType() == typeof(MotionSpringGesturesState) || (Sonic.Tracker.InternalMotionState.CurrentState.GetType() == typeof(MotionAttackState) && Sonic.Tracker.JumpHeight > 0f)))
		{
			m_partialTime += Time.deltaTime;
			int num = (int)Mathf.Floor(m_partialTime * 10f);
			m_partialTime -= (float)num / 10f;
			IncreaseStat(StatNames.TimeAirbourne_Total, num);
			IncreaseStat(StatNames.TimeAirbourne_Run, num);
		}
		IncreaseDistance(DistanceNames.DistanceRun_Total, Sonic.Tracker.DistanceTravelled - m_previousDistance);
		IncreaseDistance(DistanceNames.DistanceRun_Session, Sonic.Tracker.DistanceTravelled - m_previousDistance);
		IncreaseDistance(DistanceNames.DistanceRun_Run, Sonic.Tracker.DistanceTravelled - m_previousDistance);
		if (DashMeterFilled)
		{
			IncreaseDistance(DistanceNames.DistanceDashMeterFilled, Sonic.Tracker.DistanceTravelled - m_previousDistance);
		}
		if (Sonic.Tracker.IsTrackerAvailable)
		{
			if (m_previousLane != Sonic.Tracker.getLane())
			{
				UpdateDistanceToCurrent(DistanceNames.DistanceChangedLane);
			}
			m_previousLane = Sonic.Tracker.getLane();
		}
		if (m_singleton.m_characterManager.GetCurrentCharacter() == Characters.Type.Knuckles)
		{
			IncreaseDistance(DistanceNames.DistanceAsKnuckles, Sonic.Tracker.DistanceTravelled - m_previousDistance);
		}
		m_previousDistance = Sonic.Tracker.DistanceTravelled;
		s_currentStats.m_trackedStats[17] = RingStorage.HeldRings;
		s_currentStats.m_trackedLongStats[0] = ScoreTracker.CurrentScore;
	}

	private void OnApplicationPause(bool paused)
	{
		if (!paused)
		{
		}
	}

	private void Event_OnSonicStumble()
	{
		IncreaseStat(StatNames.TimesDropRings_Total, 1);
		IncreaseStat(StatNames.RingsDropped_Total, RingStorage.HeldRings);
		s_currentStats.m_trackedLongStats[1] = ScoreTracker.CurrentScore;
	}

	private void Event_OnRingBankRequest()
	{
		UpdateDistanceToCurrent(DistanceNames.DistanceLastBanked);
		IncreaseStat(StatNames.RingsBanked_Total, RingStorage.RunBankedRings - m_previousRingsBanked);
		IncreaseStat(StatNames.RingsBanked_Session, RingStorage.RunBankedRings - m_previousRingsBanked);
		s_currentStats.m_trackedStats[16] = RingStorage.RunBankedRings;
		m_previousRingsBanked = RingStorage.RunBankedRings;
	}

	private void Event_OnRingStreakCompleted(int lenght, float firstRingTrackPosition, float lastRingTrackPosition)
	{
		IncreaseStat(StatNames.RingStreaks_Total, 1);
		IncreaseStat(StatNames.RingStreaks_Run, 1);
	}

	private void Event_OnRingsAwarded(int ringCount)
	{
		if (ringCount > 0)
		{
			IncreaseStat(StatNames.RingsPurchased_Total, ringCount);
		}
		else
		{
			IncreaseStat(StatNames.RingsSpent_Total, -ringCount);
		}
	}

	private void Event_OnEnemyKilled(Enemy enemy, Enemy.Kill killType)
	{
		switch (killType)
		{
		case Enemy.Kill.Homing:
			IncreaseStat(StatNames.EnemiesHoming_Total, 1);
			IncreaseStat(StatNames.Enemies_Total, 1);
			IncreaseStat(StatNames.Enemies_Session, 1);
			IncreaseStat(StatNames.Enemies_Run, 1);
			if (enemy.GetType() == typeof(Chopper))
			{
				IncreaseStat(StatNames.EnemiesAir_Total, 1);
			}
			break;
		case Enemy.Kill.Rolling:
			IncreaseStat(StatNames.EnemiesRolling_Total, 1);
			IncreaseStat(StatNames.Enemies_Total, 1);
			IncreaseStat(StatNames.Enemies_Session, 1);
			IncreaseStat(StatNames.Enemies_Run, 1);
			break;
		case Enemy.Kill.Diving:
			IncreaseStat(StatNames.EnemiesDiving_Total, 1);
			IncreaseStat(StatNames.Enemies_Total, 1);
			IncreaseStat(StatNames.Enemies_Session, 1);
			IncreaseStat(StatNames.Enemies_Run, 1);
			IncreaseStat(StatNames.EnemiesAir_Total, 1);
			break;
		}
	}

	private void Event_PowerUpLeveledUp(PowerUps.Type pUp)
	{
		if (PowerUpsInventory.GetPowerUpLevel(pUp) == 6)
		{
			IncreaseStat(StatNames.MaxedPowerUps_Total, 1);
		}
		if (PowerUpsInventory.GetPowerUpLevel(pUp) == 5)
		{
			IncreaseStat(StatNames.Level5PowerUps_Total, 1);
		}
	}

	private void Event_OnDashMeterFilled()
	{
		DashMeterFilled = true;
	}
}
