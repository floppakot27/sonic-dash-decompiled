public class PinchGesture : BaseGesture
{
	public enum PinchAction
	{
		Both = 0,
		Close = 1,
		Open = 2,
	}

	public enum PinchDirection
	{
		All = 0,
		Vertical = 1,
		LeftDiagonal = 2,
		Horizontal = 3,
		RightDiagonal = 4,
	}

	public bool doPinch;
	public bool keepAspectRatio;
	public BaseGesture.FingerLocation fingerLocation;
	public PinchAction pinchAction;
	public PinchDirection restrictDirection;
	public float pinchScaleFactor;
	public PinchDirection pinchDirection;
	public float pinchMagnitudeDelta;
}
