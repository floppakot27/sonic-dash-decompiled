using UnityEngine;

public class MenuStarRingsRewardsDialogsPanel : MonoBehaviour
{
	private int m_currentEntry;

	private int m_dialogCount;

	private void Start()
	{
		m_currentEntry = 0;
		m_dialogCount = Utils.GetEnumCount<StarRingsRewards.Reason>();
	}

	private void Trigger_ShowNextDialog()
	{
		Dialog_StarRingReward.Display((StarRingsRewards.Reason)m_currentEntry);
		m_currentEntry++;
		if (m_currentEntry >= m_dialogCount)
		{
			m_currentEntry = 0;
		}
	}
}
