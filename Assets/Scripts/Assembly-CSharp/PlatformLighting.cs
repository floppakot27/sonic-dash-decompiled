using UnityEngine;

public class PlatformLighting : MonoBehaviour
{
	[SerializeField]
	private Color m_lowSpecAmbient = new Color(1f, 1f, 1f, 1f);

	[SerializeField]
	private Color m_highSpecAmbient = new Color(0f, 0f, 0f, 0f);

	[SerializeField]
	private Light[] m_lightsToDisable;

	private void Start()
	{
		RenderSettings.ambientLight = m_lowSpecAmbient;
		for (int i = 0; i < m_lightsToDisable.Length; i++)
		{
			if (m_lightsToDisable[i] != null)
			{
				m_lightsToDisable[i].enabled = false;
				m_lightsToDisable[i].gameObject.SetActive(value: false);
			}
		}
	}
}
