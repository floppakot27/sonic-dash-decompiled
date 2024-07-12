using UnityEngine;

public class DragGesture : BaseGesture
{
	public enum DragPosition
	{
		Relative = 0,
		Centred = 1,
		NoDrag = 2,
	}

	public DragPosition dragPosition;
	public BaseGesture.FingerLocation fingerLocation;
	public BaseGesture.FingerCountRestriction restrictFingerCount;
	public bool doDrag;
	public BaseGesture.XYRestriction restrictDirection;
	public float restrictScreenMin;
	public float restrictScreenMax;
	public int dragFingerCount;
	public Vector3 startPoint;
	public Vector3 endPoint;
	public Vector3 targetPoint;
}
