using UnityEngine;

public class AudioSourceParams : MonoBehaviour
{
	private bool m_Paused;

	public bool IsPaused
	{
		get
		{
			return m_Paused;
		}
		set
		{
			m_Paused = value;
		}
	}

	private void Start()
	{
	}

	private void OnEnable()
	{
		m_Paused = false;
	}

	private void Update()
	{
	}
}
