using System;
using UnityEngine;

public class Boosters : MonoBehaviour
{
	public const int NumberOfBoosters = 5;

	public const int BoostersSlots = 3;

	public const int None = -1;

	private const string BoosterTypeStart = "Booster_";

	private const int GoldenEnemyArraySize = 100;

	private static Boosters s_singleton;

	private static int[] s_selectedBoosters = new int[3] { -1, -1, -1 };

	private bool[] m_goldenEnemies = new bool[100];

	private int m_goldenEnemyCounter;

	[SerializeField]
	private int m_springBonusScore = 500;

	[SerializeField]
	private Mesh m_meshBooster_SpringBonus;

	[SerializeField]
	private float m_enemyComboScoreMultiplier = 2f;

	[SerializeField]
	private Mesh m_meshBooster_EnemyComboBonus;

	[SerializeField]
	private int m_ringStreakBonusScore = 500;

	[SerializeField]
	private Mesh m_meshBooster_RingStreakBonus;

	[SerializeField]
	private float m_ScoreMultiplier = 2f;

	[SerializeField]
	private Mesh m_meshBooster_ScoreMultiplier;

	[SerializeField]
	private float m_goldenEnemyScoreMultipler = 2f;

	[SerializeField]
	private int m_goldenEnemyPercentage = 20;

	[SerializeField]
	private Mesh m_meshBooster_GoldenEnemy;

	public static uint SpringBonusScore => (uint)s_singleton.m_springBonusScore;

	public static float GoldenEnemyScoreMultipler => s_singleton.m_goldenEnemyScoreMultipler;

	public static float EnemyComboScoreMultiplier => s_singleton.m_enemyComboScoreMultiplier;

	public static float ScoreMultiplier => s_singleton.m_ScoreMultiplier;

	public static uint RingStreakBonusScore => (uint)s_singleton.m_ringStreakBonusScore;

	public static bool IsNextEnemyGolden => s_singleton.GetNextEnemyIsGolden();

	public static int[] GetBoostersSelected => s_selectedBoosters;

	public static int SelectBooster(PowerUps.Type booster)
	{
		if (!booster.ToString().StartsWith("Booster_"))
		{
			return -1;
		}
		if (PowerUpsInventory.GetPowerUpCount(booster) < 1)
		{
			return -1;
		}
		int emptySlot = GetEmptySlot();
		if (emptySlot != -1)
		{
			PowerUpsInventory.ModifyPowerUpStock(booster, -1);
			s_selectedBoosters[emptySlot] = (int)booster;
		}
		return emptySlot;
	}

	public static void ClearSelected()
	{
		for (int i = 0; i < 3; i++)
		{
			s_selectedBoosters[i] = -1;
		}
	}

	public static bool IsBoosterSelected(PowerUps.Type booster)
	{
		for (int i = 0; i < 3; i++)
		{
			if (s_selectedBoosters[i] == (int)booster)
			{
				return true;
			}
		}
		return false;
	}

	private static int GetEmptySlot()
	{
		for (int i = 0; i < 3; i++)
		{
			if (s_selectedBoosters[i] < 0)
			{
				return i;
			}
		}
		return -1;
	}

	private bool GetNextEnemyIsGolden()
	{
		if (!IsBoosterSelected(PowerUps.Type.Booster_GoldenEnemy))
		{
			return false;
		}
		m_goldenEnemyCounter++;
		return m_goldenEnemies[m_goldenEnemyCounter % 100];
	}

	private void ResetGoldenEnemies()
	{
		for (int i = 0; i < 100; i++)
		{
			m_goldenEnemies[i] = false;
		}
		int num = 3;
		for (int j = 0; j < Math.Min(100, m_goldenEnemyPercentage); j++)
		{
			m_goldenEnemies[num] = true;
			num = (num + 29) % 100;
			while (m_goldenEnemies[num])
			{
				num = (num + 3) % 100;
			}
		}
	}

	private void Start()
	{
		s_singleton = this;
		EventDispatch.RegisterInterest("OnGameFinished", this);
		ResetGoldenEnemies();
	}

	private void Event_OnGameFinished()
	{
		ResetGoldenEnemies();
	}
}
