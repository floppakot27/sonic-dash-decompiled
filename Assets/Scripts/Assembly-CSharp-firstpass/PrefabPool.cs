using System;
using UnityEngine;

[Serializable]
public class PrefabPool
{
	public Transform prefab;
	public int preloadAmount;
	public bool limitInstances;
	public int limitAmount;
	public bool limitFIFO;
	public bool cullDespawned;
	public int cullAbove;
	public int cullDelay;
	public int cullMaxPerPass;
	public bool _logMessages;
}
