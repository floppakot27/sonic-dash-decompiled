using System;

public class TrackEntity
{
	public enum Kind
	{
		Invalid = 0,
		Spikes = 1,
		Crabmeat = 2,
		Chopper = 4,
		Mine = 8,
		ShortWall = 0x10,
		MedWall = 0x20,
		TallWall = 0x40,
		SolidWall = 0x80,
		Ring = 0x100,
		SpikePit = 0x200,
		GapStart = 0x400,
		RandomPowerup = 0x800,
		Gap = 0x1000,
		Spring = 0x2000,
		DashPad = 0x4000,
		MagnetPowerup = 0x8000,
		ChallengePiece = 0x10000,
		ShieldPowerup = 0x20000,
		DistanceTV = 0x40000,
		RedStarRing = 0x80000,
		GCCollectable = 0x100000
	}

	public const uint GroundEnemy = 3u;

	public const uint Enemy = 7u;

	public const uint JumpOverEntity = 571u;

	public const uint RollUnderOrThroughEntity = 98u;

	public const uint AttackableEnemy = 6u;

	public const uint Wall = 240u;

	public const uint InanimateHazards = 760u;

	public const uint AllHazards = 767u;

	public const uint RollUnderEntity = 96u;

	public const uint Collectables = 1804544u;

	public const uint Powerup = 165888u;

	public Track.Lane Lane { get; private set; }

	public Kind InstanceKind { get; private set; }

	public virtual bool IsValid => true;

	public TrackEntity(Kind kind, Track.Lane lane)
	{
		Lane = lane;
		InstanceKind = kind;
	}

	public static string ToString(Kind kind)
	{
		return ToString((uint)kind);
	}

	public static string ToString(uint kindMask)
	{
		string text = "[";
		foreach (int value in Enum.GetValues(typeof(Kind)))
		{
			if (((uint)value & kindMask) != 0)
			{
				text = string.Concat(text, (Kind)value, "|");
			}
		}
		text = ((!(text == "[")) ? text.Trim('|') : (text + "empty"));
		return text + "]";
	}

	public static bool IsConcreteOfType(uint entityInstance, uint template)
	{
		bool flag = false;
		for (uint num = 1u; num != 0; num <<= 1)
		{
			uint num2 = template & num;
			uint num3 = entityInstance & num;
			if (num2 == 0 && num3 != 0)
			{
				return false;
			}
			if ((num2 & num3) != 0)
			{
				if (flag)
				{
					return false;
				}
				flag = true;
			}
		}
		return true;
	}

	public override string ToString()
	{
		return string.Concat("type ", InstanceKind, " in lane ", Lane);
	}
}
