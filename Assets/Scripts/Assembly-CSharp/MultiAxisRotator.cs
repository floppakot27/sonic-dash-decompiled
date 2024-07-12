using UnityEngine;

public class MultiAxisRotator : MonoBehaviour
{
	public float m_multiplier = 1f;

	public Vector3 m_angleVelocity;

	private void Awake()
	{
	}

	private void OnEnable()
	{
	}

	private void OnDisable()
	{
	}

	private void Update()
	{
		base.transform.localRotation = Quaternion.Slerp(base.transform.localRotation, base.transform.localRotation * Quaternion.Euler(m_angleVelocity), Time.deltaTime * m_multiplier);
	}
}
