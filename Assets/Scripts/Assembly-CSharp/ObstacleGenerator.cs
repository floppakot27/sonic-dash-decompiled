using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SpawnPoolDynamic))]
public class ObstacleGenerator : HazardGenerator
{
	private SpawnPoolDynamic[] PoolDynamic;

	private int m_subzoneIndex = -1;

	public SpawnPool SpawnPool => Pool;

	protected override SpawnPool Pool => PoolDynamic[m_subzoneIndex].Pool;

	private void Awake()
	{
		PoolDynamic = GetComponents<SpawnPoolDynamic>();
	}

	public IEnumerator OnStartGeneration(int newSubzoneIndex)
	{
		if (m_subzoneIndex != newSubzoneIndex)
		{
			if (m_subzoneIndex != -1)
			{
				yield return StartCoroutine(PoolDynamic[m_subzoneIndex].Unload());
			}
			m_subzoneIndex = newSubzoneIndex;
			yield return StartCoroutine(PoolDynamic[m_subzoneIndex].Load());
		}
		OnStartGeneration();
	}

	public GameObject GenerateObstacle(Transform prefab, LightweightTransform transform, Track track, Spline spline)
	{
		Quaternion rot = transform.Orientation * prefab.transform.rotation;
		Transform transform2 = Pool.Spawn(prefab, transform.Location, rot);
		Obstacle component = transform2.GetComponent<Obstacle>();
		component.Place(base.OnSpawnableDestruction, track, spline);
		OnNewSpawnable(component);
		return transform2.gameObject;
	}
}
