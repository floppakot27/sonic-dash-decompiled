using UnityEngine;

public class OfferRegion_Timed : OfferRegion
{
	private float m_currentTimer;

	[SerializeField]
	private float m_openTimer = 2f;

	protected override void OnRegionActivated()
	{
		m_currentTimer = 0f;
	}

	private void Update()
	{
		if (base.Active)
		{
			UpdateActive();
		}
	}

	private void UpdateActive()
	{
		m_currentTimer += IndependantTimeDelta.Delta;
		if (m_currentTimer >= m_openTimer)
		{
			Leave();
		}
	}
}
