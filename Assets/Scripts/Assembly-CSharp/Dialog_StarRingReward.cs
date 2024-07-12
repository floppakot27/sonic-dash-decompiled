using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dialog_StarRingReward : MonoBehaviour
{
	private const int ContentPoolSize = 2;

	private const string ReasonTag = "RewardDialog_Reason";

	private static string[] ReasonStrings = new string[5] { "REWARD_BRAG", "REWARD_WELCOME_BACK", "REWARD_1STPLACE", "REWARD_SEGAID", "REWARD_SINGLE_STAR" };

	private GameObject m_reasonObject;

	private static Stack<StarRingsRewards.Reason> s_stackedContent = null;

	private static Stack<StarRingsRewards.Reason> StackedContent
	{
		get
		{
			EnsureStackExists();
			return s_stackedContent;
		}
	}

	public void SetContent(StarRingsRewards.Reason reason)
	{
		StackedContent.Push(reason);
	}

	public static void Display(StarRingsRewards.Reason reason)
	{
		DialogStack.ShowDialog("Star Ring Reward");
		StackedContent.Push(reason);
	}

	private static void EnsureStackExists()
	{
		if (s_stackedContent == null)
		{
			s_stackedContent = new Stack<StarRingsRewards.Reason>(2);
		}
	}

	private void OnEnable()
	{
		StartCoroutine(StartPendingActivation());
	}

	private void OnDestroy()
	{
		StackedContent.Clear();
	}

	private void CacheDialogComponents()
	{
		if (!(m_reasonObject != null))
		{
			m_reasonObject = Utils.FindTagInChildren(base.gameObject, "RewardDialog_Reason");
		}
	}

	private void UpdateContent()
	{
		if (base.gameObject.activeInHierarchy && !(m_reasonObject == null))
		{
			StarRingsRewards.Reason thisReward = StackedContent.Peek();
			UpdateReasonString(thisReward);
		}
	}

	private void UpdateReasonString(StarRingsRewards.Reason thisReward)
	{
		m_reasonObject.SetActive(value: true);
		LocalisedStringProperties.SetLocalisedString(m_reasonObject, ReasonStrings[(int)thisReward]);
	}

	private IEnumerator StartPendingActivation()
	{
		yield return null;
		CacheDialogComponents();
		UpdateContent();
	}

	private void DialogPopped()
	{
		StackedContent.Pop();
	}
}
