public abstract class CollisionResolver
{
	public enum ResolutionType
	{
		Nothing,
		SonicDeath,
		EnemyDeath,
		SonicStumble,
		SonicKnockedLeft,
		SonicKnockedRight,
		SonicDieForwards
	}

	public ResolutionType Resolution { get; protected set; }

	protected CollisionResolver(ResolutionType defaultResolution)
	{
		Resolution = defaultResolution;
	}

	public ResolutionType Resolve(MotionState state, bool heldRings, bool ghosted)
	{
		ProcessMotionState(state, heldRings, ghosted);
		return Resolution;
	}

	public virtual void ProcessMotionState(MotionState state, bool heldRings, bool ghosted)
	{
	}
}
