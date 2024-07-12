public class Segment
{
	public enum SizeSpec
	{
		Ratio = 0,
		Bigger = 1,
		Biggest = 2,
		Smaller = 3,
		Smallest = 4,
		Ignore = 5,
	}

	public SwipeGesture.SwipeDirection direction;
	public SwipeGesture.SwipeDirection optionalDirection;
	public int relativeSize;
	public SizeSpec sizeSpec;
	public SwipeGesture.SwipeDirection directionReverse;
	public SwipeGesture.SwipeDirection optionalDirectionReverse;
}
