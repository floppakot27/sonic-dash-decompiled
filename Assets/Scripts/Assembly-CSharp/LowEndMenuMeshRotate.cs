using UnityEngine;

public class LowEndMenuMeshRotate : MenuMeshRotate
{
	private new void Start()
	{
		base.Start();
		if (FeatureSupport.IsLowEndDevice())
		{
			Object.Destroy(this);
		}
	}

	private void Update()
	{
		Spin();
	}
}
