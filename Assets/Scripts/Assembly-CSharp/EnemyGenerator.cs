using System;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("Dash/Enemies/Enemy Generator")]
public class EnemyGenerator : HazardGenerator
{
	[SerializeField]
	private Material m_defaultMaterial;

	[SerializeField]
	private Material m_goldenMaterial;

	private Enemy m_nearestGoldenEnemy;

	private TrackGenerator m_trackGen;

	protected override SpawnPool Pool { get; set; }

	public override void Start()
	{
		base.Start();
		Pool = GetComponent<SpawnPool>();
		m_trackGen = UnityEngine.Object.FindObjectOfType(typeof(TrackGenerator)) as TrackGenerator;
		EventDispatch.RegisterInterest("OnNewGameStarted", this);
	}

	public GameObject GenerateEnemy(Type enemyType, LightweightTransform atTransform, Track onTrack, Spline onSpline, Enemy.Direction headingTo, float trackDistance)
	{
		foreach (Transform value in Pool.prefabs.Values)
		{
			if (value.GetComponent(enemyType) == null)
			{
				continue;
			}
			return GenerateEnemy(value, atTransform, onTrack, onSpline, headingTo, trackDistance);
		}
		return null;
	}

	private GameObject GenerateEnemy(Transform fromPrefab, LightweightTransform atTransform, Track onTrack, Spline onSpline, Enemy.Direction headingTo, float trackDistance)
	{
		GameObject gameObject = Pool.Spawn(fromPrefab, atTransform.Location, atTransform.Orientation).gameObject;
		Enemy component = gameObject.GetComponent<Enemy>();
		List<Enemy> enemiesInRange = TargetManager.instance().GetEnemiesInRange(trackDistance - TargetManager.instance().EnemyGroupingThreshold, trackDistance);
		component.Place(base.OnSpawnableDestruction, onTrack, onSpline, headingTo, trackDistance);
		component.SetGoldenState(m_defaultMaterial, m_goldenMaterial);
		OnNewSpawnable(component);
		foreach (Enemy item in enemiesInRange)
		{
			component.GroupTogether(item);
		}
		return gameObject;
	}

	private Enemy GetNearestGoldenEnemy(float lowestDistance)
	{
		Enemy result = null;
		float num = float.MaxValue;
		Component[] componentsInChildren = m_trackGen.GetComponentsInChildren<Enemy>(includeInactive: true);
		Component[] array = componentsInChildren;
		for (int i = 0; i < array.Length; i++)
		{
			Enemy enemy = (Enemy)array[i];
			if (enemy.Golden && enemy.TrackDistance > lowestDistance && enemy.TrackDistance < num)
			{
				num = enemy.TrackDistance;
				result = enemy;
			}
		}
		return result;
	}

	private void Event_OnNewGameStarted()
	{
		Component[] componentsInChildren = m_trackGen.GetComponentsInChildren<Enemy>(includeInactive: true);
		Component[] array = componentsInChildren;
		for (int i = 0; i < array.Length; i++)
		{
			Enemy enemy = (Enemy)array[i];
			enemy.SetGoldenState(m_defaultMaterial, m_goldenMaterial);
		}
		m_nearestGoldenEnemy = null;
		ParticleSystem goldnikAura = Sonic.ParticleController.m_goldnikAura;
		goldnikAura.gameObject.SetActive(value: true);
		if (Boosters.IsBoosterSelected(PowerUps.Type.Booster_GoldenEnemy))
		{
			ParticlePlayer.Play(goldnikAura);
		}
		else
		{
			ParticlePlayer.Stop(goldnikAura);
		}
	}

	private void Update()
	{
		if (!Boosters.IsBoosterSelected(PowerUps.Type.Booster_GoldenEnemy) || null == Sonic.Tracker)
		{
			return;
		}
		ParticleSystem goldnikAura = Sonic.ParticleController.m_goldnikAura;
		if ((bool)m_nearestGoldenEnemy && m_nearestGoldenEnemy.TrackDistance > Sonic.Tracker.TrackPosition)
		{
			Vector3 vector = m_nearestGoldenEnemy.transform.position + m_nearestGoldenEnemy.getGoldernFlareLocation();
			Vector3 position = BehindCamera.Instance.Camera.transform.position;
			Vector3 vector2 = position - vector;
			vector2.y = 0f;
			vector2.Normalize();
			goldnikAura.transform.position = vector - vector2 * 0.5f;
			return;
		}
		m_nearestGoldenEnemy = GetNearestGoldenEnemy(Sonic.Tracker.TrackPosition);
		if (!(m_nearestGoldenEnemy != null))
		{
			return;
		}
		goldnikAura.gameObject.renderer.enabled = true;
		foreach (Transform item in goldnikAura.transform)
		{
			item.gameObject.renderer.enabled = true;
		}
		goldnikAura.transform.parent = m_nearestGoldenEnemy.transform;
		ParticlePlayer.Play(goldnikAura);
		Vector3 vector3 = m_nearestGoldenEnemy.transform.position + m_nearestGoldenEnemy.getGoldernFlareLocation();
		Vector3 position2 = BehindCamera.Instance.Camera.transform.position;
		Vector3 vector4 = position2 - vector3;
		vector4.y = 0f;
		vector4.Normalize();
		goldnikAura.transform.position = vector3 - vector4 * 0.5f;
	}
}
