using System;
using UnityEngine;

public class HudContent_PowerUps
{
	[Flags]
	private enum State
	{
		None = 1,
		Active = 2,
		RespawnVisible = 4,
		HeadStartVisible = 8,
		SuperHeadStartVisible = 0x10,
		FreeRespawnVisible = 0x20
	}

	private GameObject m_respawnTrigger;

	private GameObject m_freeRespawnTrigger;

	private GameObject m_headstartTrigger;

	private GameObject m_superHeadstartTrigger;

	private State m_state;

	private float m_respawnHideCountDown;

	private float m_freeRespawnHideCountDown;

	private float m_headStartHideCountDown;

	private float m_superHeadStartHideCountDown;

	public HudContent_PowerUps(GameObject respawnTrigger, GameObject freeRespawnTrigger, GameObject headstartTrigger, GameObject superHeadstartTrigger)
	{
		m_respawnTrigger = respawnTrigger;
		m_freeRespawnTrigger = freeRespawnTrigger;
		m_headstartTrigger = headstartTrigger;
		m_superHeadstartTrigger = superHeadstartTrigger;
		EventDispatch.RegisterInterest("PowerUpCountChanged", this);
		EventDispatch.RegisterInterest("HeadStartActivated", this);
	}

	public void Update()
	{
		if ((m_state & State.Active) == State.Active)
		{
			UpdatePowerUpDisplay(m_respawnTrigger, State.RespawnVisible, ref m_respawnHideCountDown);
			UpdatePowerUpDisplay(m_freeRespawnTrigger, State.FreeRespawnVisible, ref m_freeRespawnHideCountDown);
			UpdatePowerUpDisplay(m_headstartTrigger, State.HeadStartVisible, ref m_headStartHideCountDown);
			UpdatePowerUpDisplay(m_superHeadstartTrigger, State.SuperHeadStartVisible, ref m_superHeadStartHideCountDown);
		}
	}

	public void OnResetOnNewGame()
	{
		if (PowerUpsInventory.GetPowerUpCount(PowerUps.Type.FreeRevive) > 0)
		{
			ToggleInitialDisplay(m_freeRespawnTrigger, State.FreeRespawnVisible, PowerUps.Type.FreeRevive, ref m_freeRespawnHideCountDown);
		}
		else
		{
			ToggleInitialDisplay(m_respawnTrigger, State.RespawnVisible, PowerUps.Type.Respawn, ref m_respawnHideCountDown);
		}
		if (PowerUpsInventory.GetPowerUpCount(PowerUps.Type.SuperHeadStart) == 0)
		{
			ToggleInitialDisplay(m_headstartTrigger, State.HeadStartVisible, PowerUps.Type.HeadStart, ref m_headStartHideCountDown);
		}
		ToggleInitialDisplay(m_superHeadstartTrigger, State.SuperHeadStartVisible, PowerUps.Type.SuperHeadStart, ref m_superHeadStartHideCountDown);
	}

	public void OnPauseStateChanged(bool paused)
	{
	}

	public void OnPlayerDeath()
	{
	}

	public void HudVisible(bool visible)
	{
		if (visible)
		{
			m_state |= State.Active;
		}
		else
		{
			m_state &= ~State.Active;
		}
	}

	private void ToggleInitialDisplay(GameObject trigger, State powerUpState, PowerUps.Type powerUpType, ref float m_showCountDown)
	{
		bool flag = PowerUpsInventory.GetPowerUpCount(powerUpType) > 0 && !TutorialSystem.instance().isTrackTutorialEnabled();
		if ((IsPowerUpVisible(powerUpState) && !flag) || (!IsPowerUpVisible(powerUpState) && flag))
		{
			TogglePowerUpDisplay(trigger, powerUpState, ref m_showCountDown);
		}
		m_showCountDown = 0f;
	}

	private void UpdatePowerUpDisplay(GameObject trigger, State powerUpState, ref float hideCountDown)
	{
		if ((m_state & powerUpState) == powerUpState)
		{
			hideCountDown += Time.deltaTime;
			if (hideCountDown > 3f)
			{
				TogglePowerUpDisplay(trigger, powerUpState, ref hideCountDown);
			}
		}
	}

	private void TogglePowerUpDisplay(GameObject trigger, State powerUpState, ref float countdown)
	{
		GameObject gameObject = trigger.transform.parent.gameObject;
		gameObject.SetActive(value: true);
		trigger.SendMessage("OnClick", SendMessageOptions.RequireReceiver);
		if ((m_state & powerUpState) == powerUpState)
		{
			m_state &= ~powerUpState;
			return;
		}
		m_state |= powerUpState;
		countdown = 0f;
	}

	private bool IsPowerUpVisible(State powerUp)
	{
		return (m_state & powerUp) == powerUp;
	}

	private void Event_HeadStartActivated(bool super)
	{
		if ((m_state & State.Active) != State.Active)
		{
			return;
		}
		if (super)
		{
			if ((m_state & State.SuperHeadStartVisible) == State.SuperHeadStartVisible)
			{
				TogglePowerUpDisplay(m_superHeadstartTrigger, State.SuperHeadStartVisible, ref m_superHeadStartHideCountDown);
			}
		}
		else if ((m_state & State.HeadStartVisible) == State.HeadStartVisible)
		{
			TogglePowerUpDisplay(m_headstartTrigger, State.HeadStartVisible, ref m_headStartHideCountDown);
		}
	}

	private void Event_PowerUpCountChanged(PowerUps.Type powerUp)
	{
		if ((m_state & State.Active) == State.Active && powerUp == PowerUps.Type.Respawn)
		{
			if ((m_state & State.RespawnVisible) == State.RespawnVisible || (m_state & State.FreeRespawnVisible) == State.FreeRespawnVisible)
			{
				m_respawnHideCountDown = 0f;
				m_freeRespawnHideCountDown = 0f;
			}
			else
			{
				TogglePowerUpDisplay(m_respawnTrigger, State.RespawnVisible, ref m_respawnHideCountDown);
			}
		}
	}
}
