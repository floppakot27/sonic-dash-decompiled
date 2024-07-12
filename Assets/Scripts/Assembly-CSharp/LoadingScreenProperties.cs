using UnityEngine;

public class LoadingScreenProperties : MonoBehaviour
{
	public enum ScreenType
	{
		SegaLogo,
		HLLogo,
		SonicLoading
	}

	[SerializeField]
	public int m_screenOrder;

	[SerializeField]
	public float m_displayTime = 1f;

	[SerializeField]
	public float m_transitionTime = 0.5f;

	[SerializeField]
	public ScreenType m_screenType;

	public int ScreenOrder => m_screenOrder;

	public float DisplayTime => m_displayTime;

	public float TransitionTime => m_transitionTime;
}
