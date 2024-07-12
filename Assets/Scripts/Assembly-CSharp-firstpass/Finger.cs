using UnityEngine;

public class Finger
{
	public enum FingerMotionState
	{
		Stationary = 0,
		Moving = 1,
		Inactive = 2,
	}

	public Finger(int indexIn)
	{
	}

	public bool isDown;
	public bool hasMoved;
	public bool longPressSent;
	public bool downAndMoving;
	public FingerMotionState motionState;
	public float timeSpentStationary;
	public float startTime;
	public float downAndMovingStartTime;
	public bool possibleSwipe;
	public bool onlyStationary;
	public SwipeGesture.SwipeDirection swipeDirection;
	public TouchPhase touchPhase;
	public Vector2 position;
	public Vector2 deltaPosition;
	public TouchPhase previousTouchPhase;
	public Vector2 previousPosition;
	public Vector2 previousDeltaPosition;
	public Vector2 stationaryPosition;
	public Vector2 startPosition;
	public Vector2 endPosition;
	public Vector2 tapStartPosition;
	public bool usingMouse;
}
