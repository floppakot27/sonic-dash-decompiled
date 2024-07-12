using System;
using System.Collections;
using UnityEngine;

public class GCProgressPageDisplay : MonoBehaviour
{
	private const int NumberOfTiers = 4;

	private const float PercentagePerTier = 0.25f;

	[SerializeField]
	private GameObject[] m_tierDisplayers = new GameObject[4];

	[SerializeField]
	private UISlider[] m_localProgressSliders = new UISlider[4];

	[SerializeField]
	private UISlider[] m_communityProgressSliders = new UISlider[4];

	[SerializeField]
	private UICheckbox[] m_localTierCheckboxes = new UICheckbox[4];

	[SerializeField]
	private UICheckbox[] m_communityTierCheckboxes = new UICheckbox[4];

	[SerializeField]
	private GameObject[] m_localPadlocks = new GameObject[4];

	[SerializeField]
	private GameObject[] m_communityPadlocks = new GameObject[4];

	[SerializeField]
	private Animation[] m_tierCompleteAnims = new Animation[4];

	[SerializeField]
	private MeshFilter[] m_prizeMeshes = new MeshFilter[4];

	[SerializeField]
	private float m_sliderCompleteSpeed = 0.25f;

	[SerializeField]
	private GameObject m_timer;

	[SerializeField]
	private UILabel m_timerLabel;

	[SerializeField]
	private GameObject m_timerNotConnection;

	[SerializeField]
	private UILabel m_contributionLabel;

	[SerializeField]
	private GuiButtonBlocker[] m_blockers;

	private bool m_rewardingFinalTierPrize;

	private string m_daysLeftStringID = "GC_DAYS_LEFT";

	private string m_finalDayStringID = "GC_FINAL_DAY";

	private void Start()
	{
		for (int i = 0; i < 4; i++)
		{
			m_localProgressSliders[i].numberOfSteps = 10000;
			m_communityProgressSliders[i].numberOfSteps = 10000;
		}
	}

	private void OnEnable()
	{
		StartCoroutine(StartPendingActivation());
	}

	private void OnDisable()
	{
		for (int i = 0; i < 4; i++)
		{
			m_localProgressSliders[i].sliderValue = 0f;
			m_localPadlocks[i].SetActive(value: true);
			m_localTierCheckboxes[i].isChecked = false;
			m_communityPadlocks[i].SetActive(value: true);
			m_communityTierCheckboxes[i].isChecked = false;
			m_communityProgressSliders[i].sliderValue = 0f;
		}
		m_rewardingFinalTierPrize = false;
		GC3Progress.GCPageVisited();
	}

	private IEnumerator StartPendingActivation()
	{
		yield return null;
		BlockButtons(block: true);
		int lastLocalTier = GC3Progress.GetGC3LocalTierLastCheck();
		int lastComTier = GC3Progress.GetGC3GlobalTierLastCheck();
		m_contributionLabel.text = GC3Progress.PreviousPointsContributed.ToString();
		for (int i = 0; i < lastComTier; i++)
		{
			m_communityProgressSliders[i].sliderValue = 1f;
			m_communityTierCheckboxes[i].isChecked = true;
			m_communityPadlocks[i].SetActive(value: false);
		}
		if (lastComTier < 4)
		{
			m_communityProgressSliders[lastComTier].sliderValue = (GC3Progress.CalculateGlobalPercent(GC3Progress.PreviousPointsGlobal) - 0.25f * (float)lastComTier) * 4f;
			m_communityPadlocks[lastComTier].SetActive(value: false);
		}
		for (int i = 0; i < lastLocalTier; i++)
		{
			m_localProgressSliders[i].sliderValue = 1f;
			m_localTierCheckboxes[i].isChecked = true;
			if (!m_communityPadlocks[i].activeInHierarchy)
			{
				m_localPadlocks[i].SetActive(value: false);
			}
		}
		if (lastLocalTier < 4)
		{
			m_localProgressSliders[lastLocalTier].sliderValue = (GC3Progress.CalculateLocalPercent(GC3Progress.PreviousPointsLocal) - 0.25f * (float)lastLocalTier) * 4f;
			if (!m_communityPadlocks[lastLocalTier].activeInHierarchy)
			{
				m_localPadlocks[lastLocalTier].SetActive(value: false);
			}
		}
		for (int i = 0; i < 4; i++)
		{
			StoreContent.StoreEntry entry = StoreContent.GetStoreEntry(GC3Progress.TierRewards[i], StoreContent.Identifiers.Name);
			m_prizeMeshes[i].mesh = entry.m_mesh;
			if (i == Mathf.Min(lastLocalTier, lastComTier))
			{
				m_tierDisplayers[i].SetActive(value: true);
			}
			else
			{
				m_tierDisplayers[i].SetActive(value: false);
			}
		}
		if (DCTimeValidation.TrustedTime)
		{
			m_timer.SetActive(value: true);
			m_timerNotConnection.SetActive(value: false);
			DateTime now = DCTime.GetCurrentTime();
			TimeSpan span = GCState.GetChallengeDate(GCState.Challenges.gc3).Subtract(now);
			int daysRoundedUp = Mathf.CeilToInt((float)span.TotalDays);
			if (span.TotalHours > 24.0)
			{
				m_timerLabel.text = string.Format(LanguageStrings.First.GetString(m_daysLeftStringID), daysRoundedUp);
			}
			else
			{
				m_timerLabel.text = LanguageStrings.First.GetString(m_finalDayStringID);
			}
		}
		else
		{
			m_timer.SetActive(value: false);
			m_timerNotConnection.SetActive(value: true);
		}
	}

	private void Trigger_UpdateScores()
	{
		StartCoroutine(UpdateScoreVisuals());
		if (GC3Progress.PreviousPointsContributed < GC3Progress.ActualPointsContributed)
		{
			StartCoroutine(CountUpContribution());
		}
	}

	private IEnumerator CountUpContribution()
	{
		int total = GC3Progress.ActualPointsContributed;
		int count = GC3Progress.PreviousPointsContributed;
		m_contributionLabel.text = count.ToString();
		do
		{
			count++;
			m_contributionLabel.text = count.ToString();
			yield return null;
		}
		while (count < total);
	}

	private IEnumerator UpdateScoreVisuals()
	{
		int lastLocalTier = GC3Progress.GetGC3LocalTierLastCheck();
		int lastComTier = GC3Progress.GetGC3GlobalTierLastCheck();
		int curLocalTier = GC3Progress.GetGC3LocalTierCurrent();
		int curComTier = GC3Progress.GetGC3GlobalTierCurrent();
		for (int i = lastComTier; i <= curComTier; i++)
		{
			if (i >= m_communityProgressSliders.Length)
			{
				continue;
			}
			float progress = m_communityProgressSliders[i].sliderValue;
			float totalGlobalProgress = GC3Progress.CalculateGlobalPercent(GC3Progress.ActualPointsGlobal);
			float desiredTierProgress = 0f;
			desiredTierProgress = ((!(totalGlobalProgress > ((float)i + 1f) * 0.25f)) ? ((totalGlobalProgress - 0.25f * (float)i) * 4f) : 1f);
			while (progress < desiredTierProgress)
			{
				progress += m_sliderCompleteSpeed * IndependantTimeDelta.Delta;
				progress = Mathf.Min(progress, desiredTierProgress);
				m_communityProgressSliders[i].sliderValue = progress;
				yield return null;
			}
			if (m_communityProgressSliders[i].sliderValue == 1f)
			{
				m_communityPadlocks[i].SetActive(value: false);
				m_communityTierCheckboxes[i].isChecked = true;
				if (i < 3)
				{
					m_communityPadlocks[i + 1].SetActive(value: false);
				}
				yield return null;
			}
		}
		for (int j = lastLocalTier; j <= curLocalTier; j++)
		{
			if (j >= m_localProgressSliders.Length)
			{
				continue;
			}
			float locProgress = m_localProgressSliders[j].sliderValue;
			float totalLocalProgress = GC3Progress.CalculateLocalPercent(GC3Progress.ActualPointsLocal);
			float desiredLocTierProgress = 0f;
			desiredLocTierProgress = ((!(totalLocalProgress > ((float)j + 1f) * 0.25f)) ? ((totalLocalProgress - 0.25f * (float)j) * 4f) : 1f);
			while (locProgress < desiredLocTierProgress)
			{
				locProgress += 1f / m_sliderCompleteSpeed * IndependantTimeDelta.Delta;
				locProgress = Mathf.Min(locProgress, desiredLocTierProgress);
				m_localProgressSliders[j].sliderValue = locProgress;
				yield return null;
			}
			if (m_localProgressSliders[j].sliderValue == 1f)
			{
				m_localPadlocks[j].SetActive(value: false);
				m_localTierCheckboxes[j].isChecked = true;
				if (j < 3 && !m_communityPadlocks[j + 1].activeInHierarchy)
				{
					m_localPadlocks[j + 1].SetActive(value: false);
				}
				yield return null;
			}
		}
		if (GC3Progress.IsRewardDue())
		{
			if (curLocalTier < 4)
			{
				m_tierDisplayers[curLocalTier - 1].SetActive(value: false);
				m_tierDisplayers[curLocalTier].SetActive(value: true);
				m_localPadlocks[curLocalTier].SetActive(value: false);
				if (curComTier < 4)
				{
					m_communityPadlocks[curComTier].SetActive(value: false);
				}
			}
			int animIndex = ((lastLocalTier >= lastComTier) ? lastComTier : lastLocalTier);
			if (animIndex > 3)
			{
				animIndex = 3;
			}
			m_tierCompleteAnims[animIndex].Play();
			yield return new WaitForSeconds(1.5f);
			int amount = 0;
			string reward = GC3Progress.GetRewardDue(out amount, out m_rewardingFinalTierPrize);
			if (m_rewardingFinalTierPrize)
			{
				StorePurchases.RequestReward(reward, amount, 16, StorePurchases.ShowDialog.Yes);
			}
			else
			{
				StorePurchases.RequestReward(reward, amount, 15, StorePurchases.ShowDialog.Yes);
			}
			GC3Progress.GCPageVisited();
		}
		BlockButtons(block: false);
	}

	private void Trigger_HelpDialogShow()
	{
		Dialog_GCHelp.Display();
	}

	private void BlockButtons(bool block)
	{
		for (int i = 0; i < m_blockers.Length; i++)
		{
			m_blockers[i].Blocked = block;
		}
	}
}
