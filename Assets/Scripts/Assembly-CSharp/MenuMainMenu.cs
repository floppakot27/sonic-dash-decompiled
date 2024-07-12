using UnityEngine;

public class MenuMainMenu : MonoBehaviour
{
	private void OnEnable()
	{
		EventDispatch.GenerateEvent("MainMenuActive");
		CloudStorage.Sync();
		if (OfferState.CanDisplay())
		{
			OfferState.RegisterDisplay();
		}
	}
}
