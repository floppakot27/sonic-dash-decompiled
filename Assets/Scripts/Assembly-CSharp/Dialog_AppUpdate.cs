using UnityEngine;

public class Dialog_AppUpdate : MonoBehaviour
{
	public static void Display()
	{
		DialogStack.ShowDialog("App Update Dialog");
	}

	private void Trigger_UpdateToNewVersion()
	{
		DialogContent_AppUpdate.Instance.Trigger_UpdateToNewVersion();
	}

	private void Trigger_DoNotUpdate()
	{
		DialogContent_AppUpdate.Instance.Trigger_DoNotUpdate();
	}
}
