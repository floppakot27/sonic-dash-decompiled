using UnityEngine;

public class Music : MonoBehaviour
{
	private enum State
	{
		Stopped,
		Playing,
		FadeOff
	}

	public float m_volumeFadeTime = 1f;

	private State m_CurrentState;

	private AudioClip m_currentClip;

	private State m_NextState;

	private AudioClip m_nextClip;

	private bool m_nextClipLoops;

	private float m_volume;

	private float m_targetVolume;

	private float m_volumeFadeIncre;

	private AudioSource m_audioSource;

	private static Music s_singleton;

	public static Music Singleton => s_singleton;

	private void Awake()
	{
		s_singleton = this;
		m_CurrentState = State.Stopped;
		m_NextState = m_CurrentState;
		m_currentClip = null;
		m_nextClip = null;
		m_nextClipLoops = true;
		m_audioSource = (AudioSource)GetComponent(typeof(AudioSource));
		m_volumeFadeIncre = Time.fixedDeltaTime / m_volumeFadeTime;
		m_volume = 0f;
	}

	private void Update()
	{
		if (m_NextState != m_CurrentState || ((bool)m_nextClip && m_nextClip != m_currentClip))
		{
			switch (m_NextState)
			{
			case State.Stopped:
				if (m_audioSource.isPlaying)
				{
					m_audioSource.Stop();
					m_currentClip = null;
					m_nextClip = null;
				}
				break;
			case State.Playing:
				if (m_currentClip != m_nextClip)
				{
					if (m_audioSource.isPlaying)
					{
						m_audioSource.Stop();
					}
					m_audioSource.loop = m_nextClipLoops;
					m_currentClip = m_nextClip;
					m_audioSource.clip = m_currentClip;
					m_volume = m_targetVolume;
					m_audioSource.Play();
				}
				break;
			}
		}
		m_CurrentState = m_NextState;
		switch (m_CurrentState)
		{
		case State.Playing:
			if (m_volume < m_targetVolume)
			{
				m_volume += m_volumeFadeIncre;
				if (m_volume > m_targetVolume)
				{
					m_volume = m_targetVolume;
				}
			}
			else if (m_volume > m_targetVolume)
			{
				m_volume -= m_volumeFadeIncre;
				if (m_volume < m_targetVolume)
				{
					m_volume = m_targetVolume;
				}
			}
			break;
		case State.FadeOff:
			m_volume -= m_volumeFadeIncre;
			if (!m_audioSource.isPlaying || m_volume <= 0f)
			{
				m_volume = 0f;
				if (m_nextClip != m_currentClip)
				{
					m_NextState = State.Playing;
				}
				else
				{
					m_NextState = State.Stopped;
				}
			}
			break;
		}
		if (Audio.AudioEnabled && Audio.MusicEnabled && !IsDeviceMusicPlaying())
		{
			m_audioSource.volume = m_volume;
		}
		else
		{
			m_audioSource.volume = 0f;
		}
	}

	private void OnDestroy()
	{
		if (m_audioSource != null && m_audioSource.isPlaying)
		{
			m_audioSource.Stop();
		}
	}

	private bool IsDeviceMusicPlaying()
	{
		return false;
	}

	public void Play(AudioClip audioClip, bool loops, float volume)
	{
		Play(audioClip, loops, volume, force: false);
	}

	public void Play(AudioClip audioClip, bool loops, float volume, bool force)
	{
		if (force)
		{
		}
		if (m_currentClip != audioClip && !force)
		{
			m_NextState = State.FadeOff;
		}
		else
		{
			m_NextState = State.Playing;
		}
		m_nextClip = audioClip;
		m_nextClipLoops = loops;
		m_targetVolume = volume;
	}

	public void Fade(float volume)
	{
		m_targetVolume = volume;
	}
}
