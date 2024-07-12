using UnityEngine;

public class SLCachedQueries
{
	private struct CachedQuery
	{
		private float m_lastQueryTime;

		private bool m_currentAnswer;

		private Query m_query;

		private float QueryAge => Time.realtimeSinceStartup - m_lastQueryTime;

		public CachedQuery(Query query)
		{
			m_lastQueryTime = Time.realtimeSinceStartup - Random.value * s_validQueryDuration;
			m_query = query;
			m_currentAnswer = true;
		}

		public bool Get()
		{
			if (QueryAge > s_validQueryDuration)
			{
				bool newAnswer = m_query();
				CacheAnswer(newAnswer);
			}
			return m_currentAnswer;
		}

		private void CacheAnswer(bool newAnswer)
		{
			m_lastQueryTime = Time.realtimeSinceStartup;
			m_currentAnswer = newAnswer;
		}
	}

	private delegate bool Query();

	private static SLCachedQueries s_instance = new SLCachedQueries();

	private static readonly float s_validQueryDuration = 1.5f;

	private CachedQuery m_isMoreGamesAvailable = new CachedQuery(SLAds.IsMoreGamesAvailable);

	private CachedQuery m_isSocialAvailable = new CachedQuery(SLSocial.IsAvailable);

	private CachedQuery m_isGameOffersAvailable = new CachedQuery(SLAds.IsGameOffersAvailable);

	private CachedQuery m_isVideoAvailable = new CachedQuery(SLAds.IsVideoAvailable);

	public static bool IsMoreGamesAvailable()
	{
		return s_instance.m_isMoreGamesAvailable.Get();
	}

	public static bool IsAvailable()
	{
		return s_instance.m_isSocialAvailable.Get();
	}

	public static bool IsGameOffersAvailable()
	{
		return s_instance.m_isGameOffersAvailable.Get();
	}

	public static bool IsVideoAvailable()
	{
		return s_instance.m_isVideoAvailable.Get();
	}
}
