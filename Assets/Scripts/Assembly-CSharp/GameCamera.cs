using UnityEngine;

public class GameCamera : MonoBehaviour
{
	public Camera Camera { get; private set; }

	public CameraTypeMain MainCamera { get; private set; }

	public CameraType CurrentCameraType { get; private set; }

	public float CurrentTransitionTime { get; set; }

	protected void CacheMainCamera()
	{
		GameObject gameObject = GameObject.FindGameObjectWithTag("GameCamera");
		MainCamera = gameObject.GetComponent<CameraTypeMain>();
		Camera = MainCamera.GetComponentInChildren<Camera>();
	}

	public void SetActiveCamera(CameraType newCamera, float transitionTime)
	{
		if ((bool)newCamera)
		{
			CurrentCameraType = newCamera;
			CurrentTransitionTime = transitionTime;
		}
	}

	private void Update()
	{
		if (MainCamera.GetCurrentCameraType() != CurrentCameraType)
		{
			MainCamera.SetCamera(CurrentCameraType, CurrentTransitionTime);
		}
	}
}
