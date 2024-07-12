public class TrackRing : TrackEntity
{
	public override bool IsValid => ID.Ring.m_occupied && !ID.Ring.m_collected && !ID.Ring.m_forceCollecion;

	public RingID ID { get; private set; }

	public TrackRing(RingID ringID, Kind kind, Track.Lane lane)
		: base(kind, lane)
	{
		ID = ringID;
	}

	public override string ToString()
	{
		return "ring of " + base.ToString();
	}
}
