using UnityEngine;

public class GuiTriggerUtils
{
	public static UIPanel GetGuiTriggerPanel(GuiTrigger guiTrigger)
	{
		if (guiTrigger == null)
		{
			return null;
		}
		GameObject trigger = guiTrigger.Trigger;
		if (trigger == null)
		{
			return null;
		}
		UIPanel uIPanel = trigger.GetComponentInChildren<UIPanel>();
		if (uIPanel == null)
		{
			Transform parent = trigger.transform.parent;
			do
			{
				uIPanel = parent.GetComponent<UIPanel>();
				if (uIPanel != null)
				{
					break;
				}
				parent = parent.transform.parent;
			}
			while (parent != null);
		}
		return uIPanel;
	}
}
