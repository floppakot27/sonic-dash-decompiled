using System;
using UnityEngine;

public abstract class Obstacle : Hazard
{
	public enum GameplayType
	{
		JumpOver = 1,
		RollUnderOrThrough
	}

	public static readonly int GameplayTypeCount = Utils.GetEnumCount<GameplayType>();

	[SerializeField]
	private int m_gameplayTypes;

	[SerializeField]
	private int m_numLanesOccupied = 1;

	[SerializeField]
	private int m_subzoneIndex = -1;

	public int SubzoneIndex => m_subzoneIndex;

	public bool WorksOnAnySubzone => SubzoneIndex < 0;

	public int NumLanesOccupied => m_numLanesOccupied;

	public override void Start()
	{
		base.Start();
	}

	public override void Place(OnEvent onDestroy, Track track, Spline spline)
	{
		base.Place(onDestroy, track, spline);
		Place(track, spline);
	}

	protected abstract void Place(Track track, Spline spline);

	public abstract Spline getSpline();

	public static bool IsEveryGameplayType(uint types)
	{
		foreach (int value in Enum.GetValues(typeof(GameplayType)))
		{
			if ((types & (uint)value) == 0)
			{
				return false;
			}
		}
		return true;
	}

	public bool IsExactlyGameplayType(uint types)
	{
		for (int i = 0; i < GameplayTypeCount; i++)
		{
			int num = 1 << i;
			if ((types & num) != (m_gameplayTypes & num))
			{
				return false;
			}
		}
		return true;
	}

	public bool SupportsGameplayType(uint types)
	{
		return IsExactlyGameplayType(types) || (m_gameplayTypes & types) > 0;
	}

	public bool SupportsGameplayType(GameplayType type)
	{
		return SupportsGameplayType((uint)type);
	}
}
