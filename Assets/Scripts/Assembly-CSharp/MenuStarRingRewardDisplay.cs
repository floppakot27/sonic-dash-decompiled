using UnityEngine;

public class MenuStarRingRewardDisplay : MonoBehaviour
{
	private bool m_firstTime = true;

	private void OnEnable()
	{
		if (m_firstTime)
		{
			if (PlayerStats.GetCurrentStats().m_trackedStats[79] == 1)
			{
				GameAnalytics.FirstGame();
			}
			StarRingsRewards.Reward(StarRingsRewards.Reason.Returning);
			m_firstTime = false;
		}
	}
}
