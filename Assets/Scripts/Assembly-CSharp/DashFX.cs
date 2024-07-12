using UnityEngine;

public class DashFX : MonoBehaviour
{
	private bool m_activated;

	private bool m_blink;

	private bool m_locked;

	private float m_defaultYLocalPosition;

	[SerializeField]
	private float m_rollYOffset = -1f;

	public void ForceIsDashFXActive(bool isActive)
	{
		m_locked = isActive;
		SetIsDashFXActive(isActive);
	}

	private void Start()
	{
		m_activated = false;
		m_defaultYLocalPosition = base.transform.localPosition.y;
		EventDispatch.RegisterInterest("ResetGameState", this);
	}

	public void OnDestroy()
	{
		EventDispatch.UnregisterAllInterest(this);
	}

	private void Event_ResetGameState(GameState.Mode mode)
	{
		if (mode != GameState.Mode.PauseMenu)
		{
			ForceIsDashFXActive(isActive: false);
		}
	}

	private void Update()
	{
		if (null == Sonic.AnimationControl)
		{
			return;
		}
		float num = ((!Sonic.AnimationControl.IsBallShown) ? 0f : m_rollYOffset);
		base.transform.localPosition = new Vector3(base.transform.localPosition.x, m_defaultYLocalPosition + num, base.transform.localPosition.z);
		if (m_locked)
		{
			return;
		}
		bool flag = false;
		if (isDashing())
		{
			if (DashMonitor.instance().isDashNearlyFinished())
			{
				m_blink = !m_blink;
				flag = m_blink;
			}
			else
			{
				flag = true;
			}
		}
		else
		{
			flag = false;
		}
		if (m_activated)
		{
			if (!flag)
			{
				SetIsDashFXActive(isActive: false);
			}
		}
		else if (flag)
		{
			SetIsDashFXActive(isActive: true);
		}
	}

	private bool isDashing()
	{
		if (DashMonitor.instance().isDashing())
		{
			return true;
		}
		Boss instance = Boss.GetInstance();
		if (instance != null)
		{
			BossAttack bossAttack = instance.AttackPhase();
			if (bossAttack != null)
			{
				return bossAttack.AttackTimerActive();
			}
		}
		return false;
	}

	private void SetIsDashFXActive(bool isActive)
	{
		foreach (Transform item in base.transform)
		{
			item.gameObject.SetActive(isActive);
		}
		m_activated = isActive;
	}
}
