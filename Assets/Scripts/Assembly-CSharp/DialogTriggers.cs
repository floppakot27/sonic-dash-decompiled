using UnityEngine;

public class DialogTriggers : MonoBehaviour
{
	private void Trigger_ShowDialog(GameObject callerObject)
	{
		if (callerObject != null)
		{
			ShowDialogProperties component = callerObject.GetComponent<ShowDialogProperties>();
			if (component != null)
			{
				DialogStack.ShowDialog(component.DialogToShow);
			}
		}
	}

	private void Trigger_HideDialog(GameObject callerObject)
	{
		DialogStack.HideDialog();
	}
}
