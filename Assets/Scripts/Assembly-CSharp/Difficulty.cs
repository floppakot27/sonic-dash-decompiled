using UnityEngine;

[AddComponentMenu("Dash/Difficulty")]
public class Difficulty : MonoBehaviour
{
	private const string m_propertyLastRunsDistances = "LastRunDistance_";

	private static Difficulty s_difficulty;

	private bool m_usingSoftCurve;

	private float[] m_lastRunsDistances;

	[SerializeField]
	private int m_minRunsForNormalDifficulty;

	[SerializeField]
	private int m_runsToTrackForMinDistance;

	[SerializeField]
	private float m_minDistanceForNormalDifficulty;

	[SerializeField]
	private AnimationCurve m_difficultyOverDistance;

	[SerializeField]
	private AnimationCurve m_softDifficultyOverDistance;

	public static bool IsAvaliable => s_difficulty != null && Sonic.Tracker != null;

	public static bool IsSonicDifficultyAvailable => IsAvaliable && (Sonic.Tracker.CurrentSpline != null || Sonic.Tracker.Track != null);

	public static Pair<float, float> GetMinMaxDifficulty()
	{
		AnimationCurve animationCurve = null;
		animationCurve = (s_difficulty.m_usingSoftCurve ? s_difficulty.m_softDifficultyOverDistance : s_difficulty.m_difficultyOverDistance);
		float value = animationCurve.keys[0].value;
		float value2 = animationCurve.keys[animationCurve.length - 1].value;
		return new Pair<float, float>(value, value2);
	}

	public static float GetDifficultyAtDistance(float distance)
	{
		if (!s_difficulty.m_usingSoftCurve)
		{
			return s_difficulty.m_difficultyOverDistance.Evaluate(distance);
		}
		return s_difficulty.m_softDifficultyOverDistance.Evaluate(distance);
	}

	public static float GetDifficultyAtSonicDistance()
	{
		return GetDifficultyAtDistance(s_difficulty.GetSonicDifficultyDistance());
	}

	public float GetSonicDifficultyDistance()
	{
		Spline currentSpline = Sonic.Tracker.CurrentSpline;
		if (currentSpline == null)
		{
			return (Sonic.Tracker.Track as TrackGenerator).DifficultyRelevantLengthAtTrackStart;
		}
		TrackSegment segmentOfSpline = TrackSegment.GetSegmentOfSpline(currentSpline);
		return segmentOfSpline.DifficultyRelevantTrackPosition;
	}

	private void Awake()
	{
		s_difficulty = this;
		EventDispatch.RegisterInterest("ResetGameState", this);
		EventDispatch.RegisterInterest("OnGameFinished", this, EventDispatch.Priority.Lowest);
		EventDispatch.RegisterInterest("OnGameDataLoaded", this, EventDispatch.Priority.Low);
		EventDispatch.RegisterInterest("OnGameDataSaveRequest", this);
		m_lastRunsDistances = new float[m_runsToTrackForMinDistance];
	}

	private void Event_ResetGameState(GameState.Mode mode)
	{
	}

	private void Event_OnGameFinished()
	{
		StoreRunDistance();
		SelectDifficultyCurve();
	}

	private void StoreRunDistance()
	{
		for (int num = m_runsToTrackForMinDistance - 2; num >= 0; num--)
		{
			m_lastRunsDistances[num + 1] = m_lastRunsDistances[num];
		}
		PlayerStats.Stats currentStats = PlayerStats.GetCurrentStats();
		m_lastRunsDistances[0] = currentStats.m_trackedDistances[7];
	}

	private void Event_OnGameDataSaveRequest()
	{
		for (int i = 0; i < m_runsToTrackForMinDistance; i++)
		{
			PropertyStore.Store("LastRunDistance_" + i, m_lastRunsDistances[i]);
		}
	}

	private void Event_OnGameDataLoaded(ActiveProperties activeProperties)
	{
		for (int i = 0; i < m_runsToTrackForMinDistance; i++)
		{
			m_lastRunsDistances[i] = activeProperties.GetFloat("LastRunDistance_" + i);
		}
		SelectDifficultyCurve();
	}

	private void SelectDifficultyCurve()
	{
		bool flag = false;
		bool flag2 = false;
		for (int i = 0; i < m_runsToTrackForMinDistance; i++)
		{
			if (m_lastRunsDistances[i] > m_minDistanceForNormalDifficulty)
			{
				flag = true;
			}
		}
		PlayerStats.Stats currentStats = PlayerStats.GetCurrentStats();
		if (currentStats.m_trackedStats[0] > m_minRunsForNormalDifficulty)
		{
			flag2 = true;
		}
		m_usingSoftCurve = ((!flag || !flag2) ? true : false);
	}
}
