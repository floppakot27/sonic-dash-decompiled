using UnityEngine;

public class MotionMonitor : MonoBehaviour
{
	private bool m_isMoving = true;

	public void setMoving()
	{
		m_isMoving = true;
	}

	public void setStatic()
	{
		m_isMoving = false;
	}

	public bool isMoving()
	{
		return m_isMoving;
	}
}
