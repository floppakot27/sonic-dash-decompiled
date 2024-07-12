using UnityEngine;

public class SwipeGesture : BaseGesture
{
	public enum SwipeDirection
	{
		Any = 0,
		LeftDiagonal = 1,
		RightDiagonal = 2,
		Vertical = 3,
		Horizontal = 4,
		AnyCross = 5,
		AnyPlus = 6,
		Up = 7,
		Plus45 = 8,
		Right = 9,
		Plus135 = 10,
		Down = 11,
		Minus135 = 12,
		Left = 13,
		Minus45 = 14,
		None = 15,
	}

	public SwipeDirection restrictDirection;
	public BaseGesture.FingerCountRestriction restrictFingerCount;
	public BaseGesture.FingerLocation startsOnObject;
	public BaseGesture.FingerLocation movesOnObject;
	public BaseGesture.FingerLocation endsOnObject;
	public float minGestureLength;
	public float maxTime;
	public SwipeDirection swipeDirection;
	public int swipeFingerCount;
	public Vector2 swipePosition;
	public Vector2 startPosition;
	public Vector2 endPosition;
}
