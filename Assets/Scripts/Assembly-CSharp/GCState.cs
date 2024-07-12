using System;
using System.Collections;
using System.Globalization;
using UnityEngine;

public class GCState : MonoBehaviour
{
	public enum Challenges
	{
		gc1,
		gc2,
		gc3
	}

	public enum State
	{
		Inactive,
		Active,
		Finished
	}

	public const Challenges CurrentChallenge = Challenges.gc3;

	private const string RootName = "gcstates";

	private const string DatesRootName = "gcdates";

	private const string PropertyPrefix = "GC_State_";

	private const string PropertyActive = "_Active";

	private const string PropertyParticipated = "_Participated";

	private const string m_propertyGCButtonPressed = "GCButtonPressed_";

	private const string m_propertyGCButtonAnalyticsSent = "GCButtonAnalyticsSent_";

	private const string m_propertyFirstDateGCButton = "FirstGCButtonPressed_";

	private const string m_propertyLastDateGCButton = "LastGCButtonPressed_";

	private const string m_propertyTimesGCButton = "TimesGCButtonPressed_";

	private static GCState s_singleton = null;

	private static int s_numChallenges = Enum.GetNames(typeof(Challenges)).Length;

	private State[] m_activeChallenges = new State[s_numChallenges];

	private bool[] m_participatedChallenges = new bool[s_numChallenges];

	private bool[] m_StatusConfirmed = new bool[s_numChallenges];

	private DateTime[] m_datesChallenges = new DateTime[Enum.GetNames(typeof(Challenges)).Length];

	private bool[] m_isGCButtonPressed = new bool[s_numChallenges];

	private bool[] m_isGCButtonAnalyticsSent = new bool[s_numChallenges];

	private DateTime[] m_firstDateGCButtonPressed = new DateTime[s_numChallenges];

	private DateTime[] m_lastDateGCButtonPressed = new DateTime[s_numChallenges];

	private int[] m_timesGCButtonPressed = new int[s_numChallenges];

	public static State ChallengeState(Challenges challenge)
	{
		return s_singleton.m_activeChallenges[(int)challenge];
	}

	public static bool IsCurrentChallengeActive()
	{
		if (s_singleton.m_activeChallenges[2] == State.Active)
		{
			return true;
		}
		return false;
	}

	public static DateTime GetChallengeDate(Challenges challenge)
	{
		return s_singleton.m_datesChallenges[(int)challenge];
	}

	public static bool IsChallengeParticipated(Challenges challenge)
	{
		return s_singleton.m_participatedChallenges[(int)challenge];
	}

	public static void GCButtonPressed(bool internet)
	{
		for (int i = 0; i < s_singleton.m_activeChallenges.Length; i++)
		{
			if (s_singleton.m_activeChallenges[i] == State.Active)
			{
				if (!s_singleton.m_isGCButtonPressed[i])
				{
					ref DateTime reference = ref s_singleton.m_firstDateGCButtonPressed[i];
					reference = DCTime.GetCurrentTime().Date;
					s_singleton.m_isGCButtonPressed[i] = true;
				}
				ref DateTime reference2 = ref s_singleton.m_lastDateGCButtonPressed[i];
				reference2 = DCTime.GetCurrentTime().Date;
				s_singleton.m_timesGCButtonPressed[i]++;
				GameAnalytics.GCButtonPressed(internet, (Challenges)i);
			}
		}
	}

	public static void GCProgressPageVisited()
	{
		GC3Progress.GCPageVisited();
	}

	private void Start()
	{
		s_singleton = this;
		EventDispatch.RegisterInterest("OnGameDataLoaded", this);
		EventDispatch.RegisterInterest("OnGameDataSaveRequest", this);
		EventDispatch.RegisterInterest("OnGameFinished", this, EventDispatch.Priority.Lowest);
		EventDispatch.RegisterInterest("MainMenuActive", this, EventDispatch.Priority.Highest);
		EventDispatch.RegisterInterest("FeatureStateReady", this);
		StartCoroutine(WaitForFeatureState());
	}

	private void Event_OnGameFinished()
	{
		ParticipateInChallenge();
		if (IsCurrentChallengeActive())
		{
			GC3Progress.ContributeToChallenge();
		}
	}

	private void ParticipateInChallenge()
	{
		for (int i = 0; i < m_activeChallenges.Length; i++)
		{
			if (m_activeChallenges[i] == State.Active)
			{
				m_participatedChallenges[i] = true;
			}
		}
	}

	public static string PropertyActiveName(Challenges challenge)
	{
		return string.Concat("GC_State_", challenge, "_Active");
	}

	public static string PropertyParticipatedName(Challenges challenge)
	{
		return string.Concat("GC_State_", challenge, "_Participated");
	}

	private void Event_OnGameDataSaveRequest()
	{
		for (int i = 0; i < m_activeChallenges.Length; i++)
		{
			PropertyStore.Store(PropertyActiveName((Challenges)i), (int)m_activeChallenges[i]);
			PropertyStore.Store(PropertyParticipatedName((Challenges)i), m_participatedChallenges[i]);
			PropertyStore.Store("GCButtonAnalyticsSent_" + (Challenges)i, m_isGCButtonAnalyticsSent[i]);
			PropertyStore.Store("GCButtonPressed_" + (Challenges)i, m_isGCButtonPressed[i]);
			PropertyStore.Store("TimesGCButtonPressed_" + (Challenges)i, m_timesGCButtonPressed[i]);
			CultureInfo cultureInfo = new CultureInfo("en-US");
			PropertyStore.Store("FirstGCButtonPressed_" + (Challenges)i, m_firstDateGCButtonPressed[i].ToString(cultureInfo.DateTimeFormat));
			PropertyStore.Store("LastGCButtonPressed_" + (Challenges)i, m_lastDateGCButtonPressed[i].ToString(cultureInfo.DateTimeFormat));
		}
	}

	private void Event_OnGameDataLoaded(ActiveProperties activeProperties)
	{
		for (int i = 0; i < m_activeChallenges.Length; i++)
		{
			if (!s_singleton.m_StatusConfirmed[i])
			{
				string propertyName = string.Concat("GC_State_", (Challenges)i, "_Active");
				if (!PropertyStore.ActiveProperties().DoesPropertyExist(propertyName))
				{
					m_activeChallenges[i] = State.Inactive;
				}
				else
				{
					m_activeChallenges[i] = (State)activeProperties.GetInt(propertyName);
				}
			}
			string propertyName2 = string.Concat("GC_State_", (Challenges)i, "_Participated");
			m_participatedChallenges[i] = activeProperties.GetBool(propertyName2);
			m_isGCButtonAnalyticsSent[i] = activeProperties.GetBool("GCButtonAnalyticsSent_" + (Challenges)i);
			m_isGCButtonPressed[i] = activeProperties.GetBool("GCButtonPressed_" + (Challenges)i);
			m_timesGCButtonPressed[i] = activeProperties.GetInt("TimesGCButtonPressed_" + (Challenges)i);
			CultureInfo provider = new CultureInfo("en-US");
			if (!DateTime.TryParse(activeProperties.GetString("FirstGCButtonPressed_" + (Challenges)i), provider, DateTimeStyles.None, out m_firstDateGCButtonPressed[i]))
			{
				ref DateTime reference = ref m_firstDateGCButtonPressed[i];
				reference = DCTime.GetCurrentTime().AddYears(-10).Date;
			}
			if (!DateTime.TryParse(activeProperties.GetString("LastGCButtonPressed_" + (Challenges)i), provider, DateTimeStyles.None, out m_lastDateGCButtonPressed[i]))
			{
				ref DateTime reference2 = ref m_lastDateGCButtonPressed[i];
				reference2 = DCTime.GetCurrentTime().AddYears(-10).Date;
			}
		}
	}

	private IEnumerator WaitForFeatureState()
	{
		bool featureStateAvailable = false;
		bool abTestingReady = false;
		do
		{
			featureStateAvailable = FeatureState.Ready;
			abTestingReady = ABTesting.Ready;
			yield return null;
		}
		while (!featureStateAvailable && !abTestingReady);
		UpdateStatus();
	}

	private void Event_FeatureStateReady()
	{
		UpdateStatus();
	}

	private void UpdateStatus()
	{
		if (!FeatureState.Valid)
		{
			return;
		}
		CultureInfo provider = new CultureInfo("en-US");
		for (int i = 0; i < s_singleton.m_activeChallenges.Length; i++)
		{
			string propertyName = ((Challenges)i).ToString();
			LSON.Property stateProperty = FeatureState.GetStateProperty("gcstates", propertyName);
			s_singleton.m_activeChallenges[i] = State.Inactive;
			s_singleton.m_StatusConfirmed[i] = false;
			if (stateProperty != null && LSONProperties.AsInt(stateProperty, out var intValue))
			{
				s_singleton.m_activeChallenges[i] = (State)intValue;
				s_singleton.m_StatusConfirmed[i] = true;
			}
			propertyName = ((Challenges)i).ToString();
			stateProperty = FeatureState.GetStateProperty("gcdates", propertyName);
			if (stateProperty != null && LSONProperties.AsString(stateProperty, out var stringValue))
			{
				stringValue = stringValue.Replace("_", " ");
				stringValue = stringValue.Replace(".", ":");
				if (!DateTime.TryParse(stringValue, provider, DateTimeStyles.None, out m_datesChallenges[i]))
				{
					ref DateTime reference = ref m_datesChallenges[i];
					reference = DCTime.GetCurrentTime().AddDays(7.0).Date;
				}
			}
		}
	}

	private void Event_MainMenuActive()
	{
		for (int i = 0; i < m_activeChallenges.Length; i++)
		{
			if (m_activeChallenges[i] == State.Finished && !m_isGCButtonAnalyticsSent[i])
			{
				GameAnalytics.GCButtonTimesPreseed(m_timesGCButtonPressed[i], m_firstDateGCButtonPressed[i], m_lastDateGCButtonPressed[i], (Challenges)i);
				m_isGCButtonAnalyticsSent[i] = true;
			}
		}
		if (GCDialogManager.ShouldShowChallenegeActiveDialog())
		{
			GCDialogManager.ShowChallengeActiveDialog();
		}
	}
}
