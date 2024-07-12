using System;
using UnityEngine;

public class DashMeter : MonoBehaviour
{
	[Flags]
	private enum State
	{
		None = 0,
		BurningDown = 1,
		Dashing = 2,
		SpringPaused = 4
	}

	private bool m_nearlyFinished;

	private bool m_autoFill;

	private bool m_dashRequest;

	private bool m_filledEventFired;

	private float m_visibleValue;

	private float m_destinationTargetValue;

	private float m_burnDownRate;

	private State m_state;

	private int m_previousRingCount;

	[SerializeField]
	private float[] m_perRingPickUpValue = new float[7] { 0.1f, 0.1f, 0.1f, 0.1f, 0.1f, 0.1f, 0.1f };

	[SerializeField]
	private float[] m_increasePerStreak = new float[7] { 0.004f, 0.004f, 0.004f, 0.004f, 0.004f, 0.004f, 0.004f };

	[SerializeField]
	private float[] m_dashBurndownRates = new float[7] { 3f, 5f, 7f, 8f, 8.5f, 9f, 10f };

	public float m_warnPercentage = 0.3f;

	public float Value => m_visibleValue;

	public void ForceFinishBurnDown()
	{
		if ((m_state & State.Dashing) == State.Dashing && (m_state & State.BurningDown) == State.BurningDown && m_visibleValue > m_warnPercentage && m_visibleValue < 2f * m_warnPercentage)
		{
			m_visibleValue = m_warnPercentage;
		}
	}

	private void Start()
	{
		EventDispatch.RegisterInterest("OnNewGameStarted", this);
		EventDispatch.RegisterInterest("OnRingStreakCompleted", this);
		EventDispatch.RegisterInterest("OnSonicDeath", this);
		EventDispatch.RegisterInterest("OnSonicResurrection", this);
		EventDispatch.RegisterInterest("OnSonicRespawn", this);
		EventDispatch.RegisterInterest("OnSpringStart", this);
		EventDispatch.RegisterInterest("OnSpringEnd", this);
		EventDispatch.RegisterInterest("OnAutoFillEnabled", this);
		EventDispatch.RegisterInterest("OnAutoFillDisabled", this);
		EventDispatch.RegisterInterest("OnDashAutoTrigger", this);
		EventDispatch.RegisterInterest("OnDashStop", this);
	}

	private void Event_OnAutoFillEnabled()
	{
		m_autoFill = true;
	}

	private void Event_OnAutoFillDisabled()
	{
		m_autoFill = false;
	}

	private void Update()
	{
		if (!(DashMonitor.instance() == null))
		{
			if (m_dashRequest && !Sonic.Tracker.isJumping())
			{
				TriggerDash();
				m_dashRequest = false;
			}
			UpdateRingCollection();
			if ((m_state & State.SpringPaused) != State.SpringPaused)
			{
				UpdateBurnDown();
			}
			UpdateToTarget();
		}
	}

	private void InitialiseBurnDown()
	{
		m_state |= State.BurningDown;
		if (object.Equals(m_visibleValue, 1f))
		{
			int powerUpLevel = PowerUpsInventory.GetPowerUpLevel(PowerUps.Type.DashLength);
			m_burnDownRate = 1f / m_dashBurndownRates[powerUpLevel];
		}
		m_destinationTargetValue = 0f;
		m_nearlyFinished = false;
	}

	private void UpdateRingCollection()
	{
		int heldRings = RingStorage.HeldRings;
		if (!DashMonitor.instance().isDashing() && !HeadstartMonitor.instance().isHeadstarting())
		{
			if (m_autoFill && heldRings > 0)
			{
				IncreaseDashValue(100f);
			}
			else if (!m_autoFill && heldRings > m_previousRingCount)
			{
				int powerUpLevel = PowerUpsInventory.GetPowerUpLevel(PowerUps.Type.DashIncrease);
				int num = heldRings - m_previousRingCount;
				IncreaseDashValue((float)num * m_perRingPickUpValue[powerUpLevel]);
			}
		}
		m_previousRingCount = heldRings;
	}

	private void UpdateBurnDown()
	{
		if ((m_state & State.BurningDown) != State.BurningDown)
		{
			return;
		}
		float visibleValue = m_visibleValue;
		m_visibleValue -= m_burnDownRate * Time.deltaTime;
		bool flag = false;
		if (visibleValue >= m_warnPercentage && m_visibleValue < m_warnPercentage)
		{
			flag = true;
		}
		if (flag)
		{
			EventDispatch.GenerateEvent("OnDashFinishingSoon");
			m_nearlyFinished = true;
		}
		if (!(m_visibleValue < 0f))
		{
			return;
		}
		m_visibleValue = 0f;
		if (object.Equals(m_visibleValue, 0f))
		{
			EventDispatch.GenerateEvent("OnDashMeterEmpty");
			if ((m_state & State.Dashing) == State.Dashing)
			{
				EventDispatch.GenerateEvent("OnDashFinished");
				m_filledEventFired = false;
				m_nearlyFinished = false;
			}
			m_state &= ~State.BurningDown;
			m_state &= ~State.Dashing;
		}
	}

	private void UpdateToTarget()
	{
		if ((m_state & State.BurningDown) != State.BurningDown)
		{
			m_visibleValue += 1.5f * Time.deltaTime;
			if (m_visibleValue > m_destinationTargetValue)
			{
				m_visibleValue = m_destinationTargetValue;
			}
			if (m_visibleValue >= 1f && !m_filledEventFired)
			{
				m_filledEventFired = true;
				EventDispatch.GenerateEvent("OnDashMeterFilled");
			}
		}
	}

	private void IncreaseDashValue(float increaseAmount)
	{
		m_destinationTargetValue += increaseAmount;
		m_destinationTargetValue = Mathf.Clamp(m_destinationTargetValue, 0f, 1f);
	}

	private bool CanDashMeterBeTriggered()
	{
		return m_visibleValue >= 1f && (m_state & State.BurningDown) != State.BurningDown && !HeadstartMonitor.instance().isHeadstarting();
	}

	private void TriggerDash()
	{
		EventDispatch.GenerateEvent("OnDashMeterTriggered", true);
		EventDispatch.GenerateEvent("OnDashMeterStarted", m_visibleValue);
		if (object.Equals(m_visibleValue, 1f))
		{
			EventDispatch.GenerateEvent("OnDashStarted", m_visibleValue);
			m_state |= State.Dashing;
		}
		InitialiseBurnDown();
	}

	private void Event_OnNewGameStarted()
	{
		m_visibleValue = 0f;
		m_destinationTargetValue = m_visibleValue;
		m_previousRingCount = 0;
		m_state = State.None;
		m_autoFill = false;
		m_dashRequest = false;
		m_nearlyFinished = false;
		m_filledEventFired = false;
	}

	private void Event_OnRingStreakCompleted(int sequenceLength, float firstRingTrackPosition, float lastRingTrackPosition)
	{
		if (!DashMonitor.instance().isDashing() && !HeadstartMonitor.instance().isHeadstarting())
		{
			int powerUpLevel = PowerUpsInventory.GetPowerUpLevel(PowerUps.Type.DashIncrease);
			IncreaseDashValue(m_increasePerStreak[powerUpLevel]);
		}
	}

	private void Trigger_DashMeterSelected()
	{
		if (CanDashMeterBeTriggered())
		{
			m_dashRequest = true;
		}
	}

	private void Event_OnDashAutoTrigger()
	{
		bool flag = CanDashMeterBeTriggered();
		EventDispatch.GenerateEvent("OnDashMeterTriggered", flag);
		if (flag)
		{
			EventDispatch.GenerateEvent("OnDashMeterStarted", m_visibleValue);
			if (object.Equals(m_visibleValue, 1f))
			{
				EventDispatch.GenerateEvent("OnDashStarted", m_visibleValue);
				m_state |= State.Dashing;
			}
			InitialiseBurnDown();
		}
	}

	private void Event_OnDashStop()
	{
		m_burnDownRate = 2f;
		m_autoFill = false;
		m_destinationTargetValue = 0f;
	}

	private void Event_OnSonicDeath()
	{
		m_dashRequest = false;
	}

	private void Event_OnSonicResurrection()
	{
		m_dashRequest = false;
	}

	private void Event_OnSonicRespawn()
	{
		m_dashRequest = false;
	}

	private void Event_OnSpringStart(SpringTV.Type springType)
	{
		if (!m_nearlyFinished)
		{
			m_state |= State.SpringPaused;
		}
	}

	private void Event_OnSpringEnd()
	{
		m_state &= ~State.SpringPaused;
	}
}
