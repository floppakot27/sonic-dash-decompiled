using System.Collections;
using UnityEngine;

public class DCChallengeDisplay : MonoBehaviour
{
	private enum DayHighlight
	{
		Normal,
		Mystery
	}

	private const string ChallengeActiveTime = "DAILY_CHALLENGE_TIMER";

	private const string ChallengeCompletedTime = "DAILY_CHALLENGE_TIMER_NEXT";

	private const string ChallengePeicesRemaining = "DAILY_CHALLENGE_COLLECT_COUNT";

	private const string ChallengeWaitingForConnection = "DAILY_CHALLENGE_WAITING_FOR_CONNECTION";

	private int m_currentDay;

	private bool[] m_peiceState;

	private MenuMeshMover[] m_peiceMovers;

	private MenuMeshBobble[] m_peiceBobbler;

	[SerializeField]
	private MeshRenderer[] m_jigsawPeices;

	[SerializeField]
	private MeshFilter[] m_meshEntries;

	[SerializeField]
	private UILabel[] m_quantityEntries;

	[SerializeField]
	private GameObject[] m_dayPrizeRoots;

	[SerializeField]
	private GameObject[] m_dayHighlights;

	[SerializeField]
	private MeshFilter[] m_finalDayPrizes;

	[SerializeField]
	private UILabel[] m_finalDayQuantity;

	[SerializeField]
	private GameObject[] m_challengeComplete;

	[SerializeField]
	private Mesh m_mysteryPrizeMesh;

	[SerializeField]
	private MeshFilter m_currentMesh;

	[SerializeField]
	private UILabel m_countRemaining;

	[SerializeField]
	private UILabel m_currentQuantity;

	[SerializeField]
	private UILabel m_timeDescription;

	[SerializeField]
	private UILabel m_timeRemaining;

	[SerializeField]
	private AudioClip m_rewardGivenClip;

	private void Update()
	{
		UpdateTimeRemaining();
		int currentDayNumber = DCs.GetCurrentDayNumber();
		if (currentDayNumber != m_currentDay)
		{
			RefreshCurrentDailyChallenge();
		}
	}

	private void OnEnable()
	{
		m_currentDay = DCs.GetCurrentDayNumber();
		m_peiceState = DCs.GetPieces();
		StartCoroutine(StartPendingActivation());
		if (DCs.AllPiecesCollected() && !DCs.ChallengeRewarded)
		{
			StartCoroutine(StartPendingReward());
		}
	}

	private IEnumerator StartPendingActivation()
	{
		yield return null;
		for (int i = 0; i < 4; i++)
		{
			int quantityToAward = 0;
			StoreContent.StoreEntry entryToAward = DCRewards.GetDailyReward(i, out quantityToAward, getFinalQuantity: true);
			PopulateGiftDisplay(i, entryToAward, quantityToAward);
		}
		CacheDisplayProperties();
		SetJigsawState();
		SetCurrentPrize();
		SetDayHighlight();
		SetCompletedHighlight();
		SetRemainingPeices();
		SetFinalDayPrizes();
	}

	private IEnumerator StartPendingFinalDayReward()
	{
		for (int i = 0; i < 15; i++)
		{
			yield return null;
		}
		int quantity;
		StoreContent.StoreEntry rewardEntry = DCRewards.GetFinalDayReward(out quantity, getFinalQuantity: false);
		StorePurchases.RequestReward(rewardEntry.m_identifier, quantity, 9, StorePurchases.ShowDialog.Yes);
		Audio.PlayClip(m_rewardGivenClip, loop: false);
		DCs.ChallengeRewarded = true;
		GameAnalytics.DCDay5Rewarded(quantity, rewardEntry.m_identifier);
		EventDispatch.GenerateEvent("OnDCRewarded");
		PropertyStore.Save();
	}

	private IEnumerator StartPendingReward()
	{
		float fixedTimeDelay = 1f;
		while (fixedTimeDelay > 0f)
		{
			fixedTimeDelay -= IndependantTimeDelta.Delta;
			yield return null;
		}
		if (m_currentDay < 4)
		{
			int quantity;
			StoreContent.StoreEntry rewardEntry = DCRewards.GetDailyReward(m_currentDay, out quantity, getFinalQuantity: false);
			StorePurchases.RequestReward(rewardEntry.m_identifier, quantity, 10, StorePurchases.ShowDialog.Yes);
			Audio.PlayClip(m_rewardGivenClip, loop: false);
			GameAnalytics.DCCompleted();
			DCs.ChallengeRewarded = true;
			EventDispatch.GenerateEvent("OnDCRewarded");
			PropertyStore.Save();
		}
		else
		{
			DialogStack.ShowDialog("Daily Challenge Final Reward");
		}
	}

	private void PopulateGiftDisplay(int index, StoreContent.StoreEntry entry, int quantity)
	{
		if (quantity > 1)
		{
			m_quantityEntries[index].text = quantity.ToString();
		}
		else
		{
			m_quantityEntries[index].text = string.Empty;
		}
		m_meshEntries[index].mesh = entry.m_mesh;
	}

	private void CacheDisplayProperties()
	{
		if (m_peiceBobbler == null || m_peiceMovers == null)
		{
			m_peiceMovers = new MenuMeshMover[m_jigsawPeices.Length];
			m_peiceBobbler = new MenuMeshBobble[m_jigsawPeices.Length];
			for (int i = 0; i < m_jigsawPeices.Length; i++)
			{
				m_peiceMovers[i] = m_jigsawPeices[i].GetComponent<MenuMeshMover>();
				m_peiceBobbler[i] = m_jigsawPeices[i].GetComponent<MenuMeshBobble>();
			}
		}
	}

	private void SetJigsawState()
	{
		bool flag = DCs.AllPiecesCollected();
		for (int i = 0; i < m_jigsawPeices.Length; i++)
		{
			float a = ((!m_peiceState[i]) ? 0.3f : 1f);
			Material material = m_jigsawPeices[i].material;
			Color color = material.color;
			color.a = a;
			material.color = color;
			m_peiceMovers[i].enabled = !flag;
			m_peiceBobbler[i].enabled = !flag;
		}
	}

	private void SetCurrentPrize()
	{
		if (m_currentMesh == null || m_currentQuantity == null)
		{
			return;
		}
		if (m_currentDay == 4)
		{
			m_currentQuantity.text = string.Empty;
			m_currentMesh.mesh = m_mysteryPrizeMesh;
			return;
		}
		int quantity;
		StoreContent.StoreEntry dailyReward = DCRewards.GetDailyReward(m_currentDay, out quantity, getFinalQuantity: true);
		if (quantity > 1)
		{
			m_currentQuantity.text = quantity.ToString();
		}
		else
		{
			m_currentQuantity.text = string.Empty;
		}
		m_currentMesh.mesh = dailyReward.m_mesh;
	}

	private void SetDayHighlight()
	{
		if (m_currentDay == 4)
		{
			m_dayHighlights[0].SetActive(value: false);
			m_dayHighlights[1].SetActive(value: true);
			return;
		}
		Vector3 vector = m_dayHighlights[0].transform.parent.transform.position - m_dayHighlights[0].transform.position;
		m_dayHighlights[0].transform.parent = m_dayPrizeRoots[m_currentDay].transform;
		m_dayHighlights[0].transform.position = m_dayPrizeRoots[m_currentDay].transform.position - vector;
		m_dayHighlights[0].SetActive(value: false);
		m_dayHighlights[0].SetActive(value: true);
		m_dayHighlights[1].SetActive(value: false);
	}

	private void SetCompletedHighlight()
	{
		for (int i = 0; i < m_challengeComplete.Length; i++)
		{
			if (i < m_currentDay)
			{
				m_challengeComplete[i].SetActive(value: true);
			}
			else if (m_currentDay == i)
			{
				bool active = DCs.AllPiecesCollected();
				m_challengeComplete[i].SetActive(active);
			}
			else
			{
				m_challengeComplete[i].SetActive(value: false);
			}
		}
	}

	private void SetRemainingPeices()
	{
		int collectedPeicesCount = GetCollectedPeicesCount();
		int num = DCs.NumberOfPeices - collectedPeicesCount;
		if (num == 0 && DCs.GetSecondsRemaining() == 0f && !DCTimeValidation.TrustedTime)
		{
			m_countRemaining.text = LanguageStrings.First.GetString("DAILY_CHALLENGE_WAITING_FOR_CONNECTION");
			m_countRemaining.cachedTransform.localScale = new Vector3(40f, 40f, 0f);
			m_countRemaining.gameObject.SetActive(value: true);
		}
		else
		{
			string @string = LanguageStrings.First.GetString("DAILY_CHALLENGE_COLLECT_COUNT");
			m_countRemaining.text = string.Format(@string, num);
			m_countRemaining.cachedTransform.localScale = new Vector3(66f, 66f, 0f);
			m_countRemaining.gameObject.SetActive(num != 0);
		}
	}

	private void SetFinalDayPrizes()
	{
		for (int i = 0; i < 5; i++)
		{
			int quantity;
			StoreContent.StoreEntry finalDayReward = DCRewards.GetFinalDayReward(i, out quantity, getFinalQuantity: true);
			if (quantity > 1)
			{
				m_finalDayQuantity[i].text = quantity.ToString();
			}
			else
			{
				m_finalDayQuantity[i].text = string.Empty;
			}
			m_finalDayPrizes[i].mesh = finalDayReward.m_mesh;
		}
	}

	private void UpdateTimeRemaining()
	{
		float secondsRemaining = DCs.GetSecondsRemaining();
		int num = Mathf.FloorToInt(secondsRemaining / 3600f);
		int num2 = Mathf.FloorToInt((secondsRemaining - (float)num * 3600f) / 60f);
		int num3 = Mathf.FloorToInt(secondsRemaining - (float)num * 3600f - (float)num2 * 60f);
		string id = ((!DCs.AllPiecesCollected()) ? "DAILY_CHALLENGE_TIMER" : "DAILY_CHALLENGE_TIMER_NEXT");
		m_timeDescription.text = LanguageStrings.First.GetString(id);
		string text = string.Format("{0}:{1}:{2}", num.ToString("D2"), num2.ToString("D2"), num3.ToString("D2"));
		m_timeRemaining.text = text;
	}

	private void RefreshCurrentDailyChallenge()
	{
		m_currentDay = DCs.GetCurrentDayNumber();
		m_peiceState = DCs.GetPieces();
		SetJigsawState();
		SetCurrentPrize();
		SetDayHighlight();
		SetCompletedHighlight();
		SetRemainingPeices();
	}

	private int GetCollectedPeicesCount()
	{
		bool[] pieces = DCs.GetPieces();
		int num = 0;
		for (int i = 0; i < pieces.Length; i++)
		{
			if (pieces[i])
			{
				num++;
			}
		}
		return num;
	}

	private void Trigger_AwardFinalDayReward()
	{
		StartCoroutine(StartPendingFinalDayReward());
	}
}
