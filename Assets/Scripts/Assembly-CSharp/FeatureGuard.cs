using UnityEngine;

public class FeatureGuard : MonoBehaviour
{
	[SerializeField]
	private string m_featureToGuard;

	private void Start()
	{
		if (!FeatureSupport.IsSupported(m_featureToGuard))
		{
			Object.Destroy(base.gameObject);
		}
	}
}
