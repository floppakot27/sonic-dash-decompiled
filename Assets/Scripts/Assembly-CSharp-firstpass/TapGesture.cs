public class TapGesture : BaseGesture
{
	public int taps;
	public float maxTimeBetweensTaps;
	public float maxTapDistance;
	public int tapRateTapsCount;
	public BaseGesture.FingerLocation startsOnObject;
	public BaseGesture.FingerLocation movesOnObject;
	public BaseGesture.FingerLocation endsOnObject;
	public bool enforceStationary;
	public float tapsPerMinute;
}
