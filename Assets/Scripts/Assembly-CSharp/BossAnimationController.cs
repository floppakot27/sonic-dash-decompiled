using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossAnimationController : MonoBehaviour
{
	private float m_timer;

	private List<float> m_firingTimes = new List<float>();

	private BossVisualController m_visualController;

	private BossAudioController m_audioController;

	[SerializeField]
	private float m_maximumFiringAnimSpeedMultiplier = 1.5f;

	[SerializeField]
	private Animation m_animationCamera;

	[SerializeField]
	private Animation m_animationAttackCamera;

	[SerializeField]
	private Animation m_animationCharacter;

	[SerializeField]
	private AnimationClip[] m_animations = new AnimationClip[Enum.GetNames(typeof(BossAnim)).Length];

	[SerializeField]
	private AnimationClip[] m_cameraAnimations = new AnimationClip[Enum.GetNames(typeof(BossAnim)).Length];

	[SerializeField]
	private AnimationClip[] m_attackCameraAnimations = new AnimationClip[Enum.GetNames(typeof(BossAnim)).Length];

	public bool BlockIdleAnim { get; set; }

	private void OnEnable()
	{
		StartCoroutine(ManageFiringAnimations());
	}

	private void OnDisable()
	{
		StopAllCoroutines();
	}

	private void Awake()
	{
		m_visualController = GetComponent<BossVisualController>();
		m_audioController = GetComponent<BossAudioController>();
	}

	private IEnumerator ManageFiringAnimations()
	{
		float preFiringAnimLength = GetCharacterAnimationLength(BossAnim.PreFire);
		float preFiringAnimLongLength = GetCharacterAnimationLength(BossAnim.PreFireLong);
		float firingAnimLength = GetCharacterAnimationLength(BossAnim.Fire);
		float postFiringAnimLength = GetCharacterAnimationLength(BossAnim.PostFire);
		float fullAnimsToFireAgain = postFiringAnimLength + preFiringAnimLength;
		while (true)
		{
			if (m_firingTimes.Count > 0)
			{
				BossAnim preAnimToUse = BossAnim.PreFire;
				float animLength = preFiringAnimLength;
				if (m_firingTimes[0] - preFiringAnimLength > m_timer)
				{
					preAnimToUse = BossAnim.PreFireLong;
					animLength = preFiringAnimLongLength;
				}
				PlayCharacterAnimation(BossAnim.Idle);
				while (m_firingTimes[0] - preFiringAnimLength > m_timer)
				{
					yield return null;
				}
				float availableTimeForPreFire = m_firingTimes[0] - m_timer;
				float preFirePlaySpeed = Mathf.Max(animLength / availableTimeForPreFire, 1f);
				PlayCharacterAnimation(preAnimToUse, preFirePlaySpeed);
				while (m_firingTimes[0] > m_timer)
				{
					yield return null;
				}
				m_firingTimes.RemoveAt(0);
				m_visualController.PlaySpawnMissileEffect();
				m_audioController.PlayStarFiringSFX();
				float animationSpeedRequired = ((m_firingTimes.Count != 0) ? (fullAnimsToFireAgain / (m_firingTimes[0] - m_timer)) : 1f);
				while (m_firingTimes.Count > 0 && animationSpeedRequired > m_maximumFiringAnimSpeedMultiplier)
				{
					PlayCharacterAnimation(BossAnim.PostFire, 1f);
					while (m_firingTimes[0] - firingAnimLength > m_timer)
					{
						yield return null;
					}
					float availableTime = m_firingTimes[0] - m_timer;
					float playSpeed = Mathf.Max(firingAnimLength / availableTime, 1f);
					PlayCharacterAnimation(BossAnim.Fire, playSpeed);
					while (IsCharacterAnimPlaying(BossAnim.Fire))
					{
						yield return null;
					}
					m_firingTimes.RemoveAt(0);
					m_visualController.PlaySpawnMissileEffect();
					m_audioController.PlayStarFiringSFX();
					animationSpeedRequired = ((m_firingTimes.Count != 0) ? (fullAnimsToFireAgain / (m_firingTimes[0] - m_timer)) : 1f);
				}
				PlayCharacterAnimation(BossAnim.PostFire, animationSpeedRequired);
				while (IsCharacterAnimPlaying(BossAnim.PostFire))
				{
					yield return null;
				}
			}
			else
			{
				if (!BlockIdleAnim && !m_animationCharacter.isPlaying)
				{
					PlayCharacterAnimation(BossAnim.Idle);
				}
				yield return null;
			}
		}
	}

	public void StartFiring(float timeToRelease)
	{
		m_firingTimes.Add(m_timer + timeToRelease);
	}

	public void ClearAllFiring()
	{
		m_firingTimes.Clear();
		StopAllCoroutines();
		if (base.gameObject.activeInHierarchy)
		{
			StartCoroutine(ManageFiringAnimations());
		}
	}

	private void Update()
	{
		m_timer += Time.deltaTime;
	}

	public void PlayAnimation(BossAnim animation)
	{
		PlayAnimation(animation, 1f);
	}

	public void PlayAnimation(BossAnim animation, float speed)
	{
		PlayCharacterAnimation(animation, speed);
		PlayCameraAnimation(animation, speed);
		PlayAttackCameraAnimation(animation, speed);
	}

	public void PlayCharacterAnimation(BossAnim animation)
	{
		PlayCharacterAnimation(animation, 1f);
	}

	public void PlayCharacterAnimation(BossAnim animation, float speed)
	{
		AnimationClip animationClip = m_animations[(int)animation];
		if (animationClip != null)
		{
			m_animationCharacter.Play(animationClip.name);
			AnimationState animationState = m_animationCharacter[animationClip.name];
			animationState.normalizedTime = 0f;
			animationState.speed = speed;
		}
	}

	public void PlayCameraAnimation(BossAnim animation)
	{
		PlayCameraAnimation(animation, 1f);
	}

	public void PlayCameraAnimation(BossAnim animation, float speed)
	{
		AnimationClip animationClip = m_cameraAnimations[(int)animation];
		if (animationClip != null)
		{
			m_animationCamera.Play(animationClip.name);
			AnimationState animationState = m_animationCamera[animationClip.name];
			animationState.normalizedTime = 0f;
			animationState.speed = speed;
		}
	}

	public void PlayAttackCameraAnimation(BossAnim animation)
	{
		PlayAttackCameraAnimation(animation, 1f);
	}

	public void PlayAttackCameraAnimation(BossAnim animation, float speed)
	{
		AnimationClip animationClip = m_attackCameraAnimations[(int)animation];
		if (animationClip != null)
		{
			m_animationAttackCamera.Play(animationClip.name);
			AnimationState animationState = m_animationAttackCamera[animationClip.name];
			animationState.normalizedTime = 0f;
			animationState.speed = speed;
		}
	}

	public float GetCharacterAnimationLength(BossAnim animation)
	{
		AnimationClip animationClip = m_animations[(int)animation];
		if (animationClip != null)
		{
			return animationClip.length;
		}
		return 0f;
	}

	public bool IsCharacterAnimPlaying(BossAnim animation)
	{
		AnimationClip animationClip = m_animations[(int)animation];
		if (animationClip != null)
		{
			return m_animationCharacter.IsPlaying(animationClip.name);
		}
		return false;
	}

	public bool IsCameraAnimPlaying(BossAnim animation)
	{
		AnimationClip animationClip = m_cameraAnimations[(int)animation];
		if (animationClip != null)
		{
			return m_animationCamera.IsPlaying(animationClip.name);
		}
		return false;
	}

	public bool IsAttackCameraAnimPlaying(BossAnim animation)
	{
		AnimationClip animationClip = m_attackCameraAnimations[(int)animation];
		if (animationClip != null)
		{
			return m_animationAttackCamera.IsPlaying(animationClip.name);
		}
		return false;
	}
}
