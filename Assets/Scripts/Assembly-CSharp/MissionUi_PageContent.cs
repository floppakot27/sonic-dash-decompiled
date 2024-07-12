using System.Collections;
using UnityEngine;

public class MissionUi_PageContent : MonoBehaviour
{
	private const float BannerSwapDelay_Menus = 0.5f;

	private const float BannerSwapDelay_Run = 1.5f;

	private float m_swapBannerDelay;

	private float m_pendingRewardDelay = 2f;

	private float m_incentivesDelay = 2f;

	private bool m_allowSwap;

	[SerializeField]
	private MissionUI_BannerContent[] m_missionBanners;

	[SerializeField]
	private UISlider m_totalProgression;

	[SerializeField]
	private UILabel m_percentageProgression;

	[SerializeField]
	private UILabel m_multiplier;

	[SerializeField]
	private UILabel m_rewardRSR;

	[SerializeField]
	private GameObject m_progressionGroup;

	[SerializeField]
	private GameObject m_triggerCompleteStatic;

	[SerializeField]
	private GameObject m_triggerSwapActive;

	[SerializeField]
	private GameObject m_triggerInProgressStatic;

	[SerializeField]
	private GameObject m_triggerAllDoneStatic;

	[SerializeField]
	private GameObject m_triggerAllDoneActive;

	[SerializeField]
	private float m_sliderCompleteSpeed = 0.5f;

	[SerializeField]
	private float m_bannerSwapAnimationDelay = 0.1f;

	[SerializeField]
	private AudioClip m_clipIncMulitplier;

	[SerializeField]
	private bool m_swapMissionsWhenComplete = true;

	[SerializeField]
	private bool m_inRun;

	[SerializeField]
	private OfferRegion_Timed m_offerMissionSetComplete;

	[SerializeField]
	private OfferRegion_Timed m_offerMissionSetCompleteAll;

	private void Start()
	{
		EventDispatch.RegisterInterest("OnMissionComplete", this);
	}

	private void Update()
	{
		if (m_swapMissionsWhenComplete && m_allowSwap)
		{
			m_swapBannerDelay -= IndependantTimeDelta.Delta;
			if (!(m_swapBannerDelay > 0f) && !(m_totalProgression.sliderValue < 1f))
			{
				MoveToNextMissionSet();
				m_allowSwap = false;
			}
		}
	}

	private void OnEnable()
	{
		if (m_inRun)
		{
			m_swapBannerDelay = 1.5f;
		}
		else
		{
			m_swapBannerDelay = 0.5f;
		}
		m_allowSwap = true;
		StartCoroutine(StartPendingActivation(m_inRun));
	}

	private void OnDisable()
	{
		MissionTracker.Track(trackMissions: true);
	}

	private IEnumerator StartPendingActivation(bool inRun)
	{
		yield return null;
		for (int i = 0; i < m_missionBanners.Length; i++)
		{
			m_missionBanners[i].PopulateOnStart(i, inRun);
		}
		if (MissionTracker.AllMissionsComplete())
		{
			m_triggerAllDoneStatic.SendMessage("OnClick", SendMessageOptions.RequireReceiver);
			m_allowSwap = false;
			PopulateOnComplete();
		}
		else
		{
			PopulateOnStart();
		}
	}

	private void PopulateOnStart()
	{
		float missionSetProgress = MissionUtils.GetMissionSetProgress();
		SetProgressProperties(missionSetProgress);
		m_multiplier.text = $"{ScoreTracker.NextMultiplier}";
		m_rewardRSR.text = $"{MissionTracker.RSRRewardPerSet}";
		if (missionSetProgress == 1f)
		{
			m_triggerCompleteStatic.SendMessage("OnClick", SendMessageOptions.RequireReceiver);
		}
		else
		{
			m_triggerInProgressStatic.SendMessage("OnClick", SendMessageOptions.RequireReceiver);
		}
	}

	private void PopulateOnComplete()
	{
		m_progressionGroup.SetActive(value: false);
	}

	private void SetProgressProperties(float progress)
	{
		if (progress > 0.99f)
		{
			progress = 1f;
		}
		m_totalProgression.sliderValue = progress;
		m_percentageProgression.text = $"{Mathf.FloorToInt(progress * 100f).ToString()}%";
	}

	private void OnMissionComplete()
	{
		float missionSetProgress = MissionUtils.GetMissionSetProgress();
		StartCoroutine(IncreaseProgressBar(missionSetProgress));
	}

	private void MoveToNextMissionSet()
	{
		MissionTracker.Track(trackMissions: false);
		int missionSet = MissionTracker.GetMissionSet();
		MissionTracker.MoveToNextSet();
		EventDispatch.GenerateEvent("OnStarRingsAwarded", new GameAnalytics.RSRAnalyticsParam(MissionTracker.RSRRewardPerSet, GameAnalytics.RingsRecievedReason.MissionSet));
		if (MissionTracker.AllMissionsComplete())
		{
			m_triggerAllDoneActive.SendMessage("OnClick", SendMessageOptions.RequireReceiver);
			m_offerMissionSetCompleteAll.Visit();
			m_progressionGroup.SetActive(value: false);
		}
		else
		{
			m_triggerSwapActive.SendMessage("OnClick", SendMessageOptions.RequireReceiver);
			m_offerMissionSetComplete.Visit();
		}
		m_multiplier.text = $"{ScoreTracker.NextMultiplier}";
		Audio.PlayClip(m_clipIncMulitplier, loop: false);
		if (ABTesting.GetTestValue(ABTesting.Tests.MSG_MissionBenefits) != 0 && missionSet == 0)
		{
			StartCoroutine(MissionIncentivesDialog());
		}
		else
		{
			StartCoroutine(PendingRewardDialog());
		}
		StartCoroutine(SwapBannersToNewSet());
		if (!MissionTracker.AllMissionsComplete())
		{
			StartCoroutine(DecreaseProgressBar());
		}
	}

	private IEnumerator MissionIncentivesDialog()
	{
		float count = m_incentivesDelay;
		while (count > 0f)
		{
			count -= IndependantTimeDelta.Delta;
			yield return null;
		}
		if (base.gameObject.activeInHierarchy)
		{
			Dialog_MissionIncentives.Display();
		}
	}

	private IEnumerator PendingRewardDialog()
	{
		float count = m_pendingRewardDelay;
		while (count > 0f)
		{
			count -= IndependantTimeDelta.Delta;
			yield return null;
		}
		if (base.gameObject.activeInHierarchy)
		{
			Dialog_MissionsComplete.Display();
		}
	}

	private void Trigger_MissionsComplete()
	{
		Dialog_RateMe.Display();
	}

	private IEnumerator SwapBannersToNewSet()
	{
		float count = 0.5f;
		while (count > 0f)
		{
			count -= IndependantTimeDelta.Delta;
			yield return null;
		}
		for (int i = 0; i < m_missionBanners.Length; i++)
		{
			count = m_bannerSwapAnimationDelay;
			while (count > 0f)
			{
				count -= IndependantTimeDelta.Delta;
				yield return null;
			}
			m_missionBanners[i].TransitionToMission();
		}
		float targetProgress = MissionUtils.GetMissionSetProgress();
		StartCoroutine(IncreaseProgressBar(targetProgress));
		yield return null;
	}

	private IEnumerator IncreaseProgressBar(float targetProgress)
	{
		float initialProgress = m_totalProgression.sliderValue;
		float currentTime = 0f;
		do
		{
			currentTime += 1f / m_sliderCompleteSpeed * IndependantTimeDelta.Delta;
			float thisProgress = EaseInOutExpo(initialProgress, targetProgress, currentTime);
			if (thisProgress > 1f)
			{
				thisProgress = 1f;
			}
			SetProgressProperties(thisProgress);
			yield return null;
		}
		while (currentTime < 1f);
	}

	private IEnumerator DecreaseProgressBar()
	{
		float delay = 0f;
		do
		{
			delay += IndependantTimeDelta.Delta;
			yield return null;
		}
		while (delay < 0.5f);
		float currentTime = 0f;
		float intialProgress = m_totalProgression.sliderValue;
		do
		{
			currentTime += 1f / m_sliderCompleteSpeed * IndependantTimeDelta.Delta;
			float thisProgress = EaseInOutExpo(intialProgress, 0f, currentTime);
			if (thisProgress > 1f)
			{
				thisProgress = 1f;
			}
			SetProgressProperties(thisProgress);
			yield return null;
		}
		while (currentTime < 1f);
	}

	private float EaseInOutExpo(float start, float end, float value)
	{
		value /= 0.5f;
		end -= start;
		if (value < 1f)
		{
			return end / 2f * Mathf.Pow(2f, 10f * (value - 1f)) + start;
		}
		value -= 1f;
		return end / 2f * (0f - Mathf.Pow(2f, -10f * value) + 2f) + start;
	}

	private void Event_OnMissionComplete(int missionGroup, bool setComplete)
	{
		if (base.gameObject.activeInHierarchy)
		{
			m_allowSwap = true;
			OnMissionComplete();
		}
	}
}
