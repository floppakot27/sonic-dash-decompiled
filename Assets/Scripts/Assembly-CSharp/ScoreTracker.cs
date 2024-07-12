using System;
using System.Collections;
using UnityEngine;

public class ScoreTracker : MonoBehaviour
{
	public enum ScoreNotify
	{
		None,
		Normal,
		EnemyComboBonus,
		RingStreakBonus,
		SpringBonus,
		GoldenEnemyBonus
	}

	[Flags]
	public enum Event
	{
		None = 0,
		Grass = 1,
		Temple = 2,
		Beach = 4,
		All = 7
	}

	[Flags]
	private enum ScoreCategory
	{
		None = 0,
		Enemy = 1,
		Combo = 2,
		Distance = 4,
		Streak = 8,
		All = 0xF
	}

	[Flags]
	private enum State
	{
		None = 0,
		HighScore = 1,
		AutoScore = 2
	}

	private const string EventBonusRoot = "events";

	private const string GrassEventProperty = "grassevent";

	private const string TempleEventProperty = "templeevent";

	private const string BeachEventProperty = "beachevent";

	public static string[] ScoreNotifyIcons = new string[6]
	{
		string.Empty,
		string.Empty,
		"icon-booster-enemycombo",
		"icon-booster-ringstreak",
		"icon-booster-springbonus",
		"icon-booster-goldnik"
	};

	private static ScoreTracker s_scoreTracker = null;

	private uint m_baseMultiplier = 1u;

	private uint m_runMultiplier = 1u;

	private uint m_dashMultiplier = 1u;

	private uint m_currentMultiplier = 1u;

	[SerializeField]
	private int m_grassBonusMultiplier = 1;

	[SerializeField]
	private int m_templeBonusMultiplier = 1;

	[SerializeField]
	private int m_beachBonusMultiplier = 1;

	private int m_eventBonusMultiplier = 1;

	private Event m_eventBonusFlag;

	private long m_highestScore;

	private long m_currentScore;

	private long m_boosterBonusScore;

	private long m_previousHighScore;

	private State m_state;

	private float m_previousDistance;

	private float m_accumulatedDistance;

	private float m_accumulatedDistanceScore;

	private float m_currentAutoScoreCount;

	private object[] m_leaderboardParams = new object[2];

	private object[] m_eventParams = new object[2];

	private float m_lastCompletedStreakEnd;

	[SerializeField]
	private int m_pointsPerRollingKill = 100;

	[SerializeField]
	private int m_pointsPerHomingKill = 100;

	[SerializeField]
	private int m_pointsPerDivingKill = 250;

	[SerializeField]
	private int m_maxKillCombo = 5;

	[SerializeField]
	private float m_pointsPerMeter = 1f;

	[SerializeField]
	private ScoreCategory m_multiplierEffects = ScoreCategory.All;

	[SerializeField]
	private float m_automaticPointsPerSecond = 20f;

	private uint s_boosterSpringTimes;

	private uint s_boosterSpringScore;

	private uint s_boosterRingsTimes;

	private uint s_boosterRingsScore;

	private uint s_boosterComboTimes;

	private uint s_boosterComboScore;

	private uint s_boosterGoldenTimes;

	private uint s_boosterGoldenScore;

	public static bool HighScoreAchived => (s_scoreTracker.m_state & State.HighScore) == State.HighScore;

	public static long CurrentScore => s_scoreTracker.m_currentScore;

	public static long CurrentBoosterBonusScore => s_scoreTracker.m_boosterBonusScore;

	public static long HighScore => s_scoreTracker.m_highestScore;

	public static long PreviousHighScore => s_scoreTracker.m_previousHighScore;

	public static uint DashMultiplier => s_scoreTracker.m_dashMultiplier;

	public static uint BaseMultiplier => s_scoreTracker.m_baseMultiplier;

	public static uint RunMultiplier => s_scoreTracker.m_runMultiplier;

	public static uint CurrentMultiplier => s_scoreTracker.m_currentMultiplier;

	public static uint NextMultiplier => s_scoreTracker.m_baseMultiplier + 1;

	public static int EventBonusMultiplier => s_scoreTracker.m_eventBonusMultiplier;

	public static Event EventBonusFlag
	{
		get
		{
			return s_scoreTracker.m_eventBonusFlag;
		}
		set
		{
			s_scoreTracker.m_eventBonusFlag = value;
		}
	}

	public static string HighestScoreSavedProperty => "Highest Score";

	private static string MultiplierSavedProperty => "Multiplier";

	private void Start()
	{
		s_scoreTracker = this;
		EventDispatch.RegisterInterest("OnNewGameAboutToStart", this);
		EventDispatch.RegisterInterest("OnNewGameStarted", this);
		EventDispatch.RegisterInterest("OnGameFinished", this);
		EventDispatch.RegisterInterest("OnGameDataSaveRequest", this);
		EventDispatch.RegisterInterest("OnGameDataLoaded", this);
		EventDispatch.RegisterInterest("OnBossHit", this);
		EventDispatch.RegisterInterest("OnBossBeat", this);
		EventDispatch.RegisterInterest("OnEnemyKilled", this);
		EventDispatch.RegisterInterest("OnDashMeterFilled", this);
		EventDispatch.RegisterInterest("OnDashMeterTriggered", this);
		EventDispatch.RegisterInterest("OnSpringStart", this);
		EventDispatch.RegisterInterest("OnSpringEnd", this);
		EventDispatch.RegisterInterest("RequestNewTrackMidGame", this);
		EventDispatch.RegisterInterest("OnMissionSetChanged", this);
		EventDispatch.RegisterInterest("OnRingStreakCompleted", this);
		EventDispatch.RegisterInterest("OnRingStreakMissed", this);
	}

	private void Update()
	{
		if (!(null == Sonic.Tracker))
		{
			if ((m_state & State.AutoScore) == State.AutoScore)
			{
				UpdateAutomaticScore();
			}
			else
			{
				UpdateDistanceScore();
			}
		}
	}

	private void AddToScore(int additionalScore, ScoreNotify notify)
	{
		m_currentScore += additionalScore;
		if (m_currentScore > m_highestScore && m_highestScore != 0L)
		{
			m_state |= State.HighScore;
		}
		if (notify != 0)
		{
			m_eventParams[0] = additionalScore;
			m_eventParams[1] = notify;
			EventDispatch.GenerateEvent("OnScoreIncreased", m_eventParams);
		}
		switch (notify)
		{
		case ScoreNotify.SpringBonus:
			s_boosterSpringTimes++;
			s_boosterSpringScore += (uint)additionalScore;
			break;
		case ScoreNotify.RingStreakBonus:
			s_boosterRingsTimes++;
			s_boosterRingsScore += (uint)additionalScore;
			break;
		case ScoreNotify.EnemyComboBonus:
			s_boosterComboTimes++;
			s_boosterComboScore += (uint)additionalScore;
			break;
		case ScoreNotify.GoldenEnemyBonus:
			s_boosterGoldenTimes++;
			s_boosterGoldenScore += (uint)additionalScore;
			break;
		}
	}

	private uint GetMultiplier(ScoreCategory scoreComponent)
	{
		if ((m_multiplierEffects & scoreComponent) == scoreComponent)
		{
			return m_currentMultiplier;
		}
		return 1u;
	}

	private void UpdateAutomaticScore()
	{
		m_currentAutoScoreCount += m_automaticPointsPerSecond * Time.deltaTime;
		if (m_currentAutoScoreCount >= 1f)
		{
			int num = Mathf.FloorToInt(m_currentAutoScoreCount);
			m_currentAutoScoreCount -= num;
			num *= (int)GetMultiplier(ScoreCategory.Distance);
			AddToScore(num, ScoreNotify.None);
		}
	}

	private void UpdateDistanceScore()
	{
		float distanceTravelled = Sonic.Tracker.DistanceTravelled;
		m_accumulatedDistance += distanceTravelled - m_previousDistance;
		m_previousDistance = distanceTravelled;
		if (m_accumulatedDistance > 1f)
		{
			float num = Mathf.Floor(m_accumulatedDistance);
			m_accumulatedDistance -= num;
			m_accumulatedDistanceScore += num * m_pointsPerMeter * (float)GetMultiplier(ScoreCategory.Distance);
			if (m_accumulatedDistanceScore > 1f)
			{
				int num2 = Mathf.FloorToInt(m_accumulatedDistanceScore);
				m_accumulatedDistanceScore -= num2;
				AddToScore(num2, ScoreNotify.None);
			}
		}
	}

	private void Event_OnNewGameAboutToStart()
	{
		m_currentScore = 0L;
		m_boosterBonusScore = 0L;
	}

	private void Event_OnNewGameStarted()
	{
		m_currentScore = 0L;
		m_boosterBonusScore = 0L;
		m_state = State.None;
		m_previousDistance = 0f;
		m_accumulatedDistance = 0f;
		m_accumulatedDistanceScore = 0f;
		m_currentAutoScoreCount = 0f;
		m_dashMultiplier = 1u;
		m_runMultiplier = m_baseMultiplier;
		m_currentMultiplier = 1u;
		m_state &= ~State.AutoScore;
		m_eventBonusFlag &= ~Event.All;
		UpdateEventBonus(SpringTV.Destination.Grass);
		UpdateMultiplier();
		ResetBoosterDebugScores();
	}

	private void UpdateMultiplier()
	{
		m_currentMultiplier = DashMultiplier;
		uint num = BaseMultiplier;
		if (CheckEvents())
		{
			num += (uint)m_eventBonusMultiplier;
			EventDispatch.GenerateEvent("MultiplierUpdate");
		}
		m_currentMultiplier *= num;
		PlayerStats.UpdateMultiplier(m_currentMultiplier);
	}

	private bool CheckEvents()
	{
		bool result = false;
		m_eventBonusMultiplier = 0;
		if (Internet.ConnectionAvailable() && FeatureState.Valid)
		{
			if (CheckEventState(Event.Grass, "grassevent", m_grassBonusMultiplier))
			{
				result = true;
			}
			if (CheckEventState(Event.Temple, "templeevent", m_templeBonusMultiplier))
			{
				result = true;
			}
			if (CheckEventState(Event.Beach, "beachevent", m_beachBonusMultiplier))
			{
				result = true;
			}
			if (m_eventBonusMultiplier == 0)
			{
				result = false;
			}
			return result;
		}
		return false;
	}

	private bool CheckEventState(Event eventBonusActive, string eventProperty, int bonusMultiplier)
	{
		LSON.Property stateProperty = FeatureState.GetStateProperty("events", eventProperty);
		if (stateProperty != null)
		{
			bool boolValue = false;
			if (LSONProperties.AsBool(stateProperty, out boolValue) && boolValue && (m_eventBonusFlag & eventBonusActive) == eventBonusActive)
			{
				stateProperty = FeatureState.GetStateProperty("events", eventProperty + "bonus");
				int intValue = 0;
				if (stateProperty != null && LSONProperties.AsInt(stateProperty, out intValue))
				{
					m_eventBonusMultiplier += intValue;
				}
				else
				{
					m_eventBonusMultiplier += bonusMultiplier;
				}
				return true;
			}
		}
		return false;
	}

	private void Event_OnGameFinished()
	{
		if (Boosters.IsBoosterSelected(PowerUps.Type.Booster_ScoreMultiplier))
		{
			m_boosterBonusScore = (int)((float)m_currentScore * Boosters.ScoreMultiplier - (float)m_currentScore);
			m_currentScore += m_boosterBonusScore;
		}
		if (m_currentScore > m_highestScore)
		{
			m_previousHighScore = m_highestScore;
			m_highestScore = m_currentScore;
		}
		m_leaderboardParams[0] = Leaderboards.Types.sdHighestScore;
		m_leaderboardParams[1] = m_currentScore;
		EventDispatch.GenerateEvent("PostLeaderboardScore", m_leaderboardParams);
	}

	private void Event_OnGameDataSaveRequest()
	{
		PropertyStore.Store(HighestScoreSavedProperty, m_highestScore);
		PropertyStore.Store(MultiplierSavedProperty, m_baseMultiplier);
	}

	private void Event_OnGameDataLoaded(ActiveProperties activeProperties)
	{
		m_highestScore = activeProperties.GetLong(HighestScoreSavedProperty);
		m_baseMultiplier = (uint)activeProperties.GetInt(MultiplierSavedProperty);
		if (m_baseMultiplier == 0)
		{
			m_baseMultiplier = 1u;
		}
	}

	private void Event_OnBossHit(int initialscore)
	{
		uint num = ((ComboTracker.Current <= (uint)m_maxKillCombo) ? ComboTracker.Current : ((uint)m_maxKillCombo));
		initialscore *= (int)num;
		initialscore *= (int)GetMultiplier(ScoreCategory.Enemy);
		AddToScore(initialscore, ScoreNotify.Normal);
		EventDispatch.GenerateEvent("OnBossHitScoreAwarded", initialscore);
		if (num >= 3 && Boosters.IsBoosterSelected(PowerUps.Type.Booster_EnemyComboBonus))
		{
			int num2 = (int)((float)initialscore * Boosters.EnemyComboScoreMultiplier);
			EventDispatch.GenerateEvent("OnBoosterActivated", PowerUps.Type.Booster_EnemyComboBonus);
			AddToScore(num2, ScoreNotify.EnemyComboBonus);
			EventDispatch.GenerateEvent("OnBossHitScoreAwarded", num2);
		}
	}

	private void Event_OnBossBeat(int score)
	{
		AddToScore(score, ScoreNotify.None);
	}

	private void Event_OnEnemyKilled(Enemy enemy, Enemy.Kill killType)
	{
		if (killType != Enemy.Kill.Other)
		{
			int num = m_pointsPerRollingKill;
			switch (killType)
			{
			case Enemy.Kill.Homing:
				num = m_pointsPerHomingKill;
				break;
			case Enemy.Kill.Diving:
				num = m_pointsPerDivingKill;
				break;
			}
			ScoreNotify notify = ScoreNotify.Normal;
			uint num2 = 0u;
			int num3 = num;
			if (killType != Enemy.Kill.Diving)
			{
				num2 = ((ComboTracker.Current <= (uint)m_maxKillCombo) ? ComboTracker.Current : ((uint)m_maxKillCombo));
				num *= (int)num2;
			}
			num *= (int)GetMultiplier(ScoreCategory.Enemy);
			AddToScore(num, notify);
			if (num2 >= 3 && Boosters.IsBoosterSelected(PowerUps.Type.Booster_EnemyComboBonus))
			{
				int additionalScore = (int)((float)num * Boosters.EnemyComboScoreMultiplier);
				notify = ScoreNotify.EnemyComboBonus;
				EventDispatch.GenerateEvent("OnBoosterActivated", PowerUps.Type.Booster_EnemyComboBonus);
				AddToScore(additionalScore, notify);
			}
			if (enemy.Golden)
			{
				int additionalScore2 = (int)((float)(num3 * GetMultiplier(ScoreCategory.Enemy)) * Boosters.GoldenEnemyScoreMultipler);
				notify = ScoreNotify.GoldenEnemyBonus;
				EventDispatch.GenerateEvent("OnBoosterActivated", PowerUps.Type.Booster_GoldenEnemy);
				AddToScore(additionalScore2, notify);
				ParticlePlayer.Play(Sonic.ParticleController.m_goldenEnemiKillParticles);
			}
		}
	}

	private void Event_OnRingStreakCompleted(int length, float firstRingTrackPosition, float lastRingTrackPosition)
	{
		if (Boosters.IsBoosterSelected(PowerUps.Type.Booster_RingStreakBonus))
		{
			m_lastCompletedStreakEnd = lastRingTrackPosition;
			if (!DashMonitor.instance().isDashing())
			{
				int additionalScore = (int)(Boosters.RingStreakBonusScore * GetMultiplier(ScoreCategory.Streak));
				AddToScore(additionalScore, ScoreNotify.RingStreakBonus);
				EventDispatch.GenerateEvent("OnBoosterActivated", PowerUps.Type.Booster_RingStreakBonus);
			}
		}
	}

	private void Event_OnRingStreakMissed(int length, float firstRingTrackPosition, float lastRingTrackPosition)
	{
		if (!DashMonitor.instance().isDashing())
		{
			StartCoroutine(DoRingStreakMissed(length, firstRingTrackPosition, lastRingTrackPosition));
		}
	}

	private IEnumerator DoRingStreakMissed(int length, float firstRingTrackPosition, float lastRingTrackPosition)
	{
		yield return null;
		if (!(m_lastCompletedStreakEnd > firstRingTrackPosition))
		{
		}
	}

	private void Event_OnDashMeterFilled()
	{
		m_dashMultiplier = 2u;
		UpdateMultiplier();
	}

	private void Event_OnDashMeterTriggered(bool enabled)
	{
		m_dashMultiplier = 1u;
		UpdateMultiplier();
	}

	private void Event_OnSpringStart(SpringTV.Type springType)
	{
		m_state |= State.AutoScore;
		PlayerStats.IncreaseStat(PlayerStats.StatNames.SpringsPassed_Run, 1);
		if (Boosters.IsBoosterSelected(PowerUps.Type.Booster_SpringBonus))
		{
			int additionalScore = (int)(PlayerStats.GetCurrentStats().m_trackedStats[90] * Boosters.SpringBonusScore * GetMultiplier(ScoreCategory.Distance));
			AddToScore(additionalScore, ScoreNotify.SpringBonus);
			EventDispatch.GenerateEvent("OnBoosterActivated", PowerUps.Type.Booster_SpringBonus);
		}
	}

	private void Event_OnSpringEnd()
	{
		m_state &= ~State.AutoScore;
	}

	private void Event_OnMissionSetChanged()
	{
		m_baseMultiplier++;
	}

	private void Event_RequestNewTrackMidGame(TrackGenerator.MidGameNewTrackRequest request)
	{
		UpdateEventBonus(request.Destination);
		UpdateMultiplier();
	}

	private void UpdateEventBonus(SpringTV.Destination subzone)
	{
		switch (subzone)
		{
		case SpringTV.Destination.Grass:
			m_eventBonusFlag |= Event.Grass;
			m_eventBonusFlag &= ~Event.Temple;
			m_eventBonusFlag &= ~Event.Beach;
			break;
		case SpringTV.Destination.Temple:
			m_eventBonusFlag &= ~Event.Grass;
			m_eventBonusFlag |= Event.Temple;
			m_eventBonusFlag &= ~Event.Beach;
			break;
		case SpringTV.Destination.Beach:
			m_eventBonusFlag &= ~Event.Grass;
			m_eventBonusFlag &= ~Event.Temple;
			m_eventBonusFlag |= Event.Beach;
			break;
		}
	}

	private void ResetBoosterDebugScores()
	{
		s_boosterSpringTimes = 0u;
		s_boosterSpringScore = 0u;
		s_boosterRingsTimes = 0u;
		s_boosterRingsScore = 0u;
		s_boosterComboTimes = 0u;
		s_boosterComboScore = 0u;
		s_boosterGoldenTimes = 0u;
		s_boosterGoldenScore = 0u;
	}
}
