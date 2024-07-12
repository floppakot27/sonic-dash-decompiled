using UnityEngine;

public class CameraProperties : MonoBehaviour
{
	[SerializeField]
	private float m_fov = 75f;

	public float FOV
	{
		get
		{
			return m_fov;
		}
		set
		{
			m_fov = value;
		}
	}
}
