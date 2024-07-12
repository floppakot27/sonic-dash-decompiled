using UnityEngine;

public class Enemy_Shadow : MonoBehaviour
{
	public float m_shadowSize = 2f;

	public float m_negativeAngleForZeroAlpha = 60f;

	private Transform m_mainCameraTransform;

	private void Start()
	{
		if (!FeatureSupport.IsSupported("Enemy Shadow"))
		{
			Object.Destroy(base.gameObject);
			return;
		}
		CameraTypeMain cameraTypeMain = Object.FindObjectOfType(typeof(CameraTypeMain)) as CameraTypeMain;
		m_mainCameraTransform = cameraTypeMain.transform;
		base.gameObject.transform.localScale = Vector3.one * m_shadowSize;
	}

	private void Update()
	{
		Transform parent = base.gameObject.transform.parent;
		Vector3 position = parent.position;
		if (m_mainCameraTransform != null)
		{
			Vector3 vector = m_mainCameraTransform.position - position;
			vector.Normalize();
			base.gameObject.transform.position = position + vector * 0.5f;
		}
	}
}
