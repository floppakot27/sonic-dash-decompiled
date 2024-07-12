using UnityEngine;

public class HudContent : MonoBehaviour
{
	private HudContent_Rings m_ringContent;

	private HudContent_Score m_scoreContent;

	private HudContent_DashMeter m_dashContent;

	private HudContent_FloatingScore m_floatingScore;

	private HudContent_PowerUps m_powerUps;

	private HudContent_Boosters m_boosters;

	private HudContent_FriendDisplay m_friendDisplay;

	private HudContent_FlyingRings m_flyingRings;

	private HudContent_MissionCompleted m_missionCompleted;

	private HudContent_DCPieces m_dcPieces;

	private HudContent_BossAlerts m_bossAlerts;

	private HudContent_CollectableItem m_collectableItem;

	[SerializeField]
	private UILabel m_currentRingCount;

	[SerializeField]
	private UILabel m_bankedRingCount;

	[SerializeField]
	private float m_bankingRate = 1f;

	[SerializeField]
	private AudioClip m_bankingAudioClip;

	[SerializeField]
	private AudioClip m_startBankingAudioClip;

	[SerializeField]
	private GameObject m_bankingCountTrigger;

	[SerializeField]
	private GameObject m_ringsNonBankingDisplayTrigger;

	[SerializeField]
	private UILabel m_currentScore;

	[SerializeField]
	private UILabel m_currentMultiplier;

	[SerializeField]
	private ScoreTracker m_scoreTracker;

	[SerializeField]
	private GameObject m_highScoreTrigger;

	[SerializeField]
	private GameObject m_additionalMultiplierTrigger;

	[SerializeField]
	private GameObject m_grassEventIndicator;

	[SerializeField]
	private GameObject m_templeEventIndicator;

	[SerializeField]
	private GameObject m_beachEventIndicator;

	[SerializeField]
	private AudioClip m_highScoreAudioClip;

	[SerializeField]
	private GameObject m_scoreBarDisplayTrigger;

	[SerializeField]
	private DashMeter m_dashMeter;

	[SerializeField]
	private AudioClip m_dashMeterBlockedAudioClip;

	[SerializeField]
	private UISprite m_dashMeterFilledSprite;

	[SerializeField]
	private GameObject m_dashModeEnabledTrigger;

	[SerializeField]
	private GameObject m_dashMeterShowTrigger;

	[SerializeField]
	private UITexture m_dashMeterBar;

	[SerializeField]
	private GameObject m_bossBattleBeginHud;

	[SerializeField]
	private GameObject m_bossBattleEndHud;

	[SerializeField]
	private GameObject m_bossDodgeHint;

	[SerializeField]
	private GameObject m_bossAttackHint;

	[SerializeField]
	private GameObject m_bossFailPrompt;

	[SerializeField]
	private GameObject m_bossTargetTrigger;

	[SerializeField]
	private BossTargetReticule m_bossTargetReticule;

	[SerializeField]
	private AudioClip m_bossBattleHudEnterAudioClip;

	[SerializeField]
	private AudioClip m_bossBattleHudExitAudioClip;

	[SerializeField]
	private UILabel m_bossBattleAttackOutcomeLabel;

	[SerializeField]
	private UILabel m_bossBattleScoreBonusLabel;

	[SerializeField]
	private GameObject m_sourceLabel;

	[SerializeField]
	private int m_floatingScorePool = 3;

	[SerializeField]
	private float m_floatingScoreTime = 3f;

	[SerializeField]
	private string m_floatingScoreSource;

	[SerializeField]
	private float m_floatingScoreSourceOffset;

	[SerializeField]
	private AnimationCurve m_floatingScoreSpeed;

	[SerializeField]
	private Color m_floatingScoreColor;

	[SerializeField]
	private Color m_floatingScoreBonusColor;

	[SerializeField]
	private UISprite m_boosterHUDTop;

	[SerializeField]
	private UISprite m_boosterHUDMid;

	[SerializeField]
	private UISprite m_boosterHUDBot;

	[SerializeField]
	private Animation m_animationHUDTop;

	[SerializeField]
	private Animation m_animationHUDMid;

	[SerializeField]
	private Animation m_animationHUDBot;

	[SerializeField]
	private AudioClip m_boosterEnemyAudio;

	[SerializeField]
	private AudioClip m_boosterRingsAudio;

	[SerializeField]
	private ParticleSystem m_boosterHUDParticles;

	[SerializeField]
	private GameObject m_boosterDisplayTrigger;

	[SerializeField]
	private GameObject m_instaRespawnDisplayTrigger;

	[SerializeField]
	private GameObject m_instaFreeRespawnDisplayTrigger;

	[SerializeField]
	private GameObject m_headStartDisplayTrigger;

	[SerializeField]
	private GameObject m_superheadStartDisplayTrigger;

	[SerializeField]
	private GameObject m_friendDisplayTrigger;

	[SerializeField]
	private GameObject m_friendDisplayRoot;

	[SerializeField]
	private Texture2D m_defaultFriendImage;

	[SerializeField]
	private float m_friendChangeDelay = 0.5f;

	[SerializeField]
	private int m_friendScoreBoundary;

	[SerializeField]
	private GameObject m_flyingRing;

	[SerializeField]
	private GameObject m_flyingMana;

	[SerializeField]
	private string m_flyingRingsSource;

	[SerializeField]
	private int m_flyingRingsPoolSize;

	[SerializeField]
	private GameObject m_bankingTarget;

	[SerializeField]
	private GameObject m_dashTarget;

	[SerializeField]
	private GameObject m_missionCompletedDisplayTrigger;

	[SerializeField]
	private GameObject m_missionCompletedDisplayRoot;

	[SerializeField]
	private float m_missionCompletedDisplayDuration;

	[SerializeField]
	private AudioClip m_missionCompletedAudio;

	[SerializeField]
	private GameObject m_dcPiecesTrigger;

	[SerializeField]
	private GameObject m_dcPiecesDisplayRoot;

	[SerializeField]
	private float m_dcPiecesDisplayDuration;

	[SerializeField]
	private MeshRenderer[] m_dcPiecesMeshes = new MeshRenderer[4];

	[SerializeField]
	private AudioClip m_dcPiecesAudio;

	[SerializeField]
	private GameObject m_gcCollectableItemPanelTrigger;

	[SerializeField]
	private GameObject[] m_gcPickUps = new GameObject[1];

	public Texture2D DefaultFriendImage => m_defaultFriendImage;

	public void HudVisible(bool visible)
	{
		m_powerUps.HudVisible(visible);
		m_boosters.HudVisible(visible);
		m_friendDisplay.HudVisible(visible);
		m_dashContent.HudVisible(visible);
		m_missionCompleted.HudVisible(visible);
		m_dcPieces.HudVisible(visible);
		m_bossAlerts.HudVisible(visible);
		m_collectableItem.HudVisible(visible);
	}

	public void NewGameHudVisible()
	{
		m_scoreContent.NewGameActivateEvent();
	}

	private void Start()
	{
		EventDispatch.RegisterInterest("OnNewGameStarted", this);
		EventDispatch.RegisterInterest("StartGameState", this);
		EventDispatch.RegisterInterest("OnSonicDeath", this);
		EventDispatch.RegisterInterest("OnSonicResurrection", this);
		m_ringContent = new HudContent_Rings(m_currentRingCount, m_bankedRingCount, m_bankingRate, m_bankingCountTrigger, m_startBankingAudioClip, m_bankingAudioClip, m_ringsNonBankingDisplayTrigger);
		m_scoreContent = new HudContent_Score(m_scoreTracker, m_currentScore, m_currentMultiplier, m_highScoreTrigger, m_additionalMultiplierTrigger, m_grassEventIndicator, m_templeEventIndicator, m_beachEventIndicator, m_highScoreAudioClip, m_scoreBarDisplayTrigger);
		m_dashContent = new HudContent_DashMeter(m_dashMeter, m_dashMeterFilledSprite, m_dashMeterShowTrigger, m_dashModeEnabledTrigger, m_dashMeterBlockedAudioClip, m_dashMeterBar);
		m_floatingScore = new HudContent_FloatingScore(m_sourceLabel, m_floatingScorePool, m_floatingScoreTime, m_floatingScoreSpeed, m_floatingScoreSource, m_floatingScoreSourceOffset, m_floatingScoreColor, m_floatingScoreBonusColor);
		m_powerUps = new HudContent_PowerUps(m_instaRespawnDisplayTrigger, m_instaFreeRespawnDisplayTrigger, m_headStartDisplayTrigger, m_superheadStartDisplayTrigger);
		m_boosters = new HudContent_Boosters(m_boosterHUDTop, m_boosterHUDMid, m_boosterHUDBot, m_animationHUDTop, m_animationHUDMid, m_animationHUDBot, m_boosterEnemyAudio, m_boosterRingsAudio, m_boosterHUDParticles, m_boosterDisplayTrigger);
		m_friendDisplay = new HudContent_FriendDisplay(m_friendDisplayRoot, m_friendDisplayTrigger, m_scoreTracker, m_defaultFriendImage, m_friendChangeDelay, m_friendScoreBoundary);
		m_flyingRings = new HudContent_FlyingRings(m_flyingRing, m_flyingMana, m_flyingRingsPoolSize, m_flyingRingsSource, m_bankingTarget, m_bankingRate, m_dashTarget);
		m_missionCompleted = new HudContent_MissionCompleted(m_missionCompletedDisplayRoot, m_missionCompletedDisplayTrigger, m_missionCompletedDisplayDuration, m_missionCompletedAudio);
		m_dcPieces = new HudContent_DCPieces(m_dcPiecesTrigger, m_dcPiecesDisplayRoot, m_dcPiecesDisplayDuration, m_dcPiecesMeshes, m_dcPiecesAudio);
		m_bossAlerts = new HudContent_BossAlerts(m_bossBattleBeginHud, m_bossBattleEndHud, m_bossDodgeHint, m_bossAttackHint, m_bossFailPrompt, m_bossTargetTrigger, m_bossTargetReticule, m_bossBattleHudEnterAudioClip, m_bossBattleHudExitAudioClip, m_bossBattleAttackOutcomeLabel, m_bossBattleScoreBonusLabel);
		m_collectableItem = new HudContent_CollectableItem(m_gcCollectableItemPanelTrigger, m_gcPickUps);
	}

	private void Update()
	{
		m_ringContent.Update();
		m_scoreContent.Update();
		m_dashContent.Update();
		m_floatingScore.Update();
		m_powerUps.Update();
		m_boosters.Update();
		m_friendDisplay.Update();
		m_flyingRings.Update();
		m_missionCompleted.Update();
		m_dcPieces.Update();
		m_bossAlerts.Update();
		m_collectableItem.Update();
	}

	private void LateUpdate()
	{
		m_floatingScore.LateUpdate();
	}

	private void BankingLabelTransitioned()
	{
		m_ringContent.BankingLabelTransitioned();
	}

	private void Event_OnNewGameStarted()
	{
		m_ringContent.OnResetOnNewGame();
		m_scoreContent.OnResetOnNewGame();
		m_dashContent.OnResetOnNewGame();
		m_floatingScore.OnResetOnNewGame();
		m_powerUps.OnResetOnNewGame();
		m_boosters.OnResetOnNewGame();
		m_friendDisplay.OnResetOnNewGame();
		m_flyingRings.OnResetOnNewGame();
		m_missionCompleted.OnResetOnNewGame();
		m_dcPieces.OnResetOnNewGame();
		m_bossAlerts.OnResetOnNewGame();
		m_collectableItem.OnResetOnNewGame();
	}

	private void Event_StartGameState(GameState.Mode startState)
	{
		if (startState != 0)
		{
			bool paused = false;
			if (startState == GameState.Mode.PauseMenu)
			{
				paused = true;
			}
			m_ringContent.OnPauseStateChanged(paused);
			m_scoreContent.OnPauseStateChanged(paused);
			m_dashContent.OnPauseStateChanged(paused);
			m_floatingScore.OnPauseStateChanged(paused);
			m_powerUps.OnPauseStateChanged(paused);
			m_boosters.OnPauseStateChanged(paused);
			m_friendDisplay.OnPauseStateChanged(paused);
			m_flyingRings.OnPauseStateChanged(paused);
			m_missionCompleted.OnPauseStateChanged(paused);
			m_dcPieces.OnPauseStateChanged(paused);
			m_bossAlerts.OnPauseStateChanged(paused);
			m_collectableItem.OnPauseStateChanged(paused);
		}
	}

	private void Event_OnSonicDeath()
	{
		m_ringContent.OnPlayerDeath();
		m_scoreContent.OnPlayerDeath();
		m_dashContent.OnPlayerDeath();
		m_floatingScore.OnPlayerDeath();
		m_powerUps.OnPlayerDeath();
		m_boosters.OnPlayerDeath();
		m_friendDisplay.OnPlayerDeath();
		m_flyingRings.OnPlayerDeath();
		m_missionCompleted.OnPlayerDeath();
		m_dcPieces.OnPlayerDeath();
		m_bossAlerts.OnPlayerDeath();
		m_collectableItem.OnPlayerDeath();
	}

	private void Event_OnSonicResurrection()
	{
		m_ringContent.OnSonicResurrection();
	}
}
