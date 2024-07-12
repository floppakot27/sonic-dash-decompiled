using UnityEngine;

public class SideCamera : GameCamera
{
	[SerializeField]
	private CameraType m_defaultCamera;

	[SerializeField]
	private float m_transitionTime = 1f;

	private void Start()
	{
		CacheMainCamera();
	}

	private void OnEnable()
	{
		if ((bool)m_defaultCamera)
		{
			SetActiveCamera(m_defaultCamera, m_transitionTime);
		}
	}
}
