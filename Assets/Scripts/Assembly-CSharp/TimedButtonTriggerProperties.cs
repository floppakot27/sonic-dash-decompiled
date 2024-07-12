using System.Collections;
using UnityEngine;

public class TimedButtonTriggerProperties : MonoBehaviour
{
	[SerializeField]
	private float m_timeLimit = 1f;

	[SerializeField]
	private GameObject m_trigger;

	[SerializeField]
	private GuiButtonBlocker m_blocker;

	private float m_timerCount;

	public void OnEnable()
	{
		m_blocker.Blocked = true;
		StartCoroutine(CountDownTimer());
	}

	private IEnumerator CountDownTimer()
	{
		while (m_timeLimit > m_timerCount)
		{
			m_timerCount += IndependantTimeDelta.Delta;
			yield return null;
		}
		m_trigger.SendMessage("OnClick", SendMessageOptions.RequireReceiver);
	}

	private void Trigger_Finished()
	{
		m_blocker.Blocked = false;
	}

	public void Reset()
	{
		m_timerCount = 0f;
	}
}
