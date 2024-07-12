using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class BundleState : MonoBehaviour
{
	public struct Bundle
	{
		public string m_name;

		public int m_version;

		public Bundle(string name, int version)
		{
			m_name = name;
			m_version = version;
		}
	}

	private const string FileName = "bundles.txt";

	private List<Bundle> m_pendingBundles;

	public List<Bundle> GetValidLocalBundles(out string bundleVersion)
	{
		bundleVersion = string.Empty;
		LSON.Root[] array = LoadBundleState();
		if (array == null)
		{
			return null;
		}
		LSON.Property[] properties = LSONProperties.GetProperties(array, "version");
		if (properties == null)
		{
			return null;
		}
		LSON.Property property = LSONProperties.GetProperty(properties, "number");
		if (property == null)
		{
			return null;
		}
		string stringValue = null;
		if (!LSONProperties.AsString(property, out stringValue))
		{
			return null;
		}
		VersionIdentifiers.VersionStatus versionStatus = VersionIdentifiers.CheckVersionNumbers("1.8.0", stringValue);
		if (versionStatus == VersionIdentifiers.VersionStatus.Lower)
		{
			Caching.CleanCache();
		}
		if (versionStatus != 0)
		{
			return null;
		}
		List<Bundle> list = ExtractBundleInformation(array);
		if (list == null || list.Count == 0)
		{
			return null;
		}
		bundleVersion = stringValue;
		return list;
	}

	public void SaveBundleState()
	{
		if (m_pendingBundles != null && m_pendingBundles.Count != 0)
		{
			LSON.Root[] array = new LSON.Root[m_pendingBundles.Count + 1];
			array[0] = new LSON.Root();
			array[0].m_name = "version";
			array[0].m_properties = new LSON.Property[1];
			array[0].m_properties[0] = new LSON.Property();
			array[0].m_properties[0].m_name = "number";
			array[0].m_properties[0].m_value = "1.8.0";
			for (int i = 0; i < m_pendingBundles.Count; i++)
			{
				int num = i + 1;
				array[num] = new LSON.Root();
				array[num].m_name = "bundle";
				array[num].m_properties = new LSON.Property[2];
				array[num].m_properties[0] = new LSON.Property();
				array[num].m_properties[1] = new LSON.Property();
				array[num].m_properties[0].m_name = "name";
				array[num].m_properties[1].m_name = "version";
				array[num].m_properties[0].m_value = m_pendingBundles[i].m_name;
				array[num].m_properties[1].m_value = m_pendingBundles[i].m_version.ToString();
			}
			string contents = LSONWriter.Write(array);
			string filePath = GetFilePath("bundles.txt");
			File.WriteAllText(filePath, contents);
		}
	}

	public void RegisterBundle(string bundleName, int version)
	{
		if (m_pendingBundles == null)
		{
			m_pendingBundles = new List<Bundle>();
		}
		m_pendingBundles.Add(new Bundle(bundleName, version));
	}

	private LSON.Root[] LoadBundleState()
	{
		string filePath = GetFilePath("bundles.txt");
		if (!File.Exists(filePath))
		{
			return null;
		}
		string fileContent = File.ReadAllText(filePath);
		return LSONReader.Parse(fileContent);
	}

	private List<Bundle> ExtractBundleInformation(LSON.Root[] currentBundleState)
	{
		List<Bundle> list = new List<Bundle>();
		for (int i = 0; i < currentBundleState.Length; i++)
		{
			string text = currentBundleState[i].m_name.ToLowerInvariant();
			if (text != "bundle")
			{
				continue;
			}
			LSON.Property property = LSONProperties.GetProperty(currentBundleState[i].m_properties, "name");
			LSON.Property property2 = LSONProperties.GetProperty(currentBundleState[i].m_properties, "version");
			if (property == null || property2 == null)
			{
				continue;
			}
			string stringValue = null;
			if (LSONProperties.AsString(property, out stringValue) && !string.IsNullOrEmpty(stringValue))
			{
				int intValue = 0;
				if (LSONProperties.AsInt(property2, out intValue))
				{
					list.Add(new Bundle(stringValue, intValue));
				}
			}
		}
		return list;
	}

	private static string GetFilePath(string filename)
	{
		return $"{Application.persistentDataPath}/{filename}";
	}
}
