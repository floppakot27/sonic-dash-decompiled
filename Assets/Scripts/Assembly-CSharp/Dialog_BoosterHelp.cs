using UnityEngine;

public class Dialog_BoosterHelp : MonoBehaviour
{
	[SerializeField]
	private UILabel m_springBonusLabel;

	[SerializeField]
	private UILabel m_enemyComboLabel;

	[SerializeField]
	private UILabel m_ringStreakLabel;

	[SerializeField]
	private UILabel m_scoreBonusLabel;

	[SerializeField]
	private UILabel m_goldenBadnikLabel;

	private void OnEnable()
	{
		SetLocalisedLabel(m_springBonusLabel, PowerUps.Type.Booster_SpringBonus, "BOOSTERS_DESC_SPRING_BOOSTER");
		SetLocalisedLabel(m_enemyComboLabel, PowerUps.Type.Booster_EnemyComboBonus, "BOOSTERS_DESC_COMBO_BOOSTER");
		SetLocalisedLabel(m_ringStreakLabel, PowerUps.Type.Booster_RingStreakBonus, "BOOSTERS_DESC_STREAK_BOOSTER");
		SetLocalisedLabel(m_scoreBonusLabel, PowerUps.Type.Booster_ScoreMultiplier, "BOOSTERS_DESC_SCORE_BOOSTER");
		SetLocalisedLabel(m_goldenBadnikLabel, PowerUps.Type.Booster_GoldenEnemy, "BOOSTERS_DESC_GOLDEN_BADNIK");
	}

	private void SetLocalisedLabel(UILabel descriptionLabel, PowerUps.Type boosterType, string stringId)
	{
		LocalisedStringProperties component = descriptionLabel.GetComponent<LocalisedStringProperties>();
		switch (boosterType)
		{
		case PowerUps.Type.Booster_ScoreMultiplier:
		{
			string string2 = LanguageStrings.First.GetString(stringId);
			string text2 = string.Format(string2, Boosters.ScoreMultiplier * 100f - 100f);
			descriptionLabel.text = text2;
			component.SetLocalisationID(null);
			break;
		}
		case PowerUps.Type.Booster_GoldenEnemy:
		{
			string @string = LanguageStrings.First.GetString(stringId);
			string text = string.Format(@string, Boosters.GoldenEnemyScoreMultipler);
			descriptionLabel.text = text;
			component.SetLocalisationID(null);
			break;
		}
		default:
			component.SetLocalisationID(stringId);
			break;
		}
		LocalisedStringStatic component2 = descriptionLabel.GetComponent<LocalisedStringStatic>();
		component2.ForceStringUpdate();
	}
}
