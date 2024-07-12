using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

public class DCs : MonoBehaviour
{
	private const int m_maxChallengeDays = 5;

	private const int m_numberOfPieces = 4;

	private const string m_propertyDCLastDateCompleted = "CDLastDateCompleted";

	private const string m_propertyDCTrustedLastDateCompleted = "CDTrustedLastDateCompleted";

	private const string m_propertyDCCurrentDate = "CDCurrentDate";

	private const string m_propertyDCTrustedCurrentDate = "CDTrustedCurrentDate";

	private const string m_propertyDCDayNumber = "CDDayNumber";

	private const string m_propertyDCChallengeRewarded = "CDChallengeRewarded";

	private const string m_propertyDCCompleted = "CDCompleted";

	private const string m_propertyDCActive = "CDActive";

	private static DCs s_singleton;

	[SerializeField]
	private string m_magneticTarget;

	[SerializeField]
	private AnimationCurve m_chanceChallengeSpringDay1;

	[SerializeField]
	private AnimationCurve m_chanceIncreasePerRunDay1;

	[SerializeField]
	private AnimationCurve m_chanceMaxDay1;

	[SerializeField]
	private AnimationCurve m_chanceChallengeSpringDay2;

	[SerializeField]
	private AnimationCurve m_chanceIncreasePerRunDay2;

	[SerializeField]
	private AnimationCurve m_chanceMaxDay2;

	[SerializeField]
	private AnimationCurve m_chanceChallengeSpringDay3;

	[SerializeField]
	private AnimationCurve m_chanceIncreasePerRunDay3;

	[SerializeField]
	private AnimationCurve m_chanceMaxDay3;

	[SerializeField]
	private AnimationCurve m_chanceChallengeSpringDay4;

	[SerializeField]
	private AnimationCurve m_chanceIncreasePerRunDay4;

	[SerializeField]
	private AnimationCurve m_chanceMaxDay4;

	[SerializeField]
	private AnimationCurve m_chanceChallengeSpringDay5;

	[SerializeField]
	private AnimationCurve m_chanceIncreasePerRunDay5;

	[SerializeField]
	private AnimationCurve m_chanceMaxDay5;

	private bool m_spawnChallengePiece;

	private int m_challengeDay;

	private bool m_challengeCompleted;

	private bool m_challengeCompletedToSave;

	private bool m_challengeActive = true;

	private bool m_challengeActiveToSave = true;

	private DateTime m_lastCompletedChallengeDate;

	private bool m_trustedLastCompletedChallengeDate;

	private DateTime m_currentChallengeDate;

	private bool m_trustedCurrentChallengeDate;

	private bool[] m_piecesCollected = new bool[4];

	private bool[] m_piecesCollectedToSave = new bool[4];

	private GameObject m_spawnedPiece;

	private double m_timePullingPiece;

	private double m_previousDistanceToPiece;

	private DCTimeValidation m_DCTimeValidation;

	private static string[] s_propertyCDPieces = new string[4] { "CDPiece1", "CDPiece2", "CDPiece3", "CDPiece4" };

	public static bool ChallengeRewarded { get; set; }

	public static int NumberOfPeices => s_singleton.m_piecesCollected.Length;

	public static bool ChallengeSpringCreated { get; set; }

	public static bool IsChallengeCompleted()
	{
		return s_singleton.m_challengeCompleted;
	}

	public static int GetCurrentDayNumber()
	{
		s_singleton.m_DCTimeValidation.EnforceValidTime(save: true);
		s_singleton.CheckData();
		if (s_singleton.m_challengeDay < 0)
		{
			return 0;
		}
		return s_singleton.m_challengeDay % 5;
	}

	public static int GetInternalDayNumber()
	{
		return s_singleton.m_challengeDay;
	}

	public static bool[] GetPieces()
	{
		return s_singleton.m_piecesCollected;
	}

	public static float GetSecondsRemaining()
	{
		DateTime date = s_singleton.m_currentChallengeDate.AddDays(1.0).Date;
		TimeSpan timeSpan = date - DCTime.GetCurrentTime();
		if (timeSpan.TotalSeconds <= 0.0)
		{
			return 0f;
		}
		return (float)timeSpan.TotalSeconds;
	}

	public static bool DaySkipped()
	{
		DateTime date = s_singleton.m_currentChallengeDate.AddDays(1.0).Date;
		if ((DCTime.GetCurrentTime() - date).TotalDays >= 1.0)
		{
			return true;
		}
		return false;
	}

	public static void ChangeDay()
	{
		if (!s_singleton.m_challengeCompleted || ChallengeRewarded)
		{
			if (!s_singleton.m_challengeCompleted || DaySkipped())
			{
				GameAnalytics.DCReset();
				PlayerStats.GetCurrentStats().m_trackedStats[82] = 0;
				s_singleton.m_challengeDay = 0;
			}
			else
			{
				s_singleton.m_lastCompletedChallengeDate = s_singleton.m_currentChallengeDate;
				s_singleton.m_trustedLastCompletedChallengeDate = s_singleton.m_trustedCurrentChallengeDate;
				s_singleton.m_challengeDay++;
			}
			s_singleton.m_currentChallengeDate = DCTime.GetCurrentTime();
			s_singleton.m_trustedCurrentChallengeDate = DCTimeValidation.TrustedTime;
			s_singleton.m_challengeCompleted = false;
			s_singleton.m_challengeCompletedToSave = false;
			s_singleton.m_challengeActive = true;
			s_singleton.m_challengeActiveToSave = true;
			ChallengeRewarded = false;
			s_singleton.ResetPiecesCollected(collected: false);
		}
	}

	public static void EnforceDCActualDate()
	{
		if (DCTimeValidation.TrustedTime && !s_singleton.m_trustedCurrentChallengeDate)
		{
			s_singleton.m_currentChallengeDate = DCTime.GetCurrentTime();
			s_singleton.m_trustedCurrentChallengeDate = DCTimeValidation.TrustedTime;
		}
	}

	public static void SetToCheatingState(bool save)
	{
		s_singleton.m_challengeDay = -1;
		if (DCTimeValidation.TrustedTime)
		{
			s_singleton.m_currentChallengeDate = DCTime.GetCurrentTime();
			s_singleton.m_trustedCurrentChallengeDate = DCTimeValidation.TrustedTime;
		}
		s_singleton.m_challengeCompleted = true;
		s_singleton.m_challengeCompletedToSave = true;
		s_singleton.m_challengeActive = false;
		s_singleton.m_challengeActiveToSave = false;
		ChallengeRewarded = true;
		s_singleton.ResetPiecesCollected(collected: true);
		if (save)
		{
			PropertyStore.Save();
		}
	}

	public static bool GetSpawnChallengePiece()
	{
		return s_singleton.m_spawnChallengePiece;
	}

	public static void SetChallengePieceSpawn(bool spawn)
	{
		if (s_singleton.m_challengeActive && spawn)
		{
			s_singleton.m_spawnChallengePiece = true;
		}
		else
		{
			s_singleton.m_spawnChallengePiece = false;
		}
	}

	public static int GetNextPieceNumber()
	{
		List<int> list = new List<int>();
		for (int i = 0; i < 4; i++)
		{
			if (!s_singleton.m_piecesCollected[i])
			{
				list.Add(i);
			}
		}
		if (list.Count == 0)
		{
			return 0;
		}
		return list[UnityEngine.Random.Range(0, list.Count - 1)];
	}

	public static void CollectPiece(int number)
	{
		s_singleton.m_piecesCollected[number] = true;
		ParticleSystem componentInChildren = s_singleton.m_spawnedPiece.GetComponentInChildren<ParticleSystem>();
		componentInChildren.Stop();
		componentInChildren.Clear();
		s_singleton.m_spawnedPiece = null;
		EventDispatch.GenerateEvent("OnDCPieceCollected", number);
		Sonic.RenderManager.playDCPieceCollectedParticles();
		if (AllPiecesCollected())
		{
			CompleteChallenge();
		}
	}

	public static bool AllPiecesCollected()
	{
		bool[] piecesCollected = s_singleton.m_piecesCollected;
		for (int i = 0; i < piecesCollected.Length; i++)
		{
			if (!piecesCollected[i])
			{
				return false;
			}
		}
		return true;
	}

	public static void SetSpawnedPiece(GameObject piece)
	{
		s_singleton.m_spawnedPiece = piece;
	}

	public static void PreparePiecesToSave(bool normal)
	{
		if (normal)
		{
			s_singleton.m_challengeCompletedToSave = s_singleton.m_challengeCompleted;
			s_singleton.m_challengeActiveToSave = s_singleton.m_challengeActive;
			for (int i = 0; i < 4; i++)
			{
				s_singleton.m_piecesCollectedToSave[i] = s_singleton.m_piecesCollected[i];
			}
		}
		else
		{
			s_singleton.m_challengeCompleted = s_singleton.m_challengeCompletedToSave;
			s_singleton.m_challengeActive = s_singleton.m_challengeActiveToSave;
			for (int j = 0; j < 4; j++)
			{
				s_singleton.m_piecesCollected[j] = s_singleton.m_piecesCollectedToSave[j];
			}
		}
	}

	public static float GetChallengeSpringChance(float distance)
	{
		if (ChallengeSpringCreated)
		{
			return 0f;
		}
		if (IsChallengeCompleted())
		{
			return 0f;
		}
		if (s_singleton.NumPiecesCollected() == 3 && s_singleton.m_spawnChallengePiece)
		{
			return 0f;
		}
		float time = distance / 10000f;
		float num = PlayerStats.GetCurrentStats().m_trackedStats[1];
		switch (s_singleton.m_challengeDay)
		{
		case 0:
		{
			float num5 = s_singleton.m_chanceChallengeSpringDay1.Evaluate(time);
			float num6 = num5 + num * s_singleton.m_chanceIncreasePerRunDay1.Evaluate(time);
			float num7 = s_singleton.m_chanceMaxDay1.Evaluate(time);
			return (!(num6 > num7)) ? num6 : num7;
		}
		case 1:
		{
			float num11 = s_singleton.m_chanceChallengeSpringDay2.Evaluate(time);
			float num12 = num11 + num * s_singleton.m_chanceIncreasePerRunDay2.Evaluate(time);
			float num13 = s_singleton.m_chanceMaxDay2.Evaluate(time);
			return (!(num12 > num13)) ? num12 : num13;
		}
		case 2:
		{
			float num8 = s_singleton.m_chanceChallengeSpringDay3.Evaluate(time);
			float num9 = num8 + num * s_singleton.m_chanceIncreasePerRunDay3.Evaluate(time);
			float num10 = s_singleton.m_chanceMaxDay3.Evaluate(time);
			return (!(num9 > num10)) ? num9 : num10;
		}
		case 3:
		{
			float num14 = s_singleton.m_chanceChallengeSpringDay4.Evaluate(time);
			float num15 = num14 + num * s_singleton.m_chanceIncreasePerRunDay4.Evaluate(time);
			float num16 = s_singleton.m_chanceMaxDay4.Evaluate(time);
			return (!(num15 > num16)) ? num15 : num16;
		}
		default:
		{
			float num2 = s_singleton.m_chanceChallengeSpringDay5.Evaluate(time);
			float num3 = num2 + num * s_singleton.m_chanceIncreasePerRunDay5.Evaluate(time);
			float num4 = s_singleton.m_chanceMaxDay5.Evaluate(time);
			return (!(num3 > num4)) ? num3 : num4;
		}
		}
	}

	private void Start()
	{
		s_singleton = this;
		EventDispatch.RegisterInterest("OnGameDataLoaded", this);
		EventDispatch.RegisterInterest("OnGameDataSaveRequest", this);
		EventDispatch.RegisterInterest("OnNewGameStarted", this);
		EventDispatch.RegisterInterest("OnGameFinished", this);
		EventDispatch.RegisterInterest("OnDCRewarded", this);
		m_DCTimeValidation = new DCTimeValidation();
	}

	private void Update()
	{
		if (m_spawnedPiece == null)
		{
			m_timePullingPiece = 0.0;
			m_previousDistanceToPiece = 9999.0;
			return;
		}
		double num = Math.Cos(Time.realtimeSinceStartup * 2f);
		m_spawnedPiece.transform.localPosition = m_spawnedPiece.transform.localPosition + (float)num * Time.deltaTime * Vector3.up;
		float num2 = 35f;
		float num3 = num2 * num2;
		float num4 = 0.25f;
		Vector3 position = Sonic.Bones[m_magneticTarget].transform.position;
		float sqrMagnitude = (position - m_spawnedPiece.transform.position).sqrMagnitude;
		if (sqrMagnitude < num3)
		{
			float num5 = (float)(m_timePullingPiece * (double)num4 + (double)(1f / sqrMagnitude));
			m_timePullingPiece += Time.deltaTime;
			m_spawnedPiece.transform.position = Vector3.Lerp(m_spawnedPiece.transform.position, position, num5);
			if (num5 >= 1f)
			{
				m_spawnedPiece.GetComponent<DCPiece>().notifyCollection();
			}
			else if ((double)sqrMagnitude > m_previousDistanceToPiece)
			{
				m_spawnedPiece.GetComponent<DCPiece>().notifyCollection();
			}
			else
			{
				m_previousDistanceToPiece = sqrMagnitude;
			}
		}
	}

	private static void CompleteChallenge()
	{
		s_singleton.m_challengeCompleted = true;
		s_singleton.m_challengeActive = false;
		s_singleton.m_spawnChallengePiece = false;
	}

	private void Event_OnNewGameStarted()
	{
		m_DCTimeValidation.EnforceValidTime(save: true);
		CheckData();
		m_spawnChallengePiece = false;
		m_spawnedPiece = null;
	}

	private void Event_OnGameFinished()
	{
		m_DCTimeValidation.EnforceValidTime(save: true);
		CheckData();
		m_spawnChallengePiece = false;
		if (!AllPiecesCollected())
		{
			PreparePiecesToSave(normal: true);
		}
	}

	private void Event_OnDCRewarded()
	{
		PlayerStats.IncreaseStat(PlayerStats.StatNames.DCsCompleted_Total, 1);
		PlayerStats.IncreaseStat(PlayerStats.StatNames.DCsCompletedConsecutive_Total, 1);
		PreparePiecesToSave(normal: true);
	}

	private void CheckData()
	{
		if (GetSecondsRemaining() == 0f && DCTimeValidation.TrustedTime)
		{
			ChangeDay();
		}
		else if (!m_challengeCompleted)
		{
			m_challengeActive = true;
		}
	}

	private void ResetPiecesCollected(bool collected)
	{
		for (int i = 0; i < 4; i++)
		{
			m_piecesCollected[i] = collected;
			m_piecesCollectedToSave[i] = collected;
		}
	}

	private int NumPiecesCollected()
	{
		int num = 0;
		for (int i = 0; i < 4; i++)
		{
			if (m_piecesCollected[i])
			{
				num++;
			}
		}
		return num;
	}

	private void Event_OnGameDataSaveRequest()
	{
		CultureInfo cultureInfo = new CultureInfo("en-US");
		PropertyStore.Store("CDLastDateCompleted", m_lastCompletedChallengeDate.Date.ToString(cultureInfo.DateTimeFormat));
		PropertyStore.Store("CDTrustedLastDateCompleted", m_trustedLastCompletedChallengeDate);
		PropertyStore.Store("CDCurrentDate", m_currentChallengeDate.Date.ToString(cultureInfo.DateTimeFormat));
		PropertyStore.Store("CDTrustedCurrentDate", m_trustedCurrentChallengeDate);
		PropertyStore.Store("CDChallengeRewarded", ChallengeRewarded);
		PropertyStore.Store("CDCompleted", m_challengeCompletedToSave);
		PropertyStore.Store("CDActive", s_singleton.m_challengeActiveToSave);
		PropertyStore.Store("CDDayNumber", m_challengeDay);
		for (int i = 0; i < 4; i++)
		{
			PropertyStore.Store(s_propertyCDPieces[i], m_piecesCollectedToSave[i]);
		}
		m_DCTimeValidation.Save();
	}

	private void Event_OnGameDataLoaded(ActiveProperties activeProperties)
	{
		CultureInfo provider = new CultureInfo("en-US");
		if (!DateTime.TryParse(activeProperties.GetString("CDLastDateCompleted"), provider, DateTimeStyles.None, out m_lastCompletedChallengeDate))
		{
			m_lastCompletedChallengeDate = DCTime.GetCurrentTime().AddYears(-1).Date;
			s_singleton.m_trustedLastCompletedChallengeDate = DCTimeValidation.TrustedTime;
		}
		else
		{
			m_trustedLastCompletedChallengeDate = activeProperties.GetBool("CDTrustedLastDateCompleted");
		}
		if (!DateTime.TryParse(activeProperties.GetString("CDCurrentDate"), provider, DateTimeStyles.None, out m_currentChallengeDate))
		{
			m_currentChallengeDate = DCTime.GetCurrentTime().Date;
			s_singleton.m_trustedCurrentChallengeDate = DCTimeValidation.TrustedTime;
		}
		else
		{
			m_trustedCurrentChallengeDate = activeProperties.GetBool("CDTrustedCurrentDate");
		}
		m_challengeCompleted = activeProperties.GetBool("CDCompleted");
		m_challengeCompletedToSave = activeProperties.GetBool("CDCompleted");
		m_challengeActive = activeProperties.GetBool("CDActive");
		m_challengeActiveToSave = activeProperties.GetBool("CDActive");
		m_challengeDay = activeProperties.GetInt("CDDayNumber");
		ChallengeRewarded = activeProperties.GetBool("CDChallengeRewarded");
		for (int i = 0; i < 4; i++)
		{
			m_piecesCollected[i] = activeProperties.GetBool(s_propertyCDPieces[i]);
			m_piecesCollectedToSave[i] = activeProperties.GetBool(s_propertyCDPieces[i]);
		}
		m_DCTimeValidation.Load(activeProperties);
		m_DCTimeValidation.EnforceValidTime(save: false);
		CheckData();
	}
}
