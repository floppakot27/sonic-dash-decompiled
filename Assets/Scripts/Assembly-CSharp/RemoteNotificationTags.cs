using UnityEngine;

public class RemoteNotificationTags : MonoBehaviour
{
	private bool m_firstTime = true;

	private string m_tagEngagedUser = "Engaged_User";

	private string m_tagLongTermUser = "Long_Term_User";

	private string m_tagCasualIAPUser = "Casual_IAP_User";

	private string m_tagCoreIAPUser = "Core_IAP_User";

	private string m_tagBoughtCharacter = "Bought_Character";

	private string m_tagPaidUser = "Paid User";

	private string m_tagFreeUser = "Free User";

	private void Start()
	{
		EventDispatch.RegisterInterest("OnAllAssetsLoaded", this);
	}

	private void Event_OnAllAssetsLoaded()
	{
		if (!m_firstTime)
		{
			return;
		}
		PlayerStats.Stats currentStats = PlayerStats.GetCurrentStats();
		if (currentStats.m_trackedStats[79] >= 3)
		{
			SLAds.AddPushNotificationTag(m_tagEngagedUser);
		}
		if (currentStats.m_trackedStats[79] >= 5)
		{
			SLAds.AddPushNotificationTag(m_tagLongTermUser);
		}
		if (currentStats.m_trackedStats[73] > 0)
		{
			SLAds.AddPushNotificationTag(m_tagCasualIAPUser);
		}
		if (currentStats.m_trackedStats[73] >= 3)
		{
			SLAds.AddPushNotificationTag(m_tagCoreIAPUser);
		}
		bool flag = false;
		for (int i = 0; i < Utils.GetEnumCount<Characters.Type>(); i++)
		{
			if (Characters.CharacterUnlocked((Characters.Type)i) && i != 0)
			{
				flag = true;
			}
		}
		if (flag)
		{
			SLAds.AddPushNotificationTag(m_tagBoughtCharacter);
		}
		if (PaidUser.Paid)
		{
			SLAds.AddPushNotificationTag(m_tagPaidUser);
		}
		else
		{
			SLAds.AddPushNotificationTag(m_tagFreeUser);
		}
		SLAds.UpdatePushNotificationRegistration();
		m_firstTime = false;
	}
}
