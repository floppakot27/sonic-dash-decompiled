using UnityEngine;

public class GCTriggerSelector : MonoBehaviour
{
	[SerializeField]
	private GameObject m_gcActiveTrigger;

	[SerializeField]
	private GameObject m_gcInactiveTrigger;

	[SerializeField]
	private GameObject m_gcCompleteTrigger;

	private GameObject m_cachedLastTrigger;

	private void OnClick()
	{
		if (m_cachedLastTrigger != null)
		{
			m_cachedLastTrigger.SendMessage("OnClick", SendMessageOptions.DontRequireReceiver);
			m_cachedLastTrigger = null;
			return;
		}
		if (GC3Progress.IsRewardDue())
		{
			m_cachedLastTrigger = m_gcActiveTrigger;
			m_gcActiveTrigger.SendMessage("OnClick", SendMessageOptions.DontRequireReceiver);
			return;
		}
		if (GC3Progress.GetGC3LocalTierCurrent() == 4 && GC3Progress.GetGC3GlobalTierCurrent() == 4)
		{
			m_cachedLastTrigger = m_gcCompleteTrigger;
			m_gcCompleteTrigger.SendMessage("OnClick", SendMessageOptions.DontRequireReceiver);
			return;
		}
		switch (GCState.ChallengeState(GCState.Challenges.gc3))
		{
		case GCState.State.Inactive:
			m_cachedLastTrigger = m_gcInactiveTrigger;
			m_gcInactiveTrigger.SendMessage("OnClick", SendMessageOptions.DontRequireReceiver);
			break;
		case GCState.State.Active:
			m_cachedLastTrigger = m_gcActiveTrigger;
			m_gcActiveTrigger.SendMessage("OnClick", SendMessageOptions.DontRequireReceiver);
			break;
		case GCState.State.Finished:
			m_cachedLastTrigger = m_gcInactiveTrigger;
			m_gcInactiveTrigger.SendMessage("OnClick", SendMessageOptions.DontRequireReceiver);
			break;
		}
	}
}
