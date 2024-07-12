using UnityEngine;

public class CameraType : MonoBehaviour
{
	public Vector3 CachedLookAt { get; set; }

	public virtual bool EnableSmoothing => true;

	public virtual void onActive()
	{
	}

	public virtual void onInactive()
	{
	}
}
