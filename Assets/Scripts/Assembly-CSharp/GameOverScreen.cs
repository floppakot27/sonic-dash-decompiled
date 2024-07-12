using System;
using System.Collections;
using UnityEngine;

public class GameOverScreen : MonoBehaviour
{
	[Flags]
	private enum State
	{
		Finished = 1
	}

	private const float DefaultSpeedfactor = 3f;

	private const int ScoreCountingIndex = 3;

	private State m_state;

	private float m_transitionMultiplier = 3f;

	private bool m_requiresGameOverProcess = true;

	private GameOver_Component[] m_components = new GameOver_Component[7]
	{
		new GameOver_Rewards(),
		new GameOver_DailyChallenge(),
		new GameOver_Missions(),
		new GameOver_ScoreCount(),
		new GameOver_Hint(),
		new GameOver_Leaderboards(),
		new GameOver_Buttons()
	};

	[SerializeField]
	private AudioClip m_audioCountingScore;

	[SerializeField]
	private AudioClip m_audioCountingRings;

	[SerializeField]
	private AudioClip m_audioHighScoreAchieved;

	[SerializeField]
	private AudioClip m_audioBoosterScoreBonus;

	[SerializeField]
	private AudioClip m_audioDoubleRingsShown;

	[SerializeField]
	private AudioClip m_audioPanelShown;

	[SerializeField]
	private float m_hintDisplayTime = 3f;

	[SerializeField]
	private float m_speedUpFactor = 3f;

	[SerializeField]
	private OfferRegion_Timed m_offerResultsEnd;

	public static bool EndGameActiveAndFinished { get; private set; }

	public void Start()
	{
		EventDispatch.RegisterInterest("OnNewGameStarted", this);
		EventDispatch.RegisterInterest("OnDialogHidden", this);
		GameOver_ScoreCount.SetAudioProperties(m_audioCountingScore, m_audioCountingRings, m_audioHighScoreAchieved, m_audioBoosterScoreBonus, m_audioDoubleRingsShown, m_audioPanelShown);
		GameOver_Leaderboards.SetAudioProperties(m_audioPanelShown);
		GameOver_Buttons.SetAudioProperties(m_audioPanelShown);
		GameOver_Hint.SetDisplayTime(m_hintDisplayTime);
	}

	private void OnEnable()
	{
		m_transitionMultiplier = 3f;
		UpdateTweenPositionSpeed(m_transitionMultiplier);
		if (m_requiresGameOverProcess)
		{
			m_state &= ~State.Finished;
			StartCoroutine(RunGameOverFlow());
			m_requiresGameOverProcess = false;
			return;
		}
		GameOver_Component[] components = m_components;
		foreach (GameOver_Component gameOver_Component in components)
		{
			gameOver_Component.Show();
		}
		m_state |= State.Finished;
		EndGameActiveAndFinished = true;
	}

	private void OnDisable()
	{
		EndGameActiveAndFinished = false;
		GameOver_Component[] components = m_components;
		foreach (GameOver_Component gameOver_Component in components)
		{
			gameOver_Component.Hide();
		}
	}

	private IEnumerator RunGameOverFlow()
	{
		yield return null;
		for (int i = 0; i < m_components.Length; i++)
		{
			m_components[i].Reset();
			if (i == 3)
			{
				m_components[i + 1].Reset();
			}
			bool sectionActive = false;
			do
			{
				float timeDelta = IndependantTimeDelta.Delta * m_transitionMultiplier;
				sectionActive = m_components[i].Update(timeDelta);
				if (i == 3)
				{
					sectionActive = m_components[i + 1].Update(timeDelta) && sectionActive;
				}
				yield return null;
			}
			while (!sectionActive);
			if (i == 3)
			{
				i++;
			}
		}
		GameOver_Component[] components = m_components;
		foreach (GameOver_Component thisComponent in components)
		{
			thisComponent.ProcessFinished();
		}
		m_offerResultsEnd.Visit();
		m_transitionMultiplier = 3f;
		UpdateTweenPositionSpeed(m_transitionMultiplier);
		PropertyStore.Save();
		CloudStorage.Sync();
		m_state |= State.Finished;
		EndGameActiveAndFinished = true;
		if (OfferState.CanDisplay())
		{
			OfferState.RegisterDisplay();
		}
	}

	private void UpdateTweenPositionSpeed(float transitionSpeed)
	{
		float num = 0.5f;
		float num2 = 1f / transitionSpeed;
		UIPanel componentInParent = Utils.GetComponentInParent<UIPanel>(base.gameObject);
		UITweener[] componentsInChildren = componentInParent.GetComponentsInChildren<UITweener>(includeInactive: true);
		UITweener[] array = componentsInChildren;
		foreach (UITweener uITweener in array)
		{
			if (uITweener.tweenFactor == 1f || uITweener.tweenFactor == 0f)
			{
				uITweener.duration = num * num2;
			}
		}
	}

	private void Trigger_TransitionFinished(UITweener tween)
	{
		GameOver_Component[] components = m_components;
		foreach (GameOver_Component gameOver_Component in components)
		{
			gameOver_Component.TransitionFinished(tween);
		}
	}

	private void Trigger_BuyDoubleRings()
	{
		if (PowerUpsInventory.GetPowerUpCount(PowerUps.Type.DoubleRing) <= 0 && (m_state & State.Finished) == State.Finished)
		{
			DialogStack.ShowDialog("Buy Double Rings");
		}
	}

	private void Trigger_ComponentClosed()
	{
		GameOver_Component[] components = m_components;
		foreach (GameOver_Component gameOver_Component in components)
		{
			gameOver_Component.ComponentClosed();
		}
	}

	private void Trigger_SpeedUp()
	{
		m_transitionMultiplier = m_speedUpFactor;
		UpdateTweenPositionSpeed(m_transitionMultiplier);
	}

	private void Event_OnNewGameStarted()
	{
		m_requiresGameOverProcess = true;
	}

	private void Event_OnDialogHidden()
	{
		GameOver_Rewards.DialogsHidden();
	}
}
