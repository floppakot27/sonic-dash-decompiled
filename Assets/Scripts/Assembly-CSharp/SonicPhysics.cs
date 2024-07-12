using UnityEngine;

[RequireComponent(typeof(SonicAnimationControl))]
public class SonicPhysics
{
	public enum Layer
	{
		SONIC = 16,
		GAP_COLLISION = 24,
		GAP_TRIGGER = 25
	}

	public enum LayerMask
	{
		TRACK_COLLISION = 0x80000,
		OBSTACLE_COLLISION = 0x100000
	}

	public const float m_collisionRadius = 0.4f;

	public const float m_collisionDiameter = 0.8f;

	private float m_accelerationBoostFactor = 1f;

	private float m_lastTargetDashSpeed;

	private bool m_dashing;

	private float m_dashTimer;

	private IJumpCurve m_jump;

	private float m_jumpTime;

	private float m_currentJumpHeight;

	private float m_targetSpeed;

	private bool m_isTargetSpeedOverriden;

	private float m_overrideAcceleration;

	private bool m_isAccelerationOverriden;

	private float m_currentSpeed;

	private float m_idealSpeed;

	private float m_timeSpentNotRolling;

	private float m_dashSpeed;

	public float TargetSpeed
	{
		get
		{
			return m_targetSpeed;
		}
		set
		{
			m_targetSpeed = value;
			m_isTargetSpeedOverriden = true;
		}
	}

	public float CurrentSpeed => m_currentSpeed;

	public float IdealSpeed => m_idealSpeed;

	public float AccelerationOverride
	{
		get
		{
			return (!m_isAccelerationOverriden) ? (Sonic.Handling.SpeedSmoothing * m_accelerationBoostFactor) : m_overrideAcceleration;
		}
		set
		{
			m_overrideAcceleration = value;
			m_isAccelerationOverriden = true;
		}
	}

	public float TimeInJump => m_jumpTime;

	public float JumpDuration => m_jump.JumpDuration;

	public float JumpHeight
	{
		get
		{
			if (IsJumping)
			{
				return m_currentJumpHeight;
			}
			return 0f;
		}
		set
		{
			m_currentJumpHeight = value;
		}
	}

	public bool IsJumping => m_jump != null;

	public float JumpTimeRemaining => m_jump.JumpDuration - m_jumpTime;

	public float JumpProgress => m_jumpTime / m_jump.JumpDuration;

	public float TimeSpentNotRolling
	{
		get
		{
			return m_timeSpentNotRolling;
		}
		set
		{
			m_timeSpentNotRolling = value;
		}
	}

	public SonicPhysics()
	{
		m_targetSpeed = (m_currentSpeed = Sonic.Handling.StartSpeed);
		m_currentSpeed = ((!Sonic.AnimationControl) ? 0f : Sonic.AnimationControl.JogAnimSpeed);
		m_timeSpentNotRolling = Sonic.Handling.RollCoolDownDuration;
		ResetDash();
		m_accelerationBoostFactor = 1f;
		EventDispatch.RegisterInterest("OnSonicRespawn", this);
		EventDispatch.RegisterInterest("OnSonicResurrection", this);
	}

	private void Event_OnSonicRespawn()
	{
		m_accelerationBoostFactor = 2f;
	}

	private void Event_OnSonicResurrection()
	{
		m_accelerationBoostFactor = 2f;
	}

	public void HaltSonic()
	{
		m_currentSpeed = 0f;
	}

	public void StartJump(float initialGroundHeight)
	{
		StartJump(Sonic.Handling.CreateJumpFrom(initialGroundHeight));
	}

	public void StartJump(IJumpCurve jumpCurve)
	{
		m_jump = jumpCurve;
		m_jumpTime = 0f;
		m_currentJumpHeight = 0f;
	}

	public void StartFall(float initialGroundHeight)
	{
		StartJump(initialGroundHeight - Sonic.Handling.JumpHeight);
		m_jumpTime = Sonic.Handling.JumpDuration * 0.5f;
		m_currentJumpHeight = Sonic.Handling.JumpHeight;
	}

	public void SlowSonic()
	{
		m_currentSpeed *= 0.3f;
	}

	public void PreUpdate()
	{
		m_idealSpeed = Sonic.Handling.CalculateVelocityAt(GameState.TimeInGame);
		m_timeSpentNotRolling += Time.deltaTime;
	}

	public void Update()
	{
		UpdateSpeed();
	}

	public bool UpdateJump(bool pauseHalfway)
	{
		bool result = false;
		m_jumpTime += Time.deltaTime;
		if (pauseHalfway && m_jumpTime > m_jump.JumpDuration * 0.5f)
		{
			m_jumpTime = m_jump.JumpDuration * 0.5f;
		}
		if (m_jumpTime > m_jump.JumpDuration)
		{
			result = true;
		}
		m_jumpTime = Mathf.Clamp(m_jumpTime, 0f, m_jump.JumpDuration);
		m_currentJumpHeight = m_jump.CalculateHeight(m_jumpTime);
		return result;
	}

	public bool IsSonicGrounded(LightweightTransform groundTransform, float heightOffset, float forwardOffset, LayerMask nLayerMask)
	{
		float num = 0.8f;
		Vector3 vector = groundTransform.Location + groundTransform.Up * (1f + heightOffset);
		Vector3 end = vector - groundTransform.Up * 2f;
		vector += groundTransform.Forwards * forwardOffset;
		end += groundTransform.Forwards * forwardOffset;
		Debug.DrawLine(vector, end, Color.yellow);
		if (!Physics.Linecast(vector, end, (int)nLayerMask))
		{
			vector -= groundTransform.Forwards * (num + forwardOffset);
			end -= groundTransform.Forwards * (num + forwardOffset);
			Debug.DrawLine(vector, end, Color.magenta);
			if (!Physics.Linecast(vector, end, (int)nLayerMask))
			{
				return false;
			}
		}
		return true;
	}

	public void ClearJump()
	{
		m_jump = null;
	}

	private void UpdateSpeed()
	{
		if (DashMonitor.instance().isDashing() || HeadstartMonitor.instance().isHeadstarting())
		{
			if (!m_dashing)
			{
				m_dashTimer = 0f;
				m_lastTargetDashSpeed = 0f;
				m_dashing = true;
			}
			else
			{
				m_dashTimer += Time.deltaTime;
			}
			float num = 0f;
			num = ((!Sonic.Handling.InstantDashSpeedGain) ? (m_dashTimer / Sonic.Handling.TimeToDashMaxSpeed) : 1f);
			if (num > 1f)
			{
				num = 1f;
			}
			float num2 = 1f;
			if (HeadstartMonitor.instance().isHeadstarting())
			{
				num2 = ((!HeadstartMonitor.instance().isSuperHeadstart()) ? Sonic.Handling.HeadstartSpeedMultiplier : Sonic.Handling.SuperHeadstartSpeedMultiplier);
			}
			else if (DashMonitor.instance().isDashing())
			{
				num2 = Sonic.Handling.DashSpeedMultiplier;
			}
			m_dashSpeed = Mathf.Lerp(0f, m_lastTargetDashSpeed = m_idealSpeed * num2, num);
		}
		else
		{
			if (m_dashing)
			{
				m_dashTimer = 0f;
				m_dashing = false;
			}
			else
			{
				m_dashTimer += Time.deltaTime;
			}
			float num3 = 0f;
			num3 = ((!Sonic.Handling.InstantDashSpeedLoss) ? (m_dashTimer / Sonic.Handling.TimeToDashNormalSpeed) : 1f);
			if (num3 > 1f)
			{
				num3 = 1f;
			}
			m_dashSpeed = Mathf.Lerp(m_lastTargetDashSpeed, 0f, num3);
		}
		if (!m_isTargetSpeedOverriden)
		{
			m_targetSpeed = m_idealSpeed + m_dashSpeed;
		}
		if (m_currentSpeed > m_targetSpeed)
		{
			m_currentSpeed = Mathf.MoveTowards(m_currentSpeed, m_targetSpeed, Sonic.Handling.SpeedSmoothing * Time.deltaTime * 12f);
			m_accelerationBoostFactor = 1f;
		}
		else
		{
			float maxDelta = AccelerationOverride * Time.deltaTime;
			m_currentSpeed = Mathf.MoveTowards(m_currentSpeed, m_targetSpeed, maxDelta);
			if (Mathf.Abs(m_targetSpeed - m_currentSpeed) < m_targetSpeed * 0.01f)
			{
				m_accelerationBoostFactor = 1f;
			}
		}
		m_isTargetSpeedOverriden = false;
		m_isAccelerationOverriden = false;
	}

	private void ResetDash()
	{
		m_dashing = false;
		m_dashSpeed = 0f;
	}
}
