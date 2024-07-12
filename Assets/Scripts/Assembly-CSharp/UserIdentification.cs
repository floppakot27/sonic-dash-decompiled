using UnityEngine;

public class UserIdentification : MonoBehaviour
{
	private static string s_playerId = string.Empty;

	public static string Current => s_playerId;

	private void Start()
	{
		s_playerId = CurrentDeviceIdentifier();
	}

	private static string CurrentDeviceIdentifier()
	{
		AndroidJavaClass androidJavaClass = new AndroidJavaClass("com.hardlightstudio.dev.sonicdash.plugin.SLGlobal");
		return androidJavaClass.CallStatic<string>("GetDeviceID", new object[0]);
	}
}
