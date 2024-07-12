using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Hazard))]
internal class EnemyExplosion : MonoBehaviour
{
	private struct ParticleInfo
	{
		public Transform m_transform;

		public Vector3 m_impulse;

		public float m_time;
	}

	private const string ChunkSpawnPoolName = "EnemySupportChunks";

	private const string EffectSpawnPoolName = "EnemySupportEffects";

	[SerializeField]
	private List<GameObject> m_deathChunks = new List<GameObject>();

	[SerializeField]
	private float m_ForwardImpluse = 10f;

	[SerializeField]
	private float m_UpImpluse = 5f;

	[SerializeField]
	private float m_FragmentLifeSpan = 3f;

	[SerializeField]
	private float m_AngularVelocity = 10f;

	[SerializeField]
	private float m_AngleVariation = 45f;

	[SerializeField]
	private ParticleSystem m_deathParticleFX;

	[SerializeField]
	private float m_ParticleForwardImpulse = 1f;

	private SpawnPool m_chunkSpawnPool;

	private SpawnPool m_effectSpawnPool;

	private List<ParticleInfo> m_particlesActive = new List<ParticleInfo>();

	public void Start()
	{
		bool flag = true;
		m_chunkSpawnPool = ((!PoolManager.Pools.ContainsKey("EnemySupportChunks")) ? null : PoolManager.Pools["EnemySupportChunks"]);
		if (!flag)
		{
			m_deathChunks.Clear();
			m_deathChunks = null;
			if ((bool)m_chunkSpawnPool)
			{
				Object.Destroy(m_chunkSpawnPool.gameObject);
				m_chunkSpawnPool = null;
			}
		}
		m_effectSpawnPool = PoolManager.Pools["EnemySupportEffects"];
	}

	private Vector3 CalculateForwardImpulse(SonicSplineTracker sonicSplineTracker)
	{
		Vector3 forwards = sonicSplineTracker.InternalTracker.CurrentSplineTransform.Forwards;
		forwards *= sonicSplineTracker.Speed + sonicSplineTracker.Speed * m_ForwardImpluse;
		return forwards * 0.1f;
	}

	private Vector3 CalculateParticleForwardImpulse(SonicSplineTracker sonicSplineTracker)
	{
		Vector3 forwards = sonicSplineTracker.InternalTracker.CurrentSplineTransform.Forwards;
		return forwards * (sonicSplineTracker.Speed * m_ParticleForwardImpulse);
	}

	public void OnDeath(object[] onDeathParams)
	{
		SonicSplineTracker sonicSplineTracker = (SonicSplineTracker)onDeathParams[0];
		bool flag = (bool)onDeathParams[1];
		Component[] componentsInChildren = base.transform.GetComponentsInChildren(typeof(Renderer), includeInactive: false);
		Component[] array = componentsInChildren;
		for (int i = 0; i < array.Length; i++)
		{
			Renderer renderer = (Renderer)array[i];
			renderer.enabled = false;
		}
		Collider[] componentsInChildren2 = GetComponentsInChildren<Collider>();
		foreach (Collider collider in componentsInChildren2)
		{
			collider.enabled = false;
		}
		if (!flag)
		{
			Vector3 forwardImpulse = CalculateForwardImpulse(sonicSplineTracker);
			Vector3 particleForwardImpulse = CalculateParticleForwardImpulse(sonicSplineTracker);
			Explode(sonicSplineTracker, forwardImpulse, particleForwardImpulse);
		}
	}

	private void OnSpawned()
	{
		Component[] componentsInChildren = base.transform.GetComponentsInChildren(typeof(Renderer), includeInactive: false);
		Component[] array = componentsInChildren;
		for (int i = 0; i < array.Length; i++)
		{
			Renderer renderer = (Renderer)array[i];
			renderer.enabled = true;
		}
		Collider[] componentsInChildren2 = GetComponentsInChildren<Collider>();
		foreach (Collider collider in componentsInChildren2)
		{
			collider.enabled = true;
		}
	}

	private void OnDisable()
	{
		m_particlesActive.Clear();
	}

	private void Explode(SonicSplineTracker sonicSplineTracker, Vector3 forwardImpulse, Vector3 particleForwardImpulse)
	{
		if ((bool)m_effectSpawnPool)
		{
			FireParticleFX(m_deathParticleFX, sonicSplineTracker, particleForwardImpulse);
		}
		if (m_chunkSpawnPool != null)
		{
			SpawnFragments(sonicSplineTracker, forwardImpulse);
		}
	}

	private ParticleSystem FireParticleFX(ParticleSystem fxPrefab, SonicSplineTracker sonicSplineTracker, Vector3 forwardsImpulse)
	{
		if (fxPrefab == null)
		{
			return null;
		}
		ParticleSystem result = null;
		Vector3 position = base.transform.position;
		Quaternion orientation = sonicSplineTracker.InternalTracker.CurrentSplineTransform.Orientation;
		Vector3 vector = new Vector3(0f, 1f, 0f);
		Vector3 pos = position + vector;
		Transform transform = m_effectSpawnPool.Spawn(fxPrefab.transform, pos, orientation);
		if ((bool)transform)
		{
			WorldCollector.MarkAsMovable(transform.gameObject);
			result = transform.GetComponent<ParticleSystem>();
			ParticlePlayer.Play(result);
			ParticleInfo item = default(ParticleInfo);
			item.m_transform = transform;
			item.m_time = fxPrefab.duration + fxPrefab.startLifetime;
			item.m_impulse = forwardsImpulse;
			m_effectSpawnPool.Despawn(transform, item.m_time);
			m_particlesActive.Add(item);
		}
		return result;
	}

	private void SpawnFragments(SonicSplineTracker sonicSplineTracker, Vector3 forwardsImpulse)
	{
		Vector3 position = base.transform.position;
		Vector3 vector = new Vector3(0f, 0.5f, 0f);
		Vector3 pos = position + vector;
		foreach (GameObject deathChunk in m_deathChunks)
		{
			Transform transform = m_chunkSpawnPool.Spawn(deathChunk.transform, pos, Quaternion.identity);
			if ((bool)transform)
			{
				WorldCollector.MarkAsMovable(transform.gameObject);
				Rigidbody component = transform.GetComponent<Rigidbody>();
				float angle = (Random.value * 2f - 1f) * m_AngleVariation;
				Vector3 vector2 = sonicSplineTracker.InternalTracker.CurrentSplineTransform.Up * m_UpImpluse;
				vector2 = Quaternion.AngleAxis(angle, sonicSplineTracker.InternalTracker.CurrentSplineTransform.Forwards) * vector2;
				component.AddForce(-component.velocity, ForceMode.VelocityChange);
				component.AddForce(vector2 + forwardsImpulse, ForceMode.Impulse);
				Vector3 torque = new Vector3(Random.value, Random.value, Random.value) * m_AngularVelocity;
				component.AddTorque(-component.angularVelocity, ForceMode.VelocityChange);
				component.AddTorque(torque, ForceMode.Impulse);
				m_chunkSpawnPool.Despawn(transform, m_FragmentLifeSpan);
			}
		}
	}

	private void Update()
	{
		int num = 0;
		while (num < m_particlesActive.Count)
		{
			ParticleInfo particleInfo = m_particlesActive[num];
			particleInfo.m_time -= Time.deltaTime;
			if (particleInfo.m_time > 0f)
			{
				particleInfo.m_transform.position += particleInfo.m_impulse * Time.deltaTime;
				num++;
			}
		}
	}
}
