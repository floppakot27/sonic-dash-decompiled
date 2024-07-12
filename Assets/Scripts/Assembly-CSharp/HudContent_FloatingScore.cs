using UnityEngine;

public class HudContent_FloatingScore
{
	private class PoolEntry
	{
		public bool m_free = true;

		public GameObject m_root;

		public UILabel m_label;

		public UISprite m_icon;

		public UISprite m_background;

		public GameObject m_container;

		public float m_runningTime;

		public float m_currentOffset;

		public float m_initialOffset;

		public uint m_displayedScore;
	}

	private const float ScoreReuseTime = 2f;

	private const float ScaleIncrement = 0f;

	private const float BonusScale = 1.2f;

	private const float StandardScale = 1f;

	private const float IconScale = 1f;

	private PoolEntry[] m_floatingScorePool;

	private int m_currentScoreEntry;

	private float m_floatingScoreTime;

	private AnimationCurve m_floatingScoreSpeed;

	private string m_floatingScoreSource = string.Empty;

	private float m_floatingScoreSourceOffset;

	private Camera m_mainGameCamera;

	private Camera m_guiCamera;

	private Color m_normalColor;

	private Color m_bonusColor;

	private float m_lastScoreSpawnedTime;

	private Vector3 m_currentScoreSource = Vector3.zero;

	private Vector3 m_tempScaleVector;

	public HudContent_FloatingScore(GameObject sourceObject, int floatingScorePool, float floatingScoreTime, AnimationCurve floatingScoreSpeed, string floatingScoreSource, float floatingScoreSourceOffset, Color normalColor, Color bonusColor)
	{
		CreateScorePool(sourceObject, floatingScorePool);
		m_floatingScoreTime = floatingScoreTime;
		m_floatingScoreSource = floatingScoreSource;
		m_floatingScoreSourceOffset = floatingScoreSourceOffset;
		m_floatingScoreSpeed = floatingScoreSpeed;
		m_normalColor = normalColor;
		m_bonusColor = bonusColor;
		ResetPool();
		FindMainCamera();
		EventDispatch.RegisterInterest("OnScoreIncreased", this);
		EventDispatch.RegisterInterest("OnPickupRings", this);
	}

	public void Update()
	{
		UpdateActiveEntries(m_currentScoreSource);
	}

	public void LateUpdate()
	{
		m_currentScoreSource = FindFloatingScoreSource(m_floatingScoreSource, m_floatingScoreSourceOffset, m_floatingScorePool[0].m_label);
	}

	public void OnResetOnNewGame()
	{
		ResetPool();
	}

	public void OnPauseStateChanged(bool paused)
	{
	}

	public void OnPlayerDeath()
	{
	}

	private void CreateScorePool(GameObject sourceObject, int poolSize)
	{
		sourceObject.SetActive(value: true);
		m_floatingScorePool = new PoolEntry[poolSize];
		m_currentScoreEntry = 0;
		m_floatingScorePool[0] = new PoolEntry();
		m_floatingScorePool[0].m_free = true;
		m_floatingScorePool[0].m_root = sourceObject;
		m_floatingScorePool[0].m_label = sourceObject.GetComponentInChildren<UILabel>();
		UISprite[] componentsInChildren = sourceObject.GetComponentsInChildren<UISprite>();
		m_floatingScorePool[0].m_icon = componentsInChildren[0];
		m_floatingScorePool[0].m_background = componentsInChildren[1];
		m_floatingScorePool[0].m_container = m_floatingScorePool[0].m_label.transform.parent.gameObject;
		for (int i = 1; i < poolSize; i++)
		{
			GameObject gameObject = Object.Instantiate(sourceObject) as GameObject;
			gameObject.transform.parent = sourceObject.transform.parent;
			gameObject.transform.localScale = sourceObject.transform.localScale;
			gameObject.transform.localRotation = sourceObject.transform.localRotation;
			gameObject.transform.localPosition = sourceObject.transform.localPosition;
			m_floatingScorePool[i] = new PoolEntry();
			m_floatingScorePool[i].m_free = true;
			m_floatingScorePool[i].m_root = gameObject;
			m_floatingScorePool[i].m_label = gameObject.GetComponentInChildren<UILabel>();
			componentsInChildren = gameObject.GetComponentsInChildren<UISprite>();
			m_floatingScorePool[i].m_icon = componentsInChildren[0];
			m_floatingScorePool[i].m_background = componentsInChildren[1];
			m_floatingScorePool[i].m_container = m_floatingScorePool[i].m_label.transform.parent.gameObject;
			UpdateAnimationTrigger(m_floatingScorePool[i].m_root);
			UpdateWidgetLinks(m_floatingScorePool[i].m_root, m_floatingScorePool[i].m_label);
		}
	}

	private void ResetPool()
	{
		PoolEntry[] floatingScorePool = m_floatingScorePool;
		foreach (PoolEntry poolEntry in floatingScorePool)
		{
			poolEntry.m_free = true;
			poolEntry.m_runningTime = 0f;
			poolEntry.m_root.SetActive(value: false);
		}
		m_currentScoreEntry = 0;
	}

	private void UpdateActiveEntries(Vector3 currentScoreSource)
	{
		int num = 0;
		PoolEntry[] floatingScorePool = m_floatingScorePool;
		foreach (PoolEntry poolEntry in floatingScorePool)
		{
			if (!poolEntry.m_free)
			{
				poolEntry.m_runningTime += Time.deltaTime;
				if (poolEntry.m_runningTime >= m_floatingScoreTime)
				{
					poolEntry.m_free = true;
					poolEntry.m_root.SetActive(value: false);
					poolEntry.m_currentOffset = 0f;
					poolEntry.m_initialOffset = 0f;
				}
				else
				{
					float num2 = m_floatingScoreSpeed.Evaluate(poolEntry.m_runningTime);
					poolEntry.m_currentOffset += num2 * Time.deltaTime;
					Vector3 position = currentScoreSource;
					position.y += poolEntry.m_initialOffset;
					position.y += poolEntry.m_currentOffset;
					poolEntry.m_container.transform.position = position;
				}
				num++;
			}
		}
	}

	private void UpdateAnimationTrigger(GameObject source)
	{
		UIButtonPlayAnimation uIButtonPlayAnimation = source.GetComponent<UIButtonPlayAnimation>();
		if (uIButtonPlayAnimation == null)
		{
			uIButtonPlayAnimation = source.GetComponentInChildren<UIButtonPlayAnimation>();
		}
		Animation animation = source.GetComponent<Animation>();
		if (animation == null)
		{
			animation = source.GetComponentInChildren<Animation>();
		}
		uIButtonPlayAnimation.target = animation;
	}

	private void UpdateWidgetLinks(GameObject source, UILabel theLabel)
	{
		NGUIWidgetColour nGUIWidgetColour = source.GetComponent<NGUIWidgetColour>();
		if (nGUIWidgetColour == null)
		{
			nGUIWidgetColour = source.GetComponentInChildren<NGUIWidgetColour>();
		}
		nGUIWidgetColour.m_targetWidget = theLabel;
	}

	private void FindMainCamera()
	{
		if (!(m_mainGameCamera != null) || !(m_guiCamera != null))
		{
			CameraTypeMain cameraTypeMain = Object.FindObjectOfType(typeof(CameraTypeMain)) as CameraTypeMain;
			if (!(cameraTypeMain == null))
			{
				m_mainGameCamera = cameraTypeMain.GetComponentInChildren<Camera>();
				GameObject gameObject = GameObject.FindGameObjectWithTag("HudCamera");
				Transform transform = gameObject.transform;
				m_guiCamera = transform.GetComponent<Camera>();
			}
		}
	}

	private Vector3 FindFloatingScoreSource(string sourceObject, float sourceOffset, UILabel activeLabel)
	{
		Vector3 zero = Vector3.zero;
		Vector3 zero2 = Vector3.zero;
		Vector3 zero3 = Vector3.zero;
		if (Sonic.ScoreAnchor != null && Sonic.ScoreAnchor.activeSelf)
		{
			zero = Sonic.ScoreAnchor.transform.position;
			zero2 = m_mainGameCamera.WorldToViewportPoint(zero);
			zero3 = m_guiCamera.ViewportToWorldPoint(zero2);
			zero3.z = 0f;
			zero3.y += sourceOffset;
			return zero3;
		}
		if (Sonic.Bones == null)
		{
			return zero;
		}
		Transform transform = Sonic.Bones[sourceObject];
		if (null == transform)
		{
			return Vector3.zero;
		}
		zero = transform.position;
		zero2 = m_mainGameCamera.WorldToViewportPoint(zero);
		zero3 = m_guiCamera.ViewportToWorldPoint(zero2);
		zero3.z = 0f;
		zero3.y += sourceOffset;
		return zero3;
	}

	private PoolEntry FindNextEntry()
	{
		m_currentScoreEntry++;
		if (m_currentScoreEntry >= m_floatingScorePool.Length)
		{
			m_currentScoreEntry = 0;
		}
		return m_floatingScorePool[m_currentScoreEntry];
	}

	private void Event_OnScoreIncreased(int score, ScoreTracker.ScoreNotify notify)
	{
		string iconName = ScoreTracker.ScoreNotifyIcons[(int)notify];
		ShowScoreIncrease(score, iconName, showAsMultiply: false);
	}

	private void Event_OnPickupRings(int ringCount)
	{
		ShowScoreIncrease(ringCount, "prize-gold-ring", showAsMultiply: true);
	}

	private void ShowScoreIncrease(int score, string iconName, bool showAsMultiply)
	{
		PoolEntry poolEntry = FindEntryToUse(iconName);
		if (poolEntry != null)
		{
			poolEntry.m_displayedScore += (uint)score;
			poolEntry.m_runningTime = 0f;
			Animation componentInChildren = poolEntry.m_root.GetComponentInChildren<Animation>();
			componentInChildren.Rewind();
			float x = poolEntry.m_container.transform.localScale.x;
			m_tempScaleVector.Set(x, x, 1f);
			poolEntry.m_container.transform.localScale = m_tempScaleVector;
		}
		else
		{
			m_lastScoreSpawnedTime = Time.realtimeSinceStartup;
			poolEntry = FindNextEntry();
			poolEntry.m_root.SetActive(value: true);
			poolEntry.m_free = false;
			poolEntry.m_runningTime = 0f;
			poolEntry.m_currentOffset = 0f;
			poolEntry.m_initialOffset = GetInitialOffset() - 0.1f;
			poolEntry.m_displayedScore = (uint)score;
			poolEntry.m_icon.spriteName = iconName;
			if (iconName != string.Empty)
			{
				poolEntry.m_icon.gameObject.SetActive(value: true);
				poolEntry.m_background.gameObject.SetActive(value: true);
				poolEntry.m_icon.spriteName = iconName;
				poolEntry.m_icon.MakePixelPerfect();
				m_tempScaleVector = poolEntry.m_icon.transform.localScale;
				m_tempScaleVector.x *= 1f;
				m_tempScaleVector.y *= 1f;
				poolEntry.m_icon.transform.localScale = m_tempScaleVector;
			}
			else
			{
				poolEntry.m_icon.gameObject.SetActive(value: false);
				poolEntry.m_background.gameObject.SetActive(value: false);
			}
			if (iconName != string.Empty)
			{
				m_tempScaleVector.Set(1f, 1f, 1f);
			}
			else
			{
				m_tempScaleVector.Set(1.2f, 1.2f, 1f);
			}
			poolEntry.m_container.transform.localScale = m_tempScaleVector;
			poolEntry.m_root.SendMessage("OnClick", SendMessageOptions.RequireReceiver);
		}
		poolEntry.m_icon.alpha = 1f;
		poolEntry.m_label.alpha = 1f;
		poolEntry.m_label.text = string.Format((!showAsMultiply) ? "+{0}" : "x{0}", LanguageUtils.FormatNumber(poolEntry.m_displayedScore));
		NGUIWidgetColour component = poolEntry.m_label.gameObject.GetComponent<NGUIWidgetColour>();
		if (iconName == string.Empty)
		{
			component.m_widgetColour = m_normalColor;
		}
		else
		{
			component.m_widgetColour = m_bonusColor;
		}
	}

	private float GetInitialOffset()
	{
		float num = 0f;
		float num2 = 0f;
		for (int i = 0; i < m_floatingScorePool.Length; i++)
		{
			num2 = m_floatingScorePool[i].m_currentOffset + m_floatingScorePool[i].m_initialOffset;
			if (num2 < num)
			{
				num = num2;
			}
		}
		return num;
	}

	private PoolEntry FindEntryToUse(string iconName)
	{
		for (int i = 0; i < m_floatingScorePool.Length; i++)
		{
			PoolEntry poolEntry = m_floatingScorePool[i];
			if (!poolEntry.m_free && poolEntry.m_runningTime <= 2f && Time.realtimeSinceStartup - m_lastScoreSpawnedTime < 2f && poolEntry.m_icon.spriteName == iconName)
			{
				return m_floatingScorePool[i];
			}
		}
		return null;
	}
}
