using UnityEngine;

public class UIGestureRecognised : MonoBehaviour
{
	private const float m_kfShownDuration = 2f;

	private float m_fTimer;

	private UILabel m_uiLabel;

	public bool m_debugOutput;

	private void Start()
	{
		Object.DontDestroyOnLoad(this);
		GameObject gameObject = GameObject.Find("Label (Gesture Recognised)");
		if (gameObject != null)
		{
			m_uiLabel = GetComponent<UILabel>();
		}
	}

	private void UpdateDisplay()
	{
		if (m_uiLabel != null && m_fTimer > 0f)
		{
			m_fTimer -= Time.deltaTime;
			if (m_fTimer <= 0f)
			{
				ApplyText(string.Empty);
			}
		}
	}

	private void Update()
	{
		UpdateDisplay();
	}

	private void ApplyText(string s)
	{
		if ((bool)m_uiLabel)
		{
			m_fTimer = 2f;
			m_uiLabel.text = s;
		}
		if (!m_debugOutput)
		{
		}
	}

	public void Strafe(SideDirection direction)
	{
		if (direction == SideDirection.Left)
		{
			ApplyText($"Gesture: Strafe Left");
		}
		else
		{
			ApplyText($"Gesture: Strafe Right");
		}
	}

	public void Roll()
	{
		ApplyText($"Gesture: Roll");
	}

	public void Dive()
	{
		ApplyText($"Gesture: Dive");
	}

	public void Jump()
	{
		ApplyText($"Gesture: Jump");
	}

	public void Tap(float[] array)
	{
		ApplyText($"Gesture: Tap");
	}
}
