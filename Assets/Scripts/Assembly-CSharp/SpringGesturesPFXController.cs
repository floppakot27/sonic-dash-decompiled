using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[AddComponentMenu("Dash/Sonic/Spring Gesture PFX Controller")]
public class SpringGesturesPFXController : MonoBehaviour
{
	private ParticleSystem[] m_particleLevels;

	private int m_nextPFXToPlay;

	private void Awake()
	{
		m_particleLevels = FindAllPFX().ToArray();
		OrderPFXByLevel(m_particleLevels);
	}

	private void Start()
	{
		EventDispatch.RegisterInterest("OnNewGameStarted", this);
		EventDispatch.RegisterInterest("OnGameFinished", this);
		EventDispatch.RegisterInterest("OnGameReset", this);
		EventDispatch.RegisterInterest("ResetGameState", this);
		EventDispatch.RegisterInterest("OnSpringStart", this);
		EventDispatch.RegisterInterest("OnSingleSpringGestureSuccess", this);
		ParentPFXToSonic();
	}

	public void OnDestroy()
	{
		EventDispatch.UnregisterAllInterest(this);
	}

	private void Event_OnNewGameStarted()
	{
		StopAndClearParticles();
	}

	private void Event_OnGameFinished()
	{
		StopAndClearParticles();
	}

	private void Event_OnGameReset()
	{
		StopAndClearParticles();
	}

	private void Event_ResetGameState(GameState.Mode resetState)
	{
		StopAndClearParticles();
	}

	private void StopAndClearParticles()
	{
		ParticleSystem[] particleLevels = m_particleLevels;
		foreach (ParticleSystem particleSystem in particleLevels)
		{
			particleSystem.Stop();
			particleSystem.Clear();
		}
	}

	private void Event_OnSpringStart(SpringTV.Type springType)
	{
		m_nextPFXToPlay = 0;
	}

	private void Event_OnSingleSpringGestureSuccess(CartesianDir dir)
	{
		ParticlePlayer.Play(m_particleLevels[m_nextPFXToPlay], ParticlePlayer.Important.Yes);
		m_nextPFXToPlay++;
	}

	private IList<ParticleSystem> FindAllPFX()
	{
		List<ParticleSystem> list = new List<ParticleSystem>();
		foreach (Transform item in base.transform)
		{
			ParticleSystem component = item.GetComponent<ParticleSystem>();
			if (!(component == null))
			{
				list.Add(component);
			}
		}
		return list;
	}

	private void OrderPFXByLevel(ParticleSystem[] allPFX)
	{
		int[] keys = allPFX.Select((ParticleSystem pfx) => CalculateIndexFromName(pfx.gameObject.name)).ToArray();
		Array.Sort(keys, allPFX);
	}

	private void ParentPFXToSonic()
	{
		if (null != Sonic.Tracker)
		{
			base.transform.parent = Sonic.Tracker.transform;
			base.transform.localPosition = Vector3.zero;
		}
	}

	private static int CalculateIndexFromName(string indexPostfixName)
	{
		int num = indexPostfixName.Length - 1;
		while (char.IsDigit(indexPostfixName[num]))
		{
			num--;
		}
		string s = indexPostfixName.Substring(num + 1);
		return int.Parse(s);
	}
}
