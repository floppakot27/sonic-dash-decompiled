using System;
using UnityEngine;

public class HudContent_CollectableItem
{
	[Flags]
	private enum State
	{
		None = 0,
		Visible = 1,
		Showing = 2
	}

	private State m_state;

	private GameObject m_displayTrigger;

	private GameObject[] m_gcPickUps;

	private float m_displayTimer;

	private float m_displayDuration;

	public HudContent_CollectableItem(GameObject trigger, GameObject[] gcPickUps)
	{
		m_gcPickUps = gcPickUps;
		m_displayTrigger = trigger;
		m_displayDuration = 3f;
		EventDispatch.RegisterInterest("GCCollectableCollected", this);
	}

	public void Update()
	{
		if ((m_state & State.Visible) == State.Visible)
		{
			UpdateDisplayTimer();
		}
	}

	public void OnResetOnNewGame()
	{
		if ((m_state & State.Showing) == State.Showing)
		{
			Hide();
		}
		m_displayTimer = 0f;
		m_state = State.None;
	}

	public void OnPauseStateChanged(bool paused)
	{
		Hide();
	}

	public void OnPlayerDeath()
	{
		Hide();
	}

	public void HudVisible(bool visible)
	{
		if (visible)
		{
			m_state |= State.Visible;
		}
		else
		{
			m_state &= ~State.Visible;
		}
	}

	private void Event_GCCollectableCollected(int collectable)
	{
		for (int i = 0; i < m_gcPickUps.Length; i++)
		{
			if (i == collectable)
			{
				m_gcPickUps[i].SetActive(value: true);
			}
			else
			{
				m_gcPickUps[i].SetActive(value: false);
			}
		}
		Display();
	}

	private void UpdateDisplayTimer()
	{
		m_displayTimer += Time.deltaTime;
		if (m_displayTimer > m_displayDuration)
		{
			Hide();
		}
	}

	private void Display()
	{
		m_displayTrigger.SendMessage("OnClick", SendMessageOptions.RequireReceiver);
		m_state |= State.Showing;
		m_displayTimer = 0f;
	}

	private void Hide()
	{
		if ((m_state & State.Showing) == State.Showing)
		{
			m_displayTrigger.SendMessage("OnClick", SendMessageOptions.RequireReceiver);
			m_state &= ~State.Showing;
			m_displayTimer = 0f;
		}
	}
}
