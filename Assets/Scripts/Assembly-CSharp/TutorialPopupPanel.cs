using AnimationOrTween;
using UnityEngine;

public class TutorialPopupPanel : MonoBehaviour
{
	private enum State
	{
		Inactive,
		TransitionIn,
		Hold,
		TransitionOut,
		End
	}

	private State m_state;

	private float m_timer;

	private Animation[] m_animations;

	private float m_sfxTimer;

	public AudioClip m_sfx;

	public float m_sfxDelay;

	public float m_transitionInTime;

	public float m_holdTime;

	public float m_transitionOutTime;

	private void FindRequiredComponents()
	{
		if (m_animations == null)
		{
			m_animations = GetComponentsInChildren<Animation>(includeInactive: true);
		}
	}

	private void Update()
	{
		if (m_state != 0)
		{
			m_timer -= Time.deltaTime;
			switch (m_state)
			{
			case State.TransitionIn:
				if (m_timer <= 0f)
				{
					m_state = State.Hold;
					m_timer = m_holdTime;
					base.transform.localScale = new Vector3(1f, 1f, 1f);
				}
				else
				{
					base.transform.localScale = new Vector3(1f, 1f - m_timer / m_transitionInTime, 1f);
				}
				break;
			case State.Hold:
				if (m_timer <= 0f)
				{
					m_state = State.TransitionOut;
					m_timer = m_transitionOutTime;
				}
				break;
			case State.TransitionOut:
				if (m_timer <= 0f)
				{
					m_state = State.End;
					base.gameObject.SetActive(value: false);
				}
				else
				{
					base.transform.localScale = new Vector3(1f, m_timer / m_transitionOutTime, 1f);
				}
				break;
			}
		}
		else if (m_transitionInTime + m_holdTime + m_transitionOutTime > 0f)
		{
			base.transform.localScale = new Vector3(1f, 0f, 1f);
		}
		if (m_sfxTimer > 0f)
		{
			m_sfxTimer -= Time.deltaTime;
			if (m_sfxTimer <= 0f)
			{
				Audio.PlayClip(m_sfx, loop: false);
			}
		}
	}

	private void Trigger_BeginAnimation()
	{
		if (m_transitionInTime + m_holdTime + m_transitionOutTime > 0f)
		{
			m_state = State.TransitionIn;
			m_timer = m_transitionInTime;
		}
		else
		{
			FindRequiredComponents();
			Animation[] animations = m_animations;
			foreach (Animation animation in animations)
			{
				animation.Stop();
				ActiveAnimation.Play(animation, null, Direction.Forward, EnableCondition.DoNothing, DisableCondition.DisableAfterReverse);
			}
		}
		if ((bool)m_sfx)
		{
			m_sfxTimer = m_sfxDelay;
		}
	}
}
