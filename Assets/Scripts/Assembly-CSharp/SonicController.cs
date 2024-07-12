using UnityEngine;

[AddComponentMenu("Dash/Sonic/Input Controller")]
public class SonicController : MonoBehaviour
{
	private SimpleGestureMonitor m_simpleGestureMonitor;

	private bool m_jump;

	private bool m_roll;

	private bool m_left;

	private bool m_right;

	private bool m_tap;

	private bool m_faketap;

	private bool m_jumpPermitted = true;

	private bool m_autoJumpActive;

	private GameObject m_labelGestureOutputDebug;

	public void Awake()
	{
		EventDispatch.RegisterInterest("ResetGameState", this);
		EventDispatch.RegisterInterest("StartGameState", this);
		m_simpleGestureMonitor = new SimpleGestureMonitor();
	}

	public void OnDestroy()
	{
		EventDispatch.UnregisterAllInterest(this);
	}

	public void Start()
	{
	}

	private void Event_ResetGameState(GameState.Mode state)
	{
		m_jumpPermitted = true;
	}

	private void Event_StartGameState(GameState.Mode state)
	{
		m_simpleGestureMonitor.reset();
	}

	public void Update()
	{
		resetControls();
		if (Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.WindowsEditor)
		{
			updateEditorControls();
		}
		m_simpleGestureMonitor.Update();
		updateGameControls();
		applyControls();
	}

	private void resetControls()
	{
		m_jump = false;
		m_roll = false;
		m_left = false;
		m_right = false;
		m_tap = false;
		m_faketap = false;
	}

	private void updateEditorControls()
	{
		m_jump |= Input.GetKeyDown(KeyCode.UpArrow);
		m_roll |= Input.GetKeyDown(KeyCode.DownArrow);
		m_left |= Input.GetKeyDown(KeyCode.LeftArrow);
		m_right |= Input.GetKeyDown(KeyCode.RightArrow);
		m_faketap |= Input.GetKeyDown(KeyCode.Space);
	}

	private void updateGameControls()
	{
		m_jump |= m_simpleGestureMonitor.swipeUpDetected();
		m_roll |= m_simpleGestureMonitor.swipeDownDetected();
		m_left |= m_simpleGestureMonitor.swipeLeftDetected();
		m_right |= m_simpleGestureMonitor.swipeRightDetected();
		m_tap |= m_simpleGestureMonitor.tapDetected();
	}

	private void applyControls()
	{
		if (m_left)
		{
			SendMessage("Strafe", SideDirection.Left);
			if ((bool)m_labelGestureOutputDebug)
			{
				m_labelGestureOutputDebug.SendMessage("Strafe", SideDirection.Left);
			}
		}
		else if (m_right)
		{
			SendMessage("Strafe", SideDirection.Right);
			if ((bool)m_labelGestureOutputDebug)
			{
				m_labelGestureOutputDebug.SendMessage("Strafe", SideDirection.Right);
			}
		}
		else if (m_roll)
		{
			if (m_jumpPermitted)
			{
				SendMessage("Roll");
				SendMessage("Dive");
				if ((bool)m_labelGestureOutputDebug)
				{
					m_labelGestureOutputDebug.SendMessage("Roll");
				}
			}
		}
		else if (m_jump && m_jumpPermitted)
		{
			SendMessage("Jump");
			if ((bool)m_labelGestureOutputDebug)
			{
				m_labelGestureOutputDebug.SendMessage("Jump");
			}
		}
		if (m_tap)
		{
			float[] value = new float[2]
			{
				m_simpleGestureMonitor.getTapX(),
				m_simpleGestureMonitor.getTapY()
			};
			SendMessage("Tap", value);
			if ((bool)m_labelGestureOutputDebug)
			{
				m_labelGestureOutputDebug.SendMessage("Tap", value);
			}
		}
		if (m_faketap)
		{
			float[] value2 = new float[2]
			{
				Screen.width / 2,
				Screen.height / 2
			};
			SendMessage("Tap", value2);
			if ((bool)m_labelGestureOutputDebug)
			{
				m_labelGestureOutputDebug.SendMessage("Tap", value2);
			}
		}
	}

	public void activateAutoJump()
	{
		m_autoJumpActive = true;
	}

	public void deactivateAutoJump()
	{
		m_autoJumpActive = false;
	}

	public bool isAutoJumpActive()
	{
		return m_autoJumpActive;
	}

	public void disallowJump()
	{
		m_jumpPermitted = false;
	}

	public void allowJump()
	{
		m_jumpPermitted = true;
	}
}
