using UnityEngine;

public class MenuInfoDialogsPanel : MonoBehaviour
{
	private int m_currentEntry;

	private int m_infoDialogCount;

	private void Start()
	{
		m_currentEntry = 0;
		m_infoDialogCount = Utils.GetEnumCount<DialogContent_GeneralInfo.Type>();
	}

	private void Trigger_ShowNextDialog()
	{
		GuiTrigger guiTrigger = DialogStack.ShowDialog("General Info Dialog");
		DialogContent_GeneralInfo.Type currentEntry = (DialogContent_GeneralInfo.Type)m_currentEntry;
		Dialog_GeneralInfo componentInChildren = Utils.GetComponentInChildren<Dialog_GeneralInfo>(guiTrigger.Trigger.gameObject);
		componentInChildren.SetContent(currentEntry);
		m_currentEntry++;
		if (m_currentEntry >= m_infoDialogCount)
		{
			m_currentEntry = 0;
		}
	}
}
