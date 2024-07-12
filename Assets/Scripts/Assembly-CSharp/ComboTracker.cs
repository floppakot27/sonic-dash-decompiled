using UnityEngine;

public class ComboTracker : MonoBehaviour
{
	private static uint s_currentRollCombo;

	private static uint s_currentHomingCombo;

	private static uint s_currentBossCombo;

	public static uint Current
	{
		get
		{
			uint num = ((s_currentRollCombo == 0) ? s_currentHomingCombo : s_currentRollCombo);
			return (s_currentBossCombo == 0) ? num : s_currentBossCombo;
		}
	}

	private void Start()
	{
		EventDispatch.RegisterInterest("OnBossHit", this, EventDispatch.Priority.Highest);
		EventDispatch.RegisterInterest("OnEnemyKilled", this, EventDispatch.Priority.Highest);
		EventDispatch.RegisterInterest("OnNewGameStarted", this);
		EventDispatch.RegisterInterest("OnBossBattleEnd", this);
		EventDispatch.RegisterInterest("OnSonicAttackEnd", this);
		EventDispatch.RegisterInterest("ExitMotionRollState", this);
	}

	private void Event_OnBossHit(int score)
	{
		s_currentBossCombo++;
	}

	private void Event_OnBossBattleEnd()
	{
		s_currentBossCombo = 0u;
	}

	private void Event_OnEnemyKilled(Enemy enemy, Enemy.Kill killType)
	{
		switch (killType)
		{
		case Enemy.Kill.Homing:
			s_currentHomingCombo++;
			break;
		case Enemy.Kill.Rolling:
			s_currentRollCombo++;
			break;
		}
	}

	private void Event_OnNewGameStarted()
	{
		s_currentRollCombo = 0u;
		s_currentHomingCombo = 0u;
		s_currentBossCombo = 0u;
	}

	private void Event_OnSonicAttackEnd()
	{
		if (Current > 1)
		{
			PlayerStats.IncreaseStat(PlayerStats.StatNames.EnemyStreaks_Total, 1);
			PlayerStats.IncreaseStat(PlayerStats.StatNames.EnemyStreaks_Run, 1);
		}
		s_currentRollCombo = 0u;
		s_currentHomingCombo = 0u;
		s_currentBossCombo = 0u;
	}

	private void Event_ExitMotionRollState()
	{
		if (Current > 1)
		{
			PlayerStats.IncreaseStat(PlayerStats.StatNames.EnemyStreaks_Total, 1);
			PlayerStats.IncreaseStat(PlayerStats.StatNames.EnemyStreaks_Run, 1);
		}
		s_currentRollCombo = 0u;
		s_currentHomingCombo = 0u;
		s_currentBossCombo = 0u;
	}
}
