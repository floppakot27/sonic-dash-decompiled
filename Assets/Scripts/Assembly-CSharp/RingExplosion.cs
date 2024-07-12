using System;
using System.Collections.Generic;
using UnityEngine;

public class RingExplosion : MonoBehaviour
{
	private class Explosion
	{
		[Flags]
		public enum State
		{
			None = 0,
			SnapPositions = 1,
			Active = 2,
			DisableCountDowns = 4
		}

		public GameObject m_explosionRoot;

		public List<RingBounce> m_ringCollisionList;

		public RingSequence m_ringSequence;

		public float m_lifeTime;

		public State m_state;
	}

	private Explosion[] m_explosions;

	private GameObject m_worldRoot;

	[SerializeField]
	private AudioClip m_dropRingsClip;

	[SerializeField]
	private GameObject m_ringExplosionRoot;

	[SerializeField]
	private string m_ringExplosionSource;

	[SerializeField]
	private int m_explosionRingCount = 20;

	[SerializeField]
	private int m_explosionPoolCount = 3;

	[SerializeField]
	private float m_resetTimeOut = 3f;

	private void Start()
	{
		InitialiseExplosionPool();
		EventDispatch.RegisterInterest("OnRingExplosion", this, EventDispatch.Priority.Highest);
		EventDispatch.RegisterInterest("OnNewGameStarted", this);
		EventDispatch.RegisterInterest("OnSonicDeath", this);
	}

	private void Update()
	{
		Explosion[] explosions = m_explosions;
		foreach (Explosion explosion in explosions)
		{
			if ((explosion.m_state & Explosion.State.Active) == Explosion.State.Active)
			{
				explosion.m_lifeTime += Time.deltaTime;
				UpdateRingPositions(explosion);
				UpdateReset(explosion);
			}
		}
	}

	private void TriggerExplosion(Explosion thisExplosion)
	{
		Vector3 forward = CalculateForwardImpulse();
		thisExplosion.m_ringSequence.Reset();
		thisExplosion.m_ringSequence.Collectable = false;
		thisExplosion.m_lifeTime = 0f;
		thisExplosion.m_state |= Explosion.State.Active;
		thisExplosion.m_state |= Explosion.State.SnapPositions;
		RepositionExplosionRoot(thisExplosion);
		int ringCount = GetRingCount(thisExplosion);
		for (int i = 0; i < ringCount; i++)
		{
			RingBounce ringBounce = thisExplosion.m_ringCollisionList[i];
			ringBounce.Enable(enable: true);
			ringBounce.Reset(Sonic.Tracker.Speed, forward);
			Quaternion ringOrientation = Quaternion.Euler(0f, UnityEngine.Random.value * 360f, 0f);
			Vector3 ringWorldPosition = ringBounce.Position();
			thisExplosion.m_ringSequence.AddRing(ringWorldPosition, ringOrientation, Vector3.zero, Vector3.zero, Sonic.Tracker.gameObject, RingSequence.NoStreak, 0f);
		}
		Audio.PlayClip(m_dropRingsClip, loop: false);
	}

	private void ShutdownExplosion(Explosion thisExplosion)
	{
		foreach (RingBounce ringCollision in thisExplosion.m_ringCollisionList)
		{
			ringCollision.Enable(enable: false);
		}
		thisExplosion.m_ringSequence.Reset();
		thisExplosion.m_state &= ~Explosion.State.Active;
	}

	private void InitialiseExplosionPool()
	{
		RingGenerator component = GetComponent<RingGenerator>();
		m_explosions = new Explosion[m_explosionPoolCount];
		for (int i = 0; i < m_explosionPoolCount; i++)
		{
			m_explosions[i] = new Explosion();
		}
		Explosion[] explosions = m_explosions;
		foreach (Explosion explosion in explosions)
		{
			explosion.m_explosionRoot = UnityEngine.Object.Instantiate(m_ringExplosionRoot) as GameObject;
			int numberToCreate = Math.Min(component.MaximumSequenceLength, m_explosionRingCount);
			IntialiseRingBounces(explosion, numberToCreate);
			explosion.m_ringSequence = component.GetReservedSequence();
			explosion.m_ringSequence.TransformPosition = false;
			explosion.m_ringSequence.Collectable = false;
			explosion.m_state = Explosion.State.None;
		}
	}

	private void IntialiseRingBounces(Explosion thisExplosion, int numberToCreate)
	{
		bool flag = FeatureSupport.IsSupported("Physics Based Collision");
		thisExplosion.m_ringCollisionList = new List<RingBounce>(numberToCreate);
		for (int i = 0; i < numberToCreate; i++)
		{
			RingBounce ringBounce = null;
			bool replicateProperties = ((i != 0) ? true : false);
			ringBounce = ((!flag) ? ((RingBounce)new RingBounceFake(thisExplosion.m_explosionRoot, replicateProperties)) : ((RingBounce)new RingBouncePhysics(thisExplosion.m_explosionRoot, replicateProperties)));
			ringBounce.Source = m_ringExplosionSource;
			thisExplosion.m_ringCollisionList.Add(ringBounce);
		}
		for (int j = 0; j < numberToCreate; j++)
		{
			RingBounce ringBounce2 = thisExplosion.m_ringCollisionList[j];
			ringBounce2.Enable(enable: false);
		}
	}

	private Vector3 CalculateForwardImpulse()
	{
		SplineTracker splineTracker = Sonic.Tracker.InternalTracker.Clone();
		splineTracker.UpdatePositionByDelta(5f);
		return splineTracker.CurrentSplineTransform.Forwards;
	}

	private void RepositionExplosionRoot(Explosion thisExplosion)
	{
		Vector3 vector = m_worldRoot.transform.InverseTransformPoint(Vector3.zero);
		thisExplosion.m_explosionRoot.transform.position = vector;
		thisExplosion.m_explosionRoot.transform.localPosition = vector;
	}

	private void UpdateRingPositions(Explosion thisExplosion)
	{
		for (int i = 0; i < thisExplosion.m_ringSequence.Length; i++)
		{
			RingBounce ringBounce = thisExplosion.m_ringCollisionList[i];
			RingSequence.Ring ring = thisExplosion.m_ringSequence.GetRing(i);
			ringBounce.Update();
			if ((thisExplosion.m_state & Explosion.State.SnapPositions) == Explosion.State.SnapPositions)
			{
				ring.m_position = ringBounce.Position();
			}
			else
			{
				ring.m_position = Vector3.Lerp(ring.m_position, ringBounce.Position(), 15f * Time.deltaTime);
			}
		}
		thisExplosion.m_state &= ~Explosion.State.SnapPositions;
	}

	private void UpdateReset(Explosion thisExplosion)
	{
		if ((thisExplosion.m_state & Explosion.State.DisableCountDowns) != Explosion.State.DisableCountDowns && thisExplosion.m_lifeTime > m_resetTimeOut)
		{
			ShutdownExplosion(thisExplosion);
		}
	}

	private Explosion FindFreeExplosion()
	{
		Explosion explosion = null;
		Explosion[] explosions = m_explosions;
		foreach (Explosion explosion2 in explosions)
		{
			if ((explosion2.m_state & Explosion.State.Active) != Explosion.State.Active)
			{
				return explosion2;
			}
			if (explosion == null || explosion2.m_lifeTime > explosion.m_lifeTime)
			{
				explosion = explosion2;
			}
		}
		return explosion;
	}

	private int GetRingCount(Explosion thisExplosion)
	{
		int heldRings = RingStorage.HeldRings;
		return Mathf.Min(heldRings, thisExplosion.m_ringSequence.Capacity, thisExplosion.m_ringCollisionList.Count);
	}

	private void Event_OnRingExplosion()
	{
		if (RingStorage.HeldRings > 0)
		{
			Explosion thisExplosion = FindFreeExplosion();
			TriggerExplosion(thisExplosion);
		}
	}

	private void Event_OnNewGameStarted()
	{
		if (m_worldRoot == null)
		{
			m_worldRoot = GameObject.Find("World");
		}
		Explosion[] explosions = m_explosions;
		foreach (Explosion explosion in explosions)
		{
			explosion.m_state &= ~Explosion.State.DisableCountDowns;
		}
	}

	private void Event_OnSonicDeath()
	{
		Explosion[] explosions = m_explosions;
		foreach (Explosion explosion in explosions)
		{
			explosion.m_state |= Explosion.State.DisableCountDowns;
		}
	}
}
