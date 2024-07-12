public class LineGesture : BaseGesture
{
	public enum LineSwipeDirection
	{
		Forward = 0,
		Backward = 1,
		Either = 2,
		Anywhere = 3,
	}

	public enum LineIdentification
	{
		Precise = 0,
		Clean = 1,
		Sloppy = 2,
	}

	public LineSwipeDirection restrictLineSwipeDirection;
	public LineFactory.LineType[] lineFactoryLineType;
	public BaseGesture.FingerLocation startsOnObject;
	public BaseGesture.FingerLocation endsOnObject;
	public LineIdentification lineIdentification;
	public bool returnSwipeAlways;
	public float matchPositionDiff;
	public float matchLengthDiffPercent;
	public float maxTimeBetweenLines;
	public LineFactory.LineType swipedLineType;
	public LineIdentification lineIdentificationUsed;
	public bool performingSwipe;
	public string errorString;
}
