using UnityEngine;

public class MotionDeathState : MotionState
{
	public override void Enter()
	{
	}

	public override bool IsDead()
	{
		return true;
	}

	protected void DoDeath(bool bAllowRespawn, MotionStateMachine stateMachine)
	{
		if (GameState.IsAvailable)
		{
			if (TutorialSystem.instance().isTrackTutorialEnabled())
			{
				float previousDialogStartPosition = TutorialSystem.instance().getPreviousDialogStartPosition();
				stateMachine.RequestState(new MotionRewindState(previousDialogStartPosition));
			}
			else
			{
				GameState.RequestGameOver(bAllowRespawn);
			}
		}
		else
		{
			Application.LoadLevel(Application.loadedLevel);
		}
	}
}
