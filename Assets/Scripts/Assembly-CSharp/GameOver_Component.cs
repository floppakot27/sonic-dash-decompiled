using UnityEngine;

public class GameOver_Component
{
	protected delegate bool CurrentState(float timeDelta);

	private const float FixedDelay = 0.2f;

	private CurrentState m_currentState;

	private CurrentState m_queuedState;

	private float m_delayCount;

	private float m_currentDelayLength;

	private bool m_active;

	private bool m_componentClosed;

	protected bool Active => m_active;

	protected bool Closed => m_componentClosed;

	public virtual void Reset()
	{
		m_currentState = null;
		m_queuedState = null;
		m_currentDelayLength = 0.2f;
		m_active = true;
		m_componentClosed = false;
	}

	public virtual bool Update(float timeDelta)
	{
		bool flag = true;
		if (m_currentState != null)
		{
			flag = m_currentState(timeDelta);
		}
		m_active = !flag;
		return flag;
	}

	public virtual void Hide()
	{
	}

	public virtual void Show()
	{
	}

	public virtual void ProcessFinished()
	{
	}

	public void TransitionFinished(MonoBehaviour transitioningObject)
	{
		if (m_active)
		{
			ActiveTransitionFinished(transitioningObject);
		}
	}

	public void ComponentClosed()
	{
		m_componentClosed = true;
	}

	public virtual void ActiveTransitionFinished(MonoBehaviour transitioningObject)
	{
	}

	protected void SetDelayTime(float lengthOfDelay)
	{
		m_currentDelayLength = lengthOfDelay;
	}

	protected void SetStateDelegates(CurrentState nextState, CurrentState queuedState)
	{
		m_currentState = nextState;
		m_queuedState = queuedState;
	}

	protected bool DelayUpdate(float timeDelta)
	{
		m_delayCount += timeDelta;
		if (m_delayCount > m_currentDelayLength)
		{
			m_currentState = m_queuedState;
			m_queuedState = null;
			m_delayCount = 0f;
			m_currentDelayLength = 0.2f;
		}
		return false;
	}
}
