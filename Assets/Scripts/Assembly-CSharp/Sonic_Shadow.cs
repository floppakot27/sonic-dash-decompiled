using UnityEngine;

public class Sonic_Shadow : MonoBehaviour
{
	public float m_shadowBias = 0.1f;

	private bool m_renderShadow;

	private Renderer[] m_renderers;

	private void Start()
	{
		m_renderers = base.gameObject.GetComponentsInChildren<Renderer>();
		EventDispatch.RegisterInterest("StartGameState", this);
	}

	private void EnableRenderer(bool value)
	{
		if (value == m_renderShadow)
		{
			return;
		}
		if (m_renderers != null)
		{
			for (int i = 0; i < m_renderers.Length; i++)
			{
				m_renderers[i].enabled = value;
			}
		}
		m_renderShadow = value;
	}

	public void Event_StartGameState(GameState.Mode gameMode)
	{
		if (gameMode == GameState.Mode.Game || gameMode == GameState.Mode.Menu)
		{
			EnableRenderer(value: true);
		}
	}

	private void Update()
	{
		if (null == Sonic.Tracker || null == Camera.main)
		{
			return;
		}
		if (Sonic.Tracker.TrackerOverGap || Sonic.Tracker.IsFalling())
		{
			EnableRenderer(value: false);
		}
		else
		{
			EnableRenderer(value: true);
		}
		Vector3 vector;
		if (Sonic.MenuAnimationControl.enabled)
		{
			vector = new Vector3(Sonic.MeshTransform.position.x, Sonic.Transform.position.y, Sonic.MeshTransform.position.z);
		}
		else
		{
			if (Sonic.Tracker.InternalTracker == null)
			{
				return;
			}
			vector = Sonic.MeshTransform.position;
			vector.y = Sonic.Tracker.InternalTracker.CurrentSplineTransform.Location.y;
		}
		Vector3 vector2 = Camera.main.gameObject.transform.position - vector;
		vector2.Normalize();
		base.gameObject.transform.position = vector + vector2 * m_shadowBias;
		base.gameObject.transform.rotation = Sonic.MeshTransform.rotation;
	}
}
