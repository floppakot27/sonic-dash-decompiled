using System.Collections;
using UnityEngine;

public class BossFlyBy : MonoBehaviour
{
	private BossMovementController m_movementController;

	private BossAnimationController m_animationController;

	private BossAudioController m_audioController;

	[SerializeField]
	private Vector3 m_flybyStartInSonicLocalSpace = Vector3.zero;

	[SerializeField]
	private Vector3 m_flybyEndInSonicLocalSpace = Vector3.zero;

	[SerializeField]
	private AnimationCurve m_flybyProgressCurve;

	[SerializeField]
	private float m_flybyAnimTriggerTime = 2f;

	[SerializeField]
	private float m_flybySFXTriggerTime = 4f;

	[SerializeField]
	private bool m_enableBossDrift = true;

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
		float timer = 0f;
		bool flybyAnimTriggered = false;
		bool tauntSFXTriggered = false;
		BossMovementController.MovementParameters moveParams = new BossMovementController.MovementParameters
		{
			m_destination = m_flybyStartInSonicLocalSpace,
			m_duration = 0f,
			m_moveWithTracker = true,
			m_faceMovementDirection = false,
			m_splineDistanceFromSonic = 0f
		};
		m_movementController.MoveToDestination(moveParams, m_enableBossDrift, snapToDestination: true);
		yield return null;
		m_movementController.SetOrientation(m_flybyEndInSonicLocalSpace - m_flybyStartInSonicLocalSpace, trackerSpace: true);
		while (timer <= m_flybyProgressCurve.keys[m_flybyProgressCurve.length - 1].time)
		{
			if (timer > m_flybyAnimTriggerTime && !flybyAnimTriggered)
			{
				m_animationController.PlayAnimation(BossAnim.Flyby);
				flybyAnimTriggered = true;
			}
			if (timer > m_flybySFXTriggerTime && !tauntSFXTriggered)
			{
				m_audioController.PlayIntroTauntSFX();
				tauntSFXTriggered = true;
			}
			Vector3 position = Vector3.Lerp(t: m_flybyProgressCurve.Evaluate(timer), from: m_flybyStartInSonicLocalSpace, to: m_flybyEndInSonicLocalSpace);
			m_movementController.MoveToDestination(position, m_enableBossDrift, snapToDestination: false);
			timer += Time.deltaTime;
			yield return null;
		}
	}
}
