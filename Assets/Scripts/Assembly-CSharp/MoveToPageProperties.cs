using UnityEngine;

public class MoveToPageProperties : MonoBehaviour
{
	[SerializeField]
	private GuiTrigger m_destinationPage;

	[SerializeField]
	private iTweenPath m_transitionPath;

	[SerializeField]
	private bool m_transitionPathInReverse;

	[SerializeField]
	private bool m_replaceCurrentPage;

	public GuiTrigger DestinationPage => m_destinationPage;

	public iTweenPath TransitionPath
	{
		get
		{
			return m_transitionPath;
		}
		set
		{
			m_transitionPath = value;
		}
	}

	public bool TransitionPathInReverse => m_transitionPathInReverse;

	public bool ReplaceCurrentPage => m_replaceCurrentPage;
}
