using System.Linq;
using UnityEngine;

public class Audio : MonoBehaviour
{
	private static float m_musicVolume = 1f;

	private static float m_sfxVolume = 1f;

	public static float m_sfxVolumeMod = 0.4f;

	private static float m_globalVolume = 1f;

	private bool m_audioInitialised;

	private static GameObject m_AudioSourceObject;

	private static SpawnPool m_poolAudioSources;

	private float m_fCleanUpTimer;

	private float m_kfCleanUpInterval = 0.25f;

	private static bool m_bPaused;

	public static bool SFXEnabled
	{
		get
		{
			return m_sfxVolume != 0f;
		}
		set
		{
			m_sfxVolume = ((!value) ? 0f : 1f);
			UpdateMenuAudio();
		}
	}

	public static bool MusicEnabled
	{
		get
		{
			return m_musicVolume != 0f;
		}
		set
		{
			m_musicVolume = ((!value) ? 0f : 1f);
		}
	}

	public static bool AudioEnabled => m_globalVolume != 0f;

	public static bool Paused => m_bPaused;

	public static Transform PlayClip(AudioClip clip, bool loop)
	{
		return PlayClip(clip, loop, 1f, 1f);
	}

	public static Transform PlayClip(AudioClip clip, bool loop, float pitch, float volume)
	{
		return PlayClipInternal(clip, loop, pitch, volume, m_sfxVolumeMod);
	}

	public static Transform PlayClipOverrideVolumeModifier(AudioClip clip, bool loop, float volumeMod)
	{
		return PlayClipInternal(clip, loop, 1f, 1f, volumeMod);
	}

	private static Transform PlayClipInternal(AudioClip clip, bool loop, float pitch, float volume, float volumeMod)
	{
		Transform transform = null;
		if (clip != null && m_AudioSourceObject != null)
		{
			transform = m_poolAudioSources.Spawn(m_AudioSourceObject.transform);
			if (transform != null)
			{
				AudioSource audioSource = transform.gameObject.audio;
				if (audioSource != null)
				{
					if (audioSource.isPlaying)
					{
						audioSource.Stop();
					}
					audioSource.volume = m_sfxVolume * m_globalVolume * volumeMod * volume;
					audioSource.clip = clip;
					audioSource.loop = loop;
					audioSource.playOnAwake = false;
					audioSource.pitch = pitch;
					audioSource.Play();
				}
			}
		}
		return transform;
	}

	public static bool IsPlaying(AudioClip clip)
	{
		Transform transform = m_poolAudioSources.FirstOrDefault((Transform source) => source.audio.clip == clip);
		return transform != null;
	}

	public static bool IsPlaying(Transform sourceTransform)
	{
		if ((bool)sourceTransform && sourceTransform.gameObject.activeInHierarchy)
		{
			return sourceTransform.gameObject.audio.isPlaying;
		}
		return false;
	}

	public static void Stop(AudioClip clip)
	{
		if (m_poolAudioSources != null)
		{
			Transform sourceTransform = m_poolAudioSources.FirstOrDefault((Transform source) => source.audio.clip == clip);
			if ((bool)sourceTransform)
			{
				Stop(ref sourceTransform);
			}
		}
	}

	public static void Stop(ref Transform sourceTransform)
	{
		if ((bool)sourceTransform && sourceTransform.gameObject.activeInHierarchy)
		{
			sourceTransform.gameObject.audio.Stop();
			sourceTransform = null;
		}
	}

	public static void StopAll()
	{
		foreach (Transform poolAudioSource in m_poolAudioSources)
		{
			poolAudioSource.audio.Stop();
		}
		m_bPaused = false;
	}

	private void Start()
	{
		EventDispatch.RegisterInterest("ResetGameState", this);
		EventDispatch.RegisterInterest("OnGameDataLoaded", this);
		EventDispatch.RegisterInterest("OnGameDataSaveRequest", this);
		EventDispatch.RegisterInterest("StartGameState", this);
		EventDispatch.RegisterInterest("3rdPartyActive", this);
		EventDispatch.RegisterInterest("3rdPartyInactive", this);
		m_AudioSourceObject = new GameObject("AudioSource");
		m_AudioSourceObject.AddComponent<AudioSource>();
		m_AudioSourceObject.AddComponent<AudioSourceParams>();
		m_poolAudioSources = PoolManager.Pools.Create("AudioSources");
		PrefabPool prefabPool = new PrefabPool(m_AudioSourceObject.transform);
		prefabPool.preloadAmount = 1;
		prefabPool.cullDespawned = false;
		prefabPool.limitInstances = true;
		prefabPool.limitAmount = 20;
		prefabPool.limitFIFO = true;
		m_poolAudioSources.CreatePrefabPool(prefabPool);
		m_fCleanUpTimer = 0f;
		m_bPaused = false;
	}

	private void Update()
	{
		if (m_bPaused)
		{
			return;
		}
		if (m_fCleanUpTimer > m_kfCleanUpInterval)
		{
			m_fCleanUpTimer = 0f;
			{
				foreach (Transform poolAudioSource in m_poolAudioSources)
				{
					if (!poolAudioSource.audio.isPlaying)
					{
						m_poolAudioSources.Despawn(poolAudioSource, 0.05f);
					}
				}
				return;
			}
		}
		m_fCleanUpTimer += Time.fixedDeltaTime;
	}

	private static void UpdateMenuAudio()
	{
		UIButtonSound[] array = Resources.FindObjectsOfTypeAll(typeof(UIButtonSound)) as UIButtonSound[];
		UIButtonSound[] array2 = array;
		foreach (UIButtonSound uIButtonSound in array2)
		{
			uIButtonSound.volume = m_sfxVolume * m_globalVolume * m_sfxVolumeMod;
		}
	}

	private void Event_ResetGameState(GameState.Mode resetState)
	{
		StopAll();
		if (!m_audioInitialised)
		{
			UpdateMenuAudio();
			m_audioInitialised = true;
		}
	}

	private void Event_OnGameDataSaveRequest()
	{
		PropertyStore.Store("MusicVolume", m_musicVolume);
		PropertyStore.Store("SfxVolume", m_sfxVolume);
	}

	private void Event_OnGameDataLoaded(ActiveProperties activeProperties)
	{
		if (activeProperties.DoesPropertyExist("MusicVolume"))
		{
			m_musicVolume = activeProperties.GetFloat("MusicVolume");
		}
		else
		{
			m_musicVolume = 1f;
		}
		if (activeProperties.DoesPropertyExist("SfxVolume"))
		{
			m_sfxVolume = activeProperties.GetFloat("SfxVolume");
		}
		else
		{
			m_sfxVolume = 1f;
		}
	}

	private void Event_StartGameState(GameState.Mode state)
	{
		switch (state)
		{
		case GameState.Mode.Menu:
			StopAll();
			break;
		case GameState.Mode.Game:
			UnpauseAll();
			break;
		case GameState.Mode.PauseMenu:
			PauseAll();
			break;
		}
	}

	private void Event_3rdPartyActive()
	{
		m_globalVolume = 0f;
		UpdateMenuAudio();
	}

	private void Event_3rdPartyInactive()
	{
		m_globalVolume = 1f;
		UpdateMenuAudio();
	}

	private void PauseAll()
	{
		m_bPaused = true;
		foreach (Transform poolAudioSource in m_poolAudioSources)
		{
			if (poolAudioSource.audio.isPlaying)
			{
				poolAudioSource.audio.Pause();
				poolAudioSource.GetComponent<AudioSourceParams>().IsPaused = true;
			}
		}
	}

	private void UnpauseAll()
	{
		foreach (Transform poolAudioSource in m_poolAudioSources)
		{
			AudioSourceParams component = poolAudioSource.GetComponent<AudioSourceParams>();
			if (component.IsPaused)
			{
				poolAudioSource.audio.volume = m_sfxVolume * m_globalVolume * m_sfxVolumeMod;
				poolAudioSource.audio.Play();
				component.IsPaused = false;
			}
		}
		m_bPaused = false;
	}
}
