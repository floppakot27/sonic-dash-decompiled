using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackMerger : MonoBehaviour
{
	public static TrackMerger s_instance;

	public TrackMerger()
	{
		s_instance = this;
	}

	public IEnumerator performStaticBatching()
	{
		if (!FeatureSupport.IsSupported("Static Batching"))
		{
			yield break;
		}
		GameObject pieces = GameObject.Find("Pieces");
		Transform rootTransform = SonicSplineTracker.FindRootTransform();
		MeshFilter[] meshFilters = pieces.GetComponentsInChildren<MeshFilter>();
		List<GameObject> batchableGameObjects = new List<GameObject>(meshFilters.Length);
		foreach (MeshFilter filter in meshFilters)
		{
			if (!(filter.gameObject.tag == "MovableShadow") && !(filter.gameObject.tag == "Dynamic"))
			{
				Vector3 scale = filter.gameObject.transform.lossyScale;
				if (Mathf.Approximately(scale.x, scale.y) && Mathf.Approximately(scale.y, scale.z) && (bool)filter.renderer && (bool)filter.gameObject.renderer.sharedMaterial && (bool)filter.sharedMesh)
				{
					batchableGameObjects.Add(filter.gameObject);
				}
			}
		}
		yield return null;
		StaticBatchingUtility.Combine(batchableGameObjects.ToArray(), rootTransform.gameObject);
	}
}
