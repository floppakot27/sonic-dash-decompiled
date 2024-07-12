using System;
using System.Collections.Generic;
using UnityEngine;

public class SimpleGestureMonitor
{
	private enum State
	{
		eNull,
		eIdle,
		eTrackingActiveTouch,
		eWaitingForNoTouch
	}

	private enum GestureDirection
	{
		eNone,
		eUp,
		eDown,
		eLeft,
		eRight
	}

	private class TapDetectionData
	{
		public Vector2 m_fingerDownPoint;

		public float m_fingerDownTime;

		public int m_fingerID;

		public bool m_tapInvalid;

		public bool m_tapFinished;
	}

	private struct FakeTouch
	{
		public int fingerId;

		public TouchPhase phase;

		public Vector2 position;
	}

	private struct TouchWrapper
	{
		[NonSerialized]
		public Touch Touch;
	}

	private const int c_maxTapTouchesToProcess = 5;

	private const int m_kMaxNumContinuousGestures = 2;

	private List<TapDetectionData> m_tapData = new List<TapDetectionData>(5);

	private State m_currentState;

	private State m_nextState;

	private float m_stateTimer;

	private bool m_touched;

	private Vector2 m_touchPosition = Vector2.zero;

	private Vector2 m_touchDelta = Vector2.zero;

	private Vector2 m_gestureStartPosition = Vector2.zero;

	private Vector2 m_gestureStartPositionDelta = Vector2.zero;

	private float m_gestureTime;

	private float m_gestureActivationLengthInPixels;

	private float m_tapDeactivationLengthInPixels;

	private float m_tapDeactivationTimeInSeconds = 0.3f;

	private float m_gestureTimeout = 2f;

	private bool m_swipeLeft;

	private bool m_swipeRight;

	private bool m_swipeUp;

	private bool m_swipeDown;

	private bool m_tap;

	private float m_tapX;

	private float m_tapY;

	private int m_numContinuousGestures;

	private GestureDirection m_lastGestureDirection;

	private int m_fingureIdForCurrentTouch = -1;

	private int m_prevNumTouches;

	public Vector2 GestureStartPosition => m_gestureStartPosition;

	public SimpleGestureMonitor()
	{
		float num = 0.03937008f;
		float num2 = 7f;
		float num3 = num2 * num;
		m_gestureActivationLengthInPixels = Screen.dpi * num3;
		if (m_gestureActivationLengthInPixels < 1f)
		{
			m_gestureActivationLengthInPixels = (float)Screen.width * 0.1f;
		}
		m_tapDeactivationLengthInPixels = m_gestureActivationLengthInPixels * 0.9f;
		reset();
	}

	private void resetGestureFlags()
	{
		m_swipeLeft = false;
		m_swipeRight = false;
		m_swipeUp = false;
		m_swipeDown = false;
		m_tap = false;
	}

	public void reset()
	{
		m_nextState = State.eIdle;
		m_currentState = State.eIdle;
		resetGestureFlags();
		m_fingureIdForCurrentTouch = -1;
		m_prevNumTouches = 0;
		m_stateTimer = 0f;
		m_numContinuousGestures = 0;
	}

	public void handleTapDetection()
	{
		for (int i = 0; i < m_tapData.Count; i++)
		{
			m_tapData[i].m_tapFinished = true;
		}
		TouchWrapper touch2 = default(TouchWrapper);
		for (int j = 0; j < Input.touchCount; j++)
		{
			Touch touch = Input.touches[j];
			touch2.Touch = touch;
			processTapForTouch(ref touch2);
		}
		int num = 0;
		while (num < m_tapData.Count)
		{
			if (m_tapData[num].m_tapFinished)
			{
				if (!m_tap && !m_tapData[num].m_tapInvalid)
				{
					m_tap = true;
					m_tapX = m_tapData[num].m_fingerDownPoint.x;
					m_tapY = m_tapData[num].m_fingerDownPoint.y;
				}
				m_tapData.RemoveAt(num);
			}
			else
			{
				num++;
			}
		}
	}

	private void processTapForTouch(ref TouchWrapper touch)
	{
		for (int i = 0; i < m_tapData.Count; i++)
		{
			if (m_tapData[i].m_fingerID != touch.Touch.fingerId)
			{
				continue;
			}
			if (touch.Touch.phase == TouchPhase.Ended || touch.Touch.phase == TouchPhase.Canceled)
			{
				m_tapData[i].m_tapFinished = true;
				return;
			}
			m_tapData[i].m_tapFinished = false;
			m_tapData[i].m_fingerDownTime += Time.deltaTime;
			if (!m_tapData[i].m_tapInvalid)
			{
				Vector2 vector = touch.Touch.position - m_tapData[i].m_fingerDownPoint;
				float fingerDownTime = m_tapData[i].m_fingerDownTime;
				if (vector.magnitude > m_tapDeactivationLengthInPixels || fingerDownTime > m_tapDeactivationTimeInSeconds)
				{
					m_tapData[i].m_tapInvalid = true;
				}
			}
			return;
		}
		if (m_tapData.Count < 5)
		{
			TapDetectionData tapDetectionData = new TapDetectionData();
			tapDetectionData.m_fingerDownPoint = touch.Touch.position;
			tapDetectionData.m_fingerDownTime = 0f;
			tapDetectionData.m_fingerID = touch.Touch.fingerId;
			tapDetectionData.m_tapInvalid = false;
			tapDetectionData.m_tapFinished = false;
			m_tapData.Add(tapDetectionData);
		}
	}

	public void Update()
	{
		resetGestureFlags();
		if (m_currentState != m_nextState)
		{
			m_currentState = m_nextState;
			m_stateTimer = 0f;
		}
		else
		{
			m_stateTimer += Time.deltaTime;
		}
		updateInput();
		switch (m_currentState)
		{
		case State.eIdle:
			m_nextState = handleIdleState();
			break;
		case State.eTrackingActiveTouch:
			m_nextState = handleActiveTouchState();
			break;
		case State.eWaitingForNoTouch:
			m_nextState = handleWaitingForNoTouchState();
			break;
		}
		handleTapDetection();
		m_prevNumTouches = Input.touchCount;
	}

	private void updateInput()
	{
		if (Input.touchCount > 0)
		{
			if (m_prevNumTouches != Input.touchCount)
			{
				m_gestureTime = 0f;
			}
			if (m_touched)
			{
				int num = -1;
				float num2 = 0f;
				for (int i = 0; i < Input.touchCount; i++)
				{
					Touch touch = Input.touches[i];
					float num3 = touch.deltaPosition.SqrMagnitude();
					if (num < 0 || num2 < num3)
					{
						num2 = num3;
						num = i;
					}
				}
				Touch touch2 = Input.touches[num];
				m_touchPosition = touch2.position;
				m_touchDelta = touch2.deltaPosition;
				if (touch2.fingerId != m_fingureIdForCurrentTouch)
				{
					resetGesture();
					m_fingureIdForCurrentTouch = touch2.fingerId;
				}
			}
			else
			{
				Touch touch3 = Input.touches[0];
				m_touchPosition = touch3.position;
				m_touchDelta = touch3.deltaPosition;
				m_fingureIdForCurrentTouch = touch3.fingerId;
			}
			m_touched = true;
		}
		else
		{
			m_touched = false;
			m_touchPosition = Vector2.zero;
		}
		if (Input.touchCount == 0 || Input.touchCount != m_prevNumTouches)
		{
			m_lastGestureDirection = GestureDirection.eNone;
			m_numContinuousGestures = 0;
		}
	}

	private void resetGesture()
	{
		m_gestureStartPosition = m_touchPosition;
		m_gestureStartPositionDelta = m_touchDelta;
		m_gestureTime = 0f;
	}

	private void beginGesture()
	{
		resetGesture();
	}

	private void updateGestures(Vector2 startToCurrent)
	{
		if (!m_touched)
		{
			return;
		}
		GestureDirection gestureDirection = GestureDirection.eNone;
		float num = 0f;
		if (startToCurrent.x > num)
		{
			num = startToCurrent.x;
			gestureDirection = GestureDirection.eRight;
		}
		if (startToCurrent.x < 0f - num)
		{
			num = 0f - startToCurrent.x;
			gestureDirection = GestureDirection.eLeft;
		}
		if (startToCurrent.y > num)
		{
			num = startToCurrent.y;
			gestureDirection = GestureDirection.eUp;
		}
		if (startToCurrent.y < 0f - num)
		{
			num = 0f - startToCurrent.y;
			gestureDirection = GestureDirection.eDown;
		}
		if (num > m_gestureActivationLengthInPixels && ((m_numContinuousGestures > 0 && m_lastGestureDirection != gestureDirection) || m_numContinuousGestures == 0))
		{
			switch (gestureDirection)
			{
			case GestureDirection.eUp:
				m_swipeUp = true;
				break;
			case GestureDirection.eDown:
				m_swipeDown = true;
				break;
			case GestureDirection.eLeft:
				m_swipeLeft = true;
				break;
			case GestureDirection.eRight:
				m_swipeRight = true;
				break;
			}
		}
		m_gestureTime += Time.deltaTime;
		if (gestureDetected())
		{
			m_lastGestureDirection = gestureDirection;
			m_numContinuousGestures++;
		}
	}

	private bool gestureDetected()
	{
		return m_swipeLeft || m_swipeRight || m_swipeUp || m_swipeDown || m_tap;
	}

	private bool gestureTimedOut()
	{
		return m_gestureTime > m_gestureTimeout;
	}

	public bool swipeUpDetected()
	{
		return m_swipeUp;
	}

	public bool swipeDownDetected()
	{
		return m_swipeDown;
	}

	public bool swipeLeftDetected()
	{
		return m_swipeLeft;
	}

	public bool swipeRightDetected()
	{
		return m_swipeRight;
	}

	public bool tapDetected()
	{
		return m_tap;
	}

	private State handleIdleState()
	{
		if (m_touched)
		{
			beginGesture();
			updateGestures(m_gestureStartPositionDelta);
			if (gestureDetected())
			{
				return State.eWaitingForNoTouch;
			}
			return State.eTrackingActiveTouch;
		}
		return State.eIdle;
	}

	private State handleActiveTouchState()
	{
		updateGestures(m_gestureStartPositionDelta + (m_touchPosition - m_gestureStartPosition));
		if (m_touched)
		{
			if (gestureDetected())
			{
				return State.eWaitingForNoTouch;
			}
			if (gestureTimedOut())
			{
				return State.eWaitingForNoTouch;
			}
			return State.eTrackingActiveTouch;
		}
		return State.eIdle;
	}

	private State handleWaitingForNoTouchState()
	{
		if (Input.touchCount >= m_prevNumTouches)
		{
			if (m_numContinuousGestures < 2)
			{
				return State.eIdle;
			}
			return State.eWaitingForNoTouch;
		}
		return State.eIdle;
	}

	public float getTapX()
	{
		return m_tapX;
	}

	public float getTapY()
	{
		return m_tapY;
	}
}
