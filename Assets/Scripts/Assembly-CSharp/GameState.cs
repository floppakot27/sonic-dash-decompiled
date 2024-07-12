using System;
using UnityEngine;

public class GameState : MonoBehaviour
{
	public enum Mode
	{
		Menu,
		Game,
		PauseMenu
	}

	[Flags]
	private enum State
	{
		None = 1,
		Resetting = 2,
		Starting = 4,
		Dialog = 8,
		PollAssets = 0x10,
		GameObscured = 0x20,
		LoadingNotifed = 0x40,
		PendingLeaderboard = 0x80,
		LeaderboardReady = 0x100
	}

	private const string EventRoot = "events";

	private const string LostWorldEventProperty = "lostworldevent";

	private const float LeaderboardTimeOut = 0f;

	public static GameState g_gameState;

	private static bool s_hudTransition;

	[SerializeField]
	private LoadingScreenFlow m_screenFlow;

	[SerializeField]
	private GuiTrigger m_gameOverScreen;

	[SerializeField]
	private GuiTrigger m_countDownScreen;

	[SerializeField]
	private GuiTrigger m_continueScreen;

	[SerializeField]
	private int[] m_costToContinue;

	private TrackGenerator m_track;

	private Mode m_nextState;

	private Mode m_currentState;

	private State m_state = State.None;

	private float m_gameStartTime;

	private GameOverState m_gameOverState;

	private bool m_sonidDeath;

	private float m_leaderboardTimeout;

	public static float TimeInGame => (!(g_gameState == null)) ? (Time.time - g_gameState.m_gameStartTime) : Time.timeSinceLevelLoad;

	public static bool IsAvailable => g_gameState != null;

	public static bool HUDTransitioning
	{
		get
		{
			return s_hudTransition;
		}
		set
		{
			s_hudTransition = value;
		}
	}

	private bool LostWorldEventActive { get; set; }

	public static void RequestReset(Mode resetState)
	{
		g_gameState.StartReset(resetState);
		if (resetState == Mode.Game)
		{
			OfferRegion.EndAll();
		}
	}

	public static void RequestMode(Mode resetState)
	{
		g_gameState.StartMode(resetState);
		if (resetState == Mode.Game)
		{
			OfferRegion.EndAll();
		}
	}

	public static void RequestGameOver(bool bAllowRespawn)
	{
		g_gameState.m_gameOverState.TriggerGameOver(bAllowRespawn);
	}

	public static Mode GetMode()
	{
		return g_gameState.m_currentState;
	}

	public static bool AllAssetsLoaded()
	{
		return g_gameState.AreAllAssetsLoaded();
	}

	public static bool GetLostWorldEventActive()
	{
		return g_gameState.LostWorldEventActive;
	}

	private void Start()
	{
		EventDispatch.RegisterInterest("LoadTransitionOut", this);
		EventDispatch.RegisterInterest("LoadTransitionFinish", this);
		EventDispatch.RegisterInterest("OnDialogShown", this);
		EventDispatch.RegisterInterest("OnDialogHidden", this);
		EventDispatch.RegisterInterest("3rdPartyActive", this);
		EventDispatch.RegisterInterest("3rdPartyInactive", this);
		EventDispatch.RegisterInterest("OnNewGameStarted", this);
		EventDispatch.RegisterInterest("OnSonicDeath", this);
		EventDispatch.RegisterInterest("OnSonicResurrection", this);
		EventDispatch.RegisterInterest("LeaderboardCacheComplete", this);
		EventDispatch.RegisterInterest("FeatureStateReady", this);
		g_gameState = this;
		m_nextState = Mode.Menu;
		m_state = State.None;
		m_gameStartTime = Time.time;
		PropertyStore.Load(loadDelegates: false);
		m_gameOverState = new GameOverState(m_gameOverScreen, m_continueScreen, m_costToContinue);
	}

	private void Update()
	{
		CheckPauseRequirements();
		if ((m_state & State.Resetting) == State.Resetting)
		{
			UpdateLoadingState();
		}
	}

	private void UpdateLoadingState()
	{
		bool flag = false;
		bool flag2 = AllAssetsLoaded();
		if (flag2 && (!FeatureState.Ready || !ABTesting.Ready || !StoreModifier.Ready))
		{
			flag2 = false;
		}
		if (flag2 && (m_state & State.PendingLeaderboard) == State.PendingLeaderboard)
		{
			m_leaderboardTimeout += IndependantTimeDelta.Delta;
			if (m_leaderboardTimeout < 0f)
			{
				flag2 = false;
			}
			else
			{
				flag = true;
			}
		}
		if (flag2 && (m_state & State.LoadingNotifed) != State.LoadingNotifed)
		{
			EventDispatch.GenerateEvent("OnAllAssetsLoaded");
			m_state |= State.LoadingNotifed;
		}
		if (flag2)
		{
			m_state &= ~State.Resetting;
			m_state &= ~State.PollAssets;
			m_state &= ~State.LoadingNotifed;
			m_screenFlow.AssetsLoaded = true;
			SonicSplineTracker.AllowRunning = true;
			m_leaderboardTimeout = 0f;
			if (flag)
			{
				m_state &= ~State.PendingLeaderboard;
				m_state |= State.LeaderboardReady;
			}
		}
	}

	private void Event_FeatureStateReady()
	{
		UpdateForServer();
	}

	private void UpdateForServer()
	{
		LostWorldEventActive = false;
		if (!FeatureState.Valid)
		{
			return;
		}
		LSON.Property stateProperty = FeatureState.GetStateProperty("events", "lostworldevent");
		if (stateProperty != null)
		{
			bool boolValue = false;
			if (LSONProperties.AsBool(stateProperty, out boolValue))
			{
				LostWorldEventActive = boolValue;
			}
		}
	}

	private void CheckPauseRequirements()
	{
		if (m_currentState == Mode.Game)
		{
			bool flag = false;
			if ((m_state & State.GameObscured) == State.GameObscured)
			{
				flag = true;
			}
			if (flag)
			{
				RequestMode(Mode.PauseMenu);
			}
		}
	}

	private void StartReset(Mode resetState)
	{
		EnforceTimeScaleForMode(resetState);
		m_nextState = resetState;
		m_screenFlow.TransitionInStart = OnIdentTransitionInStarted;
		m_screenFlow.TransitionInEnd = OnIdentTransitionInFinished;
		m_screenFlow.TransitionOutEnd = OnIdentTransitionOutFinished;
		m_screenFlow.TransitionOutStart = OnIdentTransitionOutStarted;
		FeatureState.Restart();
		ABTesting.Restart();
		StoreModifier.Restart();
		GC3Progress.Restart();
		m_screenFlow.StartFlow(delayPresentation: false);
		m_state |= State.Resetting;
		m_screenFlow.AssetsLoaded = false;
		if (resetState == Mode.Menu)
		{
			m_state &= ~State.Starting;
		}
		else
		{
			m_state |= State.Starting;
		}
	}

	private void StartMode(Mode startState)
	{
		if (startState == Mode.Game && m_nextState != Mode.PauseMenu)
		{
			m_gameStartTime = Time.time;
			m_state |= State.Starting;
		}
		m_nextState = startState;
		Event_LoadTransitionFinish();
	}

	private void OnApplicationPause(bool paused)
	{
		if (paused)
		{
			if (m_currentState == Mode.PauseMenu)
			{
				EventDispatch.GenerateEvent("OnCountDownReset");
			}
			if (m_currentState == Mode.Game && !s_hudTransition && !m_sonidDeath && (m_state & State.Dialog) != State.Dialog)
			{
				MenuStack.RequestPage = m_countDownScreen;
				RequestMode(Mode.PauseMenu);
			}
		}
	}

	private void EnforceTimeScaleForMode(Mode mode)
	{
		if (mode == Mode.PauseMenu && !m_sonidDeath)
		{
			TimeScaler.Scale = 0f;
		}
		else
		{
			TimeScaler.Scale = 1f;
		}
	}

	private void EnforceTimeScaleForDialog()
	{
		float scale = (((m_state & State.Dialog) != State.Dialog) ? 1f : 0f);
		TimeScaler.Scale = scale;
	}

	private void OnIdentTransitionInStarted(int identIndex, int indexCount)
	{
		if (identIndex == 0)
		{
			LoadTransitionIn();
		}
		EventDispatch.GenerateEvent("OnTransitionStarted");
	}

	private void OnIdentTransitionInFinished(int identIndex, int indexCount)
	{
		if (identIndex == indexCount)
		{
			Event_LoadTransitionOut();
		}
	}

	private void OnIdentTransitionOutStarted(int identIndex, int indexCount)
	{
		if (identIndex == indexCount && (m_state & State.Starting) == State.Starting)
		{
			EventDispatch.GenerateEvent("OnNewGameAboutToStart");
		}
	}

	private void OnIdentTransitionOutFinished(int identIndex, int indexCount)
	{
		if (identIndex == indexCount)
		{
			Event_LoadTransitionFinish();
		}
		EventDispatch.GenerateEvent("OnTransitionFinished");
	}

	private void LoadTransitionIn()
	{
		EventDispatch.GenerateEvent("DisableGameState", m_nextState);
	}

	private void Event_LoadTransitionOut()
	{
		EventDispatch.GenerateEvent("ResetTrackState");
		EventDispatch.GenerateEvent("ResetGameState", m_nextState);
		EnforceTimeScaleForMode(m_nextState);
		m_state |= State.PollAssets;
	}

	private bool AreAllAssetsLoaded()
	{
		if ((m_state & State.PollAssets) != State.PollAssets)
		{
			return false;
		}
		bool flag = true;
		if ((m_state & State.Starting) != State.Starting)
		{
			FESceneLoader component = g_gameState.GetComponent<FESceneLoader>();
			flag &= component.ScenesLoaded;
		}
		SceneLoader component2 = g_gameState.GetComponent<SceneLoader>();
		flag &= component2.ScenesLoaded;
		CharacterSceneLoader component3 = g_gameState.GetComponent<CharacterSceneLoader>();
		flag &= component3.ScenesLoaded;
		if (m_track == null)
		{
			m_track = UnityEngine.Object.FindObjectOfType(typeof(TrackGenerator)) as TrackGenerator;
		}
		bool flag2 = ((!(m_track != null)) ? flag : m_track.IsTrackGenerated);
		return flag && flag2;
	}

	public void CacheLeaderboards()
	{
		if ((m_state & State.PendingLeaderboard) != State.PendingLeaderboard && Social.localUser.authenticated)
		{
			Leaderboards.Request request = new Leaderboards.Request();
			request.m_entries = 30;
			request.m_includePlayersRank = true;
			request.m_filter = Leaderboards.Request.Filter.Friends;
			request.m_leaderboard = Leaderboards.Types.sdHighestScore;
			request.m_leaderboardID = Leaderboards.Types.sdHighestScore.ToString();
			m_state |= State.PendingLeaderboard;
			EventDispatch.GenerateEvent("CacheLeaderboard", request);
		}
	}

	private void Event_LoadTransitionFinish()
	{
		m_currentState = m_nextState;
		EventDispatch.GenerateEvent("StartGameState", m_currentState);
		EnforceTimeScaleForMode(m_currentState);
		if ((m_state & State.Starting) == State.Starting)
		{
			EventDispatch.GenerateEvent("OnNewGameStarted");
			m_state &= ~State.Starting;
			m_gameStartTime = Time.time;
			if ((m_state & State.Dialog) == State.Dialog)
			{
				EnforceTimeScaleForDialog();
			}
		}
	}

	private void Event_OnDialogShown()
	{
		m_state |= State.Dialog;
		if (m_currentState == Mode.Game)
		{
			EnforceTimeScaleForDialog();
		}
	}

	private void Event_OnDialogHidden()
	{
		m_state &= ~State.Dialog;
		if (m_currentState == Mode.Game)
		{
			EnforceTimeScaleForDialog();
		}
	}

	private void Event_3rdPartyActive()
	{
		m_state |= State.GameObscured;
	}

	private void Event_3rdPartyInactive()
	{
		m_state &= ~State.GameObscured;
	}

	private void Event_OnNewGameStarted()
	{
		m_sonidDeath = false;
	}

	private void Event_OnSonicDeath()
	{
		m_sonidDeath = true;
	}

	private void Event_OnSonicResurrection()
	{
		m_sonidDeath = false;
	}

	private void Event_LeaderboardCacheComplete(string leaderboardID, bool leaderboardLoaded)
	{
		m_state &= ~State.PendingLeaderboard;
		if (leaderboardLoaded)
		{
			m_state |= State.LeaderboardReady;
		}
	}
}
