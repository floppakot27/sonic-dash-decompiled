using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[AddComponentMenu("Dash/Spawnable Generator")]
[RequireComponent(typeof(SpawnPool))]
public class HazardGenerator : MonoBehaviour
{
	private LinkedList<SpawnableObject> m_activeSpawnables;

	private bool m_usePhysicCollision = true;

	public IEnumerable<SpawnableObject> ActiveSpawnables => m_activeSpawnables;

	protected virtual SpawnPool Pool
	{
		get
		{
			return null;
		}
		set
		{
		}
	}

	public virtual void Start()
	{
		WorldCollector.MarkAsMovable(base.gameObject);
		m_activeSpawnables = new LinkedList<SpawnableObject>();
		m_usePhysicCollision = FeatureSupport.IsSupported("Physics Based Collision");
	}

	public T GenerateSpawnable<T>(LightweightTransform atTransform, Track onTrack, Spline onSpline) where T : SpawnableObject
	{
		GameObject gameObject = GenerateSpawnable(typeof(T), atTransform, onTrack, onSpline);
		return gameObject.GetComponent<T>();
	}

	public GameObject GenerateSpawnable(Type spawnableType, LightweightTransform atTransform, Track onTrack, Spline onSpline)
	{
		foreach (Transform value in Pool.prefabs.Values)
		{
			if (value.GetComponent(spawnableType) == null)
			{
				continue;
			}
			return GenerateSpawnable(value, atTransform, onTrack, onSpline);
		}
		return null;
	}

	public GameObject GenerateDCPiece(int pieceNumber, LightweightTransform atTransform, Track onTrack, Spline onSpline)
	{
		foreach (Transform value in Pool.prefabs.Values)
		{
			Component component = value.GetComponent(typeof(DCPiece));
			if (component == null || ((DCPiece)component).PieceNumber != pieceNumber)
			{
				continue;
			}
			return GenerateSpawnable(value, atTransform, onTrack, onSpline);
		}
		return null;
	}

	public GameObject GenerateGCCollectable(LightweightTransform atTransform, Track onTrack, Spline onSpline)
	{
		int num = UnityEngine.Random.Range(0, GCCollectableGenerator.NumCollectables);
		foreach (Transform value in Pool.prefabs.Values)
		{
			Component component = value.GetComponent(typeof(GCCollectable));
			if (component == null || ((GCCollectable)component).m_collectableNumber != num)
			{
				continue;
			}
			return GenerateSpawnable(value, atTransform, onTrack, onSpline);
		}
		return null;
	}

	private GameObject GenerateSpawnable(Transform fromPrefab, LightweightTransform atTransform, Track onTrack, Spline onSpline)
	{
		GameObject gameObject = Pool.Spawn(fromPrefab, atTransform.Location, atTransform.Orientation * fromPrefab.rotation).gameObject;
		SpawnableObject component = gameObject.GetComponent<SpawnableObject>();
		component.Place(OnSpawnableDestruction, onTrack, onSpline);
		OnNewSpawnable(component);
		return gameObject;
	}

	public void OnStartGeneration()
	{
		DespawnEverything();
	}

	private void OnDestroy()
	{
		m_activeSpawnables.Clear();
	}

	private void DespawnEverything()
	{
		m_activeSpawnables.Clear();
		while (Pool.Count() > 0)
		{
			Transform instance = Pool[Pool.Count() - 1];
			DespawnInstance(instance);
		}
	}

	protected void DisableCollisionBoxes(SpawnableObject newSpawnable)
	{
		if (m_usePhysicCollision)
		{
			return;
		}
		BoxCollider[] componentsInChildren = newSpawnable.GetComponentsInChildren<BoxCollider>();
		BoxCollider[] array = componentsInChildren;
		foreach (BoxCollider boxCollider in array)
		{
			if (!boxCollider.isTrigger)
			{
				boxCollider.enabled = false;
			}
		}
	}

	protected void OnNewSpawnable(SpawnableObject newSpawnable)
	{
		m_activeSpawnables.AddLast(newSpawnable);
		DisableCollisionBoxes(newSpawnable);
	}

	protected void OnSpawnableDestruction(SpawnableObject spawnable)
	{
		if (m_activeSpawnables.Contains(spawnable))
		{
			m_activeSpawnables.Remove(spawnable);
			DespawnInstance(spawnable.transform);
		}
	}

	private void DespawnInstance(Transform instance)
	{
		Pool.Despawn(instance);
		instance.parent = base.transform;
	}
}
