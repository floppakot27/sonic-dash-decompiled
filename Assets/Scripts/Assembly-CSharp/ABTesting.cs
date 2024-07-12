using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ABTesting : MonoBehaviour
{
	public enum Tests
	{
		RSR_MissionSetAmount,
		RSR_Brag,
		RSR_Return,
		RSR_Leader,
		RSR_Highscore,
		RSR_Facebook,
		RSR_RunsBefore1,
		RSR_Spread1,
		RSR_RunsBeforeNext,
		RSR_SpreadNext,
		RSR_MaxDaily,
		MSG_MissionBenefits,
		ADS_Revive
	}

	private const string PopertyActualCohort = "ActualCohort";

	private static ABTesting instance;

	private LSON.Root[] m_roots;

	private int m_actualCohort = -1;

	private int[] m_defaults;

	public static bool Ready { get; private set; }

	public static bool URLReady { get; set; }

	public static int Cohort => instance.m_actualCohort;

	public static int GetTestValue(Tests test)
	{
		if (instance.m_roots != null)
		{
			LSON.Root root = instance.m_roots[0];
			for (int i = 0; i < root.m_properties.Length; i++)
			{
				LSON.Property property = root.m_properties[i];
				if (!(property.m_name != test.ToString()))
				{
					int.TryParse(property.m_value, out var result);
					return result;
				}
			}
		}
		return instance.m_defaults[(int)test];
	}

	public static void Restart()
	{
		Ready = false;
		instance.SetDefaults();
		instance.StartCoroutine(instance.DownloadConfigFile());
		instance.StartCoroutine(instance.DownloadServerFile());
	}

	private void Start()
	{
		EventDispatch.RegisterInterest("OnGameDataLoaded", this);
		EventDispatch.RegisterInterest("OnGameDataSaveRequest", this);
		instance = this;
	}

	private IEnumerator DownloadConfigFile()
	{
		FileDownloader fDownloader = new FileDownloader(FileDownloader.Files.ABConfig, keepAndUseLocalCopy: true);
		yield return fDownloader.Loading;
		if (fDownloader.Error == null)
		{
			JsonParser parser = new JsonParser(fDownloader.Text);
			Dictionary<string, object> jsonObject = parser.Parse();
			int status = -1;
			object valStatus = null;
			if (jsonObject.TryGetValue("status", out valStatus) && (int)((double)valStatus + 0.1) == 0)
			{
				object param = null;
				if (jsonObject.TryGetValue("storeconfig", out param))
				{
					string storeconfig = (string)param;
					if (storeconfig != null)
					{
						FileDownloader.TweakABTestingURLs(FileDownloader.Files.StoreModifier, storeconfig);
					}
				}
				if (jsonObject.TryGetValue("abconfig", out param))
				{
					string abconfig = (string)param;
					if (abconfig != null)
					{
						FileDownloader.TweakABTestingURLs(FileDownloader.Files.ABTesting, abconfig);
					}
				}
				if (jsonObject.TryGetValue("cohort", out param))
				{
					int cohort = (int)((double)param + 0.1);
					if (m_actualCohort != cohort)
					{
						GameAnalytics.ABTestingCohortChange(m_actualCohort.ToString(), cohort.ToString());
						m_actualCohort = cohort;
					}
				}
			}
		}
		URLReady = true;
		StoreModifier.URLReady = true;
	}

	private IEnumerator DownloadServerFile()
	{
		while (!URLReady)
		{
			yield return null;
		}
		FileDownloader fDownloader = new FileDownloader(FileDownloader.Files.ABTesting, keepAndUseLocalCopy: true);
		yield return fDownloader.Loading;
		if (fDownloader.Error == null)
		{
			m_roots = LSONReader.Parse(fDownloader.Text);
			if (m_roots != null)
			{
				EventDispatch.GenerateEvent("ABTestingReady");
			}
		}
		Ready = true;
	}

	private void SetDefaults()
	{
		m_defaults = new int[Utils.GetEnumCount<Tests>()];
		m_defaults[0] = 1;
		m_defaults[1] = 1;
		m_defaults[2] = 1;
		m_defaults[3] = 1;
		m_defaults[4] = 1;
		m_defaults[5] = 1;
		m_defaults[11] = 1;
		m_defaults[6] = -1;
		m_defaults[7] = -1;
		m_defaults[8] = -1;
		m_defaults[9] = -1;
		m_defaults[10] = -1;
	}

	private void Event_OnGameDataSaveRequest()
	{
		PropertyStore.Store("ActualCohort", m_actualCohort);
	}

	private void Event_OnGameDataLoaded(ActiveProperties activeProperties)
	{
		if (activeProperties.DoesPropertyExist("ActualCohort"))
		{
			m_actualCohort = activeProperties.GetInt("ActualCohort");
		}
	}
}
