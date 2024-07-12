using System.Collections;
using UnityEngine;

public class GCButtonDisplayer : MonoBehaviour
{
	[SerializeField]
	public bool m_ForceShowOnEditor;

	[SerializeField]
	public bool m_showWhenNotActive;

	private GameObject m_target;

	private void Start()
	{
		m_target = base.gameObject;
	}

	private void OnEnable()
	{
		StartCoroutine(HideButtons());
	}

	private IEnumerator HideButtons()
	{
		do
		{
			yield return null;
		}
		while (!m_target.activeInHierarchy);
		if (!IsObjectNeeded())
		{
			m_target.SetActive(value: false);
		}
	}

	private bool IsObjectNeeded()
	{
		bool flag = GCState.ChallengeState(GCState.Challenges.gc3) == GCState.State.Active || GCState.ChallengeState(GCState.Challenges.gc3) == GCState.State.Finished;
		if (m_showWhenNotActive)
		{
			return !flag;
		}
		return flag;
	}
}
