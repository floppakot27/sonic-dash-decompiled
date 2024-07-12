using UnityEngine;

public class BoostersHandHolder : MonoBehaviour
{
	private static bool m_firstTimeHandHoldingDone;

	[SerializeField]
	private GameObject m_playButton;

	[SerializeField]
	private GameObject m_promptButton;

	[SerializeField]
	private UILabel m_promptLabel;

	private static string HandHoldingDoneSaveProperty => "BoostersHandHoldingDone";

	private static bool FirstTimeHandHoldingDone => m_firstTimeHandHoldingDone;

	private void Awake()
	{
		EventDispatch.RegisterInterest("OnGameDataLoaded", this);
		EventDispatch.RegisterInterest("OnGameDataSaveRequest", this);
	}

	public void ShowCorrectButton()
	{
		if (!m_firstTimeHandHoldingDone)
		{
			m_playButton.SetActive(value: false);
			m_promptButton.SetActive(value: true);
		}
		else
		{
			m_playButton.SetActive(value: true);
			m_promptButton.SetActive(value: false);
		}
	}

	public void UpdatePromptButtonText(int boostersLeft)
	{
		if (m_firstTimeHandHoldingDone)
		{
			return;
		}
		if (boostersLeft > 0)
		{
			if (m_playButton.activeSelf)
			{
				m_playButton.SetActive(value: false);
				m_promptButton.SetActive(value: true);
			}
			string empty = string.Empty;
			string empty2 = string.Empty;
			empty = ((boostersLeft != 1) ? LanguageStrings.First.GetString("BOOSTERS_SELECT") : LanguageStrings.First.GetString("BOOSTERS_SELECT_SINGULAR"));
			empty2 = string.Format(empty, boostersLeft);
			m_promptLabel.text = empty2;
		}
		else
		{
			m_playButton.SetActive(value: true);
			m_promptButton.SetActive(value: false);
		}
	}

	public void CompleteHandholding()
	{
		m_firstTimeHandHoldingDone = true;
	}

	public bool CanRun()
	{
		return m_playButton.activeSelf;
	}

	private void Event_OnGameDataLoaded(ActiveProperties ap)
	{
		m_firstTimeHandHoldingDone = ap.GetBool(HandHoldingDoneSaveProperty);
	}

	private void Event_OnGameDataSaveRequest()
	{
		PropertyStore.Store(HandHoldingDoneSaveProperty, m_firstTimeHandHoldingDone);
	}
}
