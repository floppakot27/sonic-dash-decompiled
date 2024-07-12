using UnityEngine;

public class GCCompleteDisplay : MonoBehaviour
{
	[SerializeField]
	private MeshFilter m_prizeMesh;

	private void OnEnable()
	{
		StoreContent.StoreEntry storeEntry = StoreContent.GetStoreEntry(GC3Progress.TierRewards[GC3Progress.TierRewards.Length - 1], StoreContent.Identifiers.Name);
		m_prizeMesh.mesh = storeEntry.m_mesh;
	}
}
