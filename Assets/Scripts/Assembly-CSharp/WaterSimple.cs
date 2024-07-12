using UnityEngine;

public class WaterSimple : MonoBehaviour
{
	private void Awake()
	{
		WorldCollector.MarkAsMovable(base.gameObject);
	}
}
