using UnityEngine;

public class MenuMeshBobble : MonoBehaviour
{
	private Quaternion m_baseRotation;

	private bool m_bobble;

	[SerializeField]
	private Vector3 m_bobbleStart = new Vector3(0f, 0f, 0f);

	[SerializeField]
	private Vector3 m_bobbleSpeed = new Vector3(0f, 0f, 0f);

	[SerializeField]
	private Vector3 m_bobbleAmount = new Vector3(0f, 0f, 0f);

	[SerializeField]
	private bool m_ignoreTimeScale = true;

	private void Start()
	{
		m_baseRotation = base.transform.localRotation;
		m_bobble = true;
	}

	private void OnEnable()
	{
		m_bobble = true;
	}

	private void OnDisable()
	{
		m_bobble = false;
		base.transform.localRotation = m_baseRotation;
	}

	private void Update()
	{
		if (m_bobble)
		{
			Bobble();
		}
	}

	public void Bobble()
	{
		float num = ((!m_ignoreTimeScale) ? Time.deltaTime : IndependantTimeDelta.Delta);
		m_bobbleStart.x += num * m_bobbleSpeed.x;
		m_bobbleStart.y += num * m_bobbleSpeed.y;
		m_bobbleStart.z += num * m_bobbleSpeed.z;
		float num2 = Mathf.Sin(m_bobbleStart.x);
		float num3 = Mathf.Sin(m_bobbleStart.y);
		float num4 = Mathf.Sin(m_bobbleStart.z);
		base.transform.localRotation = m_baseRotation;
		base.transform.Rotate(m_bobbleAmount.x * num2, m_bobbleAmount.y * num3, m_bobbleAmount.z * num4);
	}
}
