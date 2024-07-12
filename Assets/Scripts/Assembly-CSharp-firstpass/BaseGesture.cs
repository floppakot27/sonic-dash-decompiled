using UnityEngine;

public class BaseGesture : MonoBehaviour
{
	public enum FingerLocation
	{
		Over = 0,
		Always = 1,
		NotOver = 2,
		AtLeastOneOver = 3,
	}

	public enum FingerCountRestriction
	{
		Any = 0,
		One = 1,
		Two = 2,
		Three = 3,
		Four = 4,
		Five = 5,
		OneOrTwo = 6,
		OneOrTwoOrThree = 7,
		TwoOrThree = 8,
	}

	public enum XYRestriction
	{
		AllDirections = 0,
		XDirecton = 1,
		YDirection = 2,
	}

	public GameObject[] targetMessageObjects;
	public Camera alternateCamera;
	public Collider targetCollider;
	public bool topColliderOnly;
	public int fingerCount;
	public bool activeChange;
	public Bounds emptyBounds;
}
