using UnityEngine;

public class WaterConfiguration : MonoBehaviour
{
	public void Start()
	{
		bool flag = FeatureSupport.IsSupported("Reef Scene");
		if (flag)
		{
			Water component = base.gameObject.GetComponent<Water>();
			if ((bool)component)
			{
				component.m_WaterMode = ((!flag) ? Water.WaterMode.Reflective : Water.WaterMode.Refractive);
			}
		}
		if (Application.isPlaying)
		{
			Object.Destroy(this);
		}
	}
}
