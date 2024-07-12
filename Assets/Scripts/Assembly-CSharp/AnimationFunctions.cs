using System.Collections;
using UnityEngine;

public class AnimationFunctions : MonoBehaviour
{
	private void OnEnable()
	{
	}

	private void OnDisable()
	{
		StopAllCoroutines();
	}

	private void OnDestroy()
	{
		StopAllCoroutines();
	}

	public void GenerateEvent(string eventName)
	{
		EventDispatch.GenerateEvent(eventName);
	}

	public void AlterTimeScale(float scale, float duration)
	{
		StartCoroutine(DoAlterScale(scale, duration));
	}

	private IEnumerator DoAlterScale(float scale, float duration)
	{
		TimeScaler.Scale = scale;
		for (float timer = 0f; timer < duration; timer += IndependantTimeDelta.Delta)
		{
			yield return null;
		}
		TimeScaler.Scale = 1f;
	}

	public void WobbleCamera(float intensity)
	{
		CameraWobble.WobbleCamera(intensity);
	}

	public void FreezeGameIntro(float duration)
	{
		StartCoroutine(DoFreezeGameAndTriggerHUD(duration, "OnBossBattleIntroStart", "OnBossBattleIntroEnd"));
		StartCoroutine(TriggerEventWait(0.25f, "BossMusicStart"));
	}

	public void FreezeGameOutro(float duration)
	{
		StartCoroutine(DoFreezeGameAndTriggerHUD(duration, "OnBossBattleOutroStart", "OnBossBattleOutroEnd"));
		StartCoroutine(TriggerEventWait(0.25f, "GameMusicStart", 0.5f));
	}

	private IEnumerator DoFreezeGameAndTriggerHUD(float duration, string eventnameStart, string eventnameEnd)
	{
		EventDispatch.GenerateEvent(eventnameStart);
		TimeScaler.BossIntroTimeScale = 0f;
		for (float timer = 0f; timer < duration; timer += IndependantTimeDelta.Delta * TimeScaler.Scale)
		{
			yield return null;
		}
		EventDispatch.GenerateEvent(eventnameEnd);
		yield return null;
		TimeScaler.BossIntroTimeScale = 1f;
	}

	private IEnumerator TriggerEventWait(float waitTime, string eventName)
	{
		return TriggerEventWait(waitTime, eventName, null);
	}

	private IEnumerator TriggerEventWait(float waitTime, string eventName, object parameter)
	{
		for (float timer = 0f; timer < waitTime; timer += IndependantTimeDelta.Delta * TimeScaler.Scale)
		{
			yield return null;
		}
		EventDispatch.GenerateEvent(eventName, parameter);
	}
}
