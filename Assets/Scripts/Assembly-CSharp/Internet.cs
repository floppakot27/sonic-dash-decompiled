using UnityEngine;

public class Internet : MonoBehaviour
{
	public static bool ConnectionAvailable()
	{
		return CheckNetworkConnection();
	}

	public static bool CheckNetworkConnection()
	{
		bool result = false;
		AndroidJavaClass androidJavaClass = new AndroidJavaClass("com.hardlightstudio.dev.sonicdash.plugin.SLGlobal");
		if (androidJavaClass.CallStatic<bool>("IsConnected", new object[0]))
		{
			result = true;
		}
		return result;
	}
}
