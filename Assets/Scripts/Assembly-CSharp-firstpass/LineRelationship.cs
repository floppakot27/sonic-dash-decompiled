public class LineRelationship
{
	public enum LinePosition
	{
		Percent = 0,
		Start = 1,
		End = 2,
		Upper = 3,
		Lower = 4,
		Left = 5,
		Right = 6,
		BetweenTopBottom = 7,
		BetweenLeftRight = 8,
	}

	public LineRelationship(LineSwipe targetLineIn, int targetSegmentNumIn, LineRelationship.LinePosition targetPositionIn, LineSwipe relativeLineIn, int relativeSegmentNumIn, LineRelationship.LinePosition relativePositionIn)
	{
	}

	public int targetSegmentNum;
	public LinePosition targetPosition;
	public float targetPercentPosition;
	public int relativeSegmentNum;
	public LinePosition relativePosition;
	public float relativePercentPosition;
}
