using System;
using UnityEngine;

public class BehindCamera : GameCamera
{
	[Flags]
	private enum State
	{
		None = 0,
		NewGameNext = 1
	}

	private static BehindCamera m_instance;

	[SerializeField]
	private CameraType m_defaultCamera;

	[SerializeField]
	private CameraTypeSetPiece m_setPieceCamera;

	[SerializeField]
	private CameraType m_deathCamera;

	[SerializeField]
	private CameraType m_attackCamera;

	[SerializeField]
	private CameraType m_springCamera;

	[SerializeField]
	private CameraType m_springBossCamera;

	[SerializeField]
	private float m_toGameTransitionTime = 3f;

	private CameraType m_initialCamera;

	private State m_state;

	private float m_transitionTime = 3f;

	public static BehindCamera Instance => m_instance;

	public CameraTypeSetPiece SetPieceCamera => m_setPieceCamera;

	public CameraType DeathCamera => m_deathCamera;

	public void ResetToGameCamera(float transitionTime)
	{
		SetActiveCamera(m_defaultCamera, transitionTime);
	}

	public static CameraTypeMain GetMainCamera()
	{
		return Instance.MainCamera;
	}

	private void Awake()
	{
		if (m_instance == null)
		{
			m_instance = this;
		}
		CacheMainCamera();
	}

	private void Start()
	{
		EventDispatch.RegisterInterest("ResetGameState", this);
		EventDispatch.RegisterInterest("StartGameState", this);
		EventDispatch.RegisterInterest("EnterMotionRollState", this);
		EventDispatch.RegisterInterest("ExitMotionRollState", this);
		EventDispatch.RegisterInterest("OnSonicDeath", this);
		EventDispatch.RegisterInterest("OnSpringStart", this);
		EventDispatch.RegisterInterest("OnSonicAttack", this);
		EventDispatch.RegisterInterest("OnSonicAttackEnd", this);
		EventDispatch.RegisterInterest("OnSonicResurrection", this);
		base.Camera.enabled = false;
		base.Camera.enabled = true;
	}

	private void ActivateDefaultCamera()
	{
		SetActiveCamera(m_defaultCamera, m_transitionTime);
	}

	private void Event_ResetGameState(GameState.Mode resetState)
	{
		if (resetState == GameState.Mode.Menu)
		{
			m_state |= State.NewGameNext;
			m_transitionTime = m_toGameTransitionTime;
		}
		else
		{
			SetActiveCamera(m_defaultCamera, 0f);
		}
	}

	private void Event_StartGameState(GameState.Mode state)
	{
		if (state == GameState.Mode.Game && (m_state & State.NewGameNext) == State.NewGameNext)
		{
			ActivateDefaultCamera();
			m_state &= ~State.NewGameNext;
		}
	}

	private void Event_EnterMotionRollState()
	{
	}

	private void Event_ExitMotionRollState()
	{
		if (base.MainCamera.GetCurrentCameraType() != m_setPieceCamera)
		{
			SetActiveCamera(m_defaultCamera, m_transitionTime);
		}
	}

	private void Event_OnSonicDeath()
	{
		SetActiveCamera(m_deathCamera, 0.6f);
	}

	private void Event_OnSpringStart(SpringTV.Type springType)
	{
		if (springType == SpringTV.Type.Boss)
		{
			SetActiveCamera(m_springBossCamera, 0.25f);
		}
		else
		{
			SetActiveCamera(m_springCamera, 0.25f);
		}
	}

	private void Event_OnSonicAttack()
	{
		CameraTypeJumpAttack cameraTypeJumpAttack = (CameraTypeJumpAttack)m_attackCamera;
		SetActiveCamera(m_attackCamera, cameraTypeJumpAttack.m_transitionInTime);
	}

	private void Event_OnSonicAttackEnd()
	{
		CameraTypeJumpAttack cameraTypeJumpAttack = (CameraTypeJumpAttack)m_attackCamera;
		ResetToGameCamera(cameraTypeJumpAttack.m_transitionOutTime);
	}

	private void Event_OnSonicResurrection()
	{
		SetActiveCamera(m_defaultCamera, 0.6f);
	}
}
