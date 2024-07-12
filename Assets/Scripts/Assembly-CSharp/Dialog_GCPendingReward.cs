using UnityEngine;

public class Dialog_GCPendingReward : MonoBehaviour
{
	private static Dialog_GCPendingReward instance;

	[SerializeField]
	private MoveToPageProperties m_properties;

	private iTweenPath m_storedPath;

	public static void Display()
	{
		if (GameState.GetMode() != 0)
		{
			instance.m_properties.TransitionPath = null;
		}
		else
		{
			instance.m_properties.TransitionPath = instance.m_storedPath;
		}
		DialogStack.ShowDialog("GC Pending Reward Dialog");
	}

	private void Start()
	{
		instance = this;
		m_storedPath = m_properties.TransitionPath;
	}
}
