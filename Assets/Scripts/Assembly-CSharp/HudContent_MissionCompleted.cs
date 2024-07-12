using System;
using UnityEngine;

public class HudContent_MissionCompleted
{
	[Flags]
	private enum State
	{
		None = 0,
		Visible = 1,
		Showing = 2
	}

	public struct MissionCompletedNotification
	{
		public int m_missionGroup;

		public bool m_used;

		public MissionAssets.Assets m_assets;
	}

	private const int m_maxNotifications = 3;

	private State m_state;

	private GameObject m_displayRoot;

	private GameObject m_displayTrigger;

	private float m_displayTimer;

	private float m_displayDuration;

	private float m_displayCooldown = 0.5f;

	private AudioClip m_missionCompletedAudio;

	private UILabel m_mission;

	private MeshFilter m_trophyMesh;

	private MissionCompletedNotification[] m_notifications = new MissionCompletedNotification[3];

	private int m_currentNotification;

	public HudContent_MissionCompleted(GameObject displayRoot, GameObject displayTrigger, float displayDuration, AudioClip missionCompletedAudio)
	{
		m_displayRoot = displayRoot;
		m_displayTrigger = displayTrigger;
		m_displayDuration = displayDuration;
		m_missionCompletedAudio = missionCompletedAudio;
		EventDispatch.RegisterInterest("OnMissionComplete", this);
		CacheDisplayElements(displayRoot);
	}

	public void Update()
	{
		if ((m_state & State.Visible) != State.Visible)
		{
			return;
		}
		if ((m_state & State.Showing) != State.Showing)
		{
			if (CooldownFinished())
			{
				int nextNotificationIndex = GetNextNotificationIndex();
				if (nextNotificationIndex >= 0)
				{
					m_currentNotification = nextNotificationIndex;
					Display();
				}
			}
		}
		else
		{
			UpdateDisplayTimer();
		}
	}

	public void OnResetOnNewGame()
	{
		if ((m_state & State.Showing) == State.Showing)
		{
			Hide();
		}
		ResetPool();
		m_displayTimer = 0f;
		m_state = State.None;
	}

	public void OnPauseStateChanged(bool paused)
	{
	}

	public void OnPlayerDeath()
	{
	}

	public void HudVisible(bool visible)
	{
		if (visible)
		{
			m_state |= State.Visible;
		}
		else
		{
			m_state &= ~State.Visible;
		}
	}

	private void CacheDisplayElements(GameObject displayRoot)
	{
		MeshFilter[] componentsInChildren = displayRoot.GetComponentsInChildren<MeshFilter>(includeInactive: true);
		m_trophyMesh = componentsInChildren[0];
	}

	private void Display()
	{
		if (m_mission != null)
		{
			LocalisedStringProperties component = m_mission.GetComponent<LocalisedStringProperties>();
			string format = component.SetLocalisationID("MISSION_TITLE") + " {0}";
			string text = string.Format(format, m_notifications[m_currentNotification].m_missionGroup + 1);
			m_mission.text = text;
		}
		m_trophyMesh.mesh = m_notifications[m_currentNotification].m_assets.m_mesh;
		m_displayTrigger.SendMessage("OnClick", SendMessageOptions.RequireReceiver);
		Audio.PlayClip(m_missionCompletedAudio, loop: false);
		m_state |= State.Showing;
		m_displayTimer = 0f;
	}

	private void Hide()
	{
		if ((m_state & State.Showing) == State.Showing)
		{
			m_notifications[m_currentNotification].m_used = false;
			m_displayTrigger.SendMessage("OnClick", SendMessageOptions.RequireReceiver);
			m_state &= ~State.Showing;
			m_displayTimer = 0f;
		}
	}

	private void UpdateDisplayTimer()
	{
		m_displayTimer += Time.deltaTime;
		if (m_displayTimer > m_displayDuration)
		{
			Hide();
		}
	}

	private bool CooldownFinished()
	{
		m_displayTimer += Time.deltaTime;
		if (m_displayTimer > m_displayCooldown)
		{
			return true;
		}
		return false;
	}

	private MissionAssets.Assets GetMissionAssets(int missionGroup)
	{
		MissionAssets.AssetLists listOfAssets = MissionAssets.ListOfAssets;
		MissionAssets.Assets[] array = null;
		switch (missionGroup)
		{
		case 0:
			array = listOfAssets.m_missionOneAssets;
			break;
		case 1:
			array = listOfAssets.m_missionTwoAssets;
			break;
		case 2:
			array = listOfAssets.m_missionThreeAssets;
			break;
		}
		int translatedIndex = MissionTracker.GetTranslatedIndex(missionGroup);
		return array[translatedIndex];
	}

	private int GetEmptyNotificationIndex()
	{
		int num = m_currentNotification;
		for (int i = 0; i < 3; i++)
		{
			if (!m_notifications[num].m_used)
			{
				return num;
			}
			num = ++num % 3;
		}
		return 0;
	}

	private int GetNextNotificationIndex()
	{
		int num = m_currentNotification;
		for (int i = 0; i < 3; i++)
		{
			if (m_notifications[num].m_used)
			{
				return num;
			}
			num = ++num % 3;
		}
		return -1;
	}

	private void ResetPool()
	{
		for (int i = 0; i < 3; i++)
		{
			m_notifications[i].m_used = false;
		}
		m_currentNotification = 0;
	}

	private void Event_OnMissionComplete(int missionGroup, bool setComplete)
	{
		if ((m_state & State.Visible) == State.Visible)
		{
			int emptyNotificationIndex = GetEmptyNotificationIndex();
			m_notifications[emptyNotificationIndex].m_missionGroup = missionGroup;
			m_notifications[emptyNotificationIndex].m_used = true;
			m_notifications[emptyNotificationIndex].m_assets = GetMissionAssets(missionGroup);
		}
	}
}
