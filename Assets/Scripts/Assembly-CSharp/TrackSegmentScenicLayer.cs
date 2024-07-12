using System;
using System.Collections.Generic;
using UnityEngine;

public class TrackSegmentScenicLayer : MonoBehaviour
{
	[SerializeField]
	private List<string> m_optionalScenicNames;

	private Transform ScenicInstance { get; set; }

	private SpawnPool ScenicsPool { get; set; }

	private TrackSegment TrackSegment { get; set; }

	private void Awake()
	{
		TrackSegment = GetComponent<TrackSegment>();
	}

	private string GetLayer(System.Random rng)
	{
		if (m_optionalScenicNames == null || m_optionalScenicNames.Count == 0)
		{
			return null;
		}
		if (!AllowScenicDisplay(rng))
		{
			return null;
		}
		int index = rng.Next(0, m_optionalScenicNames.Count);
		return m_optionalScenicNames[index];
	}

	public void InstanceScenicLayer(SpawnPool pool, System.Random rng)
	{
		string layer = GetLayer(rng);
		if (layer == null)
		{
			return;
		}
		GameObject prefab = pool.GetPrefab(layer);
		ScenicsPool = pool;
		ScenicInstance = pool.Spawn(prefab.transform);
		if (ScenicInstance != null)
		{
			ScenicInstance.parent = base.transform;
			ScenicInstance.localPosition = Vector3.zero;
			ScenicInstance.rotation = base.transform.rotation;
			if (TrackSegment != null)
			{
				TrackSegment.SetGameplayEnabledOn(ScenicInstance);
			}
		}
	}

	private void OnDespawned()
	{
		if (!(ScenicInstance == null))
		{
			ScenicInstance.parent = ScenicsPool.transform;
			ScenicsPool.Despawn(ScenicInstance);
		}
	}

	private bool AllowScenicDisplay(System.Random rng)
	{
		return !FeatureSupport.IsSupported("FavourMemoryOverTrackGenSpeed") || rng.Next(3) < 2;
	}
}
