using UnityEngine;

[AddComponentMenu("Dash/Cameras/Rubbernecker (death cam)")]
public class CameraTypeDeath : CameraType
{
	private void LateUpdate()
	{
		if (!(Sonic.MeshTransform == null))
		{
			Vector3 position = Sonic.MeshTransform.position;
			Debug.DrawLine(base.transform.position, position, Color.red);
			Vector3 vector = position + (-Sonic.MeshTransform.forward * 0.75f + Sonic.MeshTransform.up * 0.6f).normalized * 10f;
			Debug.DrawLine(base.transform.position, vector, Color.blue);
			base.transform.position = vector;
			base.CachedLookAt = Sonic.MeshTransform.position;
			base.transform.rotation = Quaternion.LookRotation((base.CachedLookAt - base.transform.position).normalized, Sonic.MeshTransform.up);
		}
	}
}
