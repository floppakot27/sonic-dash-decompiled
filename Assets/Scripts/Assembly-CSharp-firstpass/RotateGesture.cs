using UnityEngine;

public class RotateGesture : BaseGesture
{
	public enum RotateAxis
	{
		X = 0,
		Y = 1,
		Z = 2,
	}

	public bool doRotate;
	public float minSqrDistanceToCenter;
	public BaseGesture.FingerLocation fingerLocation;
	public RotateAxis rotateAxis;
	public BaseGesture.FingerCountRestriction restrictFingerCount;
	public float rotationAngleDelta;
	public Vector2 touchPosition;
	public bool isDown;
}
