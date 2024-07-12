using UnityEngine;

public class FPSDisplay : MonoBehaviour
{
	public float updateInterval = 0.5f;

	private float lastInterval;

	private int frames;

	private float fps;

	private UILabel m_uiLabel;

	private void Start()
	{
		lastInterval = Time.realtimeSinceStartup;
		frames = 0;
		m_uiLabel = GetComponent<UILabel>();
	}

	private void UpdateDisplay()
	{
		if ((bool)m_uiLabel)
		{
			string text = string.Format("{0} FPS", fps.ToString("f2"));
			m_uiLabel.text = text;
		}
	}

	private void Update()
	{
		frames++;
		float realtimeSinceStartup = Time.realtimeSinceStartup;
		if (realtimeSinceStartup > lastInterval + updateInterval)
		{
			fps = (float)frames / (realtimeSinceStartup - lastInterval);
			frames = 0;
			lastInterval = realtimeSinceStartup;
		}
		UpdateDisplay();
	}
}
