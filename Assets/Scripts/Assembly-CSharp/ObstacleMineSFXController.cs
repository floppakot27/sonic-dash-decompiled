using UnityEngine;

internal class ObstacleMineSFXController : MonoBehaviour
{
	private float m_beepTimer;

	[SerializeField]
	private float m_beepTimeInterval = 1f;

	[SerializeField]
	private AudioClip m_mineBeepAudioClip;

	[SerializeField]
	private AudioClip m_bombAudioClip;

	private Transform m_beepAudioTrasform;

	private GameState.Mode m_currentMode;

	private void Awake()
	{
		m_beepTimer = m_beepTimeInterval;
		m_beepAudioTrasform = null;
		m_currentMode = GameState.Mode.Menu;
		EventDispatch.RegisterInterest("ResetGameState", this);
	}

	private void Update()
	{
		if (m_currentMode == GameState.Mode.Game)
		{
			m_beepTimer += Time.deltaTime;
			if (m_beepTimer >= m_beepTimeInterval)
			{
				m_beepTimer = 0f;
				m_beepAudioTrasform = Audio.PlayClip(m_mineBeepAudioClip, loop: false);
			}
		}
		if (m_beepAudioTrasform != null)
		{
			m_beepAudioTrasform.position = base.transform.position;
		}
	}

	private void Event_ResetGameState(GameState.Mode mode)
	{
		m_currentMode = mode;
	}

	public void OnDeath(object[] onDeathParams)
	{
		if (m_bombAudioClip != null)
		{
			Audio.PlayClip(m_bombAudioClip, loop: false);
		}
		m_beepAudioTrasform = null;
	}

	public void OnSonicKill()
	{
		if (m_bombAudioClip != null)
		{
			Audio.PlayClip(m_bombAudioClip, loop: false);
		}
		m_beepAudioTrasform = null;
	}

	public void OnStumble(SonicSplineTracker killer)
	{
		if (m_bombAudioClip != null)
		{
			Audio.PlayClip(m_bombAudioClip, loop: false);
		}
		m_beepAudioTrasform = null;
	}
}
