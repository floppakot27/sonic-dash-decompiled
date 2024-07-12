using System.Collections.Generic;

public class TrackInfo
{
	private OneDTree<TrackEntity> m_info = new OneDTree<TrackEntity>();

	private int m_insertsSinceRebalance;

	public void EntitiesInRange(float minInclusive, float maxInclusive, ref IList<TrackEntity> entitiesOut)
	{
		m_info.AllNodesInRange(minInclusive, maxInclusive, ref entitiesOut);
	}

	public void EntitiesInRange(uint entityMask, float minInclusive, float maxInclusive, ref IList<TrackEntity> entitiesOut)
	{
		m_info.AllNodesInRange(minInclusive, maxInclusive, ref entitiesOut, (TrackEntity e) => ((uint)e.InstanceKind & entityMask) != 0);
	}

	public void EntitiesInRange(float minInclusive, float maxInclusive, uint entityMask, Track.Lane lane, ref IList<TrackEntity> entitiesOut)
	{
		m_info.AllNodesInRange(minInclusive, maxInclusive, ref entitiesOut, (TrackEntity e) => ((uint)e.InstanceKind & entityMask) != 0 && e.Lane == lane);
	}

	public void DistanceEntitiesInRange(float minInclusive, float maxInclusive, uint entityMask, Track.Lane lane, ref IList<Pair<float, TrackEntity>> distanceEntitiesOut)
	{
		m_info.AllDistanceNodesInRange(minInclusive, maxInclusive, ref distanceEntitiesOut, (TrackEntity e) => ((uint)e.InstanceKind & entityMask) != 0 && e.Lane == lane);
	}

	public void RegisterEntity(TrackEntity entity, float distance)
	{
		m_info.Insert(entity, distance);
		m_insertsSinceRebalance++;
		if (m_insertsSinceRebalance > 15)
		{
			m_info.Rebalance();
			m_insertsSinceRebalance = 0;
		}
	}

	public void RemoveAllEntitiesBeforeTrackDistance(float trackDistance)
	{
		m_info.RemoveAllBeforePosition(trackDistance);
	}

	public bool IsEntityInRange(uint entitiesMask, float minDistance, float maxDistance)
	{
		return m_info.IsInRange(minDistance, maxDistance, (TrackEntity e) => ((uint)e.InstanceKind & entitiesMask) != 0 && e.IsValid);
	}

	public bool IsEntityInRange(uint entitiesMask, float minDistance, float maxDistance, Track.Lane inLane)
	{
		return m_info.IsInRange(minDistance, maxDistance, (TrackEntity e) => ((uint)e.InstanceKind & entitiesMask) != 0 && e.Lane == inLane && e.IsValid);
	}

	public bool IsEntityInRange(TrackEntity.Kind entity, float minDistance, float maxDistance)
	{
		return IsEntityInRange((uint)entity, minDistance, maxDistance);
	}

	public bool IsEntityAroundDistance(uint entitiesMask, float distance, float epsilon)
	{
		return IsEntityInRange(entitiesMask, distance - epsilon, distance + epsilon);
	}

	public bool IsEntityAroundDistance(TrackEntity.Kind entity, float distance, float epsilon)
	{
		return IsEntityAroundDistance((uint)entity, distance, epsilon);
	}
}
