using System;
using UnityEngine;

public class GameOver_ScoreCount : GameOver_Component
{
	[Flags]
	private enum State
	{
		DisplayVisible = 1,
		TotalsVisible = 2,
		HighScoreVisible = 4,
		DoubleRing = 8,
		BoosterBonusVisible = 0x10
	}

	private const float ScoreCountSpeed = 1f;

	private static AudioClip m_audioCountingScore;

	private static AudioClip m_audioCountingRings;

	private static AudioClip m_audioHighScoreAchieved;

	private static AudioClip m_audioBoosterScoreBonus;

	private static AudioClip m_audioDoubleRingsShown;

	private static AudioClip m_audioPanelShown;

	private static bool m_highScoreVisible;

	private State m_state;

	private GameObject m_triggerScore;

	private GameObject m_triggerRingCount;

	private GameObject m_triggerHighlightRings;

	private GameObject m_triggerHighlightRingsReset;

	private GameObject m_triggerHighScore;

	private GameObject m_triggerHighScoreReset;

	private GameObject m_triggerDoubleRing;

	private GameObject m_triggerDoubleRingReset;

	private GameObject m_triggerDoubleRingButton;

	private GameObject m_triggerDoubleRingButtonReset;

	private GameObject m_bankRingsAudioTrigger;

	private GameObject m_triggerBoosterAnimation;

	private GameObject m_triggerBoosterAnimationReset;

	private GameObject m_gcTokenStampAnimation;

	private GameObject m_gcTokenButtonAnimation;

	private GameObject m_gcTokenStampAnimationReset;

	private GameObject m_gcTokenButtonAnimationReset;

	private Animation m_gcTokenButtonLoopAnimation;

	public static Animation m_GCAnimation;

	private UILabel m_scoreLabel;

	private UILabel m_boosterBonusScoreLabel;

	private UILabel m_ringLabel;

	private UILabel m_bankedLabel;

	private UILabel m_multiplierLabel;

	private UILabel m_GCTokensValue;

	private GuiButtonBlocker m_doubleRingBlocker;

	private GuiButtonBlocker m_multiplierBlocker;

	private GuiButtonBlocker m_buyRingsBlocker;

	private GuiButtonBlocker m_tokenBlocker;

	private float m_scoreIncreaseRate;

	private float m_currentScoreDisplay;

	private float m_currentScoreIncrease;

	private float m_currentBonusScoreDisplay;

	private float m_pauseTimeCount;

	private float m_pauseTimeValue = 1f;

	private float m_targetHighScore;

	private float m_ringIncreaseRate;

	private float m_currentRingsDisplay;

	private float m_currentBankedDisplay;

	private float m_currentRingsIncrease;

	public static bool HighScoreVisible => m_highScoreVisible;

	public static void SetAudioProperties(AudioClip audioCountingScore, AudioClip audioCountingRings, AudioClip audioHighScoreAchieved, AudioClip audioBoosterScoreBonus, AudioClip audioDoubleRingsShown, AudioClip audioPanelShown)
	{
		m_audioCountingScore = audioCountingScore;
		m_audioCountingRings = audioCountingRings;
		m_audioHighScoreAchieved = audioHighScoreAchieved;
		m_audioBoosterScoreBonus = audioBoosterScoreBonus;
		m_audioDoubleRingsShown = audioDoubleRingsShown;
		m_audioPanelShown = audioPanelShown;
	}

	public override void Reset()
	{
		base.Reset();
		m_state = (State)0;
		SetStateDelegates(DisplayInitialise, null);
		m_highScoreVisible = false;
	}

	public override void Hide()
	{
		m_triggerScore.SendMessage("OnClick", SendMessageOptions.RequireReceiver);
		m_triggerRingCount.SendMessage("OnClick", SendMessageOptions.RequireReceiver);
	}

	public override void Show()
	{
		m_triggerScore.SendMessage("OnClick", SendMessageOptions.RequireReceiver);
		m_triggerRingCount.SendMessage("OnClick", SendMessageOptions.RequireReceiver);
	}

	public override void ProcessFinished()
	{
		if (PowerUpsInventory.GetPowerUpCount(PowerUps.Type.DoubleRing) == 0)
		{
			m_triggerDoubleRingButton.SendMessage("OnClick", SendMessageOptions.RequireReceiver);
			m_doubleRingBlocker.Blocked = false;
		}
		else
		{
			m_doubleRingBlocker.gameObject.SetActive(value: false);
		}
		m_multiplierBlocker.Blocked = false;
		if (m_tokenBlocker != null)
		{
			m_tokenBlocker.Blocked = false;
		}
		if (GCState.IsCurrentChallengeActive() && true && !GC3Progress.ChallengeFullycompleted())
		{
			m_gcTokenButtonAnimation.SendMessage("OnClick", SendMessageOptions.RequireReceiver);
			GameObject gameObject = GameObject.FindGameObjectWithTag("ResultsScreen_GCTokenLoopAnimation");
			m_gcTokenButtonLoopAnimation = gameObject.GetComponent<Animation>();
			if (!GC3Progress.PageVisitedFromResult)
			{
				m_gcTokenButtonLoopAnimation.enabled = true;
				GC3ButtonAnimationDisplayer.m_keepActive = true;
			}
			Debug.Log("ScoreCount Finished : PRE FirstVisitCheck");
			if (GCDialogManager.ShouldShowFirstTimeVisitDialog())
			{
				GCDialogManager.ShowFirstTimeVisitDialog();
			}
			else if (GC3Progress.IsRewardDue())
			{
				Dialog_GCPendingReward.Display();
			}
		}
	}

	public override void ActiveTransitionFinished(MonoBehaviour transitioningObject)
	{
		State state = (State)0;
		if (transitioningObject.name == "Score Group GC" || transitioningObject.name == "Score Group")
		{
			state = State.DisplayVisible;
		}
		else if (transitioningObject.name == "Ring Counter (parent)")
		{
			state = State.TotalsVisible;
		}
		if (state != 0)
		{
			if ((m_state & state) == state)
			{
				m_state &= ~state;
			}
			else
			{
				m_state |= state;
			}
		}
	}

	private bool DisplayInitialise(float timeDelta)
	{
		if (GCState.IsCurrentChallengeActive() && true && !GC3Progress.ChallengeFullycompleted())
		{
			m_triggerScore = GameObject.Find("Score Panel GC (Show) [Trigger]");
			m_gcTokenStampAnimation = GameObject.Find("Score GC Tokens Animation Stamp (Show) [Trigger]");
			m_gcTokenStampAnimationReset = GameObject.Find("Score GC Tokens Animation Stamp (Reset) [Trigger]");
			m_gcTokenButtonAnimation = GameObject.Find("Score GC Tokens Animations Stamp Button (Show) [Trigger]");
			m_gcTokenButtonAnimationReset = GameObject.Find("Score GC Tokens Animations Stamp Button (Reset) [Trigger]");
			GC3ButtonAnimationDisplayer.m_keepActive = false;
			m_triggerBoosterAnimation = GameObject.Find("Score Bonus Booster Animation GC (Show) [Trigger]");
			m_triggerBoosterAnimationReset = GameObject.Find("Score Bonus Booster Animation GC (Reset) [Trigger]");
			m_triggerHighScore = GameObject.Find("High Score GC (Show) [Trigger]");
			m_triggerHighScoreReset = GameObject.Find("High Score GC (Reset) [Trigger]");
			m_triggerDoubleRing = GameObject.Find("Double Rings GC (Show) [Trigger]");
			m_triggerDoubleRingReset = GameObject.Find("Double Rings GC (Reset) [Trigger]");
			m_triggerDoubleRingButton = GameObject.Find("Double Rings Button GC (Show) [Trigger]");
			m_triggerDoubleRingButtonReset = GameObject.Find("Double Rings Button GC (Reset) [Trigger]");
			m_triggerHighlightRings = GameObject.Find("Highlight Rings GC (Show) [Trigger]");
			m_triggerHighlightRingsReset = GameObject.Find("Highlight Rings GC (Reset) [Trigger]");
		}
		else
		{
			m_triggerScore = GameObject.Find("Score Panel (Show) [Trigger]");
			m_triggerBoosterAnimation = GameObject.Find("Score Bonus Booster Animation (Show) [Trigger]");
			m_triggerBoosterAnimationReset = GameObject.Find("Score Bonus Booster Animation (Reset) [Trigger]");
			m_triggerHighScore = GameObject.Find("High Score (Show) [Trigger]");
			m_triggerHighScoreReset = GameObject.Find("High Score (Reset) [Trigger]");
			m_triggerDoubleRing = GameObject.Find("Double Rings (Show) [Trigger]");
			m_triggerDoubleRingReset = GameObject.Find("Double Rings (Reset) [Trigger]");
			m_triggerDoubleRingButton = GameObject.Find("Double Rings Button (Show) [Trigger]");
			m_triggerDoubleRingButtonReset = GameObject.Find("Double Rings Button (Reset) [Trigger]");
			m_triggerHighlightRings = GameObject.Find("Highlight Rings (Show) [Trigger]");
			m_triggerHighlightRingsReset = GameObject.Find("Highlight Rings (Reset) [Trigger]");
		}
		if (m_triggerRingCount == null)
		{
			m_triggerRingCount = GameObject.Find("Ring Count (Show) [Trigger]");
		}
		if (m_bankRingsAudioTrigger == null)
		{
			m_bankRingsAudioTrigger = GameObject.Find("Bank Rings (Audio) [Trigger]");
		}
		if (PowerUpsInventory.GetPowerUpCount(PowerUps.Type.DoubleRing) > 0)
		{
			m_state |= State.DoubleRing;
		}
		else
		{
			m_state &= ~State.DoubleRing;
		}
		SetStateDelegates(DisplayTrigger, null);
		return false;
	}

	private bool DisplayTrigger(float timeDelta)
	{
		m_triggerScore.SendMessage("OnClick", SendMessageOptions.RequireReceiver);
		SetStateDelegates(DisplayCacheContent, null);
		Audio.PlayClip(m_audioPanelShown, loop: false);
		return false;
	}

	private bool DisplayCacheContent(float timeDelta)
	{
		GameObject source;
		if (GCState.IsCurrentChallengeActive() && true && !GC3Progress.ChallengeFullycompleted())
		{
			source = GameObject.Find("Score Group GC");
			GameObject gameObject = GameObject.FindGameObjectWithTag("ResultsScreen_GCTokenLabel");
			m_GCTokensValue = gameObject.GetComponent(typeof(UILabel)) as UILabel;
		}
		else
		{
			source = GameObject.Find("Score Group");
		}
		GameObject gameObject2 = Utils.FindTagInChildren(source, "ResultsScreen_BoosterBonusScore");
		m_boosterBonusScoreLabel = gameObject2.GetComponent(typeof(UILabel)) as UILabel;
		GameObject gameObject3 = Utils.FindTagInChildren(source, "ResultsScreen_Score");
		m_scoreLabel = gameObject3.GetComponent(typeof(UILabel)) as UILabel;
		GameObject gameObject4 = Utils.FindTagInChildren(source, "ResultsScreen_Ring");
		m_ringLabel = gameObject4.GetComponent(typeof(UILabel)) as UILabel;
		GameObject gameObject5 = Utils.FindTagInChildren(source, "ResultsScreen_Multiplier");
		m_multiplierLabel = gameObject5.GetComponent(typeof(UILabel)) as UILabel;
		GameObject gameObject6 = Utils.FindTagInChildren(source, "ResultsScreen_DoubleRing");
		m_doubleRingBlocker = gameObject6.GetComponent(typeof(GuiButtonBlocker)) as GuiButtonBlocker;
		GameObject gameObject7 = Utils.FindTagInChildren(source, "ResultsScreen_MultiplerButton");
		m_multiplierBlocker = gameObject7.GetComponent(typeof(GuiButtonBlocker)) as GuiButtonBlocker;
		GameObject gameObject8 = Utils.FindTagInChildren(source, "ResultsScreen_TokenButton");
		if (gameObject8 != null)
		{
			m_tokenBlocker = gameObject8.GetComponent(typeof(GuiButtonBlocker)) as GuiButtonBlocker;
		}
		else
		{
			m_tokenBlocker = null;
		}
		m_scoreLabel.text = "0";
		if (m_boosterBonusScoreLabel != null)
		{
			m_boosterBonusScoreLabel.text = "0";
		}
		if (m_GCTokensValue != null)
		{
			m_GCTokensValue.text = string.Empty;
		}
		m_scoreIncreaseRate = (float)ScoreTracker.CurrentScore / 1f;
		m_currentScoreDisplay = 0f;
		m_currentScoreIncrease = 0f;
		m_currentBonusScoreDisplay = 0f;
		if (ScoreTracker.CurrentScore >= ScoreTracker.HighScore)
		{
			m_targetHighScore = ScoreTracker.PreviousHighScore;
		}
		else
		{
			m_targetHighScore = ScoreTracker.HighScore;
		}
		m_multiplierLabel.text = ScoreTracker.RunMultiplier.ToString();
		m_doubleRingBlocker.Blocked = true;
		m_multiplierBlocker.Blocked = true;
		if (m_tokenBlocker != null)
		{
			m_tokenBlocker.Blocked = true;
		}
		m_ringIncreaseRate = (float)RingStorage.RunBankedRings / 1f;
		if ((m_state & State.DoubleRing) == State.DoubleRing)
		{
			m_currentRingsDisplay = RingStorage.RunBankedRings / 2;
		}
		else
		{
			m_currentRingsDisplay = RingStorage.RunBankedRings;
		}
		m_ringLabel.text = LanguageUtils.FormatNumber((int)m_currentRingsDisplay);
		m_currentRingsIncrease = 0f;
		SetStateDelegates(DisplayResetContent, null);
		return false;
	}

	private bool DisplayResetContent(float timeDelta)
	{
		m_triggerDoubleRingReset.SendMessage("OnClick", SendMessageOptions.RequireReceiver);
		if (PowerUpsInventory.GetPowerUpCount(PowerUps.Type.DoubleRing) == 0 && m_triggerDoubleRingButtonReset != null)
		{
			m_triggerDoubleRingButtonReset.SendMessage("OnClick", SendMessageOptions.RequireReceiver);
		}
		else if (m_doubleRingBlocker.gameObject != null)
		{
			m_doubleRingBlocker.gameObject.SetActive(value: false);
		}
		m_triggerHighScoreReset.SendMessage("OnClick", SendMessageOptions.RequireReceiver);
		m_triggerHighlightRingsReset.SendMessage("OnClick", SendMessageOptions.RequireReceiver);
		m_triggerBoosterAnimationReset.SendMessage("OnClick", SendMessageOptions.RequireReceiver);
		if (m_gcTokenStampAnimationReset != null)
		{
			m_gcTokenStampAnimationReset.SendMessage("OnClick", SendMessageOptions.RequireReceiver);
		}
		if (m_gcTokenButtonAnimationReset != null)
		{
			m_gcTokenButtonAnimationReset.SendMessage("OnClick", SendMessageOptions.RequireReceiver);
		}
		SetStateDelegates(DisplayWaitUntilVisible, null);
		return false;
	}

	private bool DisplayWaitUntilVisible(float timeDelta)
	{
		if ((m_state & State.DisplayVisible) == State.DisplayVisible)
		{
			SetStateDelegates(base.DelayUpdate, ScoreCountCount);
		}
		return false;
	}

	private bool ScoreCountCount(float timeDelta)
	{
		if ((m_state & State.BoosterBonusVisible) == State.BoosterBonusVisible && m_pauseTimeCount < m_pauseTimeValue)
		{
			m_pauseTimeCount += timeDelta;
			return false;
		}
		m_currentScoreIncrease += m_scoreIncreaseRate * timeDelta;
		if (m_currentScoreIncrease > 1f)
		{
			float num = Mathf.Floor(m_currentScoreIncrease);
			m_currentScoreDisplay += num;
			if (Boosters.IsBoosterSelected(PowerUps.Type.Booster_ScoreMultiplier) && m_currentScoreDisplay > (float)(ScoreTracker.CurrentScore - ScoreTracker.CurrentBoosterBonusScore))
			{
				m_currentBonusScoreDisplay += num;
			}
			m_currentScoreIncrease -= num;
			Audio.PlayClip(m_audioCountingScore, loop: false);
		}
		if (m_currentScoreDisplay >= (float)ScoreTracker.CurrentScore)
		{
			m_currentScoreDisplay = ScoreTracker.CurrentScore;
			m_currentBonusScoreDisplay = ScoreTracker.CurrentBoosterBonusScore;
			if (m_gcTokenStampAnimation != null)
			{
				m_gcTokenStampAnimation.SendMessage("OnClick", SendMessageOptions.RequireReceiver);
				m_GCTokensValue.text = "+ " + GC3Progress.CollectedThisRun();
			}
			if ((m_state & State.DoubleRing) == State.DoubleRing && RingStorage.RunBankedRings > 0)
			{
				SetStateDelegates(base.DelayUpdate, RingShowDouble);
			}
			else
			{
				SetStateDelegates(base.DelayUpdate, RingShowTotals);
			}
		}
		if ((m_state & State.BoosterBonusVisible) != State.BoosterBonusVisible && m_currentBonusScoreDisplay > 0f)
		{
			m_state |= State.BoosterBonusVisible;
			m_triggerBoosterAnimation.SendMessage("OnClick", SendMessageOptions.RequireReceiver);
			Audio.PlayClip(m_audioBoosterScoreBonus, loop: false);
			m_pauseTimeCount = 0f;
		}
		if ((m_state & State.HighScoreVisible) != State.HighScoreVisible && m_targetHighScore != 0f && m_currentScoreDisplay > m_targetHighScore)
		{
			m_state |= State.HighScoreVisible;
			m_triggerHighScore.SendMessage("OnClick", SendMessageOptions.RequireReceiver);
			Audio.PlayClip(m_audioHighScoreAchieved, loop: false);
			m_highScoreVisible = true;
		}
		int num2 = (int)Math.Floor(m_currentScoreDisplay);
		m_scoreLabel.text = LanguageUtils.FormatNumber(num2);
		if (m_boosterBonusScoreLabel != null)
		{
			int num3 = (int)Math.Floor(m_currentBonusScoreDisplay);
			m_boosterBonusScoreLabel.text = LanguageUtils.FormatNumber(num3);
		}
		return false;
	}

	private bool RingShowDouble(float timeDelta)
	{
		m_triggerDoubleRing.SendMessage("OnClick", SendMessageOptions.RequireReceiver);
		SetStateDelegates(RingDoubleRings, null);
		Audio.PlayClip(m_audioDoubleRingsShown, loop: false);
		return false;
	}

	private bool RingDoubleRings(float timeDelta)
	{
		m_triggerHighlightRings.SendMessage("OnClick", SendMessageOptions.RequireReceiver);
		SetDelayTime(1f);
		SetStateDelegates(base.DelayUpdate, RingShowTotals);
		m_currentRingsDisplay = RingStorage.RunBankedRings;
		m_ringLabel.text = LanguageUtils.FormatNumber((int)m_currentRingsDisplay);
		return false;
	}

	private bool RingShowTotals(float timeDelta)
	{
		m_triggerRingCount.SendMessage("OnClick", SendMessageOptions.RequireReceiver);
		SetStateDelegates(RingsCacheContentDisplay, null);
		Audio.PlayClip(m_audioPanelShown, loop: false);
		return false;
	}

	private bool RingsCacheContentDisplay(float timeDelta)
	{
		if (m_bankedLabel == null)
		{
			GameObject gameObject = GameObject.FindGameObjectWithTag("ResultsScreen_Banked");
			m_bankedLabel = gameObject.GetComponent(typeof(UILabel)) as UILabel;
		}
		if (m_buyRingsBlocker == null)
		{
			GameObject gameObject2 = GameObject.FindGameObjectWithTag("ResultsScreen_RingsButton");
			m_buyRingsBlocker = gameObject2.GetComponent(typeof(GuiButtonBlocker)) as GuiButtonBlocker;
		}
		m_currentBankedDisplay = RingStorage.TotalBankedRings - RingStorage.RunBankedRings;
		m_bankedLabel.text = LanguageUtils.FormatNumber((int)m_currentBankedDisplay);
		RingCountDisplay component = m_bankedLabel.GetComponent<RingCountDisplay>();
		component.enabled = false;
		m_buyRingsBlocker.Blocked = true;
		m_buyRingsBlocker.gameObject.SetActive(value: false);
		SetStateDelegates(RingWaitUntilShownTotals, null);
		return false;
	}

	private bool RingWaitUntilShownTotals(float timeDelta)
	{
		if ((m_state & State.TotalsVisible) == State.TotalsVisible)
		{
			if (RingStorage.RunBankedRings > 0)
			{
				SetDelayTime(1f);
				SetStateDelegates(base.DelayUpdate, RingCount);
			}
			else
			{
				SetStateDelegates(base.DelayUpdate, DisplayFinished);
				RingCountDisplay component = m_bankedLabel.GetComponent<RingCountDisplay>();
				component.enabled = true;
			}
		}
		return false;
	}

	private bool RingCount(float timeDelta)
	{
		m_currentRingsIncrease += m_ringIncreaseRate * timeDelta;
		if (m_currentRingsIncrease >= 1f)
		{
			float num = Mathf.Floor(m_currentRingsIncrease);
			m_currentRingsDisplay -= num;
			m_currentBankedDisplay += num;
			Audio.PlayClip(m_audioCountingRings, loop: false);
			m_currentRingsIncrease -= num;
		}
		if (m_currentRingsDisplay <= 0f || m_currentBankedDisplay >= (float)RingStorage.TotalBankedRings)
		{
			m_currentRingsDisplay = 0f;
			m_currentBankedDisplay = RingStorage.TotalBankedRings;
			SetStateDelegates(base.DelayUpdate, DisplayFinished);
			m_bankRingsAudioTrigger.SendMessage("OnClick", SendMessageOptions.RequireReceiver);
			RingCountDisplay component = m_bankedLabel.GetComponent<RingCountDisplay>();
			component.enabled = true;
		}
		int num2 = (int)Math.Floor(m_currentRingsDisplay);
		m_ringLabel.text = LanguageUtils.FormatNumber(num2);
		int num3 = (int)Math.Ceiling(m_currentBankedDisplay);
		m_bankedLabel.text = LanguageUtils.FormatNumber(num3);
		return false;
	}

	private bool DisplayFinished(float timeDelta)
	{
		PlayerStats.UpdateFinalScore((long)m_currentScoreDisplay);
		return true;
	}
}
