using UnityEngine;

public class PaidUser : MonoBehaviour
{
	private const string PaidUserSaveProperty = "User Is Paid";

	private const string RemovedAdsSaveProperty = "AdsRemoved";

	private static bool s_paidUser = true;

	private static bool s_removedAds;

	private bool m_skipLoad = true;

	public static bool Paid => s_paidUser;

	public static bool RemovedAds
	{
		get
		{
			return s_removedAds;
		}
		set
		{
			s_removedAds = value;
		}
	}

	private void Start()
	{
		EventDispatch.RegisterInterest("OnGameDataLoaded", this);
		EventDispatch.RegisterInterest("OnGameDataSaveRequest", this);
	}

	private void Event_OnGameDataLoaded(ActiveProperties activeProperties)
	{
		if (m_skipLoad)
		{
			m_skipLoad = false;
			return;
		}
		if (activeProperties.DoesPropertyExist("User Is Paid"))
		{
			s_paidUser = activeProperties.GetBool("User Is Paid");
		}
		else
		{
			bool flag = false;
			string[] array = new string[4] { "TimePlayed_Total", "Score", "Banked Rings Total", "Star Rings Total" };
			foreach (string propertyName in array)
			{
				if (activeProperties.DoesPropertyExist(propertyName))
				{
					flag = true;
				}
			}
			s_paidUser = flag;
		}
		s_removedAds = activeProperties.GetBool("AdsRemoved");
	}

	private void Event_OnGameDataSaveRequest()
	{
		ActiveProperties activeProperties = PropertyStore.ActiveProperties();
		if (!activeProperties.DoesPropertyExist("User Is Paid"))
		{
			PropertyStore.Store("User Is Paid", s_paidUser);
		}
		PropertyStore.Store("AdsRemoved", s_removedAds);
	}
}
