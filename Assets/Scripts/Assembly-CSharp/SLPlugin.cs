using System.IO;
using UnityEngine;

public class SLPlugin
{
	public static void Init()
	{
	}

	public static void OnActivate()
	{
	}

	public static void OnDeactivate()
	{
	}

	public static void SetToken(string TokenName, string TokenValue)
	{
		AndroidJavaClass androidJavaClass = new AndroidJavaClass("com.hardlightstudio.dev.sonicdash.plugin.TokenMap");
		androidJavaClass.CallStatic("AddToken", TokenName, TokenValue);
	}

	public static bool IsCupCakeTasty()
	{
		return false;
	}

	public static bool IsNetworkConnected()
	{
		AndroidJavaClass androidJavaClass = new AndroidJavaClass("com.hardlightstudio.dev.sonicdash.plugin.SLGlobal");
		return androidJavaClass.CallStatic<bool>("IsConnected", new object[0]);
	}

	public static void DisplayUIAlertView(string Title, string Message, string btnTitle)
	{
		AndroidJavaClass androidJavaClass = new AndroidJavaClass("com.hardlightstudio.dev.sonicdash.plugin.SLAlertView");
		androidJavaClass.CallStatic("Display", Title, Message, btnTitle);
	}

	public static void OpenRatePage()
	{
		SLWebView.OpenWebWindow("http://play.google.com/store/apps/details?id=com.sega.sonicdash");
	}

	public static void SaveTextureToPhotoAlbum(Texture2D textureToSave, string wallpaperName)
	{
		AndroidJavaClass androidJavaClass = new AndroidJavaClass("com.hardlightstudio.dev.sonicdash.plugin.SLGlobal");
		string text = androidJavaClass.CallStatic<string>("FindWallpaperSaveDir", new object[0]);
		string text2 = null;
		string text3 = wallpaperName.Replace("_ipad", string.Empty).Trim() + ".png";
		if (text != null)
		{
			text2 = text + text3;
			byte[] bytes = textureToSave.EncodeToPNG();
			File.WriteAllBytes(text2, bytes);
		}
		androidJavaClass.CallStatic("UpdateWallpaperMedia", text2);
	}
}
