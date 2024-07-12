public class PowerUps
{
	public enum Type
	{
		Respawn,
		Magnet,
		HeadStart,
		RollBoost,
		IncreasedAttackRange,
		DashLength,
		DashIncrease,
		DoubleRing,
		SuperHeadStart,
		Shield,
		FreeRevive,
		Booster_SpringBonus,
		Booster_EnemyComboBonus,
		Booster_RingStreakBonus,
		Booster_ScoreMultiplier,
		Booster_GoldenEnemy
	}

	public const int NumberOfPowerUps = 16;

	public const int NumberOfUpgradables = 5;

	public const int UpgradeSlots = 7;

	public static int GetRingsForRingsPickup()
	{
		float current = RingPerMinute.Current;
		return (current < 50f) ? 100 : ((current < 100f) ? 50 : ((!(current < 130f)) ? 10 : 20));
	}

	public static void DoRingPowerupAction(int ringCount)
	{
		RingPickupMonitor.instance().PickupRings(ringCount);
		Sonic.RenderManager.playRingPickupParticles(ringCount);
	}

	public static bool CanPowerUpBeCollected(Type powerUp)
	{
		return powerUp != Type.RollBoost && powerUp != Type.DashIncrease && powerUp != Type.DashLength && powerUp != Type.IncreasedAttackRange;
	}

	public static bool CanPowerUpBeUpgraded(Type powerUp)
	{
		return powerUp != Type.DoubleRing && powerUp != Type.FreeRevive && powerUp != 0 && powerUp != Type.SuperHeadStart && powerUp != Type.Booster_SpringBonus && powerUp != Type.Booster_ScoreMultiplier && powerUp != Type.Booster_GoldenEnemy && powerUp != Type.Booster_EnemyComboBonus && powerUp != Type.Booster_RingStreakBonus;
	}

	public static bool CanPowerUpBeHinted(Type powerUp)
	{
		return powerUp != Type.DoubleRing && powerUp != Type.FreeRevive && powerUp != 0 && powerUp != Type.SuperHeadStart && powerUp != Type.IncreasedAttackRange && powerUp != Type.RollBoost && powerUp != Type.Booster_SpringBonus && powerUp != Type.Booster_ScoreMultiplier && powerUp != Type.Booster_GoldenEnemy && powerUp != Type.Booster_EnemyComboBonus && powerUp != Type.Booster_RingStreakBonus;
	}
}
