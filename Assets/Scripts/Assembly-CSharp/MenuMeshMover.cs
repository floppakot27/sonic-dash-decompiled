using UnityEngine;

public class MenuMeshMover : MonoBehaviour
{
	private Vector3 m_basePosition;

	private bool m_positionCached;

	[SerializeField]
	private Vector3 m_activePosition = new Vector3(0f, 0f, 0f);

	private void OnEnable()
	{
		if (!m_positionCached)
		{
			m_basePosition = base.transform.localPosition;
		}
		Vector3 basePosition = m_basePosition;
		basePosition += m_activePosition;
		base.transform.localPosition = basePosition;
	}

	private void OnDisable()
	{
		base.transform.localPosition = m_basePosition;
	}
}
