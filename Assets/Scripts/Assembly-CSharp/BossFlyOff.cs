using System.Collections;
using UnityEngine;

public class BossFlyOff : MonoBehaviour
{
	private BossMovementController m_movementController;

	private BossAnimationController m_animationController;

	private BossAudioController m_audioController;

	[SerializeField]
	private Vector3 m_escapeDestination = Vector3.up * 10f;

	[SerializeField]
	private AnimationCurve m_escapeCurve;

	[SerializeField]
	private float m_escapeDistanceFromSonic = 10f;

	[SerializeField]
	private float m_preEscapeYDistanceFromSpline = 1.3f;

	[SerializeField]
	private float m_preEscapeAnimationTriggerTime = 1f;

	[SerializeField]
	private float m_escapeSFXTriggerTime = 1f;

	[SerializeField]
	private bool m_enableBossDrift = true;

	[SerializeField]
	private float m_cameraWobbleIntensity = 2f;

	private void Awake()
	{
		m_movementController = GetComponent<BossMovementController>();
		m_animationController = GetComponent<BossAnimationController>();
		m_audioController = GetComponent<BossAudioController>();
	}

	private void OnEnable()
	{
		StartCoroutine(performBehaviour());
	}

	private void OnDisable()
	{
		StopAllCoroutines();
	}

	private IEnumerator performBehaviour()
	{
		float maxSpeed = Sonic.Handling.StartSpeed;
		Vector3 desiredPositionOffSpline = Vector3.up * m_preEscapeYDistanceFromSpline;
		BossMovementController.MovementParameters parameters = new BossMovementController.MovementParameters
		{
			m_splineDistanceFromSonic = m_escapeDistanceFromSonic,
			m_destination = desiredPositionOffSpline,
			m_moveWithTracker = true,
			m_duration = 1f,
			m_faceMovementDirection = false,
			m_lane = Track.Lane.Middle
		};
		m_movementController.MoveToDestination(parameters, m_enableBossDrift, snapToDestination: false);
		StartCoroutine(playSFX());
		float waitTimeDistanceRequired = maxSpeed * m_preEscapeAnimationTriggerTime;
		float escapeAnimationTriggerDistance = m_movementController.SplineDistanceTravelled + waitTimeDistanceRequired;
		while (m_movementController.SplineDistanceTravelled < escapeAnimationTriggerDistance)
		{
			yield return null;
		}
		m_animationController.PlayAnimation(BossAnim.FlyOff);
		float timeToWait = m_animationController.GetCharacterAnimationLength(BossAnim.FlyOff);
		while ((double)timeToWait > 0.0)
		{
			yield return null;
			timeToWait -= Time.deltaTime;
		}
		CameraWobble.WobbleCamera(m_cameraWobbleIntensity);
		float timer = 0f;
		for (float maxTime = m_escapeCurve.keys[m_escapeCurve.length - 1].time; timer <= maxTime; timer += Time.deltaTime)
		{
			float progress = m_escapeCurve.Evaluate(timer);
			Vector3 pos = Vector3.Lerp(t: Mathf.Clamp01(progress), from: desiredPositionOffSpline, to: m_escapeDestination);
			m_movementController.MoveToDestination(pos, useDrift: false, snapToDestination: false);
		}
	}

	private IEnumerator playSFX()
	{
		float waitTimeDistanceRequired = Sonic.Handling.StartSpeed * m_escapeSFXTriggerTime;
		float sfxTriggerDistance = m_movementController.SplineDistanceTravelled + waitTimeDistanceRequired;
		while (m_movementController.SplineDistanceTravelled < sfxTriggerDistance)
		{
			yield return null;
		}
		m_audioController.PlayFlyOffSFX();
	}
}
