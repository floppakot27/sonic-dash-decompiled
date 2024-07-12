using UnityEngine;

public class HudTriggers : MonoBehaviour
{
	private void Trigger_PauseGame()
	{
		GameState.RequestMode(GameState.Mode.PauseMenu);
	}

	private void Trigger_HeadStartActivated()
	{
		EventDispatch.GenerateEvent("HeadStartActivated", false);
	}

	private void Trigger_SuperHeadStartActivated()
	{
		EventDispatch.GenerateEvent("HeadStartActivated", true);
	}
}
