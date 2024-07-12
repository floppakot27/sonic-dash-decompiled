using UnityEngine;

public class SLWebView
{
	public static void OpenWebWindow(string urlStr)
	{
		AndroidJavaClass androidJavaClass = new AndroidJavaClass("com.hardlightstudio.dev.sonicdash.plugin.SLGlobal");
		androidJavaClass.CallStatic("OpenURL", urlStr);
	}
}
