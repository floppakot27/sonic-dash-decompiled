using System;
using System.Collections.Generic;
using UnityEngine;

public class PowerUpsInventory : MonoBehaviour
{
	private static int[] s_powerUpCount = new int[16]
	{
		1, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0
	};

	private static int[] s_powerUpUpgrades = new int[16];

	public static int GetPowerUpCount(PowerUps.Type powerUp)
	{
		bool flag = PowerUps.CanPowerUpBeCollected(powerUp);
		return s_powerUpCount[(int)powerUp];
	}

	public static int GetPowerUpLevel(PowerUps.Type powerUp)
	{
		bool flag = PowerUps.CanPowerUpBeUpgraded(powerUp);
		return s_powerUpUpgrades[(int)powerUp];
	}

	public static void ModifyPowerUpStock(PowerUps.Type powerUp, int modifyValue)
	{
		if (PowerUps.CanPowerUpBeCollected(powerUp))
		{
			s_powerUpCount[(int)powerUp] += modifyValue;
			if (s_powerUpCount[(int)powerUp] < 0)
			{
				s_powerUpCount[(int)powerUp] = 0;
			}
			EventDispatch.GenerateEvent("PowerUpCountChanged", powerUp);
		}
	}

	public static string GetSaveProperty(PowerUps.Type powerup)
	{
		return $"{s_powerUpCount[(int)powerup]},{s_powerUpUpgrades[(int)powerup]}";
	}

	public static void ModifyPowerUpLevel(PowerUps.Type powerUp, uint increase)
	{
		if (!PowerUps.CanPowerUpBeUpgraded(powerUp))
		{
			return;
		}
		if (s_powerUpUpgrades[(int)powerUp] < 6)
		{
			s_powerUpUpgrades[(int)powerUp] += (int)increase;
			if (s_powerUpUpgrades[(int)powerUp] > 6)
			{
				s_powerUpUpgrades[(int)powerUp] = 6;
			}
			EventDispatch.GenerateEvent("PowerUpLeveledUp", powerUp);
		}
	}

	public static List<PowerUps.Type> GetLowestLevelHintablePowerUps(out int min)
	{
		min = 7;
		for (int i = 0; i < Utils.GetEnumCount<PowerUps.Type>(); i++)
		{
			if (PowerUps.CanPowerUpBeHinted((PowerUps.Type)i) && s_powerUpUpgrades[i] < min)
			{
				min = s_powerUpUpgrades[i];
			}
		}
		List<PowerUps.Type> list = new List<PowerUps.Type>();
		for (int j = 0; j < Utils.GetEnumCount<PowerUps.Type>(); j++)
		{
			if (PowerUps.CanPowerUpBeHinted((PowerUps.Type)j) && s_powerUpUpgrades[j] == min)
			{
				list.Add((PowerUps.Type)j);
			}
		}
		return list;
	}

	private void Start()
	{
		EventDispatch.RegisterInterest("OnGameDataSaveRequest", this);
		EventDispatch.RegisterInterest("OnGameDataLoaded", this);
	}

	private void Event_OnGameDataSaveRequest()
	{
		for (int i = 0; i < 16; i++)
		{
			PowerUps.Type type = (PowerUps.Type)i;
			PropertyStore.Store(type.ToString(), GetSaveProperty(type));
		}
	}

	private void Event_OnGameDataLoaded(ActiveProperties activeProperties)
	{
		for (int i = 0; i < 16; i++)
		{
			PowerUps.Type type = (PowerUps.Type)i;
			string @string = activeProperties.GetString(type.ToString());
			if (@string == null)
			{
				s_powerUpCount[i] = 0;
				s_powerUpUpgrades[i] = 0;
				continue;
			}
			string[] array = @string.Split(',');
			s_powerUpCount[i] = Convert.ToInt32(array[0]);
			s_powerUpUpgrades[i] = Convert.ToInt32(array[1]);
		}
	}
}
