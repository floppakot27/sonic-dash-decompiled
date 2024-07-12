public interface IJumpCurve
{
	float JumpDuration { get; }

	float CalculateHeight(float atTime);
}
