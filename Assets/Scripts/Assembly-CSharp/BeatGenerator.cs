using UnityEngine;

public class BeatGenerator : MonoBehaviour
{
	private static BeatGenerator m_instance;

	public float m_beatInterval = 2f;

	public float m_preBeatDuration = 0.5f;

	private float m_timer;

	private bool m_beatThisFrame;

	private bool m_preBeatThisFrame;

	private void Awake()
	{
		m_instance = this;
		reset();
	}

	private void Start()
	{
		EventDispatch.RegisterInterest("OnNewGameStarted", this);
		reset();
	}

	private void Event_OnNewGameStarted()
	{
		reset();
	}

	private void reset()
	{
		m_beatThisFrame = false;
		m_preBeatThisFrame = false;
		m_timer = 0f;
	}

	private void Update()
	{
		float timer = m_timer;
		m_timer += Time.deltaTime;
		m_preBeatThisFrame = false;
		if (timer < m_beatInterval - m_preBeatDuration && m_timer >= m_beatInterval - m_preBeatDuration)
		{
			m_preBeatThisFrame = true;
		}
		if (m_timer >= m_beatInterval)
		{
			m_timer = 0f;
			m_beatThisFrame = true;
		}
		else
		{
			m_beatThisFrame = false;
		}
	}

	public bool beatThisFrame()
	{
		return m_beatThisFrame;
	}

	public bool prebeatThisFrame()
	{
		return m_preBeatThisFrame;
	}

	public static BeatGenerator instance()
	{
		return m_instance;
	}
}
