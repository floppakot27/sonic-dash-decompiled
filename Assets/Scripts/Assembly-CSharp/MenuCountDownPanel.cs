using System.Collections;
using AnimationOrTween;
using UnityEngine;

public class MenuCountDownPanel : MonoBehaviour
{
	private const int MAX_COUNT = 3;

	public GameObject m_countDownTrigger;

	private Animation m_displayAnimation;

	private ActiveAnimation m_activeAnimation;

	private UILabel m_label;

	private int m_currentCount = -1;

	private bool m_triggerValid = true;

	private void OnEnable()
	{
		EventDispatch.RegisterInterest("OnCountDownReset", this);
	}

	private void OnDisable()
	{
		EventDispatch.UnregisterInterest("OnCountDownReset", this);
	}

	private void FindRequiredComponents()
	{
		if (!(m_displayAnimation != null) || !(m_label != null))
		{
			m_displayAnimation = GetComponentInChildren<Animation>();
			m_activeAnimation = GetComponentInChildren<ActiveAnimation>();
			m_label = GetComponentInChildren<UILabel>();
		}
	}

	private void StartAnimation()
	{
		SetValidCountDownLabel();
		m_displayAnimation.Stop();
		ActiveAnimation.Play(m_displayAnimation, null, Direction.Forward, EnableCondition.EnableThenPlay, DisableCondition.DisableAfterReverse);
	}

	private bool CanCountDownUpdate()
	{
		if (m_label == null)
		{
			return false;
		}
		if (m_displayAnimation == null)
		{
			return false;
		}
		if (m_currentCount < 0)
		{
			return false;
		}
		return true;
	}

	private void SetValidCountDownLabel()
	{
		if (m_currentCount == 0)
		{
			m_label.text = "NULL";
		}
		else
		{
			m_label.text = m_currentCount.ToString().Trim();
		}
	}

	private IEnumerator CountDown()
	{
		yield return null;
		FindRequiredComponents();
		float timerWait = 1f;
		m_currentCount = 3;
		while (m_currentCount > 0)
		{
			StartAnimation();
			if (m_currentCount > 0)
			{
				SendMessage("OnClick", this, SendMessageOptions.DontRequireReceiver);
			}
			m_currentCount--;
			timerWait = 1f;
			while (timerWait > 0f)
			{
				timerWait -= IndependantTimeDelta.Delta;
				yield return null;
			}
		}
		GameState.RequestMode(GameState.Mode.Game);
		if ((bool)m_countDownTrigger)
		{
			m_countDownTrigger.SendMessage("OnClick", SendMessageOptions.DontRequireReceiver);
		}
		m_countDownTrigger.SetActive(value: false);
	}

	private void Trigger_CountDownStarted()
	{
		if (!m_triggerValid)
		{
			m_triggerValid = true;
			return;
		}
		m_countDownTrigger.SetActive(value: true);
		m_currentCount = 3;
		StartCoroutine(CountDown());
		m_triggerValid = false;
	}

	private void Event_OnCountDownReset()
	{
		m_currentCount = 3;
	}
}
