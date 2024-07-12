using UnityEngine;

public class SwipeSegment
{
	public SwipeSegment(SwipeSegment prev)
	{
	}

	public SwipeGesture.SwipeDirection direction;
	public float distance;
	public float velocity;
	public float startTime;
	public float endTime;
	public Vector2 startPosition;
	public Vector2 endPosition;
	public bool initalized;
}
