using UnityEngine;

public class AdvertStates : MonoBehaviour
{
	private const string StateRoot = "adisplay";

	private const string AEnableProperty = "aenable";

	private const string ALimitProperty = "alimit";

	private const string APeriodProperty = "alimitperiod";

	private const string MMRateProperty = "mm_rate";

	private const string ResultRateProperty = "result_rate";

	private const string FlurryPercentProperty = "flurry_percent";

	private const string ChartboostPercentProperty = "chartboost_percent";

	private const string BurstlyPercentProperty = "burstly_percent";

	private const string VunglePercentProperty = "vungle_percent";

	private const string VungleInitialPlaysProperty = "vungle_initial_plays";

	private const int DefaultLimit = 60;

	private const int DefaultPeriod = 1;

	private const int DefaultMMRate = 2;

	private const int DefaultResult = 3;

	private const int DefaultFlurryPercent = 10;

	private const int DefaultChartboostPercent = 50;

	private const int DefaultBurstlyPercent = 0;

	private const int DefaultVunglePercent = 40;

	private int VungleInitialPlays;

	private static bool s_adsEnabled = true;

	private static int s_adsLimit;

	private static int s_adsLimitPeriod;

	private static int s_adsMainMenuRate;

	private static int s_adsResultScreenRate;

	private static AdvertStates s_state;

	private bool m_adsRationSet;

	public static bool AdsEnabled => s_adsEnabled;

	public static int AdsLimit => s_adsLimit;

	public static int AdsLimitPeriod => s_adsLimitPeriod;

	public static int AdsMainMenuRate => s_adsMainMenuRate;

	public static int AdsResultScreenRate => s_adsResultScreenRate;

	private void Start()
	{
		s_state = this;
		EventDispatch.RegisterInterest("FeatureStateReady", this);
		EventDispatch.RegisterInterest("OnAllAssetsLoaded", this, EventDispatch.Priority.Lowest);
		if (FeatureState.Ready)
		{
			GetFeatureState();
		}
	}

	private bool GetAdState(LSON.Property thisProperty)
	{
		if (thisProperty == null)
		{
			return true;
		}
		bool boolValue = false;
		return !LSONProperties.AsBool(thisProperty, out boolValue) || boolValue;
	}

	private void Event_FeatureStateReady()
	{
		GetFeatureState();
	}

	private void GetFeatureState()
	{
		LSON.Property stateProperty = FeatureState.GetStateProperty("adisplay", "aenable");
		s_adsEnabled = GetAdState(stateProperty);
		if (s_adsEnabled)
		{
			bool flag = false;
			LSON.Property stateProperty2 = FeatureState.GetStateProperty("adisplay", "alimit");
			if (stateProperty2 != null)
			{
				flag = LSONProperties.AsInt(stateProperty2, out s_adsLimit);
			}
			if (!flag)
			{
				s_adsLimit = 60;
			}
			flag = false;
			stateProperty2 = FeatureState.GetStateProperty("adisplay", "alimitperiod");
			if (stateProperty2 != null)
			{
				flag = LSONProperties.AsInt(stateProperty2, out s_adsLimitPeriod);
			}
			if (!flag)
			{
				s_adsLimitPeriod = 1;
			}
			flag = false;
			stateProperty2 = FeatureState.GetStateProperty("adisplay", "mm_rate");
			if (stateProperty2 != null)
			{
				flag = LSONProperties.AsInt(stateProperty2, out s_adsMainMenuRate);
			}
			if (!flag)
			{
				s_adsMainMenuRate = 2;
			}
			flag = false;
			stateProperty2 = FeatureState.GetStateProperty("adisplay", "result_rate");
			if (stateProperty2 != null)
			{
				flag = LSONProperties.AsInt(stateProperty2, out s_adsResultScreenRate);
			}
			if (!flag)
			{
				s_adsResultScreenRate = 3;
			}
		}
	}

	private void Event_OnAllAssetsLoaded()
	{
		if (!m_adsRationSet)
		{
			int intValue = 0;
			bool flag = false;
			LSON.Property stateProperty = FeatureState.GetStateProperty("adisplay", "flurry_percent");
			if (stateProperty != null)
			{
				flag = LSONProperties.AsInt(stateProperty, out intValue);
			}
			if (!flag)
			{
				intValue = 10;
			}
			SLAds.SetProviderRatio(AdProvider.FlurryAds, intValue);
			flag = false;
			stateProperty = FeatureState.GetStateProperty("adisplay", "chartboost_percent");
			if (stateProperty != null)
			{
				flag = LSONProperties.AsInt(stateProperty, out intValue);
			}
			if (!flag)
			{
				intValue = 50;
			}
			SLAds.SetProviderRatio(AdProvider.Chartboost, intValue);
			flag = false;
			stateProperty = FeatureState.GetStateProperty("adisplay", "burstly_percent");
			if (stateProperty != null)
			{
				flag = LSONProperties.AsInt(stateProperty, out intValue);
			}
			if (!flag)
			{
				intValue = 0;
			}
			SLAds.SetProviderRatio(AdProvider.Burstly, intValue);
			flag = false;
			stateProperty = FeatureState.GetStateProperty("adisplay", "vungle_percent");
			if (stateProperty != null)
			{
				flag = LSONProperties.AsInt(stateProperty, out intValue);
			}
			if (!flag)
			{
				intValue = 40;
			}
			SLAds.SetProviderRatio(AdProvider.Vungle, intValue);
			flag = false;
			stateProperty = FeatureState.GetStateProperty("adisplay", "vungle_initial_plays");
			if (stateProperty != null)
			{
				flag = LSONProperties.AsInt(stateProperty, out VungleInitialPlays);
			}
			if (!flag)
			{
				VungleInitialPlays = 0;
			}
			SLAds.SetVungleInitialPlays(VungleInitialPlays);
			m_adsRationSet = true;
		}
	}
}
