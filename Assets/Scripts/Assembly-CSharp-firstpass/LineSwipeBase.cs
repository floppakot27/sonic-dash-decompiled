public class LineSwipeBase
{
	public LineSwipeBase(string nameIn)
	{
	}

	public string name;
	public bool closed;
	public bool startAnywhere;
	public bool doCompareLengths;
	public bool biDirectional;
	public bool maintainAspectRatio;
	public LineGesture.LineIdentification identificationUsed;
	public LineGesture.LineSwipeDirection restrictLineSwipeDirectionUsed;
	public bool isForwardUsed;
}
