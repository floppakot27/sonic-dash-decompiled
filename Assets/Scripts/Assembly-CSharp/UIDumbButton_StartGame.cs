public class UIDumbButton_StartGame : UIDumbButton
{
	private void Start()
	{
		EventDispatch.RegisterInterest("StartGameState", this);
	}

	private void Event_StartGameState(GameState.Mode state)
	{
		if (state == GameState.Mode.Menu)
		{
			base.Enabled = true;
		}
		else
		{
			base.Enabled = false;
		}
	}

	protected override void Event_ButtonSelected()
	{
		GameState.RequestMode(GameState.Mode.Game);
	}
}
