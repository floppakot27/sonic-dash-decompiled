using System.Collections;
using UnityEngine;

public class HudBoostersDisplayer : MonoBehaviour
{
	private GameObject m_target;

	private bool objectSurvives;

	private bool checkDone;

	private void Start()
	{
		m_target = base.gameObject;
		EventDispatch.RegisterInterest("OnNewGameStarted", this, EventDispatch.Priority.Highest);
	}

	private void Event_OnNewGameStarted()
	{
		objectSurvives = IsObjectNeeded();
		checkDone = true;
		if (base.gameObject.activeInHierarchy)
		{
			StartCoroutine(HideHUD());
		}
	}

	private void OnEnable()
	{
		if (!checkDone)
		{
			objectSurvives = IsObjectNeeded();
			checkDone = true;
		}
		StartCoroutine(HideHUD());
	}

	private IEnumerator HideHUD()
	{
		do
		{
			yield return null;
		}
		while (!m_target.activeInHierarchy);
		if (!objectSurvives)
		{
			m_target.SetActive(value: false);
		}
	}

	private bool IsObjectNeeded()
	{
		return !TutorialSystem.instance().isTrackTutorialEnabled();
	}
}
