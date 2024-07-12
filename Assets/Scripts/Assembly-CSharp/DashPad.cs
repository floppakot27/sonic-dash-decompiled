using UnityEngine;

[AddComponentMenu("Dash/Track/DashPad")]
public class DashPad : SpawnableObject
{
	private void OnSpawned()
	{
		base.collider.enabled = false;
	}
}
