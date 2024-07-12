using System;
using System.Collections;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

public class FileDownloader
{
	public enum Files
	{
		FeatureState,
		SegaTime,
		GC3,
		OfferState,
		ABConfig,
		ABTesting,
		StoreModifier
	}

	private const float DefaultTimeout = 15f;

	private const string DefaultABTesting = "abtesting_Default.lson";

	private const string DefaultStoreModifier = "storemodifier_Default.lson";

	private const string FileIDString = "FileIdentifier:";

	private const string FileFlavour = "Cupcake";

	private static string[] s_urls = new string[7] { "http://www.hardlightstudio.com/sd/sdstate.lson", "http://tp.sega.com", "https://s3.amazonaws.com/sonicdash/global+challenges/gc3progress.txt", "http://54.227.247.235:8080/SonicDash/GetOffer.hl", "http://54.227.247.235:8080/SonicDash/GetConfig.hl", "https://s3.amazonaws.com/sonicdash/ab/production/abtesting_Default.lson", "https://s3.amazonaws.com/sonicdash/ab/production/storemodifier_Default.lson" };

	private static GameObject s_auxObject;

	private static MonoBehaviour s_auxBehaviour;

	public string Text { get; private set; }

	public string Error { get; private set; }

	public Coroutine Loading { get; private set; }

	public FileDownloader(Files file, bool keepAndUseLocalCopy)
	{
		if (s_auxBehaviour == null)
		{
			s_auxObject = new GameObject();
			s_auxObject.name = "Aux Object For FileDownloader";
			s_auxObject.AddComponent<MonoBehaviour>();
			s_auxBehaviour = s_auxObject.GetComponent<MonoBehaviour>();
		}
		Loading = s_auxBehaviour.StartCoroutine(DownloadServerFile(file, keepAndUseLocalCopy));
	}

	public static void TweakABTestingURLs(Files file, string fileName)
	{
		string text = s_urls[(int)file];
		switch (file)
		{
		case Files.ABTesting:
			text = text.Replace("abtesting_Default.lson", fileName);
			break;
		case Files.StoreModifier:
			text = text.Replace("storemodifier_Default.lson", fileName);
			break;
		}
		s_urls[(int)file] = text;
	}

	private IEnumerator DownloadServerFile(Files file, bool loadLocal)
	{
		string finalUrl = GetFinalURL(file);
		WWW www = new WWW(finalUrl);
		float timeTaken = 0f;
		bool timeOut = false;
		while (!timeOut && !www.isDone)
		{
			timeTaken += Time.deltaTime;
			if (timeTaken > 15f)
			{
				www.Dispose();
				timeOut = true;
			}
			yield return null;
		}
		Error = ((!timeOut) ? www.error : "FileDownloader timed out");
		if (!timeOut && www.error == null)
		{
			Text = www.text;
			Error = null;
			if (loadLocal)
			{
				SaveLocalCopy(file);
			}
		}
		else
		{
			Text = null;
			if (loadLocal)
			{
				LoadLocalCopy(file);
			}
		}
		GameAnalytics.NotifyFileDownload(file, timeTaken);
	}

	private string GetFinalURL(Files file)
	{
		string text = s_urls[(int)file];
		int num = UnityEngine.Random.Range(0, 1234);
		text += $"?random={num}";
		if (file == Files.OfferState || file == Files.ABConfig)
		{
			text = text + "&mid=" + UserIdentification.Current;
		}
		if (file == Files.ABConfig)
		{
			text = text + "&new=" + (PlayerStats.GetStat(PlayerStats.StatNames.TimePlayed_Total) < 100).ToString().ToLower();
		}
		return text;
	}

	private void SaveLocalCopy(Files file)
	{
		string text = Text + "FileIdentifier:" + CRC32.Generate(Text + "Cupcake" + file, CRC32.Case.AsIs);
		string filePath = GetFilePath(file);
		File.WriteAllText(filePath, ConvertStringToHex(text));
	}

	private void LoadLocalCopy(Files file)
	{
		string filePath = GetFilePath(file);
		if (File.Exists(filePath))
		{
			string input = ConvertHexToString(File.ReadAllText(filePath));
			string[] array = Regex.Split(input, "FileIdentifier:");
			if (array.Length != 2)
			{
				Error = "Couldn't find CRC of local file.";
			}
			else if (array[1] == CRC32.Generate(array[0] + "Cupcake" + file, CRC32.Case.AsIs).ToString())
			{
				Text = array[0];
				Error = null;
			}
			else
			{
				Error = "Wrong CRC of local file.";
			}
		}
	}

	private string GetFilePath(Files file)
	{
		return string.Format("{0}/{1}", Application.persistentDataPath, CRC32.Generate(file.ToString() + "Cupcake", CRC32.Case.AsIs));
	}

	private string ConvertStringToHex(string text)
	{
		string text2 = string.Empty;
		for (int i = 0; i < text.Length; i++)
		{
			text2 += $"{Convert.ToUInt32(((int)text[i]).ToString()):x2}";
		}
		return text2;
	}

	private string ConvertHexToString(string hexText)
	{
		string text = string.Empty;
		while (hexText.Length > 0)
		{
			text += Convert.ToChar(Convert.ToUInt32(hexText.Substring(0, 2), 16));
			hexText = hexText.Substring(2, hexText.Length - 2);
		}
		return text;
	}
}
