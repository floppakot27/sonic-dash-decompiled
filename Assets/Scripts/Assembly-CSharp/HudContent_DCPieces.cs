using System;
using UnityEngine;

public class HudContent_DCPieces
{
	[Flags]
	private enum State
	{
		None = 0,
		Visible = 1,
		Showing = 2
	}

	private MeshRenderer[] m_piecesMeshes;

	private bool[] m_pieces;

	private State m_state;

	private GameObject m_displayRoot;

	private GameObject m_displayTrigger;

	private float m_displayDuration;

	private AudioClip m_audio;

	private float m_displayTimer;

	public HudContent_DCPieces(GameObject displayRoot, GameObject displayTrigger, float displayDuration, MeshRenderer[] meshes, AudioClip audio)
	{
		m_displayRoot = displayRoot;
		m_displayTrigger = displayTrigger;
		m_piecesMeshes = meshes;
		m_displayDuration = displayDuration;
		m_audio = audio;
		EventDispatch.RegisterInterest("OnDCPieceCollected", this, EventDispatch.Priority.Low);
		m_pieces = DCs.GetPieces();
	}

	public void Update()
	{
		if ((m_state & State.Visible) == State.Visible)
		{
			UpdateDisplayTimer();
		}
	}

	private void Display()
	{
		for (int i = 0; i < m_pieces.Length; i++)
		{
			float a = ((!m_pieces[i]) ? 0.1f : 1f);
			Material material = m_piecesMeshes[i].material;
			Color color = material.color;
			color.a = a;
			material.color = color;
		}
		m_displayTrigger.SendMessage("OnClick", SendMessageOptions.RequireReceiver);
		Audio.PlayClip(m_audio, loop: false);
		m_state |= State.Showing;
		m_displayTimer = 0f;
	}

	private void Hide()
	{
		if ((m_state & State.Showing) == State.Showing)
		{
			m_displayTrigger.SendMessage("OnClick", SendMessageOptions.RequireReceiver);
			m_state &= ~State.Showing;
			m_displayTimer = 0f;
		}
	}

	private void UpdateDisplayTimer()
	{
		m_displayTimer += Time.deltaTime;
		if (m_displayTimer > m_displayDuration)
		{
			Hide();
		}
	}

	public void OnResetOnNewGame()
	{
		if ((m_state & State.Showing) == State.Showing)
		{
			Hide();
		}
		m_displayTimer = 0f;
		m_state = State.None;
	}

	public void OnPauseStateChanged(bool paused)
	{
	}

	public void OnPlayerDeath()
	{
	}

	public void HudVisible(bool visible)
	{
		if (visible)
		{
			m_state |= State.Visible;
		}
		else
		{
			m_state &= ~State.Visible;
		}
	}

	private void Event_OnDCPieceCollected(int number)
	{
		m_pieces = DCs.GetPieces();
		Display();
	}
}
