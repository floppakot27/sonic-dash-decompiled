using System;
using UnityEngine;

public class LoadingScreenFlow : MonoBehaviour
{
	public delegate void TransitionEvent(int identIndex, int indexCount);

	private delegate void TransitionState();

	private const float FixedDelayTime = 0.5f;

	[SerializeField]
	private UISprite m_background;

	private LoadingScreenType[] m_identDisplays;

	private float m_thisLoadingScreenTime;

	private bool m_firstScreen = true;

	private float m_transitionValue;

	private float m_transitionRate;

	private float m_displayTime;

	private float m_delayTime;

	private int m_currentIndex;

	private TransitionEvent m_transitionInStart;

	private TransitionEvent m_transitionInFinish;

	private TransitionEvent m_transitionOutStart;

	private TransitionEvent m_transitionOutFinish;

	private TransitionState m_transitionState;

	public TransitionEvent TransitionInStart
	{
		set
		{
			m_transitionInStart = value;
		}
	}

	public TransitionEvent TransitionInEnd
	{
		set
		{
			m_transitionInFinish = value;
		}
	}

	public TransitionEvent TransitionOutStart
	{
		set
		{
			m_transitionOutStart = value;
		}
	}

	public TransitionEvent TransitionOutEnd
	{
		set
		{
			m_transitionOutFinish = value;
		}
	}

	public bool AssetsLoaded { private get; set; }

	private bool OnLastIdent => m_currentIndex == m_identDisplays.Length - 1;

	public void StartFlow(bool delayPresentation)
	{
		m_currentIndex = 0;
		if (delayPresentation)
		{
			m_transitionState = StartTransitionDelay;
		}
		else
		{
			m_transitionState = StartTransitionIn;
		}
	}

	private void Start()
	{
		m_identDisplays = GetComponentsInChildren<LoadingScreenType>(includeInactive: true);
		for (int i = 0; i < m_identDisplays.Length; i++)
		{
			m_identDisplays[i].SetTransitionState(0f);
			m_identDisplays[i].gameObject.SetActive(value: false);
		}
		Array.Sort(m_identDisplays, delegate(LoadingScreenType screen1, LoadingScreenType screen2)
		{
			LoadingScreenProperties component = screen1.GetComponent<LoadingScreenProperties>();
			LoadingScreenProperties component2 = screen2.GetComponent<LoadingScreenProperties>();
			return component.ScreenOrder.CompareTo(component2.ScreenOrder);
		});
	}

	private void Update()
	{
		if (m_transitionState != null)
		{
			m_thisLoadingScreenTime += Time.deltaTime;
			m_transitionState();
		}
	}

	private void StartTransitionDelay()
	{
		m_delayTime = 0f;
		m_transitionState = UpdateTransitionDelay;
	}

	private void UpdateTransitionDelay()
	{
		m_delayTime += Time.deltaTime;
		if (m_delayTime >= 0.5f)
		{
			m_transitionState = StartTransitionIn;
		}
	}

	private void StartTransitionIn()
	{
		m_transitionValue = 0f;
		LoadingScreenType loadingScreenType = m_identDisplays[m_currentIndex];
		loadingScreenType.gameObject.SetActive(value: true);
		loadingScreenType.SetTransitionState(m_transitionValue);
		LoadingScreenProperties component = loadingScreenType.GetComponent<LoadingScreenProperties>();
		m_transitionRate = 1f / component.TransitionTime;
		m_transitionState = UpdateTransitionIn;
		if (m_transitionInStart != null)
		{
			m_transitionInStart(m_currentIndex, m_identDisplays.Length - 1);
		}
		m_thisLoadingScreenTime = 0f;
	}

	private void UpdateTransitionIn()
	{
		LoadingScreenType loadingScreenType = m_identDisplays[m_currentIndex];
		m_transitionValue += m_transitionRate * Time.deltaTime;
		m_transitionValue = Mathf.Clamp(m_transitionValue, 0f, 1f);
		loadingScreenType.SetTransitionState(m_transitionValue);
		if (m_transitionValue.Equals(1f))
		{
			if (m_transitionInFinish != null)
			{
				m_transitionInFinish(m_currentIndex, m_identDisplays.Length - 1);
			}
			m_transitionState = StartDisplay;
		}
	}

	private void StartDisplay()
	{
		m_displayTime = 0f;
		m_transitionState = UpdateDisplay;
	}

	private void UpdateDisplay()
	{
		m_displayTime += Time.deltaTime;
		bool flag = false;
		if (OnLastIdent)
		{
			flag = !AssetsLoaded;
		}
		LoadingScreenType loadingScreenType = m_identDisplays[m_currentIndex];
		LoadingScreenProperties component = loadingScreenType.GetComponent<LoadingScreenProperties>();
		if (m_displayTime >= component.DisplayTime && !flag)
		{
			m_transitionState = StartTransitionOut;
		}
	}

	private void StartTransitionOut()
	{
		m_transitionValue = 1f;
		m_transitionState = UpdateTransitionOut;
		if (m_transitionOutStart != null)
		{
			m_transitionOutStart(m_currentIndex, m_identDisplays.Length - 1);
		}
	}

	private void UpdateTransitionOut()
	{
		m_transitionValue -= m_transitionRate * Time.deltaTime;
		m_transitionValue = Mathf.Clamp(m_transitionValue, 0f, 1f);
		if ((bool)m_background && OnLastIdent)
		{
			Color color = m_background.color;
			color.a = m_transitionValue;
			m_background.color = color;
		}
		LoadingScreenType loadingScreenType = m_identDisplays[m_currentIndex];
		loadingScreenType.SetTransitionState(m_transitionValue);
		if (m_transitionValue.Equals(0f))
		{
			m_transitionState = FinishTransitionOut;
			if (m_transitionOutFinish != null)
			{
				m_transitionOutFinish(m_currentIndex, m_identDisplays.Length - 1);
			}
		}
	}

	private void OnDestroy()
	{
		if (m_transitionState != null)
		{
			FinishTransitionOut();
		}
	}

	private void FinishTransitionOut()
	{
		LoadingScreenType loadingScreenType = m_identDisplays[m_currentIndex];
		loadingScreenType.gameObject.SetActive(value: false);
		if (OnLastIdent)
		{
			m_transitionState = null;
		}
		else
		{
			m_currentIndex++;
			m_transitionState = StartTransitionIn;
		}
		LoadingScreenProperties component = loadingScreenType.GetComponent<LoadingScreenProperties>();
		GameAnalytics.NotifyLoadScreen(component, m_thisLoadingScreenTime, m_firstScreen);
		m_firstScreen = false;
	}
}
