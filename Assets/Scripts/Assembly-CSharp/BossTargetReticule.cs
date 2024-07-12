using UnityEngine;

public class BossTargetReticule : MonoBehaviour
{
	private Camera m_mainGameCamera;

	private Camera m_guiCamera;

	private Animation m_animation;

	private bool m_playLoopAnim = true;

	private bool m_allowManualUpdate = true;

	private bool m_acceptingTouch;

	private float m_gestureProgress;

	private bool m_gestureMissed;

	private bool m_failed;

	private float m_failTimer;

	private float m_scaleDif;

	private float m_distDif;

	private float m_redDif;

	private float m_greenDif;

	private float m_startDist;

	private Vector3 m_currentScale = Vector3.one;

	private Vector3 m_currentDist = Vector3.zero;

	private BossAttack.HitpointSetting m_hitBox;

	[SerializeField]
	private GameObject m_trigger;

	[SerializeField]
	private string m_targetLoopnimation = string.Empty;

	[SerializeField]
	private GameObject[] m_trianglePointMovers;

	[SerializeField]
	private float m_destinationOfTriangleY;

	[SerializeField]
	private UISprite[] m_trianglePointColourers;

	[SerializeField]
	private GameObject m_ringMover;

	[SerializeField]
	private float m_destinationOfRingScale = 0.5f;

	[SerializeField]
	private UISprite m_ringColourer;

	[SerializeField]
	private UISprite m_centerColourer;

	[SerializeField]
	private Color m_startColour;

	[SerializeField]
	private Color m_endColour;

	[SerializeField]
	private float m_failLerpTime = 1f;

	public BossAttack.HitpointSetting HitBox
	{
		get
		{
			return m_hitBox;
		}
		set
		{
			m_hitBox = value;
			m_acceptingTouch = true;
		}
	}

	private void Awake()
	{
		m_animation = GetComponentInChildren<Animation>();
		m_startDist = m_trianglePointMovers[0].transform.localPosition.y;
		m_distDif = m_startDist - m_destinationOfTriangleY;
		m_scaleDif = m_ringMover.transform.localScale.x - m_destinationOfRingScale;
		m_redDif = m_startColour.r - m_endColour.r;
		m_greenDif = m_startColour.g - m_endColour.g;
	}

	private void OnEnable()
	{
		FindMainCamera();
		EventDispatch.RegisterInterest("OnBossBattleOutroStart", this);
		EventDispatch.RegisterInterest("OnGestureWindowTimeOut", this);
		EventDispatch.RegisterInterest("OnGestureInput", this);
		m_gestureMissed = false;
		m_failed = false;
	}

	private void OnDisable()
	{
		EventDispatch.UnregisterInterest("OnBossBattleOutroStart", this);
		EventDispatch.UnregisterInterest("OnGestureWindowTimeOut", this);
		EventDispatch.UnregisterInterest("OnGestureInput", this);
	}

	private void Update()
	{
		if (HitBox != null)
		{
			UpdatePositionBasedOnWorldSpace();
			if (m_allowManualUpdate)
			{
				UpdateForFail();
				UpdateColourAndScale();
			}
		}
		else
		{
			base.gameObject.SetActive(value: false);
		}
	}

	private void Event_OnGestureInput(BossBattleSystem.GestureSettings.Types gesture)
	{
		if (m_acceptingTouch)
		{
			m_acceptingTouch = false;
			m_playLoopAnim = false;
			m_allowManualUpdate = false;
			DismissReticule();
		}
	}

	private void Trigger_PlayAnim()
	{
		if (m_playLoopAnim)
		{
			m_playLoopAnim = false;
			m_gestureMissed = false;
			m_animation.Play(m_targetLoopnimation);
		}
		else
		{
			m_playLoopAnim = true;
			ResetReticule();
		}
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

	private void ResetReticule()
	{
		m_allowManualUpdate = true;
		m_currentDist.y = m_trianglePointMovers[0].transform.localPosition.y + m_distDif;
		for (int i = 0; i < m_trianglePointMovers.Length; i++)
		{
			m_trianglePointMovers[i].transform.localPosition = m_currentDist;
			m_trianglePointColourers[i].color = m_startColour;
		}
		m_ringMover.transform.localScale = Vector3.one;
		m_ringColourer.color = m_startColour;
		m_centerColourer.color = m_startColour;
	}

	private void DismissReticule()
	{
		m_animation.Stop(m_targetLoopnimation);
		m_trigger.SendMessage("OnClick", SendMessageOptions.RequireReceiver);
	}

	private void UpdatePositionBasedOnWorldSpace()
	{
		Vector3 zero = Vector3.zero;
		Vector3 zero2 = Vector3.zero;
		Vector3 zero3 = Vector3.zero;
		zero = HitBox.m_transform.position;
		zero2 = m_mainGameCamera.WorldToViewportPoint(zero);
		zero3 = m_guiCamera.ViewportToWorldPoint(zero2);
		zero3.z = 0f;
		base.transform.position = zero3;
	}

	private void UpdateForFail()
	{
		if (!m_failed && !Boss.GetInstance().AttackPhase().AttackSuccess)
		{
			m_failed = true;
			m_failTimer = m_failLerpTime;
		}
		else if (m_failed && m_failTimer > 0f)
		{
			m_failTimer -= Time.deltaTime;
			if (m_failTimer < 0f)
			{
				m_failTimer = 0f;
			}
		}
	}

	private void UpdateColourAndScale()
	{
		float num;
		if (!m_failed)
		{
			m_gestureProgress = 1f - Boss.GetInstance().AttackPhase().GetGestureProgress();
			num = m_gestureProgress;
		}
		else
		{
			num = 1f - m_failTimer / m_failLerpTime * (1f - m_gestureProgress);
		}
		float num2 = 1f + (m_destinationOfRingScale - 1f) * num;
		m_currentScale.x = num2;
		m_currentScale.y = num2;
		m_ringMover.transform.localScale = m_currentScale;
		float r = m_startColour.r;
		float g = m_startColour.g;
		if (num < 0.5f)
		{
			r = m_startColour.r - m_redDif * (num * 2f);
		}
		else
		{
			r = m_endColour.r;
			g = m_startColour.g - m_greenDif * ((num - 0.5f) * 2f);
		}
		Color color = new Color(r, g, m_startColour.b, m_startColour.a);
		m_ringColourer.color = color;
		m_centerColourer.color = color;
		m_currentDist.y = m_startDist - m_distDif * num;
		for (int i = 0; i < m_trianglePointMovers.Length; i++)
		{
			m_trianglePointMovers[i].transform.localPosition = m_currentDist;
			m_trianglePointColourers[i].color = color;
		}
	}

	private void Event_OnBossBattleOutroStart()
	{
		HitBox = null;
	}

	private void Event_OnGestureWindowTimeOut()
	{
		m_gestureMissed = true;
		m_acceptingTouch = false;
		DismissReticule();
	}
}
