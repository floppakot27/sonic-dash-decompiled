using System.Collections;
using UnityEngine;

public class Dialog_BoosterBreadcrumb : MonoBehaviour
{
	private const string BreadcrumbDescription = "BOOSTERS_DESC_{0}";

	private const string BreadcrumbTitle = "BOOSTERS_TITLE_{0}";

	private static StoreContent.StoreEntry s_booster;

	private string m_boosterName = string.Empty;

	[SerializeField]
	private UILabel m_descriptionObject;

	[SerializeField]
	private UILabel m_titleObject;

	[SerializeField]
	private MeshFilter m_meshObject;

	[SerializeField]
	private TimedButtonTriggerProperties m_buttonTriggerTimer;

	[SerializeField]
	private UILabel m_okLabel;

	[SerializeField]
	private UISlicedSprite m_okGlow;

	[SerializeField]
	private UISlicedSprite m_okBackground;

	[SerializeField]
	private UISprite m_okIcon;

	public static void Display(StoreContent.StoreEntry booster)
	{
		s_booster = booster;
		DialogStack.ShowDialog("Booster Breadcrumb Dialog");
	}

	private void OnEnable()
	{
		StartCoroutine(StartPendingActivation());
	}

	private void OnDisable()
	{
		m_buttonTriggerTimer.Reset();
		m_okLabel.color = new Color(m_okLabel.color.r, m_okLabel.color.g, m_okLabel.color.b, 0f);
		m_okGlow.color = new Color(m_okGlow.color.r, m_okGlow.color.g, m_okGlow.color.b, 0f);
		m_okBackground.color = new Color(m_okBackground.color.r, m_okBackground.color.g, m_okBackground.color.b, 0f);
		m_okIcon.color = new Color(m_okIcon.color.r, m_okIcon.color.g, m_okIcon.color.b, 0f);
		m_okLabel.gameObject.SetActive(value: false);
		m_okGlow.gameObject.SetActive(value: false);
		m_okBackground.gameObject.SetActive(value: false);
		m_okIcon.gameObject.SetActive(value: false);
	}

	private IEnumerator StartPendingActivation()
	{
		yield return null;
		switch (s_booster.m_identifier)
		{
		case "booster_spring_bonus":
			m_boosterName = "SPRING_BOOSTER";
			break;
		case "booster_score_multiplier":
			m_boosterName = "SCORE_BOOSTER";
			break;
		case "booster_golden_enemy":
			m_boosterName = "GOLDEN_BADNIK";
			break;
		case "booster_enemy_combo":
			m_boosterName = "COMBO_BOOSTER";
			break;
		case "booster_ring_streak":
			m_boosterName = "STREAK_BOOSTER";
			break;
		}
		UpdateContent();
	}

	private void UpdateContent()
	{
		if (base.gameObject.activeInHierarchy)
		{
			m_titleObject.text = LanguageStrings.First.GetString($"BOOSTERS_TITLE_{m_boosterName}");
			if (m_boosterName == "SCORE_BOOSTER")
			{
				string @string = LanguageStrings.First.GetString($"BOOSTERS_DESC_{m_boosterName}");
				string text = string.Format(@string, Boosters.ScoreMultiplier * 100f - 100f);
				m_descriptionObject.text = text;
			}
			else if (m_boosterName == "GOLDEN_BADNIK")
			{
				string string2 = LanguageStrings.First.GetString($"BOOSTERS_DESC_{m_boosterName}");
				string text2 = string.Format(string2, Boosters.GoldenEnemyScoreMultipler);
				m_descriptionObject.text = text2;
			}
			else
			{
				m_descriptionObject.text = LanguageStrings.First.GetString($"BOOSTERS_DESC_{m_boosterName}");
			}
			m_meshObject.mesh = s_booster.m_mesh;
		}
	}
}
