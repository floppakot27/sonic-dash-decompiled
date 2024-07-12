using UnityEngine;

public class GameAnalyticsLocationUpdate : MonoBehaviour
{
	[SerializeField]
	public GameAnalytics.PurchaseLocations m_location;

	private void OnEnable()
	{
		if ((GameAnalytics.GetPurchaseLocation() == GameAnalytics.PurchaseLocations.ResultScreen && m_location == GameAnalytics.PurchaseLocations.MissionsMainMenu) || (GameAnalytics.GetPurchaseLocation() == GameAnalytics.PurchaseLocations.DCScreenResultScreen && m_location == GameAnalytics.PurchaseLocations.MissionsMainMenu) || (GameAnalytics.GetPurchaseLocation() == GameAnalytics.PurchaseLocations.WOFScreenResultScreen && m_location == GameAnalytics.PurchaseLocations.MissionsMainMenu) || (GameAnalytics.GetPurchaseLocation() == GameAnalytics.PurchaseLocations.Plus_MissionsMenuResultScreen && m_location == GameAnalytics.PurchaseLocations.MissionsMainMenu))
		{
			GameAnalytics.SetPurchaseLocation(GameAnalytics.PurchaseLocations.MissionsMenuResultScreen);
		}
		else if ((GameAnalytics.GetPurchaseLocation() == GameAnalytics.PurchaseLocations.ResultScreen && m_location == GameAnalytics.PurchaseLocations.WOFScreen) || (GameAnalytics.GetPurchaseLocation() == GameAnalytics.PurchaseLocations.MissionsMenuResultScreen && m_location == GameAnalytics.PurchaseLocations.WOFScreen) || (GameAnalytics.GetPurchaseLocation() == GameAnalytics.PurchaseLocations.DCScreenResultScreen && m_location == GameAnalytics.PurchaseLocations.WOFScreen) || (GameAnalytics.GetPurchaseLocation() == GameAnalytics.PurchaseLocations.Plus_WOFScreenResultScreen && m_location == GameAnalytics.PurchaseLocations.WOFScreen))
		{
			GameAnalytics.SetPurchaseLocation(GameAnalytics.PurchaseLocations.WOFScreenResultScreen);
		}
		else if ((GameAnalytics.GetPurchaseLocation() == GameAnalytics.PurchaseLocations.ResultScreen && m_location == GameAnalytics.PurchaseLocations.DCScreen) || (GameAnalytics.GetPurchaseLocation() == GameAnalytics.PurchaseLocations.MissionsMenuResultScreen && m_location == GameAnalytics.PurchaseLocations.DCScreen) || (GameAnalytics.GetPurchaseLocation() == GameAnalytics.PurchaseLocations.WOFScreenResultScreen && m_location == GameAnalytics.PurchaseLocations.DCScreen) || (GameAnalytics.GetPurchaseLocation() == GameAnalytics.PurchaseLocations.Plus_DCScreenResultScreen && m_location == GameAnalytics.PurchaseLocations.DCScreen))
		{
			GameAnalytics.SetPurchaseLocation(GameAnalytics.PurchaseLocations.DCScreenResultScreen);
		}
		else
		{
			GameAnalytics.SetPurchaseLocation(m_location);
		}
	}
}
