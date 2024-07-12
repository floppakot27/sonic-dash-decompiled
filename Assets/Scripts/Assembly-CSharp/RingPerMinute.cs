using UnityEngine;

public class RingPerMinute : MonoBehaviour
{
	private float m_currentRunningTime;

	public static float Current { get; private set; }

	private void Start()
	{
		EventDispatch.RegisterInterest("OnNewGameStarted", this);
	}

	private void Update()
	{
		if (GameState.GetMode() == GameState.Mode.Game)
		{
			m_currentRunningTime += Time.deltaTime;
			float num = (float)RingStorage.RunBankedRings + (float)RingStorage.HeldRings;
			Current = num / (m_currentRunningTime / 60f);
		}
	}

	private void Event_OnNewGameStarted()
	{
		m_currentRunningTime = 0f;
	}
}
