using UnityEngine;

public class DialogContent_BoosterAnnounce : MonoBehaviour
{
	[SerializeField]
	private UILabel m_announceDescLabel;

	private void OnEnable()
	{
		UpdateDescription();
	}

	private void UpdateDescription()
	{
		LocalisedStringProperties component = m_announceDescLabel.GetComponent<LocalisedStringProperties>();
		string text = string.Format(component.GetLocalisedString(), Boosters.GoldenEnemyScoreMultipler);
		m_announceDescLabel.text = text;
		component.SetLocalisationID(null);
	}
}
