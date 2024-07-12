using UnityEngine;

public class MenuRevivePurchase : MonoBehaviour
{
	[SerializeField]
	private GuiTrigger m_continuePage;

	[SerializeField]
	private UILabel m_reviveTokensNeeded;

	public static int RevivesRequired { private get; set; }

	private void Start()
	{
	}

	private void OnEnable()
	{
		GameAnalytics.SetPurchaseLocation(GameAnalytics.PurchaseLocations.ContinueScreen);
		SetRequiredTokenDisplay();
		EventDispatch.RegisterInterest("OnStorePurchaseCompleted", this, EventDispatch.Priority.Lowest);
	}

	private void OnDisable()
	{
		EventDispatch.UnregisterInterest("OnStorePurchaseCompleted", this);
	}

	private void SetRequiredTokenDisplay()
	{
		string @string = LanguageStrings.First.GetString("DIALOG_CONTINUE_REVIVES_NEEDED_SINGLE");
		if (RevivesRequired > 1)
		{
			@string = LanguageStrings.First.GetString("DIALOG_CONTINUE_REVIVES_NEEDED_MULTIPLE");
		}
		string text = string.Format(@string, RevivesRequired);
		m_reviveTokensNeeded.text = text;
	}

	private void Trigger_CancelPurchase()
	{
		GameAnalytics.ContinueCancelled(GameAnalytics.CancelContinueReasons.CancelPurchase);
		EventDispatch.GenerateEvent("OnContinueGameCancel");
	}

	private void Event_OnStorePurchaseCompleted(StoreContent.StoreEntry thisEntry, StorePurchases.Result result)
	{
		if (thisEntry != null)
		{
			int powerUpCount = PowerUpsInventory.GetPowerUpCount(PowerUps.Type.Respawn);
			if (powerUpCount >= RevivesRequired)
			{
				GameAnalytics.ContinueUsed(thisEntry.m_identifier);
				EventDispatch.GenerateEvent("OnContinueGameOk", false);
			}
		}
	}
}
