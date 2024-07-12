using UnityEngine;

public class CollisionCallback : MonoBehaviour
{
	public delegate void CollisionResult(Collision other);

	private CollisionResult m_collisionEnterResult;

	public CollisionResult CollisionEnter
	{
		set
		{
			m_collisionEnterResult = value;
		}
	}

	private void OnCollisionEnter(Collision other)
	{
		if (m_collisionEnterResult != null)
		{
			m_collisionEnterResult(other);
		}
	}
}
