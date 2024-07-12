using UnityEngine;

public class DialogContent_GCPendingReward : MonoBehaviour
{
	[SerializeField]
	private UILabel m_descLabel;

	[SerializeField]
	private string m_finalPrizeLocalisationStringID;

	private void OnEnable()
	{
		UpdateDescription();
	}

	private void UpdateDescription()
	{
		bool finalReward = false;
		int amount = 0;
		GC3Progress.GetRewardDue(out amount, out finalReward);
		if (finalReward)
		{
			LocalisedStringProperties component = m_descLabel.GetComponent<LocalisedStringProperties>();
			m_descLabel.text = component.SetLocalisationID(m_finalPrizeLocalisationStringID);
			component.SetLocalisationID(null);
		}
	}
}
