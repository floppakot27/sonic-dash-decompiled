using UnityEngine;

public class TouchGesture : BaseGesture
{
	public BaseGesture.FingerLocation startsOnObject;
	public BaseGesture.FingerLocation movesOnObject;
	public BaseGesture.FingerLocation endsOnObject;
	public BaseGesture.FingerCountRestriction restrictFingerCount;
	public bool averagePoint;
	public bool isDown;
	public bool[] isActives;
	public Vector2 touchPositionStart;
	public Vector2 touchPosition;
	public Vector2 touchPositionEnd;
	public float touchMagnitude;
}
