using System.Collections;
using UnityEngine;

[AddComponentMenu("Dash/Sonic/Menu Animations Control")]
public class SonicMenuAnimationsControl : MonoBehaviour
{
	public GameObject m_OnReadyEventReceiver;

	private LightweightTransform m_gameStartTransform;

	private LightweightTransform m_menuStartTransform;

	private float m_fSpeed;

	[SerializeField]
	private float m_startTransitionTime = 1.25f;

	private void Awake()
	{
		base.transform.position = Vector3.zero;
		base.transform.rotation = Quaternion.identity;
		m_gameStartTransform = new LightweightTransform(base.transform);
		m_menuStartTransform = new LightweightTransform(new Vector3(0f, 0f, -6f), Quaternion.identity);
		float magnitude = (m_menuStartTransform.Location - m_gameStartTransform.Location).magnitude;
		m_fSpeed = magnitude / m_startTransitionTime;
		EventDispatch.RegisterInterest("CharacterLoaded", this);
		EventDispatch.RegisterInterest("ResetGameState", this);
	}

	private void Event_CharacterLoaded()
	{
		Sonic.Transform.position = m_gameStartTransform.Location - Vector3.up * 10000f;
	}

	public void OnDestroy()
	{
		EventDispatch.UnregisterAllInterest(this);
	}

	private void Event_ResetGameState(GameState.Mode mode)
	{
		if (mode == GameState.Mode.Menu && (bool)Sonic.Transform)
		{
			Sonic.Transform.position = m_gameStartTransform.Location - Vector3.up * 10000f;
		}
	}

	public IEnumerator GameIntroSequence()
	{
		StopAllCoroutines();
		return NewGameIntroSequence();
	}

	private IEnumerator NewGameIntroSequence()
	{
		EventDispatch.GenerateEvent("OnNewGameAboutToStart");
		GameMusic.Singleton.OnNewGameStarted();
		yield return new WaitForEndOfFrame();
		m_menuStartTransform.ApplyTo(Sonic.Transform);
		Sonic.AnimationControl.RestartRun();
		yield return new WaitForEndOfFrame();
		if ((bool)m_OnReadyEventReceiver)
		{
			m_OnReadyEventReceiver.SendMessage("OnClick");
		}
		yield return new WaitForEndOfFrame();
		float gameIntroLerp = 0f;
		while (gameIntroLerp < 1f)
		{
			LightweightTransform.Lerp(m_menuStartTransform, m_gameStartTransform, gameIntroLerp).ApplyTo(Sonic.Transform);
			gameIntroLerp += Time.deltaTime / m_startTransitionTime;
			Sonic.AnimationControl.UpdateRun(m_fSpeed * 3f);
			yield return null;
		}
		m_gameStartTransform.ApplyTo(base.transform);
	}
}
