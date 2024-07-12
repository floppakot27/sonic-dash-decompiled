using System.Collections;
using UnityEngine;

public class GameMusic : MonoBehaviour
{
	private enum State
	{
		First,
		MainMenu,
		Game,
		Paused,
		Spring,
		ReviveDialog,
		Death,
		Results
	}

	public AudioClip m_menuAudioClip;

	public AudioClip m_gameAudioClip;

	public AudioClip m_bossAudioClip;

	private float m_fullVolume = 1f;

	public float m_pausedVolume = 0.5f;

	public float m_springVolume = 0.5f;

	public float m_resultsVolume = 0.5f;

	private AudioSource m_audioSource;

	private Music m_music;

	private State m_state;

	private bool m_bossBattleMusicStarted;

	private static GameMusic s_singleton;

	public static GameMusic Singleton => s_singleton;

	private void Start()
	{
		s_singleton = this;
		m_music = Music.Singleton;
		m_state = State.First;
		m_audioSource = (AudioSource)GetComponent(typeof(AudioSource));
		m_fullVolume = m_audioSource.volume;
		EventDispatch.RegisterInterest("StartGameState", this);
		EventDispatch.RegisterInterest("OnGameFinished", this);
		EventDispatch.RegisterInterest("OnSpringStart", this);
		EventDispatch.RegisterInterest("OnSpringEnd", this);
		EventDispatch.RegisterInterest("OnSonicResurrection", this);
		EventDispatch.RegisterInterest("BossMusicStart", this);
		EventDispatch.RegisterInterest("BossMusicEnd", this);
		EventDispatch.RegisterInterest("GameMusicStart", this);
	}

	private void Update()
	{
		if (m_state == State.Death && !m_audioSource.isPlaying)
		{
			m_music.Play(m_gameAudioClip, loops: true, m_resultsVolume);
			m_state = State.Results;
		}
	}

	private void Event_StartGameState(GameState.Mode state)
	{
		switch (state)
		{
		case GameState.Mode.Menu:
			m_music.Play(m_menuAudioClip, loops: true, m_fullVolume);
			m_state = State.MainMenu;
			break;
		case GameState.Mode.Game:
			if (m_bossBattleMusicStarted)
			{
				m_music.Play(m_bossAudioClip, loops: true, m_fullVolume);
			}
			else
			{
				m_music.Play(m_gameAudioClip, loops: true, m_fullVolume);
			}
			m_state = State.Game;
			break;
		case GameState.Mode.PauseMenu:
			m_music.Fade(m_pausedVolume);
			m_state = State.Paused;
			break;
		}
	}

	private void Event_OnGameFinished()
	{
		m_music.Play(m_menuAudioClip, loops: true, m_fullVolume);
		m_state = State.Results;
	}

	public void OnNewGameStarted()
	{
		m_music.Play(m_gameAudioClip, loops: true, m_fullVolume, force: true);
		m_bossBattleMusicStarted = false;
		m_state = State.Game;
	}

	private void Event_OnSpringStart(SpringTV.Type springType)
	{
		if (!m_bossBattleMusicStarted)
		{
			m_music.Fade(m_springVolume);
		}
		m_state = State.Spring;
	}

	private void Event_OnSpringEnd()
	{
		m_music.Fade(m_fullVolume);
		m_state = State.Game;
	}

	public void OnShowReviveDialog()
	{
		m_music.Fade(m_pausedVolume);
		m_state = State.ReviveDialog;
	}

	private void Event_OnSonicResurrection()
	{
		m_music.Fade(m_fullVolume);
		m_state = State.Game;
	}

	private void Event_BossMusicStart()
	{
		if (!m_bossBattleMusicStarted)
		{
			m_bossBattleMusicStarted = true;
			m_music.Play(m_bossAudioClip, loops: true, m_fullVolume, force: true);
		}
	}

	private void Event_BossMusicEnd(float fadeTime)
	{
		if (m_bossBattleMusicStarted)
		{
			StartCoroutine(BossMusicFadeDown(fadeTime));
		}
	}

	private void Event_GameMusicStart(float fadeTime)
	{
		if (m_bossBattleMusicStarted)
		{
			m_bossBattleMusicStarted = false;
			StartCoroutine(GameMusicFadeUp(fadeTime));
		}
	}

	private IEnumerator BossMusicFadeDown(float fadeTime)
	{
		float timer = fadeTime;
		while (timer > 0f)
		{
			m_music.Fade(m_fullVolume * timer / fadeTime);
			timer -= Time.deltaTime;
			yield return new WaitForEndOfFrame();
		}
	}

	private IEnumerator GameMusicFadeUp(float fadeTime)
	{
		float targetVolume = m_springVolume;
		m_music.Play(m_gameAudioClip, loops: true, 0f, force: true);
		float timer = 0f;
		while (timer < fadeTime)
		{
			timer += Time.deltaTime;
			m_music.Fade(targetVolume * timer / fadeTime);
			yield return null;
		}
		m_music.Fade(targetVolume);
	}
}
