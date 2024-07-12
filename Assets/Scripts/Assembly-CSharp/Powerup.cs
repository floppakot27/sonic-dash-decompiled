public abstract class Powerup : SpawnableObject
{
	public TrackEntity.Kind PowerupType { get; set; }

	public override void Place(OnEvent onDestroy, Track track, Spline spline)
	{
		base.Place(onDestroy, track, spline);
		Place(track, spline);
	}

	protected abstract void Place(Track track, Spline spline);

	public abstract void notifyCollection();
}
