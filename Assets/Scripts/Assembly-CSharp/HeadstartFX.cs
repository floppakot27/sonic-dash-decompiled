using UnityEngine;

public class HeadstartFX : MonoBehaviour
{
	private bool m_activated;

	private bool m_blink;

	private void Start()
	{
		m_activated = false;
		for (int i = 0; i < base.transform.childCount; i++)
		{
			Transform child = base.transform.GetChild(i);
			child.gameObject.SetActive(value: false);
		}
	}

	private void Update()
	{
		bool flag = false;
		if (HeadstartMonitor.instance().isHeadstarting())
		{
			if (HeadstartMonitor.instance().isHeadstartNearlyFinished())
			{
				m_blink = !m_blink;
				flag = m_blink;
			}
			else
			{
				flag = true;
			}
		}
		else
		{
			flag = false;
		}
		if (m_activated)
		{
			if (!flag)
			{
				for (int i = 0; i < base.transform.childCount; i++)
				{
					Transform child = base.transform.GetChild(i);
					child.gameObject.SetActive(value: false);
				}
				m_activated = false;
			}
		}
		else if (flag)
		{
			for (int j = 0; j < base.transform.childCount; j++)
			{
				Transform child2 = base.transform.GetChild(j);
				child2.gameObject.SetActive(value: true);
			}
			m_activated = true;
		}
	}
}
