using UnityEngine;

public class GuiButtonBlocker : MonoBehaviour
{
	[SerializeField]
	private bool m_blocked;

	public bool Blocked
	{
		get
		{
			return m_blocked;
		}
		set
		{
			m_blocked = value;
		}
	}
}
