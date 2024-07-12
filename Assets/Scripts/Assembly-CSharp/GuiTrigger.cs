using System;
using System.Collections;
using UnityEngine;

public class GuiTrigger : MonoBehaviour
{
	[Flags]
	private enum State
	{
		None = 1,
		Visible = 2,
		TransitionDisabled = 4,
		ButtonsActive = 8,
		ButtonsBlocked = 0x10,
		Initialised = 0x20
	}

	private State m_state = State.None;

	private float m_transitionEndTime;

	private BoxCollider[] m_buttonList;

	private GuiButtonBlocker[][] m_buttonBlockerList;

	[SerializeField]
	private GameObject m_guiTrigger;

	[SerializeField]
	private bool m_simulateActivation;

	[SerializeField]
	private bool m_disableDuringTransition = true;

	[SerializeField]
	private bool m_cacheContentWhenShown;

	[SerializeField]
	private float m_transitionTimer = 0.5f;

	[SerializeField]
	private Transform m_characterStandPoint;

	public GameObject Trigger => m_guiTrigger;

	public Transform CharacterStandPoint => m_characterStandPoint;

	public bool Visible => (m_state & State.Visible) == State.Visible;

	public bool Transition => (m_state & State.TransitionDisabled) == State.TransitionDisabled;

	public void Show()
	{
		if ((m_state & State.TransitionDisabled) != State.TransitionDisabled && !Visible)
		{
			EnableOfferRegion(enable: true);
			StartTransition(showing: true);
			m_state |= State.Visible;
			if (m_cacheContentWhenShown)
			{
				StartCoroutine(StartPendingInitialisation());
			}
		}
	}

	public void Hide()
	{
		if ((m_state & State.TransitionDisabled) != State.TransitionDisabled && Visible)
		{
			EnableOfferRegion(enable: false);
			StartTransition(showing: false);
			m_state &= ~State.Visible;
		}
	}

	public void Toggle()
	{
		if (Visible)
		{
			Hide();
		}
		else
		{
			Show();
		}
	}

	public void ButtonsBlocked(bool blocked)
	{
		if (blocked)
		{
			m_state |= State.ButtonsBlocked;
		}
		else
		{
			m_state &= ~State.ButtonsBlocked;
		}
	}

	private void Start()
	{
		if (!m_cacheContentWhenShown)
		{
			InitialiseContent();
		}
		m_simulateActivation = false;
		m_state = State.None;
		if (m_disableDuringTransition)
		{
			ActivePanelButtons(active: false);
		}
	}

	private void Update()
	{
		UpdateManualTrigger();
		UpdateDisableCounts();
		SetBoxCollidersState();
	}

	private void UpdateManualTrigger()
	{
		if (!UICamera.inputHasFocus && m_simulateActivation && Input.GetKeyUp("space"))
		{
			Toggle();
		}
	}

	private void UpdateDisableCounts()
	{
		if ((m_state & State.TransitionDisabled) == State.TransitionDisabled && m_transitionEndTime <= Time.realtimeSinceStartup)
		{
			m_state &= ~State.TransitionDisabled;
			GameState.HUDTransitioning = false;
			if (Visible)
			{
				ActivePanelButtons(active: true);
			}
		}
	}

	private void StartTransition(bool showing)
	{
		m_guiTrigger.SendMessage("OnClick", SendMessageOptions.DontRequireReceiver);
		if (m_disableDuringTransition)
		{
			m_state |= State.TransitionDisabled;
			m_transitionEndTime = Time.realtimeSinceStartup + m_transitionTimer;
			GameState.HUDTransitioning = true;
			ActivePanelButtons(active: false);
		}
	}

	private void EnableOfferRegion(bool enable)
	{
		OfferRegion_Manual[] components = GetComponents<OfferRegion_Manual>();
		if (components == null || components.Length <= 0)
		{
			return;
		}
		OfferRegion_Manual[] array = components;
		foreach (OfferRegion_Manual offerRegion_Manual in array)
		{
			if (enable)
			{
				offerRegion_Manual.Visit();
			}
			else
			{
				offerRegion_Manual.Leave();
			}
		}
	}

	private void ActivePanelButtons(bool active)
	{
		if (active)
		{
			m_state |= State.ButtonsActive;
		}
		else
		{
			m_state &= ~State.ButtonsActive;
		}
	}

	private void SetBoxCollidersState()
	{
		if (m_buttonList == null || m_buttonList.Length == 0)
		{
			return;
		}
		bool flag = true;
		if ((m_state & State.ButtonsActive) != State.ButtonsActive)
		{
			flag = false;
		}
		if ((m_state & State.ButtonsBlocked) == State.ButtonsBlocked)
		{
			flag = false;
		}
		for (int i = 0; i < m_buttonList.Length; i++)
		{
			bool flag2 = flag;
			if (flag2 && m_buttonBlockerList[i] != null)
			{
				GuiButtonBlocker[] array = m_buttonBlockerList[i];
				GuiButtonBlocker[] array2 = array;
				foreach (GuiButtonBlocker guiButtonBlocker in array2)
				{
					flag2 &= !guiButtonBlocker.Blocked;
				}
			}
			m_buttonList[i].enabled = flag2;
		}
	}

	private void InitialiseContent()
	{
		if ((m_state & State.Initialised) == State.Initialised)
		{
			return;
		}
		UIPanel uIPanel = FindPagePanel(m_guiTrigger);
		m_buttonList = uIPanel.GetComponentsInChildren<BoxCollider>(includeInactive: true);
		if (m_buttonList != null && m_buttonList.Length > 0)
		{
			m_buttonBlockerList = new GuiButtonBlocker[m_buttonList.Length][];
			for (int i = 0; i < m_buttonList.Length; i++)
			{
				m_buttonBlockerList[i] = m_buttonList[i].GetComponents<GuiButtonBlocker>();
			}
		}
		m_state |= State.Initialised;
	}

	private IEnumerator StartPendingInitialisation()
	{
		yield return null;
		yield return null;
		InitialiseContent();
	}

	private UIPanel FindPagePanel(GameObject trigger)
	{
		UIPanel uIPanel = null;
		Transform parent = trigger.transform;
		do
		{
			if (parent != null)
			{
				uIPanel = parent.GetComponent<UIPanel>();
				parent = parent.transform.parent;
			}
		}
		while (uIPanel == null && parent != null);
		if (uIPanel == null)
		{
		}
		return uIPanel;
	}
}
