using UnityEngine;

public class IndependantTimeDelta : MonoBehaviour
{
	private static float s_timeDelta;

	private static bool s_singleInstance;

	private float m_previousTime;

	private float m_maximumTimeStep = 0.1f;

	public static float Delta => s_timeDelta;

	public void Start()
	{
		s_singleInstance = true;
		s_timeDelta = 0f;
		m_previousTime = Time.realtimeSinceStartup;
	}

	public void Update()
	{
		float realtimeSinceStartup = Time.realtimeSinceStartup;
		s_timeDelta = realtimeSinceStartup - m_previousTime;
		s_timeDelta = Mathf.Clamp(s_timeDelta, 0f, m_maximumTimeStep);
		m_previousTime = realtimeSinceStartup;
	}

	private void OnDestroy()
	{
		s_singleInstance = false;
	}
}
