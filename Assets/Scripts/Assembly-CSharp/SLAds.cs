using UnityEngine;

public class SLAds
{
	private static AndroidJavaClass m_SLAds = new AndroidJavaClass("com.hardlightstudio.dev.sonicdash.plugin.ads.SLAdsInterface");

	public static void Init(int interstitalProvider, AdProvider bannerProvider, AdProvider moreGamesProvider, AdProvider offersProvider, AdProvider gameOffersProvider, AdProvider pushProvider, AdProvider videoProvider, AdProvider loginProvider)
	{
		m_SLAds.CallStatic("AdsInit", interstitalProvider, (int)bannerProvider, (int)moreGamesProvider, (int)offersProvider, (int)gameOffersProvider, (int)pushProvider, (int)videoProvider, (int)loginProvider);
	}

	public static void RequestDateFirstSeen()
	{
		m_SLAds.CallStatic("AdsRequestFirstSeen");
	}

	public static bool IsMoreGamesAvailable()
	{
		return m_SLAds.CallStatic<bool>("AdsIsMoreGamesAvailable", new object[0]);
	}

	public static bool IsGameOffersAvailable()
	{
		return m_SLAds.CallStatic<bool>("AdsIsGameOffersAvailable", new object[0]);
	}

	public static void Update()
	{
		m_SLAds.CallStatic("AdsUpdate");
	}

	public static void ShowGameOffers()
	{
		m_SLAds.CallStatic("AdsShowGameOffers");
	}

	public static void ShowOffer(string PlacementID)
	{
		m_SLAds.CallStatic("AdsShowOffers", PlacementID);
	}

	public static void EndOffer(string PlacementID = "")
	{
		m_SLAds.CallStatic("AdsEndOffer", PlacementID);
	}

	public static bool IsVideoAvailable()
	{
		return m_SLAds.CallStatic<bool>("AdsIsVideoAvailable", new object[0]);
	}

	public static bool IsVideoReady()
	{
		return m_SLAds.CallStatic<bool>("AdsIsVideoReady", new object[0]);
	}

	public static bool IsLoginAvailable()
	{
		return m_SLAds.CallStatic<bool>("AdsIsLoginAvailable", new object[0]);
	}

	public static void ShowVideoAd()
	{
		m_SLAds.CallStatic("AdsShowVideoAd");
	}

	public static void PrepareVideoAd()
	{
		m_SLAds.CallStatic("AdsPrepareVideoAd");
	}

	public static void ShowIntersitialAd(string Placement)
	{
		m_SLAds.CallStatic("AdsShowIntersitialAd", Placement);
	}

	public static void ShowA4P(bool CupCakeTasty)
	{
		m_SLAds.CallStatic("AdsShowA4P");
	}

	public static void PrepareA4P(bool CupCakeTasty)
	{
		m_SLAds.CallStatic("AdsPrepareA4P");
	}

	public static void SetVideoAdRewardID(string RewardID)
	{
		m_SLAds.CallStatic("AdsSetVideoAdRewardID", RewardID);
	}

	public static void ShowMoreGames()
	{
		m_SLAds.CallStatic("AdsShowMoreGames");
	}

	public static void AddPushNotificationTag(string Tag)
	{
		m_SLAds.CallStatic("AdsAddPushNotificationTag", Tag);
	}

	public static void UpdatePushNotificationRegistration()
	{
		m_SLAds.CallStatic("AdsUpdatePushNotificationRegistration");
	}

	public static void SetProviderRatio(AdProvider Provider, int Percent)
	{
		m_SLAds.CallStatic("AdsSetProviderRatio", (int)Provider, Percent);
	}

	public static void SetVungleInitialPlays(int vunglePlays)
	{
		m_SLAds.CallStatic("AdsSetVungleInitialPlays", vunglePlays);
	}
}
