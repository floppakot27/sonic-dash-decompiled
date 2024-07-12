using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SonicMenuAnimationsControl))]
[AddComponentMenu("Dash/Sonic/Game State Control")]
internal class SonicGameStateControl : MonoBehaviour
{
	private Behaviour[][] m_activeStateComponents;

	private void Awake()
	{
		EventDispatch.RegisterInterest("ResetGameState", this);
		EventDispatch.RegisterInterest("StartGameState", this);
		EventDispatch.RegisterInterest("CharacterLoaded", this);
		EnsureComponentListCreated();
		Behaviour[][] activeStateComponents = m_activeStateComponents;
		foreach (Behaviour[] components in activeStateComponents)
		{
			SetEnabled(components, isEnabled: false);
		}
	}

	private void Event_CharacterLoaded()
	{
		EnsureComponentListCreated();
		UpdateComponents(GameState.Mode.Menu);
		ResetComponents(m_activeStateComponents[1], GameState.Mode.Menu);
	}

	private void Event_ResetGameState(GameState.Mode mode)
	{
		if (mode == GameState.Mode.Menu)
		{
			ResetComponents(m_activeStateComponents[(int)mode], mode);
		}
		ResetComponents(m_activeStateComponents[1], mode);
		UpdateComponents(mode);
	}

	private void Event_StartGameState(GameState.Mode mode)
	{
		UpdateComponents(mode);
	}

	private void ResetComponents(IEnumerable<Behaviour> components, GameState.Mode mode)
	{
		foreach (Behaviour component in components)
		{
			if (component is MonoBehaviour)
			{
				object[] param = new object[1] { mode };
				Utils.InstantlyInvoke(component, "OnGameReset", param, SendMessageOptions.DontRequireReceiver);
			}
		}
	}

	private void UpdateComponents(GameState.Mode newMode)
	{
		EnsureComponentListCreated();
		foreach (int value in Enum.GetValues(typeof(GameState.Mode)))
		{
			bool isEnabled = newMode == (GameState.Mode)value;
			SetEnabled(m_activeStateComponents[value], isEnabled);
		}
	}

	private void SetEnabled(Behaviour[] components, bool isEnabled)
	{
		if (components == null)
		{
			return;
		}
		foreach (Behaviour behaviour in components)
		{
			if (behaviour != null)
			{
				behaviour.enabled = isEnabled;
			}
		}
	}

	private void EnsureComponentListCreated()
	{
		m_activeStateComponents = new Behaviour[Enum.GetNames(typeof(GameState.Mode)).Length][];
		Behaviour behaviour = GetComponentInChildren(typeof(SonicMenuAnimationsControl)) as Behaviour;
		Behaviour behaviour2 = GetComponentInChildren(typeof(SonicSplineTracker)) as Behaviour;
		Behaviour behaviour3 = GetComponentInChildren(typeof(SonicAnimationControl)) as Behaviour;
		Behaviour behaviour4 = GetComponentInChildren(typeof(SonicController)) as Behaviour;
		m_activeStateComponents[0] = new Behaviour[1] { behaviour };
		m_activeStateComponents[1] = new Behaviour[3] { behaviour2, behaviour3, behaviour4 };
		m_activeStateComponents[2] = new Behaviour[0];
	}
}
