using UnityEngine;

public class DashParticleTwizzler : MonoBehaviour
{
	private void Start()
	{
	}

	private void Update()
	{
		base.transform.Rotate(base.transform.forward, Time.deltaTime * 200f * (0f - base.transform.right.y));
	}
}
