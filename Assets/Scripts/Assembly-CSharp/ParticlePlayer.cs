using System;
using System.Collections.Generic;
using UnityEngine;

public class ParticlePlayer : MonoBehaviour
{
	public enum Important
	{
		Yes,
		No
	}

	[Flags]
	private enum State
	{
		Disabled = 1
	}

	private const int ImportantConcurrentEffects = 20;

	private const int NonEssentialConcurrentEffects = 15;

	private static ParticlePlayer s_particlePlayer;

	private static List<ParticleSystem> s_importantSystems;

	private static List<ParticleSystem> s_nonEssentialSystems;

	private static List<ParticleSystem> s_disabledSystems;

	private State m_state;

	[SerializeField]
	private GameObject[] m_particleGroups;

	public static void Play(ParticleSystem particleSystem, Important important)
	{
		List<ParticleSystem> list = s_nonEssentialSystems;
		if (important == Important.Yes)
		{
			list = s_importantSystems;
		}
		if (list.Capacity != list.Count)
		{
			particleSystem.gameObject.SetActive(value: true);
			particleSystem.Clear();
			particleSystem.Play();
			list.Add(particleSystem);
		}
	}

	public static void Play(ParticleSystem particleSystem)
	{
		Play(particleSystem, Important.No);
	}

	public static void Stop(ParticleSystem particleSystem)
	{
		if (particleSystem.isPlaying)
		{
			particleSystem.Stop();
			particleSystem.Stop();
			particleSystem.gameObject.SetActive(value: false);
		}
	}

	private void Start()
	{
		s_particlePlayer = this;
		InitialiseSystemList();
		EventDispatch.RegisterInterest("ResetGameState", this, EventDispatch.Priority.Lowest);
		EventDispatch.RegisterInterest("CharacterUnloadStart", this, EventDispatch.Priority.Lowest);
	}

	private void Update()
	{
		UpdateSystemList();
	}

	private void DisableAllNodes()
	{
		GameObject[] particleGroups = m_particleGroups;
		foreach (GameObject gameObject in particleGroups)
		{
			gameObject.SetActive(value: false);
			gameObject.SetActive(value: true);
		}
	}

	private void InitialiseSystemList()
	{
		s_importantSystems = new List<ParticleSystem>(20);
		s_nonEssentialSystems = new List<ParticleSystem>(15);
		s_disabledSystems = new List<ParticleSystem>(35);
	}

	private void UpdateSystemList()
	{
		for (int i = 0; i < s_importantSystems.Count; i++)
		{
			ParticleSystem particleSystem = s_importantSystems[i];
			if (!particleSystem.gameObject.activeInHierarchy || !particleSystem.isPlaying)
			{
				s_disabledSystems.Add(particleSystem);
			}
		}
		for (int j = 0; j < s_nonEssentialSystems.Count; j++)
		{
			ParticleSystem particleSystem2 = s_nonEssentialSystems[j];
			if (!particleSystem2.gameObject.activeInHierarchy || !particleSystem2.isPlaying)
			{
				s_disabledSystems.Add(particleSystem2);
			}
		}
		for (int k = 0; k < s_disabledSystems.Count; k++)
		{
			ParticleSystem particleSystem3 = s_disabledSystems[k];
			particleSystem3.gameObject.SetActive(value: false);
			s_importantSystems.Remove(particleSystem3);
			s_nonEssentialSystems.Remove(particleSystem3);
		}
		s_disabledSystems.Clear();
	}

	public void flush()
	{
		for (int i = 0; i < s_importantSystems.Count; i++)
		{
			ParticleSystem particleSystem = s_importantSystems[i];
			particleSystem.gameObject.SetActive(value: false);
		}
		for (int j = 0; j < s_nonEssentialSystems.Count; j++)
		{
			ParticleSystem particleSystem2 = s_nonEssentialSystems[j];
			particleSystem2.gameObject.SetActive(value: false);
		}
		s_nonEssentialSystems.Clear();
		s_importantSystems.Clear();
	}

	private void Event_ResetGameState(GameState.Mode mode)
	{
		if ((m_state & State.Disabled) != State.Disabled && mode == GameState.Mode.Menu)
		{
			DisableAllNodes();
			m_state |= State.Disabled;
		}
	}

	private void Event_CharacterUnloadStart()
	{
		flush();
	}
}
