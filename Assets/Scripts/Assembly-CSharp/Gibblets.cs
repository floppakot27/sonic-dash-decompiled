using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngineExtensions;

[AddComponentMenu("Dash/Enemies/Gibblets component")]
[RequireComponent(typeof(Enemy))]
internal class Gibblets : MonoBehaviour
{
	[SerializeField]
	private float m_deathBounceInitialForce = 500f;

	[SerializeField]
	private float m_deathBounceGravity = 25f;

	[SerializeField]
	private float m_deathBounceRestitution = 0.65f;

	[SerializeField]
	private float m_deathKillerSpeedMultiplier = 1.3f;

	[SerializeField]
	private List<GameObject> m_deathChunks = new List<GameObject>();

	[SerializeField]
	private ParticleSystem m_deathParticleFX;

	[SerializeField]
	private ParticleSystem m_chunkBounceParticleFX;

	private Transform m_gibbsContainer;

	public void Awake()
	{
		m_gibbsContainer = null;
	}

	public void OnDeath(object[] onDeathParams)
	{
		SonicSplineTracker killer = (SonicSplineTracker)onDeathParams[0];
		if (!(m_gibbsContainer != null))
		{
			StopAllCoroutines();
			StartCoroutine(DoDeathBounce(killer));
		}
	}

	private void OnSpawned()
	{
		StopAllCoroutines();
		SkinnedMeshRenderer[] componentsInChildren = GetComponentsInChildren<SkinnedMeshRenderer>();
		foreach (SkinnedMeshRenderer skinnedMeshRenderer in componentsInChildren)
		{
			skinnedMeshRenderer.enabled = true;
		}
		Collider[] componentsInChildren2 = GetComponentsInChildren<Collider>();
		foreach (Collider collider in componentsInChildren2)
		{
			collider.enabled = true;
		}
		if (m_gibbsContainer != null)
		{
			UnityEngine.Object.Destroy(m_gibbsContainer.gameObject);
			m_gibbsContainer = null;
		}
	}

	private IEnumerator DoDeathBounce(SonicSplineTracker killer)
	{
		Enemy enemy = GetComponent<Enemy>();
		Renderer[] componentsInChildren = GetComponentsInChildren<Renderer>();
		foreach (Renderer mesh in componentsInChildren)
		{
			mesh.enabled = false;
		}
		Collider[] componentsInChildren2 = GetComponentsInChildren<Collider>();
		foreach (Collider collider in componentsInChildren2)
		{
			collider.enabled = false;
		}
		float killerIdealSpeed = Mathf.Max(killer.Handling.CalculateVelocityAt(GameState.TimeInGame), killer.Speed);
		FireDeathParticleFX(killer.transform, killerIdealSpeed);
		var allSplines = from s in enemy.getSpline().transform.parent.gameObject.GetComponentsInChildren<Spline>()
			select new
			{
				Spline = s,
				Dist = s.EstimateDistanceAlongSpline(base.transform.position).LineDistance
			};
		System.Random rng = new System.Random();
		Quaternion localChunkRotation = Quaternion.Euler(270f, 0f, 0f);
		m_gibbsContainer = new GameObject("Gibblets").transform;
		m_gibbsContainer.parent = base.transform;
		List<IEnumerator> chunkBouncers = new List<IEnumerator>();
		foreach (GameObject chunk in m_deathChunks)
		{
			GameObject newChunk = UnityEngine.Object.Instantiate(chunk, base.transform.position, base.transform.rotation) as GameObject;
			newChunk.transform.parent = m_gibbsContainer;
			newChunk.transform.rotation = base.transform.rotation * localChunkRotation;
			var splineData = allSplines.ElementAt(rng.Next(allSplines.Count()));
			chunkBouncers.Add(ChunkDeathBounce(killer.Track, newChunk.transform, splineData.Spline, splineData.Dist, killerIdealSpeed));
		}
		yield return this.JoinCoroutines(chunkBouncers);
		enemy.RequestDestruction();
	}

	private void FireDeathParticleFX(Transform killerTransform, float killerSpeed)
	{
		ParticleSystem particleSystem = FireParticleFX(m_deathParticleFX, base.transform.position, killerTransform.rotation);
		if (particleSystem != null)
		{
			particleSystem.startSpeed = killerSpeed;
		}
	}

	private void FireChunkBounceParticleFX(LightweightTransform bounceTransform)
	{
		Quaternion rotation = Quaternion.LookRotation(bounceTransform.Up, bounceTransform.Forwards);
		FireParticleFX(m_chunkBounceParticleFX, bounceTransform.Location, rotation);
	}

	private ParticleSystem FireParticleFX(ParticleSystem fxPrefab, Vector3 position, Quaternion rotation)
	{
		if (fxPrefab == null)
		{
			return null;
		}
		ParticleSystem particleSystem = UnityEngine.Object.Instantiate(fxPrefab) as ParticleSystem;
		particleSystem.transform.position = position;
		particleSystem.transform.rotation = rotation;
		WorldCollector.MarkAsMovable(particleSystem.gameObject);
		UnityEngine.Object.Destroy(particleSystem.gameObject, particleSystem.duration + particleSystem.startLifetime);
		ParticlePlayer.Play(particleSystem);
		return particleSystem;
	}

	private IEnumerator ChunkDeathBounce(Track track, Transform chunk, Spline spline, float splinePos, float minSpeed)
	{
		float initialForce = m_deathBounceInitialForce * UnityEngine.Random.Range(0.9f, 1.1f);
		float restitution = m_deathBounceRestitution * UnityEngine.Random.Range(0.5f, 1f);
		float trackerSpeed = minSpeed * UnityEngine.Random.Range(m_deathKillerSpeedMultiplier, m_deathKillerSpeedMultiplier + 0.1f);
		SplineBouncer bouncer = new SplineBouncer(track, spline, splinePos, trackerSpeed, initialForce, m_deathBounceGravity, restitution);
		bouncer.OnBounce += delegate
		{
			FireChunkBounceParticleFX(bouncer.CurrentTransform);
			SendMessage("OnChunkBounce", bouncer.CurrentTransform.Location);
		};
		bouncer.Update(0f);
		Vector3 targetPos = bouncer.CurrentTransform.Location;
		float rightOffset = Vector3.Dot(bouncer.CurrentTransform.Right, chunk.transform.position - targetPos);
		float offsetVel = 0f;
		float targetOffset = (chunk.gameObject.name.Contains("01") ? rightOffset : UnityEngine.Random.Range(-1f, 1f));
		Quaternion initialRotation = chunk.localRotation;
		Vector3 tumbleAxis = UnityEngine.Random.onUnitSphere;
		float rotation = 0f;
		float rotationRate = UnityEngine.Random.Range(45f, 500f);
		int maxBounces = 2;
		while (bouncer.BounceCount <= maxBounces)
		{
			bouncer.Update(Time.deltaTime);
			LightweightTransform chunkTargetTransform = bouncer.CurrentTransform;
			chunk.position = chunkTargetTransform.Location + chunkTargetTransform.Right * rightOffset;
			chunk.rotation = initialRotation * Quaternion.AngleAxis(rotation, tumbleAxis);
			if (Time.deltaTime > 0f)
			{
				rightOffset = Utils.SmoothDamp(rightOffset, targetOffset, ref offsetVel, 0.5f);
			}
			rotation += rotationRate * Time.deltaTime;
			yield return null;
		}
		UnityEngine.Object.Destroy(chunk.gameObject);
	}
}
