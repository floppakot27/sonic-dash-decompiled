using UnityEngine;

public class ShoreLine : MonoBehaviour
{
	private void Start()
	{
		base.gameObject.layer = LayerMask.NameToLayer("shoreline");
		if (!FeatureSupport.IsSupported("Shore Line"))
		{
			Object.Destroy(base.gameObject);
		}
	}
}
