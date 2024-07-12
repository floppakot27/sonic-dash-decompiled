using UnityEngine;

public class PowerUpsDisplay : MonoBehaviour
{
	[SerializeField]
	private PowerUps.Type m_powerUpType = PowerUps.Type.Magnet;

	private UILabel m_powerUpLabel;

	private void Start()
	{
		m_powerUpLabel = GetComponent<UILabel>();
	}

	private void Update()
	{
		int powerUpCount = PowerUpsInventory.GetPowerUpCount(m_powerUpType);
		m_powerUpLabel.text = LanguageUtils.FormatNumber(powerUpCount);
	}
}
