using UnityEngine;

public class DCPiece : SpawnableObject
{
	[SerializeField]
	private int m_pieceNumber;

	public int PieceNumber => m_pieceNumber;

	private Spline CurrentSpline { get; set; }

	private Track Track { get; set; }

	public override void Place(OnEvent onDestroy, Track track, Spline spline)
	{
		base.Place(onDestroy, track, spline);
		CurrentSpline = spline;
		Track = track;
		base.transform.position += 2f * base.transform.up;
		Activate();
	}

	public void notifyCollection()
	{
		DCs.CollectPiece(PieceNumber);
		Deactivate();
	}

	private void Activate()
	{
		Component[] componentsInChildren = base.transform.GetComponentsInChildren(typeof(Renderer), includeInactive: false);
		Component[] array = componentsInChildren;
		for (int i = 0; i < array.Length; i++)
		{
			Renderer renderer = (Renderer)array[i];
			renderer.enabled = true;
		}
		Collider[] componentsInChildren2 = GetComponentsInChildren<Collider>();
		foreach (Collider collider in componentsInChildren2)
		{
			collider.enabled = true;
		}
	}

	private void Deactivate()
	{
		Component[] componentsInChildren = base.transform.GetComponentsInChildren(typeof(Renderer), includeInactive: false);
		Component[] array = componentsInChildren;
		for (int i = 0; i < array.Length; i++)
		{
			Renderer renderer = (Renderer)array[i];
			renderer.enabled = false;
		}
		Collider[] componentsInChildren2 = GetComponentsInChildren<Collider>();
		foreach (Collider collider in componentsInChildren2)
		{
			collider.enabled = false;
		}
	}
}
