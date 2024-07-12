using System;
using UnityEngine;

public class CameraScheduler : MonoBehaviour
{
	public enum GameCameraType
	{
		Behind,
		Side,
		Front
	}

	private static CameraScheduler s_scheduler;

	private GameCamera[] m_cameras = new GameCamera[Utils.GetEnumCount<GameCameraType>()];

	private GameCameraType m_currentCamera;

	public CameraTypeAnimation OverridingAnimationCamera;

	public CameraTypeMain MainCamera { get; private set; }

	public GameCamera CurrentGameCamera => m_cameras[(int)m_currentCamera];

	public static GameCamera GetCurrentGameCamera()
	{
		return s_scheduler.CurrentGameCamera;
	}

	public static void UseAnimationCamera(CameraTypeAnimation animationCamera)
	{
		s_scheduler.setAnimationCamera(animationCamera);
	}

	public static void ClearAnimationCamera()
	{
		s_scheduler.setAnimationCamera(null);
	}

	private void setAnimationCamera(CameraTypeAnimation animationCam)
	{
		MainCamera.SetAnimationCamera(animationCam);
	}

	private void Start()
	{
		EventDispatch.RegisterInterest("OnBossBattlePhaseStart", this);
		EventDispatch.RegisterInterest("OnBossBattleEnd", this);
		EventDispatch.RegisterInterest("ResetGameState", this);
		s_scheduler = this;
		StoreGameCameras();
		foreach (int value in Enum.GetValues(typeof(GameCameraType)))
		{
			SetCameraTypeActive((GameCameraType)value, value == (int)m_currentCamera);
		}
		GameObject gameObject = GameObject.FindGameObjectWithTag("GameCamera");
		MainCamera = gameObject.GetComponent<CameraTypeMain>();
	}

	private void Event_OnBossBattlePhaseStart(int currentPhase)
	{
		BossBattleSystem.Phase phase = BossBattleSystem.Instance().GetPhase(currentPhase);
		SwitchToCamera(phase.CameraType);
		CurrentGameCamera.CurrentTransitionTime = 1f;
	}

	private void Event_OnBossBattleEnd()
	{
		SwitchToCamera(GameCameraType.Behind);
	}

	private void Event_ResetGameState(GameState.Mode resetState)
	{
		SwitchToCamera(GameCameraType.Behind);
		CurrentGameCamera.CurrentTransitionTime = 0f;
	}

	private void StoreGameCameras()
	{
		m_cameras[0] = GetComponentInChildren<BehindCamera>();
		m_cameras[1] = GetComponentInChildren<SideCamera>();
		m_cameras[2] = GetComponentInChildren<FrontCamera>();
	}

	private void SwitchToCamera(GameCameraType cameraType)
	{
		if (m_currentCamera != cameraType)
		{
			SetCameraTypeActive(m_currentCamera, active: false);
			m_currentCamera = cameraType;
			SetCameraTypeActive(m_currentCamera, active: true);
		}
	}

	private void SetCameraTypeActive(GameCameraType cameraType, bool active)
	{
		GameCamera gameCamera = m_cameras[(int)cameraType];
		gameCamera.enabled = active;
	}

	private void Update()
	{
	}
}
