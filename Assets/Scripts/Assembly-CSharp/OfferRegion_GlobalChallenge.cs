using System;
using UnityEngine;

public class OfferRegion_GlobalChallenge : MonoBehaviour
{
	private enum Regions
	{
		Participated,
		NotParticipated
	}

	private string[] m_globalChallengeRegionNames;

	private void OnEnable()
	{
		InitialiseGlobalChallengeRegions();
		EnableGlobalChallengeRegions();
	}

	private void OnDisable()
	{
		DisableGlobalChallengeRegions();
	}

	private void InitialiseGlobalChallengeRegions()
	{
		if (m_globalChallengeRegionNames == null)
		{
			m_globalChallengeRegionNames = new string[Enum.GetNames(typeof(Regions)).Length];
			string text = $"globalchallenge_{GCState.Challenges.gc3}_participated";
			string text2 = $"globalchallenge_{GCState.Challenges.gc3}_didnt_participate";
			m_globalChallengeRegionNames[0] = text;
			m_globalChallengeRegionNames[1] = text2;
		}
	}

	private void EnableGlobalChallengeRegions()
	{
		string regionName = ((!GCState.IsChallengeParticipated(GCState.Challenges.gc3)) ? m_globalChallengeRegionNames[1] : m_globalChallengeRegionNames[0]);
		if (GCState.ChallengeState(GCState.Challenges.gc3) != 0)
		{
			OfferRegion.Start(regionName);
		}
	}

	private void DisableGlobalChallengeRegions()
	{
		for (int i = 0; i < m_globalChallengeRegionNames.Length; i++)
		{
			string regionName = m_globalChallengeRegionNames[i];
			OfferRegion.End(regionName);
		}
	}
}
