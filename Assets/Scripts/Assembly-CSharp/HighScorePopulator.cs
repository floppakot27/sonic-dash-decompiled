using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HighScorePopulator : MonoBehaviour
{
	private UIGrid m_displayGrid;

	private UIDraggablePanel m_draggablePanel;

	private List<GameObject> m_panelEntries;

	private GameObject m_facebookLoginPrompt;

	[SerializeField]
	private int m_entriesToShow = 11;

	[SerializeField]
	private UIDragPanelContents m_entryTemplate;

	[SerializeField]
	private UIDragPanelContents m_facebookEntry;

	[SerializeField]
	private Leaderboards.Types m_leaderboardToShow;

	[SerializeField]
	private Leaderboards.Request.Filter m_filterType = Leaderboards.Request.Filter.Friends;

	[SerializeField]
	private Texture2D m_defaultAvatar;

	[SerializeField]
	private GameObject m_loadingIndicator;

	[SerializeField]
	private GameObject m_emptyBoardContent;

	[SerializeField]
	private GameObject m_notSignedInContent;

	[SerializeField]
	private int m_maximumEntriesForEmptyContent = 4;

	private void Start()
	{
		m_displayGrid = Utils.FindBehaviourInTree(this, m_displayGrid);
		m_draggablePanel = Utils.FindBehaviourInTree(this, m_draggablePanel);
		CreateStorePanelEntries();
		EventDispatch.RegisterInterest("LeaderboardRequestComplete", this);
		EventDispatch.RegisterInterest("LeaderboardCacheComplete", this);
		EventDispatch.RegisterInterest("CacheLeaderboard", this);
		EventDispatch.RegisterInterest("OnFacebookLogin", this);
		EventDispatch.RegisterInterest("OnGameCenterLogin", this);
	}

	private void OnEnable()
	{
		StartCoroutine(StartPendingActivation());
	}

	private void OnDisable()
	{
		DisableAllEntries();
	}

	private IEnumerator StartPendingActivation()
	{
		yield return null;
		if (m_notSignedInContent != null)
		{
			m_notSignedInContent.SetActive(value: false);
		}
		DisableAllEntries();
		RequestLeaderboardEntries();
		GameState.g_gameState.CacheLeaderboards();
	}

	private void CreateStorePanelEntries()
	{
		m_panelEntries = new List<GameObject>(m_entriesToShow);
		m_draggablePanel.ResetPosition();
		for (int i = 0; i < m_entriesToShow; i++)
		{
			GameObject gameObject = NGUITools.AddChild(base.gameObject, m_entryTemplate.gameObject);
			gameObject.name = string.Format("LB Entry {0}", i.ToString("D3"));
			m_panelEntries.Add(gameObject);
			gameObject.SetActive(value: false);
		}
		m_displayGrid.sorted = true;
		m_displayGrid.Reposition();
		GameObject facebookLoginPrompt = NGUITools.AddChild(base.gameObject, m_facebookEntry.gameObject);
		m_facebookLoginPrompt = facebookLoginPrompt;
		UIButtonMessage uIButtonMessage = m_facebookLoginPrompt.GetComponentInChildren(typeof(UIButtonMessage)) as UIButtonMessage;
		if (uIButtonMessage != null)
		{
			uIButtonMessage.target = Community.g_instance.gameObject;
		}
		m_facebookLoginPrompt.SetActive(value: false);
		m_draggablePanel.ResetPosition();
	}

	private void DisableAllEntries()
	{
		if (m_panelEntries == null)
		{
			return;
		}
		foreach (GameObject panelEntry in m_panelEntries)
		{
			if (panelEntry != null)
			{
				panelEntry.SetActive(value: false);
			}
		}
	}

	private void PopulateHighScoreEntries(int entryCount, Leaderboards.Entry[] entries)
	{
		m_draggablePanel.ResetPosition();
		int num = -1;
		for (int i = 0; i < m_entriesToShow; i++)
		{
			GameObject gameObject = m_panelEntries[i];
			if (i < entryCount)
			{
				Leaderboards.Entry entry = entries[i];
				gameObject.SetActive(value: true);
				m_panelEntries[i] = SetEntryContent(entry.m_rank, entry, gameObject);
				if (entry.m_playersRank)
				{
					num = i;
				}
			}
			else
			{
				gameObject.SetActive(value: false);
			}
		}
		bool isFacebookAuthenticated = ((HLLocalUser)Social.localUser).isFacebookAuthenticated;
		m_facebookLoginPrompt.SetActive(!isFacebookAuthenticated);
		if (!isFacebookAuthenticated && num != -1)
		{
			m_facebookLoginPrompt.name = m_panelEntries[num].name + "2";
		}
		else
		{
			m_facebookLoginPrompt.name = "aaaaaa";
		}
		m_displayGrid.sorted = true;
		m_displayGrid.Reposition();
		m_draggablePanel.ResetPosition();
		int num2 = ((!isFacebookAuthenticated) ? (entryCount + 1) : entryCount);
		if (num != -1)
		{
			int num3 = Mathf.FloorToInt(m_draggablePanel.panel.clipRange.w / m_displayGrid.cellHeight);
			int num4 = (num3 - 1) / 2;
			int val = Math.Min(num - num4, num2 - num3);
			val = Math.Max(val, 0);
			float y = m_panelEntries[val].transform.localPosition.y;
			m_draggablePanel.MoveRelative(Vector3.down * y);
			m_draggablePanel.RestrictWithinBounds(instant: true);
		}
		if (m_emptyBoardContent != null)
		{
			m_emptyBoardContent.SetActive(value: false);
		}
	}

	private GameObject SetEntryContent(int friendRank, Leaderboards.Entry entryData, GameObject entry)
	{
		GameObject gameObject = Utils.FindTagInChildren(entry, "HighScore_Name");
		if ((bool)gameObject)
		{
			UILabel component = gameObject.GetComponent<UILabel>();
			if (component.text.CompareTo(entryData.m_user) != 0)
			{
				string text = entry.name;
				entry.name += "deleted";
				entry.SetActive(value: false);
				UnityEngine.Object.Destroy(entry);
				GameObject gameObject2 = NGUITools.AddChild(base.gameObject, m_entryTemplate.gameObject);
				gameObject2.name = text;
				entry = gameObject2;
				gameObject = Utils.FindTagInChildren(entry, "HighScore_Name");
			}
		}
		GameObject gameObject3 = Utils.FindTagInChildren(entry, "HighScore_Avatar");
		GameObject gameObject4 = Utils.FindTagInChildren(entry, "HighScore_Score");
		GameObject gameObject5 = Utils.FindTagInChildren(entry, "HighScore_Rank");
		GameObject gameObject6 = Utils.FindTagInChildren(entry, "HighScore_Player");
		GameObject gameObject7 = Utils.FindTagInChildren(entry, "HighScore_Brag");
		GameObject gameObject8 = Utils.FindTagInChildren(entry, "Leaderboard FB");
		GameObject gameObject9 = Utils.FindTagInChildren(entry, "Leaderboard GC");
		if ((bool)gameObject9)
		{
			UISprite component2 = gameObject9.GetComponent<UISprite>();
			component2.spriteName = "icon_gpg_tiny";
		}
		if ((bool)gameObject)
		{
			UILabel component3 = gameObject.GetComponent<UILabel>();
			component3.text = entryData.m_user;
		}
		if ((bool)gameObject4)
		{
			UILabel component4 = gameObject4.GetComponent<UILabel>();
			component4.text = LanguageUtils.FormatNumber(entryData.m_score);
		}
		if ((bool)gameObject5)
		{
			UILabel component5 = gameObject5.GetComponent<UILabel>();
			if (friendRank == -1)
			{
				component5.text = string.Empty;
			}
			else
			{
				component5.text = friendRank.ToString();
			}
		}
		if ((bool)gameObject3)
		{
			gameObject3.SetActive(value: false);
			gameObject3.SetActive(value: true);
			UITexture component6 = gameObject3.GetComponent<UITexture>();
			component6.mainTexture = ((!(entryData.m_avatar == null)) ? entryData.m_avatar : m_defaultAvatar);
		}
		if ((bool)gameObject6)
		{
			if (entryData.m_playersRank)
			{
				gameObject6.SetActive(value: true);
			}
			else
			{
				gameObject6.SetActive(value: false);
			}
		}
		if ((bool)gameObject7)
		{
			gameObject7.SetActive(value: false);
		}
		if ((bool)gameObject9)
		{
			bool active = entryData.m_source == HLUserProfile.ProfileSource.GameCenter;
			gameObject9.SetActive(active);
		}
		if ((bool)gameObject8)
		{
			bool active2 = entryData.m_source == HLUserProfile.ProfileSource.Facebook;
			gameObject8.SetActive(active2);
		}
		return entry;
	}

	private void RequestLeaderboardEntries()
	{
		EventDispatch.GenerateEvent("RequestLeaderboard", Leaderboards.Types.sdHighestScore.ToString());
	}

	private void Event_OnFacebookLogin()
	{
		GameState.g_gameState.CacheLeaderboards();
	}

	private void Event_OnGameCenterLogin()
	{
		GameState.g_gameState.CacheLeaderboards();
	}

	private void Event_CacheLeaderboard(Leaderboards.Request request)
	{
		if (base.gameObject.activeInHierarchy)
		{
			m_loadingIndicator.SetActive(value: true);
		}
	}

	private void Event_LeaderboardCacheComplete(string leaderboardID, bool leaderboardLoaded)
	{
		if (base.gameObject.activeInHierarchy)
		{
			RequestLeaderboardEntries();
		}
	}

	private void Event_LeaderboardRequestComplete(string leaderboardID, Leaderboards.Entry[] entries)
	{
		if (!base.gameObject.activeInHierarchy)
		{
			return;
		}
		m_loadingIndicator.SetActive(value: false);
		int num = entries?.Count((Leaderboards.Entry entry) => entry != null && entry.m_valid) ?? 0;
		if (num > 0)
		{
			Leaderboards.Entry entry2 = entries[entries.Length - 1];
			if (entry2 != null && entry2.m_valid && !entry2.m_playersRank)
			{
				num--;
			}
		}
		PopulateHighScoreEntries(num, entries);
	}
}
