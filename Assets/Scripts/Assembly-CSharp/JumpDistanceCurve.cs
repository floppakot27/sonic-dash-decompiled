public class JumpDistanceCurve
{
	public JumpCurve JumpTimeCurve { get; private set; }

	public float TotalDistance { get; private set; }

	public JumpDistanceCurve(JumpCurve curve, float speed)
	{
		JumpTimeCurve = curve;
		TotalDistance = speed * JumpTimeCurve.JumpDuration;
	}

	public float CalculateHeight(float distance)
	{
		float time = Utils.MapValue(distance, 0f, TotalDistance, 0f, JumpTimeCurve.JumpDuration);
		return JumpTimeCurve.CalculateHeight(time);
	}
}
