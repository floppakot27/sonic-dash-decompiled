using System;
using UnityEngine;

public class UIDumbButton : MonoBehaviour
{
	[Flags]
	private enum State
	{
		None = 0,
		Enabled = 1
	}

	private GUITexture m_guiTexture;

	protected bool Enabled { private get; set; }

	protected virtual void Event_ButtonSelected()
	{
	}

	private void Start()
	{
		EventDispatch.RegisterInterest("DisableGameState", this);
		EventDispatch.RegisterInterest("ResetGameState", this);
		m_guiTexture = GetComponent<GUITexture>();
		if (m_guiTexture == null)
		{
			throw new Exception();
		}
		Enabled = false;
	}

	private void Update()
	{
		if (m_guiTexture == null)
		{
			return;
		}
		ShowButton(Enabled);
		if (Enabled && Input.GetMouseButtonUp(0))
		{
			Vector3 mousePosition = Input.mousePosition;
			if (m_guiTexture.HitTest(mousePosition))
			{
				Event_ButtonSelected();
			}
		}
	}

	private void ShowButton(bool show)
	{
		float a = ((!show) ? 0f : 1f);
		Color color = m_guiTexture.color;
		color.a = a;
		m_guiTexture.color = color;
	}

	private void Event_DisableGameState(GameState.Mode state)
	{
		Enabled = false;
	}

	private void Event_ResetGameState(GameState.Mode state)
	{
		Enabled = false;
	}
}
