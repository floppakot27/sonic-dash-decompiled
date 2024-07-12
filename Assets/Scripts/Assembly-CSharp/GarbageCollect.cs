using System;
using UnityEngine;

internal class GarbageCollect : MonoBehaviour
{
	public int frameFreq = 60;

	private void Update()
	{
		if (Time.frameCount % frameFreq == 0)
		{
			GC.Collect();
		}
	}
}
