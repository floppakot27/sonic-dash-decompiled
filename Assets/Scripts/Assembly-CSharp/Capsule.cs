using UnityEngine;

[AddComponentMenu("Dash/Powerups/Capsule")]
internal class Capsule : Powerup
{
	private Spline CurrentSpline { get; set; }

	private Track Track { get; set; }

	protected override void Place(Track track, Spline spline)
	{
		CurrentSpline = spline;
		Track = track;
		base.transform.position += base.transform.up;
		Activate();
	}

	public override void notifyCollection()
	{
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
