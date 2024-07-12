public struct RingID
{
	public static RingID Invalid = default(RingID);

	public RingSequence Sequence;

	public RingSequence.Ring Ring;

	public bool IsValid => Sequence != null && Ring != null;
}
