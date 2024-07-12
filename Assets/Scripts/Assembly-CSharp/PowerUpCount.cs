using UnityEngine;

public class PowerUpCount : MonoBehaviour
{
	public PowerUps.Type m_powerUpType = PowerUps.Type.Magnet;

	private UILabel m_countLabel;

	private void Start()
	{
		m_countLabel = GetComponent<UILabel>();
	}

	private void Update()
	{
		if (!(m_countLabel == null))
		{
			int powerUpCount = PowerUpsInventory.GetPowerUpCount(m_powerUpType);
			m_countLabel.text = powerUpCount.ToString();
		}
	}
}
