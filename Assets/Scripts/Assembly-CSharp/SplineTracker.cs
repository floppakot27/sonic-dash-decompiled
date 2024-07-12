using System.Runtime.CompilerServices;
using UnityEngine;

public class SplineTracker
{
	public enum Type
	{
		OneShot,
		Loop,
		PingPong
	}

	private struct TrackerState
	{
		public float m_currentPosition;

		public float m_splineLength;

		public bool m_inReverse;
	}

	public delegate void OnEvent(SplineTracker eventSource);

	private TrackerState m_trackerState;

	private LightweightTransform m_currentSplineTransform;

	private bool m_teleport;

	private bool m_forceSnapRotation;

	private Spline.TrackerContext m_transformContext = new Spline.TrackerContext();

	public bool RunBackwards { get; set; }

	public Type TrackerType { get; set; }

	public float TrackSpeed { get; set; }

	public bool Tracking { get; private set; }

	public Spline Target { get; set; }

	public bool IsVIP { get; set; }

	public float CurrentDistance => m_trackerState.m_currentPosition;

	public LightweightTransform CurrentSplineTransform => m_currentSplineTransform;

	public float PreviousDelta { get; private set; }

	public bool IsReversed => m_trackerState.m_inReverse;

	public bool CanUpdate => Target != null && Tracking;

	[method: MethodImpl(32)]
	public event OnEvent OnStop;

	public SplineTracker()
	{
		TrackerType = Type.OneShot;
		Tracking = false;
		Target = null;
		PreviousDelta = 0f;
		IsVIP = false;
		m_teleport = false;
		m_forceSnapRotation = false;
		RunBackwards = false;
	}

	public SplineTracker(Spline target, float startPos, float speed)
		: this()
	{
		Target = target;
		Start(speed, startPos, Direction_1D.Forwards);
	}

	public SplineTracker(SplineTracker rhs)
		: this()
	{
		CopyFrom(rhs);
	}

	public SplineTracker Clone()
	{
		SplineTracker splineTracker = MemberwiseClone() as SplineTracker;
		splineTracker.m_transformContext = m_transformContext.Clone();
		splineTracker.IsVIP = false;
		return splineTracker;
	}

	public void CopyFrom(SplineTracker copyTarget)
	{
		m_trackerState = copyTarget.m_trackerState;
		TrackerType = copyTarget.TrackerType;
		TrackSpeed = copyTarget.TrackSpeed;
		Tracking = copyTarget.Tracking;
		Target = copyTarget.Target;
		m_currentSplineTransform = copyTarget.m_currentSplineTransform;
		this.OnStop = copyTarget.OnStop;
		PreviousDelta = copyTarget.PreviousDelta;
		m_transformContext = copyTarget.m_transformContext.Clone();
	}

	public void Start(Vector3 initialPos, Spline target, float initialSpeed)
	{
		Start(initialPos, target, initialSpeed, Direction_1D.Forwards);
	}

	public void Start(Vector3 initialPos, Spline target, float initialSpeed, Direction_1D dir)
	{
		Utils.ClosestPoint closestPoint = target.EstimateDistanceAlongSpline(initialPos);
		Target = target;
		Start(initialSpeed, closestPoint.LineDistance, dir);
	}

	public void Start(float initialSpeed)
	{
		Start(initialSpeed, 0f, Direction_1D.Forwards);
	}

	public void Stop()
	{
		Tracking = false;
	}

	public void Start(float initialSpeed, float initialPosition, Direction_1D dir)
	{
		Tracking = true;
		TrackSpeed = initialSpeed;
		ResetTrackingState(initialPosition, dir);
		if (Target != null)
		{
			m_currentSplineTransform = CalculateTargetTransform();
		}
		PreviousDelta = 0f;
		m_teleport = false;
	}

	public void ForceSnapRotation()
	{
		m_forceSnapRotation = true;
	}

	public void DisableForceSnapRotation()
	{
		m_forceSnapRotation = false;
	}

	public LightweightTransform Update(LightweightTransform currentTransform)
	{
		if (m_teleport)
		{
			m_teleport = false;
			return SnapToTargetPosition(currentTransform);
		}
		float num = IndependantTimeDelta.Delta / (1f / 30f);
		float num2 = num * 5f;
		if (num2 < 0.01f)
		{
			num2 = 0.01f;
		}
		float num3 = UpdatePosition();
		return SmoothTowardsTargetPosition(currentTransform, 1.5f * num3, num2, m_forceSnapRotation);
	}

	public SplineKnot.EndBehaviour GetEndBehaviourInCurrentDirection()
	{
		return Target.GetBehaviourAt((!m_trackerState.m_inReverse) ? Spline.ControlPoint.End : Spline.ControlPoint.Start);
	}

	public void Reverse()
	{
		m_trackerState.m_inReverse = !m_trackerState.m_inReverse;
	}

	public void Restart()
	{
		Tracking = true;
	}

	public float UpdatePosition()
	{
		float num = TrackSpeed * Time.deltaTime;
		return (!UpdatePositionByDelta(num)) ? 0f : num;
	}

	public bool UpdatePositionByDelta(float delta)
	{
		if (!CanUpdate)
		{
			return false;
		}
		m_trackerState.m_splineLength = Target.Length;
		float num = ((!m_trackerState.m_inReverse) ? 1f : (-1f)) * delta;
		float num2 = num;
		while (num2 != 0f && Tracking)
		{
			num2 = bouncePositionOffSplineEnd(num2);
		}
		m_currentSplineTransform = CalculateTargetTransform();
		PreviousDelta = num - num2;
		return true;
	}

	public void ForceUpdateTransform()
	{
		m_currentSplineTransform = CalculateTargetTransform();
	}

	public LightweightTransform SmoothTowardsTargetPosition(LightweightTransform currentTransform, float maxPositionBlend, float maxRotation, bool snapRotation)
	{
		if (Target == null)
		{
			return currentTransform;
		}
		LightweightTransform lightweightTransform = CalculateTargetTransform();
		Vector3 pos = Vector3.MoveTowards(currentTransform.Location, lightweightTransform.Location, maxPositionBlend);
		Quaternion orientation = lightweightTransform.Orientation;
		Quaternion identity = Quaternion.identity;
		identity = ((!snapRotation) ? Quaternion.RotateTowards(currentTransform.Orientation, orientation, maxRotation) : orientation);
		return new LightweightTransform(pos, identity);
	}

	public LightweightTransform SnapToTargetPosition(LightweightTransform currentTransform)
	{
		if (Target == null)
		{
			return currentTransform;
		}
		return CalculateTargetTransform();
	}

	public float CalculatePositionAfterForwardMovement(float forwardMovementDelta)
	{
		float num = m_trackerState.m_currentPosition + ((!m_trackerState.m_inReverse) ? 1f : (-1f)) * forwardMovementDelta;
		if (TrackerType == Type.PingPong && num > m_trackerState.m_splineLength)
		{
			return m_trackerState.m_splineLength + (m_trackerState.m_splineLength - num);
		}
		if (TrackerType == Type.PingPong && num < 0f)
		{
			return 0f - num;
		}
		if (TrackerType == Type.Loop)
		{
			if (num > m_trackerState.m_splineLength && !m_trackerState.m_inReverse)
			{
				return num - m_trackerState.m_splineLength;
			}
			if (num < 0f && m_trackerState.m_inReverse)
			{
				return m_trackerState.m_splineLength + num;
			}
		}
		else if (TrackerType == Type.OneShot && ((!m_trackerState.m_inReverse && num > m_trackerState.m_splineLength) || (m_trackerState.m_inReverse && num < 0f)))
		{
			return Mathf.Clamp(num, 0f, m_trackerState.m_splineLength);
		}
		return num;
	}

	private LightweightTransform CalculateTargetTransform()
	{
		LightweightTransform transform = Target.GetTransform(m_trackerState.m_currentPosition, m_transformContext);
		if (m_trackerState.m_inReverse && !RunBackwards)
		{
			transform.Orientation = Quaternion.LookRotation(-transform.Forwards, transform.Up);
		}
		return transform;
	}

	private float bouncePositionOffSplineEnd(float positionDelta)
	{
		m_trackerState.m_currentPosition += positionDelta;
		if (TrackerType == Type.PingPong && m_trackerState.m_currentPosition > m_trackerState.m_splineLength)
		{
			m_trackerState.m_currentPosition = m_trackerState.m_splineLength + (m_trackerState.m_splineLength - m_trackerState.m_currentPosition);
			m_trackerState.m_inReverse = true;
			return 0f;
		}
		if (TrackerType == Type.PingPong && m_trackerState.m_currentPosition < 0f)
		{
			m_trackerState.m_inReverse = false;
			m_trackerState.m_currentPosition = 0f - m_trackerState.m_currentPosition;
			return 0f;
		}
		if (TrackerType == Type.Loop)
		{
			if (m_trackerState.m_currentPosition > m_trackerState.m_splineLength && !m_trackerState.m_inReverse)
			{
				m_trackerState.m_currentPosition -= m_trackerState.m_splineLength;
			}
			else if (m_trackerState.m_currentPosition < 0f && m_trackerState.m_inReverse)
			{
				m_trackerState.m_currentPosition = m_trackerState.m_splineLength + m_trackerState.m_currentPosition;
			}
			return 0f;
		}
		if (TrackerType == Type.OneShot && ((!m_trackerState.m_inReverse && m_trackerState.m_currentPosition > m_trackerState.m_splineLength) || (m_trackerState.m_inReverse && m_trackerState.m_currentPosition < 0f)))
		{
			float num = Mathf.Clamp(m_trackerState.m_currentPosition, 0f, m_trackerState.m_splineLength);
			float result = m_trackerState.m_currentPosition - num;
			m_trackerState.m_currentPosition = num;
			Tracking = false;
			if (this.OnStop != null)
			{
				m_currentSplineTransform = CalculateTargetTransform();
				this.OnStop(this);
			}
			return result;
		}
		return 0f;
	}

	private void ResetTrackingState(float initialPosition, Direction_1D dir)
	{
		m_trackerState.m_currentPosition = initialPosition;
		m_trackerState.m_splineLength = ((!(Target == null)) ? Target.Length : 0f);
		m_trackerState.m_inReverse = dir == Direction_1D.Backwards;
		m_transformContext.Reset();
		m_transformContext.Direction = dir;
	}

	public void requestTeleport()
	{
		m_teleport = true;
	}
}
