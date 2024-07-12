using System.Collections.Generic;
using System.Linq;

public class MotionStateMachine
{
	private Stack<MotionState> m_stateStack = new Stack<MotionState>();

	private MotionState m_pendingState;

	public bool HasActiveState => m_stateStack.Count > 0;

	public MotionState CurrentState => m_stateStack.Peek();

	public MotionState PendingState => m_pendingState;

	public MotionStateMachine(MotionState firstState)
	{
		m_pendingState = firstState;
	}

	public void RequestState(MotionState newState)
	{
		if (m_pendingState == null && newState != null)
		{
			m_pendingState = newState;
		}
	}

	public void ForceState(MotionState newState)
	{
		if (newState != null)
		{
			m_pendingState = newState;
		}
	}

	public void Update()
	{
		ActivatePendingState();
		if (CurrentState != null)
		{
			CurrentState.Execute();
		}
	}

	public void PopTopState()
	{
		if (CurrentState != null)
		{
			CurrentState.Exit();
			EventDispatch.UnregisterAllInterest(CurrentState);
		}
		m_stateStack.Pop();
		if (m_pendingState == null)
		{
			m_stateStack.Peek().Enter();
		}
	}

	public void ShutDown()
	{
		if (!m_stateStack.Any())
		{
			return;
		}
		m_stateStack.Peek().Exit();
		foreach (MotionState item in m_stateStack)
		{
			EventDispatch.UnregisterAllInterest(item);
		}
	}

	private void ActivatePendingState()
	{
		if (m_pendingState == null)
		{
			return;
		}
		if (m_stateStack.Any())
		{
			MotionState.InterruptCode interruptCode = CurrentState.OnInterrupt(this, m_pendingState);
			CurrentState.Exit();
			if (interruptCode == MotionState.InterruptCode.KillMe)
			{
				EventDispatch.UnregisterAllInterest(CurrentState);
				m_stateStack.Pop();
			}
		}
		m_pendingState.Enter();
		m_stateStack.Push(m_pendingState);
		m_pendingState = null;
	}
}
