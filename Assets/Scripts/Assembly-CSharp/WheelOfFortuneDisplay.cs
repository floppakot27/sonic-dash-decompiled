using System.Collections;
using UnityEngine;

public class WheelOfFortuneDisplay : MonoBehaviour
{
	public const int NumberOfWheelSegments = 8;

	private StoreContent.StoreEntry[] m_wheelPrizes = new StoreContent.StoreEntry[8];

	private int m_awardedPrizeIndex = -1;

	private bool m_awardingPrize;

	private bool m_cyclingJackpot;

	private bool m_savedDuringCycling;

	private bool m_forceStopJackpotCycle;

	private bool m_canForce = true;

	private bool m_isJackpot;

	private bool m_isSpinning;

	private bool m_spinButtonPressed;

	private bool m_homeButtonPressed;

	private float m_targetSpinnerAngle;

	private float m_currentAngle;

	private float m_anglePerSegment = 45f;

	private Vector3 m_eulerAngles = Vector3.zero;

	private StoreContent.StoreEntry m_wheelSpinEntry;

	private StoreContent.StoreEntry[] m_cachedJackpotChoices;

	[SerializeField]
	private UILabel[] m_quantityEntries = new UILabel[8];

	[SerializeField]
	private MeshFilter[] m_prizeMeshes = new MeshFilter[8];

	[SerializeField]
	private GameObject[] m_prizeHighlight = new GameObject[8];

	[SerializeField]
	private UILabel m_timeRemaining;

	[SerializeField]
	private UILabel m_cost;

	[SerializeField]
	private GameObject m_safeTimeArea;

	[SerializeField]
	private GameObject m_warningTimeArea;

	[SerializeField]
	private AudioClip m_normalRewardSound;

	[SerializeField]
	private AudioClip m_jackpotRewardSound;

	[SerializeField]
	private AudioClip m_normalPreRewardSound;

	[SerializeField]
	private AudioClip m_jackpotPreRewardSound;

	[SerializeField]
	private AudioClip m_spinSound;

	[SerializeField]
	private AudioClip m_cycleJackpotSound;

	[SerializeField]
	private AudioClip m_replaceJackpotSound;

	[SerializeField]
	private AudioClip m_forceStopPressSound;

	[SerializeField]
	private int m_numberOfJackpotPrizeCycles = 32;

	[SerializeField]
	private int m_numberOfSlowJackpotPrizeCycles = 8;

	[SerializeField]
	private int m_jackpotCyclesLeftAfterTap = 2;

	[SerializeField]
	private float m_jackpotPrizeCycleTimeMin = 0.1f;

	[SerializeField]
	private float m_jackpotPrizeCycleTimeMax = 0.8f;

	[SerializeField]
	private float m_spinSpeed = 600f;

	[SerializeField]
	private float m_slowDownAngle = 900f;

	[SerializeField]
	private float m_numberOfSpins = 5f;

	[SerializeField]
	private float m_angleTolerance = 5f;

	[SerializeField]
	private GameObject m_spinnerObject;

	[SerializeField]
	private GameObject m_counterSpinnerObject;

	[SerializeField]
	private ParticleSystem m_spinParticle;

	[SerializeField]
	private Transform m_spinParticleAnchor;

	[SerializeField]
	private ParticleSystem m_winNormalParticle;

	[SerializeField]
	private ParticleSystem m_winJackpotParticle;

	[SerializeField]
	private GuiButtonBlocker[] m_blockers;

	[SerializeField]
	private GameObject m_buttonSpinner;

	[SerializeField]
	private GameObject m_freeSpinButton;

	[SerializeField]
	private GameObject m_paidSpinButton;

	[SerializeField]
	private StoreBlocker m_spinButtonStoreBlocker;

	[SerializeField]
	private GameObject m_homeButtonIcon;

	[SerializeField]
	private GameObject m_homeButtonSpinner;

	[SerializeField]
	private MenuTriggers m_menuTriggerRef;

	private void OnEnable()
	{
		EventDispatch.RegisterInterest("OnDialogHidden", this);
		EventDispatch.RegisterInterest("OnStorePurchaseCompleted", this);
		EventDispatch.RegisterInterest("OnPlayerRewarded", this);
		m_homeButtonPressed = false;
		m_spinButtonPressed = false;
		if (m_wheelSpinEntry == null)
		{
			m_wheelSpinEntry = StoreContent.GetStoreEntry("Wheel Spin", StoreContent.Identifiers.Name);
		}
		if (!WheelOfFortuneSettings.Instance.KnowAboutFreeSpin)
		{
			WheelOfFortuneSettings.Instance.KnowAboutFreeSpin = true;
		}
		m_cost.text = m_wheelSpinEntry.m_cost.m_playerCost[0].ToString();
		UpdateButtonGraphics();
		WheelOfFortuneRewards.Instance.ValidateJackpotRewards();
		StartCoroutine(StartPendingActivation());
		WheelofFortuneAnalytics.Instance.Visit();
	}

	private void OnDisable()
	{
		EventDispatch.UnregisterAllInterest(this);
		WheelOfFortuneRewards.Instance.ClearPrizePopulationCount();
		WheelofFortuneAnalytics.Instance.FirstAction(WheelofFortuneAnalytics.Actions.Leave);
	}

	private void Update()
	{
		UpdateTimeRemaining();
		UpdateButtonGraphics();
		if (m_isSpinning)
		{
			float num = Mathf.Min(m_spinSpeed, m_spinSpeed * (m_targetSpinnerAngle - m_currentAngle) / m_slowDownAngle) * IndependantTimeDelta.Delta;
			int num2 = AngleToPrize(m_currentAngle);
			m_currentAngle += num;
			m_eulerAngles.z = 0f - m_currentAngle;
			m_spinnerObject.transform.eulerAngles = m_eulerAngles;
			m_counterSpinnerObject.transform.eulerAngles = -m_eulerAngles;
			m_buttonSpinner.transform.eulerAngles = -m_eulerAngles;
			int num3 = AngleToPrize(m_currentAngle);
			m_spinParticle.transform.position = new Vector3(m_spinParticleAnchor.position.x, m_spinParticleAnchor.position.y, -1f);
			if (num2 != num3)
			{
				m_prizeHighlight[num2].SetActive(value: false);
				m_prizeHighlight[num3].SetActive(value: true);
				Audio.PlayClip(m_spinSound, loop: false);
			}
			if (m_currentAngle >= m_targetSpinnerAngle - m_angleTolerance)
			{
				StartCoroutine(EndSpin());
			}
		}
		else if (m_blockers[0].Blocked && !m_awardingPrize)
		{
			m_blockers[0].Blocked = StoreUtils.IsStoreActive();
		}
	}

	private void UpdateButtonGraphics()
	{
		if (!m_isSpinning)
		{
			if ((DCTimeValidation.TrustedTime || WheelOfFortuneSettings.Instance.FirstFreeSpinAvailable) && WheelOfFortuneSettings.Instance.HasFreeSpin)
			{
				if (!m_freeSpinButton.activeSelf || (!m_freeSpinButton.activeSelf && !m_paidSpinButton.activeSelf))
				{
					m_spinButtonStoreBlocker.ChangeIdleObject(m_freeSpinButton);
					m_freeSpinButton.SetActive(value: true);
					m_paidSpinButton.SetActive(value: false);
				}
			}
			else if (m_freeSpinButton.activeSelf || (!m_freeSpinButton.activeSelf && !m_paidSpinButton.activeSelf))
			{
				m_spinButtonStoreBlocker.ChangeIdleObject(m_paidSpinButton);
				m_freeSpinButton.SetActive(value: false);
				m_paidSpinButton.SetActive(value: true);
			}
			if (!m_cyclingJackpot)
			{
				m_homeButtonIcon.SetActive(value: true);
				m_homeButtonSpinner.SetActive(value: false);
			}
			else
			{
				m_homeButtonIcon.SetActive(value: false);
				m_homeButtonSpinner.SetActive(value: true);
			}
		}
		else
		{
			if (m_freeSpinButton.activeSelf || m_paidSpinButton.activeSelf)
			{
				m_freeSpinButton.SetActive(value: false);
				m_paidSpinButton.SetActive(value: false);
			}
			m_homeButtonIcon.SetActive(value: false);
			m_homeButtonSpinner.SetActive(value: true);
		}
	}

	public void Trigger_PressSpinButton()
	{
		if (!m_homeButtonPressed)
		{
			m_spinButtonPressed = true;
			if (m_cyclingJackpot && m_canForce)
			{
				m_forceStopJackpotCycle = true;
			}
			if ((DCTimeValidation.TrustedTime || WheelOfFortuneSettings.Instance.FirstFreeSpinAvailable) && WheelOfFortuneSettings.Instance.HasFreeSpin)
			{
				Event_OnStorePurchaseCompleted(m_wheelSpinEntry, StorePurchases.Result.Success);
				WheelOfFortuneSettings.Instance.UpdateLastFreeSpinTime();
				WheelofFortuneAnalytics.Instance.UsedFreeSpin();
			}
			else
			{
				StorePurchases.RequestPurchase("Wheel Spin", StorePurchases.LowCurrencyResponse.PurchaseCurrencyAndItem);
				WheelofFortuneAnalytics.Instance.PaidForSpin();
			}
		}
	}

	public void Trigger_HomeButtonPressed()
	{
		if (!m_spinButtonPressed)
		{
			m_homeButtonPressed = true;
			m_menuTriggerRef.SendMessage("Trigger_MoveToPage", base.gameObject, SendMessageOptions.DontRequireReceiver);
		}
	}

	public void Trigger_StopJackpot()
	{
		if (m_cyclingJackpot && !m_forceStopJackpotCycle)
		{
			m_forceStopJackpotCycle = true;
			Audio.PlayClip(m_forceStopPressSound, loop: false);
		}
	}

	private void Event_OnStorePurchaseCompleted(StoreContent.StoreEntry thisEntry, StorePurchases.Result result)
	{
		if (m_wheelSpinEntry != thisEntry)
		{
			return;
		}
		if (result == StorePurchases.Result.Success)
		{
			m_spinButtonPressed = true;
			WheelofFortuneAnalytics.Instance.FirstAction((!WheelOfFortuneSettings.Instance.HasFreeSpin) ? WheelofFortuneAnalytics.Actions.PaidSpin : WheelofFortuneAnalytics.Actions.FreeSpin);
			SpinTheWheel();
			return;
		}
		for (int i = 0; i < m_blockers.Length; i++)
		{
			m_blockers[i].Blocked = false;
		}
		m_spinButtonPressed = false;
	}

	private void SpinTheWheel()
	{
		for (int i = 0; i < m_blockers.Length; i++)
		{
			m_blockers[i].Blocked = true;
		}
		m_buttonSpinner.SetActive(value: true);
		m_awardedPrizeIndex = WheelOfFortuneRewards.Instance.PickWinningPrize();
		m_isJackpot = m_awardedPrizeIndex == 0;
		m_currentAngle %= 360f;
		m_targetSpinnerAngle = m_numberOfSpins * 360f + (float)m_awardedPrizeIndex * m_anglePerSegment + m_angleTolerance;
		float num = (float)DCTime.GetCurrentTime().Millisecond * 0.001f;
		num = 1f - num * num;
		if (DCTime.GetCurrentTime().Second < 30)
		{
			num = 0f - num;
		}
		m_targetSpinnerAngle += num * Mathf.Floor(m_anglePerSegment * 0.5f);
		m_isSpinning = true;
		ParticlePlayer.Play(m_spinParticle);
	}

	private int AngleToPrize(float angle)
	{
		float num = m_anglePerSegment * 0.5f;
		return (int)((angle + num) / 360f % 1f * 8f) % 8;
	}

	private IEnumerator EndSpin()
	{
		m_awardingPrize = true;
		m_isSpinning = false;
		m_buttonSpinner.SetActive(value: false);
		m_spinParticle.Stop();
		if (m_isJackpot)
		{
			m_winJackpotParticle.transform.position = new Vector3(m_prizeMeshes[0].transform.position.x, m_prizeMeshes[0].transform.position.y, -1f);
			Audio.PlayClip(m_jackpotPreRewardSound, loop: false);
			ParticlePlayer.Play(m_winJackpotParticle);
		}
		else
		{
			m_winNormalParticle.transform.position = new Vector3(m_prizeMeshes[m_awardedPrizeIndex].transform.position.x, m_prizeMeshes[m_awardedPrizeIndex].transform.position.y, -1f);
			Audio.PlayClip(m_normalPreRewardSound, loop: false);
			ParticlePlayer.Play(m_winNormalParticle);
		}
		float fixedTimeDelay = 1.6f;
		while (fixedTimeDelay > 0f)
		{
			fixedTimeDelay -= IndependantTimeDelta.Delta;
			yield return null;
		}
		StorePurchases.RequestReward(quantity: int.Parse(m_quantityEntries[m_awardedPrizeIndex].text) / StoreUtils.GetAwardedQuantity(m_wheelPrizes[m_awardedPrizeIndex]), entryID: m_wheelPrizes[m_awardedPrizeIndex].m_identifier, reason: 0, showRewardDialog: StorePurchases.ShowDialog.No);
	}

	private void Event_OnPlayerRewarded()
	{
		int quantity = int.Parse(m_quantityEntries[m_awardedPrizeIndex].text) / StoreUtils.GetAwardedQuantity(m_wheelPrizes[m_awardedPrizeIndex]);
		if (!m_isJackpot)
		{
			Dialog_WOFReward.Display(m_wheelPrizes[m_awardedPrizeIndex], quantity);
			Audio.PlayClip(m_normalRewardSound, loop: false);
			WheelofFortuneAnalytics.Instance.NormalPrizeWon(m_wheelPrizes[m_awardedPrizeIndex], WheelofFortuneAnalytics.PrizeType.Normal);
		}
		else
		{
			Dialog_WOFJackpot.Display(m_wheelPrizes[0], quantity);
			WheelOfFortuneRewards.Instance.ValidateJackpotRewards();
			Audio.PlayClip(m_jackpotRewardSound, loop: false);
			WheelofFortuneAnalytics.Instance.JackpotWon(m_wheelPrizes[0], WheelOfFortuneRewards.GetJackpotType());
		}
	}

	private void Event_OnDialogHidden()
	{
		if (m_awardingPrize)
		{
			if (m_isJackpot)
			{
				m_cachedJackpotChoices = WheelOfFortuneRewards.Instance.GetAllOtherJackpotPrizes(m_wheelPrizes[0]);
				StartCoroutine(PickJackpotPrize());
			}
			else
			{
				for (int i = 0; i < m_blockers.Length; i++)
				{
					m_blockers[i].Blocked = false;
				}
			}
			m_awardingPrize = false;
			m_homeButtonPressed = false;
			m_spinButtonPressed = false;
		}
		else if (m_spinButtonPressed)
		{
			for (int j = 0; j < m_blockers.Length; j++)
			{
				m_blockers[j].Blocked = true;
			}
		}
	}

	private void UpdateTimeRemaining()
	{
		float secondsRemaining = WheelOfFortuneSettings.Instance.GetSecondsRemaining();
		int num = Mathf.FloorToInt(secondsRemaining / 3600f);
		int num2 = Mathf.FloorToInt((secondsRemaining - (float)num * 3600f) / 60f);
		int num3 = Mathf.FloorToInt(secondsRemaining - (float)num * 3600f - (float)num2 * 60f);
		string text = string.Format("{0}:{1}:{2}", num.ToString("D2"), num2.ToString("D2"), num3.ToString("D2"));
		m_timeRemaining.text = text;
		if (!DCTimeValidation.TrustedTime && !WheelOfFortuneSettings.Instance.FirstFreeSpinAvailable)
		{
			m_warningTimeArea.SetActive(value: true);
			m_safeTimeArea.SetActive(value: false);
		}
		else
		{
			m_warningTimeArea.SetActive(value: false);
			m_safeTimeArea.SetActive(value: true);
		}
	}

	private IEnumerator StartPendingActivation()
	{
		yield return null;
		m_blockers[0].Blocked = true;
		m_spinnerObject.transform.rotation = Quaternion.identity;
		m_isSpinning = false;
		m_targetSpinnerAngle = 0f;
		m_prizeHighlight[AngleToPrize(m_currentAngle)].SetActive(value: false);
		m_prizeHighlight[0].SetActive(value: true);
		m_currentAngle = 0f;
		int quantityToAward = 0;
		WheelOfFortuneRewards.Instance.SetRandomSeed();
		RestoreJackpot();
		for (int i = 1; i < 8; i++)
		{
			PopulateWheelDisplay(i, WheelOfFortuneRewards.Instance.GetRandomPrize(out quantityToAward, isJackpotPrize: false), quantityToAward);
		}
	}

	private void PopulateWheelDisplay(int index, StoreContent.StoreEntry entry, int quantity)
	{
		m_quantityEntries[index].text = quantity.ToString();
		m_wheelPrizes[index] = entry;
		m_prizeMeshes[index].mesh = entry.m_mesh;
	}

	private IEnumerator PickJackpotPrize()
	{
		m_cyclingJackpot = true;
		int finalChoiceQuant = 0;
		StoreContent.StoreEntry finalChoice = null;
		do
		{
			finalChoice = WheelOfFortuneRewards.Instance.GetRandomPrize(out finalChoiceQuant, isJackpotPrize: true);
		}
		while (finalChoice.m_identifier == StoreContent.FormatIdentifier(WheelOfFortuneRewards.Instance.CachedJackpotID));
		WheelOfFortuneRewards.Instance.CachedJackpotID = finalChoice.m_identifier;
		WheelOfFortuneRewards.Instance.CachedJackpotQuantity = finalChoiceQuant;
		if (!m_savedDuringCycling)
		{
			PropertyStore.Save();
			m_savedDuringCycling = true;
		}
		int prizeToShow = 0;
		for (int i = 0; i < m_numberOfJackpotPrizeCycles; i++)
		{
			if (m_forceStopJackpotCycle)
			{
				i = m_numberOfJackpotPrizeCycles - m_jackpotCyclesLeftAfterTap;
				m_forceStopJackpotCycle = false;
				m_canForce = false;
			}
			float cycleTime = m_jackpotPrizeCycleTimeMin;
			if (i > m_numberOfJackpotPrizeCycles - m_numberOfSlowJackpotPrizeCycles)
			{
				float factor = (float)(i - (m_numberOfJackpotPrizeCycles - m_numberOfSlowJackpotPrizeCycles)) / (float)m_numberOfSlowJackpotPrizeCycles;
				cycleTime += factor * (m_jackpotPrizeCycleTimeMax - m_jackpotPrizeCycleTimeMin);
			}
			do
			{
				prizeToShow = (prizeToShow + 1) % m_cachedJackpotChoices.Length;
			}
			while (m_cachedJackpotChoices[prizeToShow] == null);
			PopulateWheelDisplay(0, m_cachedJackpotChoices[prizeToShow], WheelOfFortuneRewards.Instance.GetQuantityForNewJackpot(m_cachedJackpotChoices[prizeToShow]));
			Audio.PlayClip(m_cycleJackpotSound, loop: false);
			yield return new WaitForSeconds(cycleTime);
		}
		PopulateWheelDisplay(0, finalChoice, finalChoiceQuant);
		Audio.PlayClip(m_replaceJackpotSound, loop: false);
		for (int i = 0; i < m_blockers.Length; i++)
		{
			m_blockers[i].Blocked = false;
		}
		m_cyclingJackpot = false;
		m_canForce = true;
		m_savedDuringCycling = false;
	}

	private void RestoreJackpot()
	{
		int quantityOut = 0;
		if (WheelOfFortuneRewards.Instance.CachedJackpotID != null && WheelOfFortuneRewards.Instance.CachedJackpotID != string.Empty)
		{
			int chanceToGetJackpot = WheelOfFortuneRewards.Instance.ChanceToGetJackpot;
			int chanceToGetJackpotBundle = WheelOfFortuneRewards.Instance.ChanceToGetJackpotBundle;
			bool cachedIsBundleJackpot = WheelOfFortuneRewards.Instance.CachedIsBundleJackpot;
			bool flag = false;
			for (int i = 0; i < WheelOfFortuneRewards.Instance.JackpotRewardIDs.Length; i++)
			{
				if (WheelOfFortuneRewards.Instance.JackpotRewardIDs[i] == WheelOfFortuneRewards.Instance.CachedJackpotID && WheelOfFortuneRewards.Instance.ValidJackpots[i])
				{
					flag = true;
				}
			}
			if (flag)
			{
				WheelOfFortuneRewards.Instance.GetRandomPrize(out quantityOut, isJackpotPrize: true);
				WheelOfFortuneRewards.Instance.ChanceToGetJackpot = chanceToGetJackpot;
				WheelOfFortuneRewards.Instance.ChanceToGetJackpotBundle = chanceToGetJackpotBundle;
				WheelOfFortuneRewards.Instance.CachedIsBundleJackpot = cachedIsBundleJackpot;
				PopulateWheelDisplay(0, StoreContent.GetStoreEntry(WheelOfFortuneRewards.Instance.CachedJackpotID, StoreContent.Identifiers.Name), WheelOfFortuneRewards.Instance.CachedJackpotQuantity);
				return;
			}
		}
		PopulateWheelDisplay(0, WheelOfFortuneRewards.Instance.GetRandomPrize(out quantityOut, isJackpotPrize: true), quantityOut);
		WheelOfFortuneRewards.Instance.CachedJackpotID = m_wheelPrizes[0].m_identifier;
		WheelOfFortuneRewards.Instance.CachedJackpotQuantity = quantityOut;
	}
}
