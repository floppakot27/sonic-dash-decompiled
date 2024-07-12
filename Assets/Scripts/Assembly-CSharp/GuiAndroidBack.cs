using UnityEngine;

[AddComponentMenu("Dash/UI")]
public class GuiAndroidBack : IgnoreTimeScale
{
	private const float m_backDisableTime = 0.5f;

	public bool m_killApplication;

	private BoxCollider m_boxCollider;

	private UISprite m_renderableChild;

	private GuiButtonBlocker m_buttonBlocker;

	private float m_timeEnabled;

	private static float m_lastClickTime;

	private void Awake()
	{
		UISprite[] componentsInChildren = GetComponentsInChildren<UISprite>();
		if (componentsInChildren.Length > 0)
		{
			m_renderableChild = componentsInChildren[0];
		}
		m_boxCollider = GetComponent<BoxCollider>();
		m_buttonBlocker = GetComponent<GuiButtonBlocker>();
	}

	private void Update()
	{
		float num = UpdateRealTimeDelta();
		if (m_boxCollider.enabled)
		{
			m_timeEnabled += num;
		}
		else
		{
			m_timeEnabled = 0f;
		}
		if (!Input.GetKeyDown(KeyCode.Escape) || m_timeEnabled < 0.5f)
		{
			return;
		}
		float realtimeSinceStartup = Time.realtimeSinceStartup;
		if (!(m_lastClickTime + 0.5f > realtimeSinceStartup) && (!(m_boxCollider != null) || m_boxCollider.enabled) && (!(m_buttonBlocker != null) || !m_buttonBlocker.Blocked) && (!(m_renderableChild != null) || m_renderableChild.isVisible))
		{
			m_timeEnabled = 0f;
			m_lastClickTime = Time.realtimeSinceStartup;
			if (m_killApplication)
			{
				Application.Quit();
			}
			else
			{
				base.gameObject.SendMessage("OnClick");
			}
		}
	}
}
