using UnityEngine;

[RequireComponent(typeof(SpawnPool))]
public class MiscGenerator : HazardGenerator
{
	protected override SpawnPool Pool { get; set; }

	private void Awake()
	{
		Pool = GetComponent<SpawnPool>();
	}
}
