using UnityEngine;

public class SliceGesture : BaseGesture
{
	public SwipeGesture.SwipeDirection restrictDirection;
	public SwipeGesture.SwipeDirection sliceDirection;
	public Vector3 sliceStartPosition;
	public Vector3 sliceEndPosition;
}
