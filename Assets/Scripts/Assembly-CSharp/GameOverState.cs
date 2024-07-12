public class GameOverState
{
	private GuiTrigger m_gameOverScreen;

	private GuiTrigger m_continueScreen;

	private int m_currentContinueCost;

	private int[] m_continueCost;

	public GameOverState(GuiTrigger gameOverScreen, GuiTrigger continueScreen, int[] continueCost)
	{
		m_gameOverScreen = gameOverScreen;
		m_continueScreen = continueScreen;
		m_continueCost = continueCost;
		EventDispatch.RegisterInterest("OnNewGameStarted", this);
		EventDispatch.RegisterInterest("OnContinueGameCancel", this);
		EventDispatch.RegisterInterest("OnContinueGameOk", this);
	}

	public void TriggerGameOver(bool bAllowRespawn)
	{
		if (!bAllowRespawn)
		{
			EndGame(instantEnd: true);
		}
		else
		{
			PromptToContinue();
		}
	}

	private void ContinueGame(bool freeRevive)
	{
		GameState.RequestMode(GameState.Mode.Game);
		Sonic.Tracker.Resurrect(freeRevive);
		if (!freeRevive)
		{
			int currentContinueCost = GetCurrentContinueCost();
			PowerUpsInventory.ModifyPowerUpStock(PowerUps.Type.Respawn, -currentContinueCost);
			PlayerStats.IncreaseStat(PlayerStats.StatNames.RevivesUsed_Total, currentContinueCost);
			PlayerStats.IncreaseStat(PlayerStats.StatNames.RevivesUsed_Session, currentContinueCost);
			PlayerStats.IncreaseStat(PlayerStats.StatNames.RevivesUsed_Run, currentContinueCost);
			m_currentContinueCost++;
		}
	}

	private void EndGame(bool instantEnd)
	{
		if (instantEnd)
		{
			MenuStack.RequestPage = m_gameOverScreen;
			GameState.RequestMode(GameState.Mode.PauseMenu);
		}
		else
		{
			MenuStack.MoveToPage(m_gameOverScreen, replaceCurrent: true);
		}
		EventDispatch.GenerateEvent("OnGameFinished");
		PropertyStore.Save();
	}

	private void PromptToContinue()
	{
		int revivesRequired = (MenuReviveScreen.RevivesRequired = GetCurrentContinueCost());
		MenuRevivePurchase.RevivesRequired = revivesRequired;
		MenuStack.RequestPage = m_continueScreen;
		GameState.RequestMode(GameState.Mode.PauseMenu);
	}

	private int GetCurrentContinueCost()
	{
		if (m_currentContinueCost >= m_continueCost.Length)
		{
			return m_continueCost[m_continueCost.Length - 1];
		}
		return m_continueCost[m_currentContinueCost];
	}

	private void Event_OnNewGameStarted()
	{
		m_currentContinueCost = 0;
	}

	private void Event_OnContinueGameCancel()
	{
		EndGame(instantEnd: false);
	}

	private void Event_OnContinueGameOk(bool freeRevive)
	{
		ContinueGame(freeRevive);
	}
}
