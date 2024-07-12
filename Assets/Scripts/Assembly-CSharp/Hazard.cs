public abstract class Hazard : SpawnableObject
{
	private CollisionResolver m_collisionResolver;

	public CollisionResolver CollisionResolver
	{
		get
		{
			return m_collisionResolver;
		}
		protected set
		{
			m_collisionResolver = value;
		}
	}

	public virtual void Start()
	{
	}

	public abstract void OnSonicKill(SonicSplineTracker sonicSplineTracker);

	public abstract void OnStumble(SonicSplineTracker sonicSplineTracker);

	public abstract void OnDeath(object[] onDeathParams);
}
