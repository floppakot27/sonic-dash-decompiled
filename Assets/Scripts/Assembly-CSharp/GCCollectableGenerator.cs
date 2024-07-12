using UnityEngine;

public class GCCollectableGenerator : MonoBehaviour
{
	private static GCCollectableGenerator s_singleton;

	[SerializeField]
	private float m_minDistance;

	[SerializeField]
	private float m_maxDistance;

	[SerializeField]
	private int m_numCollectables;

	private float m_nextDistanceToSpawn;

	private int m_spawnedThisTrack;

	public static int NumCollectables => s_singleton.m_numCollectables;

	public static bool CanSpawnGCCollectable(float distance)
	{
		if (TutorialSystem.instance().isTrackTutorialEnabled())
		{
			return false;
		}
		if (false || !GCState.IsCurrentChallengeActive() || GC3Progress.ChallengeFullycompleted())
		{
			return false;
		}
		int num = GC3Progress.CurrentCollectedThisRun + s_singleton.m_spawnedThisTrack;
		if (num >= GC3Progress.GetLocalTierSize - 1)
		{
			return false;
		}
		return distance >= s_singleton.m_nextDistanceToSpawn;
	}

	public static void GCCollectableSpawned(float distance)
	{
		s_singleton.m_nextDistanceToSpawn = distance + Random.Range(s_singleton.m_minDistance, s_singleton.m_maxDistance);
		s_singleton.m_spawnedThisTrack++;
	}

	private void Start()
	{
		s_singleton = this;
		EventDispatch.RegisterInterest("ResetTrackState", this);
		EventDispatch.RegisterInterest("OnSpringStart", this);
		m_nextDistanceToSpawn = Random.Range(m_minDistance, m_maxDistance);
	}

	private void Event_OnSpringStart(SpringTV.Type springType)
	{
		m_spawnedThisTrack = 0;
	}

	private void Event_ResetTrackState()
	{
		m_nextDistanceToSpawn = Random.Range(m_minDistance, m_maxDistance);
		m_spawnedThisTrack = 0;
	}
}
