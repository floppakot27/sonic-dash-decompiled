using UnityEngine;

public class DebugMenuCohortAndMID : MonoBehaviour
{
	[SerializeField]
	private UILabel m_playerIDValue;

	[SerializeField]
	private UILabel m_CohortValue;

	private void OnEnable()
	{
		m_playerIDValue.text = UserIdentification.Current;
		m_CohortValue.text = ABTesting.Cohort.ToString();
	}
}
