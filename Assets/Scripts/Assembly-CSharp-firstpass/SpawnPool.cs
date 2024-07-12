using UnityEngine;
using System.Collections.Generic;

public class SpawnPool : MonoBehaviour
{
	public string poolName;
	public bool matchPoolScale;
	public bool matchPoolLayer;
	public bool dontDestroyOnLoad;
	public bool forceDestroyOnDespawn;
	public bool logMessages;
	public List<PrefabPool> _perPrefabPoolOptions;
	public float maxParticleDespawnTime;
}
