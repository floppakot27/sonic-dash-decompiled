using UnityEngine;

public class BundleLocations : MonoBehaviour
{
	private const string DownloadLocation = "https://s3.amazonaws.com/sonicdash/asset bundles";

	private const string SpaceReplacement = "+";

	public static string GetVersionedPath(string assetName, string extension)
	{
		return GetReferencedPath(assetName, extension, "1.8.0");
	}

	public static string GetReferencedPath(string assetName, string extension, string folder)
	{
		string platformIdentifier = GetPlatformIdentifier();
		string arg = $"{assetName}-{platformIdentifier}.{extension}";
		string text = string.Format("{0}/{1}/{2}", "https://s3.amazonaws.com/sonicdash/asset bundles", folder, arg);
		return text.Replace(" ", "+").ToLowerInvariant();
	}

	private static string GetPlatformIdentifier()
	{
		string result = string.Empty;
		if (Application.platform == RuntimePlatform.IPhonePlayer)
		{
			result = "iphone";
		}
		else if (Application.platform == RuntimePlatform.Android)
		{
			result = "android";
		}
		else if (Application.platform == RuntimePlatform.WP8Player)
		{
			result = "wp8";
		}
		else if (Application.platform == RuntimePlatform.WindowsPlayer)
		{
			result = "standalonewindows";
		}
		return result;
	}
}
