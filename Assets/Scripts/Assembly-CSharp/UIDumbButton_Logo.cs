public class UIDumbButton_Logo : UIDumbButton
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
}
