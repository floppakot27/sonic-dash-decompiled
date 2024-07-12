using UnityEngine;

public class ShowDialogProperties : MonoBehaviour
{
	[SerializeField]
	private GuiTrigger m_dialogToShow;

	public GuiTrigger DialogToShow => m_dialogToShow;
}
