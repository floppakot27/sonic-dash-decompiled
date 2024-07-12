using UnityEngine;

public class FrontCamera : GameCamera
{
	[SerializeField]
	private CameraType m_defaultCamera;

	[SerializeField]
	private CameraTypeTransitionTrackingObject m_transitionToBackLookAtCamera;

	[SerializeField]
	private float m_transitionTime = 1f;

	public static FrontCamera Instance { get; private set; }

	private void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
		}
	}

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

	public void TransitionToBackCamera(float duration, Transform lookAt)
	{
		m_transitionToBackLookAtCamera.BeginTransition(base.MainCamera.GetCurrentCameraType(), BehindCamera.Instance.CurrentCameraType, lookAt, duration);
		SetActiveCamera(m_transitionToBackLookAtCamera, 0.1f);
	}
}
