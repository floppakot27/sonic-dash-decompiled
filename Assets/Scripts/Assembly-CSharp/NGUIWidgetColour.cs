using UnityEngine;

public class NGUIWidgetColour : MonoBehaviour
{
	public Color m_widgetColour = new Color(1f, 1f, 1f, 1f);

	public UIWidget m_targetWidget;

	private void Start()
	{
		if (m_targetWidget == null)
		{
			m_targetWidget = GetComponent<UIWidget>();
		}
	}

	private void Update()
	{
		if (!(m_targetWidget == null))
		{
			m_targetWidget.color = m_widgetColour;
		}
	}
}
