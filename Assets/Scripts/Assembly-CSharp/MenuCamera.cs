using UnityEngine;

public class MenuCamera : MonoBehaviour
{
	private enum CameraPath
	{
		Position,
		LookAt
	}

	[SerializeField]
	private iTweenPath m_initialPath;

	[SerializeField]
	private CameraTypeSpline m_menuCamera;

	[SerializeField]
	private float m_transitionTime = 1f;

	public float TransitionTime
	{
		set
		{
			m_transitionTime = value;
		}
	}

	public bool InTransition => m_menuCamera.InTransition;

	public void StartCameraTransition(iTweenPath transitionPath, iTween.EaseType positionEasyType, iTween.EaseType lookAtEasyType, CameraTypeSpline.Direction direction)
	{
		iTweenPath cameraPath = GetCameraPath(transitionPath, CameraPath.Position);
		iTweenPath cameraPath2 = GetCameraPath(transitionPath, CameraPath.LookAt);
		m_menuCamera.PositionPath = cameraPath;
		m_menuCamera.LookAtPath = cameraPath2;
		m_menuCamera.TransitionTime = m_transitionTime;
		m_menuCamera.PrepareForMovement(direction);
		m_menuCamera.StartMovement(direction, positionEasyType, lookAtEasyType);
	}

	private void Start()
	{
		EventDispatch.RegisterInterest("ResetGameState", this, EventDispatch.Priority.Lowest);
	}

	private static iTweenPath GetCameraPath(iTweenPath pathObject, CameraPath pathType)
	{
		iTweenPath[] components = pathObject.GetComponents<iTweenPath>();
		iTweenPath[] array = components;
		foreach (iTweenPath iTweenPath2 in array)
		{
			string text = iTweenPath2.pathName.ToLower();
			if (text.Contains("[position]") && pathType == CameraPath.Position)
			{
				return iTweenPath2;
			}
			if (text.Contains("[look at]") && pathType == CameraPath.LookAt)
			{
				return iTweenPath2;
			}
		}
		return null;
	}

	private void Event_ResetGameState(GameState.Mode resetState)
	{
		if (resetState == GameState.Mode.Menu && !(m_initialPath == null) && !(m_menuCamera == null))
		{
			iTweenPath cameraPath = GetCameraPath(m_initialPath, CameraPath.Position);
			iTweenPath cameraPath2 = GetCameraPath(m_initialPath, CameraPath.LookAt);
			m_menuCamera.PositionPath = cameraPath;
			m_menuCamera.LookAtPath = cameraPath2;
			m_menuCamera.PrepareForMovement(CameraTypeSpline.Direction.Forward);
			BehindCamera.Instance.SetActiveCamera(m_menuCamera, 0f);
		}
	}
}
