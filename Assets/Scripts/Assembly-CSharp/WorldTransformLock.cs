using UnityEngine;

public struct WorldTransformLock
{
	public LightweightTransform CurrentTransform => new LightweightTransform(World.TransformPoint(LocalTransform.Location), LocalTransform.Orientation);

	public bool IsValid => World != null;

	public Transform World { get; private set; }

	public LightweightTransform LocalTransform { get; private set; }

	public WorldTransformLock(Behaviour worldBehaviour, LightweightTransform worldTransform)
		: this(worldBehaviour.transform, worldTransform)
	{
	}

	public WorldTransformLock(GameObject worldObject, LightweightTransform worldTransform)
		: this(worldObject.transform, worldTransform)
	{
	}

	public WorldTransformLock(Transform world, LightweightTransform worldTransform)
	{
		World = world;
		LocalTransform = new LightweightTransform(world.InverseTransformPoint(worldTransform.Location), worldTransform.Orientation);
	}

	public void UpdateLocalTransform(LightweightTransform newWorldTransform)
	{
		UpdateLocalTransform(newWorldTransform.Location, newWorldTransform.Orientation);
	}

	public void UpdateLocalTransform(Transform newWorldTransform)
	{
		UpdateLocalTransform(newWorldTransform.position, newWorldTransform.rotation);
	}

	public void UpdateLocalTransform(Vector3 location, Quaternion rotation)
	{
		LocalTransform = new LightweightTransform(World.InverseTransformPoint(location), rotation);
	}
}
