using UnityEngine;

public class DCGiftDoor : MonoBehaviour
{
	private enum State
	{
		Closed,
		Opening,
		Open,
		Closing
	}

	private State m_currentState;

	private float m_currentCount;

	private float m_currentScale;

	private float m_realOpenCloseSpeed;

	[SerializeField]
	private Vector3 m_defaultScale = new Vector3(0f, 0f, 0f);

	[SerializeField]
	private float m_openCloseSpeed = 1f;

	[SerializeField]
	private float m_minimumTimeToOpen = 1f;

	[SerializeField]
	private float m_maximumTimeToOpen = 5f;

	[SerializeField]
	private float m_minimumTimeOpen = 1f;

	[SerializeField]
	private float m_maximumTimeOpen = 3f;

	[SerializeField]
	private float m_initialTimeOffset;

	private void OnEnable()
	{
		m_currentCount = Random.Range(m_minimumTimeToOpen, m_maximumTimeToOpen) - m_initialTimeOffset;
		m_currentScale = 1f;
		m_currentState = State.Closed;
		m_realOpenCloseSpeed = 1f / m_openCloseSpeed;
	}

	private void OnDisable()
	{
		base.transform.localScale = m_defaultScale;
	}

	private void Update()
	{
		m_currentCount -= IndependantTimeDelta.Delta;
		switch (m_currentState)
		{
		case State.Closed:
			UpdateStateClosed();
			break;
		case State.Opening:
			UpdateStateOpening();
			break;
		case State.Open:
			UpdateStateOpen();
			break;
		case State.Closing:
			UpdateStateClosing();
			break;
		}
	}

	private float EaseOutExpo(float start, float end, float value)
	{
		end -= start;
		return end * (0f - Mathf.Pow(2f, -10f * value / 1f) + 1f) + start;
	}

	private void UpdateStateClosed()
	{
		if (m_currentCount <= 0f)
		{
			m_currentState = State.Opening;
		}
	}

	private void UpdateStateOpening()
	{
		m_currentScale -= m_realOpenCloseSpeed * IndependantTimeDelta.Delta;
		m_currentScale = Mathf.Clamp(m_currentScale, 0f, 1f);
		float num = EaseOutExpo(0f, 1f, m_currentScale);
		if (num < 0.001f)
		{
			num = 0f;
			m_currentState = State.Open;
			m_currentCount = Random.Range(m_minimumTimeOpen, m_maximumTimeOpen);
		}
		Vector3 defaultScale = m_defaultScale;
		defaultScale.y *= num;
		base.transform.localScale = defaultScale;
	}

	private void UpdateStateOpen()
	{
		if (m_currentCount <= 0f)
		{
			m_currentState = State.Closing;
		}
	}

	private void UpdateStateClosing()
	{
		m_currentScale += m_realOpenCloseSpeed * IndependantTimeDelta.Delta;
		m_currentScale = Mathf.Clamp(m_currentScale, 0f, 1f);
		float num = EaseOutExpo(0f, 1f, m_currentScale);
		if (num > 0.999f)
		{
			num = 1f;
			m_currentState = State.Closed;
			m_currentCount = Random.Range(m_minimumTimeToOpen, m_maximumTimeToOpen);
		}
		Vector3 defaultScale = m_defaultScale;
		defaultScale.y *= num;
		base.transform.localScale = defaultScale;
	}
}
