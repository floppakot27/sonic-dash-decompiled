using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class BundleResource
{
	private struct Bundle
	{
		public uint m_bundleID;

		public AssetBundle m_bundle;

		public Bundle(uint bundleID, AssetBundle bundle)
		{
			m_bundleID = bundleID;
			m_bundle = bundle;
		}
	}

	private static List<Bundle> m_bundles;

	public static UnityEngine.Object AsyncAsset { get; private set; }

	public static int BundleCount
	{
		get
		{
			return (m_bundles != null) ? m_bundles.Count : 0;
		}
		set
		{
			InitialiseBundleList(value);
		}
	}

	public static void RegisterBundle(string bundleName, AssetBundle bundle)
	{
		bool flag = false;
		for (int i = 0; i < m_bundles.Count; i++)
		{
			if (m_bundles[i].m_bundleID == 0)
			{
				uint bundleID = CRC32.Generate(bundleName, CRC32.Case.Lower);
				Bundle value = new Bundle(bundleID, bundle);
				m_bundles[i] = value;
				flag = true;
				break;
			}
		}
	}

	public static void UnregisterBundle(string bundleName, bool deleteAllLoadedObjects)
	{
		if (m_bundles == null)
		{
			return;
		}
		uint num = CRC32.Generate(bundleName, CRC32.Case.Lower);
		for (int i = 0; i < m_bundles.Count; i++)
		{
			if (m_bundles[i].m_bundleID == num)
			{
				Debug.Log($"Unloading asset bundle {bundleName} with deleteAllLoadedObjects:{deleteAllLoadedObjects}");
				m_bundles[i].m_bundle.Unload(deleteAllLoadedObjects);
				Bundle value = new Bundle(0u, null);
				m_bundles[i] = value;
			}
		}
	}

	public static bool BundleRegistered(string bundleName)
	{
		if (m_bundles == null || m_bundles.Count == 0)
		{
			return false;
		}
		uint num = CRC32.Generate(bundleName, CRC32.Case.Lower);
		for (int i = 0; i < m_bundles.Count; i++)
		{
			if (m_bundles[i].m_bundleID == num)
			{
				return true;
			}
		}
		return false;
	}

	public static T Load<T>(string bundleName, string assetName) where T : UnityEngine.Object
	{
		return LoadFromBundle<T>(bundleName, assetName);
	}

	public static T[] LoadAll<T>(string bundleName) where T : UnityEngine.Object
	{
		return LoadAllFromBundle<T>(bundleName);
	}

	public static IEnumerator LoadAsync<T>(string bundleName, string assetName) where T : UnityEngine.Object
	{
		yield return LoadAsyncFromBundle<T>(bundleName, assetName);
	}

	public static List<string> GetBundleContent(string bundleName)
	{
		return GetBundleContentFromBundle(bundleName);
	}

	private static void InitialiseBundleList(int bundleListCount)
	{
		List<Bundle> list = new List<Bundle>(bundleListCount);
		for (int i = 0; i < list.Capacity; i++)
		{
			Bundle item = new Bundle(0u, null);
			list.Add(item);
		}
		if (m_bundles != null)
		{
			int num = Math.Min(list.Count, m_bundles.Count);
			for (int j = 0; j < num; j++)
			{
				list[j] = m_bundles[j];
			}
		}
		m_bundles = list;
	}

	private static AssetBundle FindAssetBundle(string bundleName)
	{
		uint num = CRC32.Generate(bundleName, CRC32.Case.Lower);
		for (int i = 0; i < m_bundles.Count; i++)
		{
			if (m_bundles[i].m_bundleID == num)
			{
				return m_bundles[i].m_bundle;
			}
		}
		return null;
	}

	public static T LoadFromBundle<T>(string bundleName, string assetName) where T : UnityEngine.Object
	{
		AssetBundle assetBundle = FindAssetBundle(bundleName);
		if (assetBundle == null)
		{
			return (T)null;
		}
		string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(assetName);
		return assetBundle.Load(fileNameWithoutExtension, typeof(T)) as T;
	}

	public static T[] LoadAllFromBundle<T>(string bundleName) where T : UnityEngine.Object
	{
		AssetBundle assetBundle = FindAssetBundle(bundleName);
		if (assetBundle == null)
		{
			return null;
		}
		return assetBundle.LoadAll(typeof(T)) as T[];
	}

	public static IEnumerator LoadAsyncFromBundle<T>(string bundleName, string assetName)
	{
		AssetBundle thisBundle = FindAssetBundle(bundleName);
		if (thisBundle != null)
		{
			string assetNameWithoutExtension = Path.GetFileNameWithoutExtension(assetName);
			AssetBundleRequest loadRequest = thisBundle.LoadAsync(assetNameWithoutExtension, typeof(T));
			while (!loadRequest.isDone)
			{
				yield return null;
			}
			AsyncAsset = loadRequest.asset;
		}
	}

	public static List<string> GetBundleContentFromBundle(string bundleName)
	{
		AssetBundle assetBundle = FindAssetBundle(bundleName);
		if (assetBundle == null)
		{
			return null;
		}
		StringCollection stringCollection = assetBundle.Load("bundlecontent", typeof(StringCollection)) as StringCollection;
		if (stringCollection == null)
		{
			return null;
		}
		return stringCollection.m_strings;
	}
}
