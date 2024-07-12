using UnityEngine;

public class DialogContent_GeneralInfo : MonoBehaviour
{
	public enum Type
	{
		FailedPurchase,
		VideoAdsNotAvailable,
		GameOffersNotAvailable,
		UnableToProcessPayment,
		SegaIdAlreadyRegistered,
		NotEnoughRings,
		RestorePurchaseFailed,
		RestorePurchaseNothing,
		GlobalChallengeNoConnection,
		ReceiptVerificationFailed,
		LoggedOutOfFacebook,
		WallpaperDownloadFail,
		WallpaperDownloadSuccess
	}

	public class Content
	{
		public readonly string m_titleLoc;

		public readonly string m_infoLoc;

		public Content(string title, string info)
		{
			m_titleLoc = title;
			m_infoLoc = info;
		}
	}

	private static Content[] s_dialogContent;

	static DialogContent_GeneralInfo()
	{
		s_dialogContent = new Content[13]
		{
			new Content("PURCHASE_FAILED", "PURCHASE_FAILED_INFO"),
			new Content("STORE_SPONSORED_VIDEO", "STORE_NO_VIDEOS"),
			new Content("MENU_GAME_OFFERS", "STORE_NO_OFFERS"),
			new Content("OFFER_FAILED", "OFFER_FAILED_INFO"),
			new Content("SEGAID_ALREADY_REGISTERED", "SEGAID_ALREADY_REGISTERED_INFO"),
			new Content("NOT_ENOUGH_RINGS", "NOT_ENOUGH_RINGS_INFO"),
			new Content("RESTORE_PURCHASES", "RESTORE_PURCHASES_ERROR"),
			new Content("RESTORE_PURCHASES", "RESTORE_PURCHASES_NOTHING_RESTORED"),
			new Content("GLOBAL_CHALLENGE_TITLE", "DAILY_CHALLENGE_WAITING_FOR_CONNECTION"),
			new Content("DIALOG_RECEIPT_VERIFICATION_TITLE", "DIALOG_RECEIPT_VERIFICATION_BODY"),
			new Content("OPTIONS_FACEBOOK_TITLE", "OPTIONS_FACEBOOK_CONFIRMATION_LOGOUT"),
			new Content("WALLPAPERS_DOWNLOADED_FAIL_TITLE", "WALLPAPERS_DOWNLOADED_FAIL_BODY"),
			new Content("WALLPAPERS_DOWNLOADED_SUCCESS_TITLE", "WALLPAPERS_DOWNLOADED_SUCCESS_BODY")
		};
	}

	public static Content GetContent(Type dialogType)
	{
		return s_dialogContent[(int)dialogType];
	}

	public static void Display(Type type)
	{
		GuiTrigger guiTrigger = DialogStack.ShowDialog("General Info Dialog");
		Dialog_GeneralInfo componentInChildren = Utils.GetComponentInChildren<Dialog_GeneralInfo>(guiTrigger.Trigger.gameObject);
		componentInChildren.SetContent(type);
	}
}
