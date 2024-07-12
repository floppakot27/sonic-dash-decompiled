using System.Collections.Generic;
using UnityEngine;

public class ObstacleMine : Obstacle
{
	private class ObstacleMineCollisionResolver : CollisionResolver
	{
		public ObstacleMineCollisionResolver()
			: base(ResolutionType.SonicDeath)
		{
		}

		public override void ProcessMotionState(MotionState state, bool heldRings, bool ghosted)
		{
			if (ghosted)
			{
				base.Resolution = ResolutionType.Nothing;
			}
			else if (heldRings)
			{
				base.Resolution = ResolutionType.SonicStumble;
			}
			else
			{
				base.Resolution = ResolutionType.SonicDeath;
			}
		}
	}

	private struct ParticleInfo
	{
		public Transform m_transform;

		public Vector3 m_impulse;

		public float m_time;
	}

	public GameObject m_particleSystemGO;

	private bool m_isPlaced;

	public List<GameObject> m_fragmentsDatabase = new List<GameObject>();

	public float m_ExplosionForwardImpluse = 0.5f;

	public float m_ExplosionUpImpluse = 0.5f;

	public float m_FragmentLifeSpan = 5f;

	public ParticleSystem m_exlosionParticleEffect;

	public float m_ParticleForwardImpulse = 0.5f;

	private SpawnPool m_supportSpawnPool;

	private List<ParticleInfo> m_particlesActive = new List<ParticleInfo>();

	private Spline CurrentSpline { get; set; }

	private Track Track { get; set; }

	public override void Start()
	{
		base.Start();
		m_supportSpawnPool = PoolManager.Pools["ObstaclesSupport"];
		base.CollisionResolver = new ObstacleMineCollisionResolver();
	}

	public void Update()
	{
		int num = 0;
		while (num < m_particlesActive.Count)
		{
			ParticleInfo particleInfo = m_particlesActive[num];
			particleInfo.m_time -= Time.deltaTime;
			if (particleInfo.m_time <= 0f)
			{
				m_supportSpawnPool.Despawn(particleInfo.m_transform, 0f);
				m_particlesActive.RemoveAt(num);
			}
			else
			{
				particleInfo.m_transform.position += particleInfo.m_impulse * Time.deltaTime;
				num++;
			}
		}
		if (m_isPlaced && CurrentSpline == null)
		{
			DestroySelf();
		}
	}

	public void OnSpawned()
	{
		SetIsVisible(isVisible: true);
	}

	public override void OnSonicKill(SonicSplineTracker sonicSplineTracker)
	{
		Explode(sonicSplineTracker, Vector3.zero, Vector3.zero);
	}

	public override void OnStumble(SonicSplineTracker sonicSplineTracker)
	{
		Vector3 forwardImpulse = CalculateForwardImpulse(sonicSplineTracker);
		Vector3 particleForwardImpulse = CalculateParticleForwardImpulse(sonicSplineTracker);
		Explode(sonicSplineTracker, forwardImpulse, particleForwardImpulse);
	}

	public override void OnDeath(object[] onDeathParams)
	{
		SonicSplineTracker sonicSplineTracker = (SonicSplineTracker)onDeathParams[0];
		bool flag = (bool)onDeathParams[1];
		Vector3 forwardImpulse = Vector3.zero;
		Vector3 particleForwardImpulse = Vector3.zero;
		if (!flag)
		{
			forwardImpulse = CalculateForwardImpulse(sonicSplineTracker);
			particleForwardImpulse = CalculateParticleForwardImpulse(sonicSplineTracker);
		}
		Explode(sonicSplineTracker, forwardImpulse, particleForwardImpulse);
	}

	private void Explode(SonicSplineTracker sonicSplineTracker, Vector3 forwardImpulse, Vector3 particleForwardImpulse)
	{
		Vector3 position = sonicSplineTracker.transform.position;
		Quaternion rotation = sonicSplineTracker.transform.rotation;
		SetIsVisible(isVisible: false);
		if (m_supportSpawnPool != null)
		{
			Vector3 vector = new Vector3(0f, 2f, 0f);
			Vector3 position2 = position + vector;
			FireParticleFX(m_exlosionParticleEffect, position2, rotation, particleForwardImpulse);
			SpawnFragments(sonicSplineTracker, position2, forwardImpulse);
		}
	}

	private void SetIsVisible(bool isVisible)
	{
		MeshRenderer[] componentsInChildren = GetComponentsInChildren<MeshRenderer>();
		foreach (MeshRenderer meshRenderer in componentsInChildren)
		{
			meshRenderer.enabled = isVisible;
		}
		Collider[] componentsInChildren2 = GetComponentsInChildren<Collider>();
		foreach (Collider collider in componentsInChildren2)
		{
			collider.enabled = isVisible;
		}
		if ((bool)m_particleSystemGO)
		{
			m_particleSystemGO.SetActive(isVisible);
		}
	}

	private Vector3 CalculateForwardImpulse(SonicSplineTracker sonicSplineTracker)
	{
		Vector3 forwards = sonicSplineTracker.InternalTracker.CurrentSplineTransform.Forwards;
		forwards *= sonicSplineTracker.Speed + sonicSplineTracker.Speed * m_ExplosionForwardImpluse;
		return forwards * 0.1f;
	}

	private Vector3 CalculateParticleForwardImpulse(SonicSplineTracker sonicSplineTracker)
	{
		Vector3 forwards = sonicSplineTracker.InternalTracker.CurrentSplineTransform.Forwards;
		return forwards * (sonicSplineTracker.Speed * m_ParticleForwardImpulse);
	}

	private ParticleSystem FireParticleFX(ParticleSystem fxPrefab, Vector3 position, Quaternion rotation, Vector3 forwardsImpulse)
	{
		if (fxPrefab == null)
		{
			return null;
		}
		Transform transform = m_supportSpawnPool.Spawn(fxPrefab.transform, position, rotation);
		WorldCollector.MarkAsMovable(transform.gameObject);
		ParticleSystem component = transform.GetComponent<ParticleSystem>();
		ParticlePlayer.Play(component);
		ParticleInfo item = default(ParticleInfo);
		item.m_transform = transform;
		item.m_time = fxPrefab.duration + fxPrefab.startLifetime;
		item.m_impulse = forwardsImpulse;
		m_particlesActive.Add(item);
		return component;
	}

	private void SpawnFragments(SonicSplineTracker killer, Vector3 position, Vector3 forwardsImpulse)
	{
		foreach (GameObject item in m_fragmentsDatabase)
		{
			Transform transform = m_supportSpawnPool.Spawn(item.transform, position, base.transform.rotation);
			if ((bool)transform)
			{
				WorldCollector.MarkAsMovable(transform.gameObject);
				Rigidbody component = transform.GetComponent<Rigidbody>();
				float num = 30f;
				float x = (Random.value * 2f - 1f) * num;
				float z = (Random.value * 2f - 1f) * num;
				Vector3 vector = Quaternion.Euler(x, 0f, z) * Vector3.up * m_ExplosionUpImpluse;
				component.AddForce(vector + forwardsImpulse, ForceMode.Impulse);
				m_supportSpawnPool.Despawn(transform, m_FragmentLifeSpan);
			}
		}
	}

	public override Spline getSpline()
	{
		return CurrentSpline;
	}

	protected override void Place(Track track, Spline spline)
	{
		CurrentSpline = spline;
		Track = track;
		m_isPlaced = true;
	}
}
