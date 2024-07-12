using System;
using UnityEngine;
using System.Collections.Generic;

public class SpawnPoolDynamic : SpawnPool
{
	[Serializable]
	public class PrefabDesc
	{
		public string Name;
		public int PreloadAmount;
		public bool LimitInstances;
		public int LimitAmount;
		public bool LimitFIFO;
		public bool CullDespawned;
		public bool LogMessages;
	}

	[SerializeField]
	public string Path;
	[SerializeField]
	private List<SpawnPoolDynamic.PrefabDesc> PrefabDescriptions;
}
