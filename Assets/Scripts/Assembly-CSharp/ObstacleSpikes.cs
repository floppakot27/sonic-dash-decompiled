public class ObstacleSpikes : Obstacle
{
	private class ObstacleSpikesCollisionResolver : CollisionResolver
	{
		public ObstacleSpikesCollisionResolver()
			: base(ResolutionType.SonicDeath)
		{
		}

		public override void ProcessMotionState(MotionState state, bool heldRings, bool ghosted)
		{
			if (ghosted)
			{
				base.Resolution = ResolutionType.Nothing;
			}
			else if (heldRings)
			{
				base.Resolution = ResolutionType.SonicStumble;
			}
			else
			{
				base.Resolution = ResolutionType.SonicDieForwards;
			}
		}
	}

	private bool m_isPlaced;

	private Spline CurrentSpline { get; set; }

	private Track Track { get; set; }

	public override void Start()
	{
		base.Start();
		base.CollisionResolver = new ObstacleSpikesCollisionResolver();
	}

	public void Update()
	{
		if (m_isPlaced && CurrentSpline == null)
		{
			DestroySelf();
		}
	}

	public override void OnSonicKill(SonicSplineTracker sonicSplineTracker)
	{
	}

	public override void OnDeath(object[] onDeathParams)
	{
		DestroySelf();
	}

	public override void OnStumble(SonicSplineTracker killer)
	{
	}

	public override Spline getSpline()
	{
		return CurrentSpline;
	}

	protected override void Place(Track track, Spline spline)
	{
		CurrentSpline = spline;
		Track = track;
		m_isPlaced = true;
	}
}
