using System;
using UnityEngine;

public class HudContent_FlyingRings
{
	[Flags]
	public enum States
	{
		Banking = 2,
		Collecting = 4
	}

	private struct PoolEntry
	{
		public bool m_free;

		public GameObject m_ringSource;

		public GameObject m_manaSource;

		public Content m_content;

		public UITexture m_texture;

		public UISprite m_sprite;

		public float m_timeTravelling;

		public Vector3 m_origin;

		public Vector3 m_destiny;

		public GameObject m_target;

		public Vector3 m_center;

		public float m_a;

		public float m_b;

		public float m_phi;

		public double m_speed;
	}

	private enum Content
	{
		Ring,
		Mana
	}

	private const float m_travelBankSpeed = 1.15f;

	private const float m_travelDashSpeed = 1.5f;

	private PoolEntry[] m_itemsPool;

	private int m_poolSize;

	private int m_currentEntry;

	private string m_ringsSource;

	private GameObject m_bankTarget;

	private float m_bankingRate;

	private float m_bankingSpeed;

	private GameObject m_dashTarget;

	private float m_collectingSpeed = 50f;

	private Camera m_mainGameCamera;

	private Camera m_guiCamera;

	private bool m_paused;

	private float m_waitingTime;

	private static States m_state;

	private static float m_itemsToRelease;

	private int m_previousRings;

	public HudContent_FlyingRings(GameObject ringSource, GameObject manaSource, int poolSize, string flyingRingsSource, GameObject bankTarget, float bankingRate, GameObject dashTarget)
	{
		CreatePool(ringSource, manaSource, poolSize);
		m_ringsSource = flyingRingsSource;
		m_bankTarget = bankTarget;
		m_dashTarget = dashTarget;
		m_bankingRate = bankingRate;
		m_poolSize = poolSize;
		FindMainCamera();
		ResetPools();
		EventDispatch.RegisterInterest("OnRingBankRequest", this, EventDispatch.Priority.Highest);
	}

	public void Update()
	{
		if (!m_paused)
		{
			CheckRingCollection();
			ReleaseItems();
			UpdateItems();
		}
	}

	public void OnResetOnNewGame()
	{
		ResetPools();
	}

	public void OnPauseStateChanged(bool paused)
	{
		m_paused = paused;
		ItemsVisibility(!m_paused);
	}

	public void OnPlayerDeath()
	{
		m_paused = true;
		ItemsVisibility(!m_paused);
	}

	public void OnSonicResurrection()
	{
		m_paused = false;
		ItemsVisibility(!m_paused);
	}

	private void CreatePool(GameObject rignSource, GameObject manaSource, int poolSize)
	{
		m_itemsPool = new PoolEntry[poolSize];
		m_currentEntry = 0;
		m_itemsPool[0] = default(PoolEntry);
		m_itemsPool[0].m_free = true;
		m_itemsPool[0].m_ringSource = rignSource;
		m_itemsPool[0].m_manaSource = manaSource;
		m_itemsPool[0].m_texture = FindTextureFromSource(rignSource);
		m_itemsPool[0].m_texture.enabled = true;
		m_itemsPool[0].m_sprite = FindSpriteFromSource(manaSource);
		m_itemsPool[0].m_sprite.enabled = true;
		for (int i = 1; i < poolSize; i++)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(manaSource) as GameObject;
			gameObject.transform.parent = rignSource.transform.parent;
			gameObject.transform.localScale = manaSource.transform.localScale;
			gameObject.transform.localRotation = manaSource.transform.localRotation;
			gameObject.transform.localPosition = manaSource.transform.localPosition;
			gameObject.transform.Rotate(0f, 0f, 140 * (i - 1) % 360, Space.Self);
			GameObject gameObject2 = UnityEngine.Object.Instantiate(rignSource) as GameObject;
			gameObject2.transform.parent = rignSource.transform.parent;
			gameObject2.transform.localScale = rignSource.transform.localScale;
			gameObject2.transform.localRotation = rignSource.transform.localRotation;
			gameObject2.transform.localPosition = rignSource.transform.localPosition;
			m_itemsPool[i] = default(PoolEntry);
			m_itemsPool[i].m_free = true;
			m_itemsPool[i].m_ringSource = gameObject2;
			m_itemsPool[i].m_manaSource = gameObject;
			m_itemsPool[i].m_texture = FindTextureFromSource(gameObject2);
			m_itemsPool[i].m_sprite = FindSpriteFromSource(gameObject);
		}
	}

	private void ResetPools()
	{
		for (int i = 0; i < m_itemsPool.Length; i++)
		{
			m_itemsPool[i].m_free = true;
			m_itemsPool[i].m_timeTravelling = 0f;
			m_itemsPool[i].m_texture.enabled = false;
			m_itemsPool[i].m_sprite.enabled = false;
		}
		m_currentEntry = 0;
		m_itemsToRelease = 0f;
		m_waitingTime = 0f;
	}

	private void ItemsVisibility(bool visibility)
	{
		for (int i = 0; i < m_itemsPool.Length; i++)
		{
			if (!m_itemsPool[i].m_free)
			{
				if (m_itemsPool[i].m_content == Content.Ring)
				{
					m_itemsPool[i].m_texture.enabled = visibility;
				}
				else
				{
					m_itemsPool[i].m_sprite.enabled = visibility;
				}
			}
		}
	}

	private void CheckRingCollection()
	{
		if (RingStorage.HeldRings > m_previousRings && !DashMonitor.instance().isDashing() && !HeadstartMonitor.instance().isHeadstarting())
		{
			int num = RingStorage.HeldRings - m_previousRings;
			if (num > m_poolSize)
			{
				num = m_poolSize;
			}
			m_state = States.Collecting;
			m_itemsToRelease = num;
			m_collectingSpeed = (float)num / m_bankingRate;
		}
		m_previousRings = RingStorage.HeldRings;
	}

	private void UpdateItems()
	{
		for (int i = 0; i < m_itemsPool.Length; i++)
		{
			if (m_itemsPool[i].m_free)
			{
				m_itemsPool[i].m_manaSource.gameObject.SetActive(value: false);
				m_itemsPool[i].m_ringSource.gameObject.SetActive(value: false);
				continue;
			}
			m_itemsPool[i].m_manaSource.gameObject.SetActive(value: true);
			m_itemsPool[i].m_ringSource.gameObject.SetActive(value: true);
			if (m_itemsPool[i].m_destiny != m_itemsPool[i].m_target.transform.position)
			{
				m_itemsPool[i].m_destiny = m_itemsPool[i].m_target.transform.position;
				BuildTrajectory(i, generateB: false);
			}
			m_itemsPool[i].m_timeTravelling += Time.deltaTime;
			if ((m_itemsPool[i].m_content == Content.Ring && m_itemsPool[i].m_timeTravelling >= 0.86956525f) || (m_itemsPool[i].m_content == Content.Mana && m_itemsPool[i].m_timeTravelling >= 2f / 3f))
			{
				m_itemsPool[i].m_free = true;
				m_itemsPool[i].m_texture.enabled = false;
				m_itemsPool[i].m_sprite.enabled = false;
				m_itemsPool[i].m_timeTravelling = 0f;
				continue;
			}
			Vector3 position = ((m_itemsPool[i].m_content != 0) ? m_itemsPool[i].m_sprite.transform.position : m_itemsPool[i].m_texture.transform.position);
			double num = (double)m_itemsPool[i].m_timeTravelling * m_itemsPool[i].m_speed;
			position.x = (float)((double)m_itemsPool[i].m_center.x + (double)m_itemsPool[i].m_a * Math.Cos(num) * Math.Cos(m_itemsPool[i].m_phi) - (double)m_itemsPool[i].m_b * Math.Sin(num) * Math.Sin(m_itemsPool[i].m_phi));
			position.y = (float)((double)m_itemsPool[i].m_center.y + (double)m_itemsPool[i].m_a * Math.Cos(num) * Math.Sin(m_itemsPool[i].m_phi) + (double)m_itemsPool[i].m_b * Math.Sin(num) * Math.Cos(m_itemsPool[i].m_phi));
			if (m_itemsPool[i].m_content == Content.Ring)
			{
				m_itemsPool[i].m_texture.cachedTransform.position = position;
			}
			else
			{
				m_itemsPool[i].m_sprite.cachedTransform.position = position;
			}
		}
	}

	private void ReleaseItems()
	{
		if (m_itemsToRelease == 0f)
		{
			m_state = (States)0;
			return;
		}
		float num;
		if ((m_state & States.Banking) == States.Banking)
		{
			if (m_waitingTime < 0.2f)
			{
				m_waitingTime += Time.deltaTime;
				return;
			}
			num = m_bankingSpeed * Time.deltaTime;
		}
		else
		{
			num = m_collectingSpeed * Time.deltaTime;
		}
		int num2 = (int)(Math.Floor(m_itemsToRelease) - Math.Floor(m_itemsToRelease - num));
		m_itemsToRelease -= num;
		if (m_itemsToRelease < 0f)
		{
			m_itemsToRelease = 0f;
			num2--;
		}
		for (int i = 0; i < num2; i++)
		{
			FindNextEntry();
			m_itemsPool[m_currentEntry].m_free = false;
			if ((m_state & States.Banking) == States.Banking)
			{
				m_itemsPool[m_currentEntry].m_origin = FindRingsSource(m_ringsSource, m_itemsPool[0].m_texture);
				m_itemsPool[m_currentEntry].m_destiny = m_bankTarget.transform.position;
				m_itemsPool[m_currentEntry].m_target = m_bankTarget;
				m_itemsPool[m_currentEntry].m_content = Content.Ring;
				m_itemsPool[m_currentEntry].m_texture.alpha = 0f;
				m_itemsPool[m_currentEntry].m_texture.enabled = true;
				m_itemsPool[m_currentEntry].m_texture.cachedTransform.position = m_itemsPool[m_currentEntry].m_origin;
				BuildTrajectory(m_currentEntry, generateB: true);
				m_itemsPool[m_currentEntry].m_texture.alpha = 1f;
			}
			else
			{
				m_itemsPool[m_currentEntry].m_origin = FindRingsSource(m_ringsSource, m_itemsPool[0].m_texture);
				m_itemsPool[m_currentEntry].m_destiny = m_dashTarget.transform.position;
				m_itemsPool[m_currentEntry].m_target = m_dashTarget;
				m_itemsPool[m_currentEntry].m_content = Content.Mana;
				m_itemsPool[m_currentEntry].m_sprite.alpha = 0f;
				m_itemsPool[m_currentEntry].m_sprite.enabled = true;
				m_itemsPool[m_currentEntry].m_sprite.cachedTransform.position = m_itemsPool[m_currentEntry].m_origin;
				BuildTrajectory(m_currentEntry, generateB: true);
				m_itemsPool[m_currentEntry].m_sprite.alpha = 1f;
			}
		}
	}

	private void BuildTrajectory(int index, bool generateB)
	{
		Vector3 vector = m_itemsPool[index].m_destiny - m_itemsPool[index].m_origin;
		m_itemsPool[index].m_center = (m_itemsPool[index].m_origin + m_itemsPool[index].m_destiny) / 2f;
		m_itemsPool[index].m_a = 0f - Math.Abs(vector.magnitude) / 2f;
		if (m_itemsPool[index].m_content == Content.Ring)
		{
			if (generateB)
			{
				m_itemsPool[index].m_b = m_itemsPool[index].m_a * 0.5f * (UnityEngine.Random.value - 0.5f);
			}
			m_itemsPool[index].m_phi = (float)((double)Vector2.Angle(Vector2.right, new Vector2(vector.x, vector.y)) * Math.PI / 180.0);
			m_itemsPool[index].m_speed = 3.6128314767268566;
		}
		else
		{
			m_itemsPool[index].m_b = 0f;
			m_itemsPool[index].m_phi = (float)(0.0 - (double)Vector2.Angle(Vector2.right, new Vector2(vector.x, vector.y)) * Math.PI / 180.0);
			m_itemsPool[index].m_speed = 4.71238898038469;
		}
	}

	private UITexture FindTextureFromSource(GameObject source)
	{
		UITexture uITexture = source.GetComponent<UITexture>();
		if (uITexture == null)
		{
			uITexture = source.GetComponentInChildren<UITexture>();
		}
		return uITexture;
	}

	private UISprite FindSpriteFromSource(GameObject source)
	{
		UISprite uISprite = source.GetComponent<UISprite>();
		if (uISprite == null)
		{
			uISprite = source.GetComponentInChildren<UISprite>();
		}
		return uISprite;
	}

	private void FindMainCamera()
	{
		if (!(m_mainGameCamera != null) || !(m_guiCamera != null))
		{
			CameraTypeMain cameraTypeMain = UnityEngine.Object.FindObjectOfType(typeof(CameraTypeMain)) as CameraTypeMain;
			if (!(cameraTypeMain == null))
			{
				m_mainGameCamera = cameraTypeMain.GetComponentInChildren<Camera>();
				GameObject gameObject = GameObject.FindGameObjectWithTag("HudCamera");
				Transform transform = gameObject.transform;
				m_guiCamera = transform.GetComponent<Camera>();
			}
		}
	}

	private Vector3 FindRingsSource(string sourceObject, UITexture activeTexture)
	{
		FindMainCamera();
		Vector3 position = Sonic.Bones[sourceObject].transform.position;
		Vector3 position2 = m_mainGameCamera.WorldToViewportPoint(position);
		Vector3 result = m_guiCamera.ViewportToWorldPoint(position2);
		result.z = activeTexture.transform.position.z;
		return result;
	}

	private Vector3 FindManaSource(Vector3 sourcePosition, UITexture activeTexture)
	{
		FindMainCamera();
		Vector3 position = m_mainGameCamera.WorldToViewportPoint(sourcePosition);
		Vector3 result = m_guiCamera.ViewportToWorldPoint(position);
		result.z = activeTexture.transform.position.z;
		return result;
	}

	private void FindNextEntry()
	{
		m_currentEntry++;
		if (m_currentEntry >= m_itemsPool.Length)
		{
			m_currentEntry = 0;
		}
	}

	private void Event_OnRingBankRequest()
	{
		int num = RingStorage.HeldRings;
		if (num > m_poolSize)
		{
			num = m_poolSize;
		}
		m_bankingSpeed = (float)num / (m_bankingRate - 0.4f);
		m_itemsToRelease = num;
		m_waitingTime = 0f;
		m_state = States.Banking;
	}
}
