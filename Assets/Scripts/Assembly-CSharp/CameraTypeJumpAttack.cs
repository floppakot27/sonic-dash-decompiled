using UnityEngine;

[AddComponentMenu("Dash/Cameras/JumpAttack")]
public class CameraTypeJumpAttack : CameraType
{
	private float m_initialDistanceFromSonic;

	public float m_transitionInTime = 0.33f;

	public float m_transitionOutTime = 0.33f;

	public float m_distanceFromSonic = 8f;

	public float m_heightComponent = 1f;

	public float m_groundComponent = 2f;

	public override bool EnableSmoothing => false;

	private void LateUpdate()
	{
		if (!(Sonic.Transform == null))
		{
			Vector3 vector = Sonic.Transform.position + Vector3.up * 1f;
			Vector3 forward = Sonic.Transform.forward;
			Vector3 up = Sonic.Transform.up;
			base.CachedLookAt = vector;
			Vector3 normalized = (forward * (0f - m_groundComponent) + Vector3.up * m_heightComponent).normalized;
			Vector3 vector2 = normalized * m_distanceFromSonic;
			Vector3 position = vector + vector2;
			Vector3 forward2 = -normalized;
			base.transform.position = position;
			base.transform.rotation = Quaternion.LookRotation(forward2, up);
		}
	}
}
