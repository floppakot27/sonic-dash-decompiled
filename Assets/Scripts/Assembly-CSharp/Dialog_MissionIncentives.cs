using UnityEngine;

public class Dialog_MissionIncentives : MonoBehaviour
{
	public static void Display()
	{
		DialogStack.ShowDialog("Mission Incentives");
	}

	private void Trigger_DismissMissionIncentives()
	{
		DialogStack.HideDialog();
		Dialog_MissionsComplete.Display();
	}
}
