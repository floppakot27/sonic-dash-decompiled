using UnityEngine;

public class HLSocialPluginAndroid
{
	private static AndroidJavaClass m_SLSocialInterfaceClass = new AndroidJavaClass("com.hardlightstudio.dev.sonicdash.plugin.social.SLSocialInterface");

	private HLSocialPluginAndroid()
	{
	}

	public static void OnAppGainedFocus()
	{
	}

	public static void OnAppTerminate()
	{
	}

	public static void Initialise(long facebookID)
	{
		string text = facebookID.ToString();
		m_SLSocialInterfaceClass.CallStatic("Initialise", text);
	}

	public static void Authenticate()
	{
		m_SLSocialInterfaceClass.CallStatic("Authenticate");
	}

	public static void RequestFacebookAccess()
	{
		m_SLSocialInterfaceClass.CallStatic("RequestFacebookAccess");
	}

	public static void Logout()
	{
		m_SLSocialInterfaceClass.CallStatic("FacebookLogOut");
	}

	public static bool IsGooglePlusSignedIn()
	{
		return m_SLSocialInterfaceClass.CallStatic<bool>("IsSignedIntoGooglePlus", new object[0]);
	}

	public static void RequestGooglePlusAccess()
	{
		m_SLSocialInterfaceClass.CallStatic("RequestGooglePlusAccess");
	}

	public static void ViewAchievements()
	{
		if (!IsGooglePlusSignedIn())
		{
			RequestGooglePlusAccess();
		}
		else
		{
			m_SLSocialInterfaceClass.CallStatic("ViewAchievements");
		}
	}

	public static void RefreshLeaderboard(string leaderboardID, int count)
	{
		m_SLSocialInterfaceClass.CallStatic("RefreshLeaderboard", leaderboardID, count);
	}

	public static void RefreshLeaderboardFinished(string leaderboardID)
	{
		m_SLSocialInterfaceClass.CallStatic("RefreshLeaderboardFinished", leaderboardID);
	}

	public static void SendScore(string leaderboardID, long score)
	{
		m_SLSocialInterfaceClass.CallStatic("SendScore", leaderboardID, score);
	}

	public static void InviteFriend(string userID, string heading, string message)
	{
		m_SLSocialInterfaceClass.CallStatic("InviteFriend", userID, heading, message);
	}

	public static bool LoginWillRequireAppSwitch()
	{
		return false;
	}

	public static void LoadAchievements()
	{
		m_SLSocialInterfaceClass.CallStatic("LoadAchievements");
	}

	public static int GetAchievementProgress(string id)
	{
		return m_SLSocialInterfaceClass.CallStatic<int>("GetAchievementProgress", new object[1] { id });
	}

	public static void UpdateAchievement(string id, double progress)
	{
		m_SLSocialInterfaceClass.CallStatic("SetAchievementProgress", id, (int)progress);
	}
}
