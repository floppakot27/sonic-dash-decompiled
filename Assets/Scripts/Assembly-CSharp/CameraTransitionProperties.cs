using UnityEngine;

public class CameraTransitionProperties : MonoBehaviour
{
	[SerializeField]
	private iTweenPath m_transitionPath;

	[SerializeField]
	private float m_transitionTime = 3f;

	public iTweenPath TransitionPath => m_transitionPath;

	public float TransitionTime => m_transitionTime;
}
