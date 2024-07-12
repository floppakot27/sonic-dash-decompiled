using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[AddComponentMenu("Dash/Track/Spring TV")]
public class SpringTV : SpawnableObject
{
	public enum Destination
	{
		Grass,
		Temple,
		Beach
	}

	public enum Type
	{
		SetPiece,
		ChangeZone,
		Bank,
		DailyChallenge,
		Random,
		Boss
	}

	[Flags]
	public enum CreateFlags
	{
		None = 0,
		BossBattle = 1
	}

	public static readonly int DestinationCount = Utils.GetEnumCount<Destination>();

	public static readonly int TypeCount = Utils.GetEnumCount<Type>();

	[SerializeField]
	private float m_sideLaneOffset = 1.2f;

	[SerializeField]
	private Renderer m_tvScreen;

	[SerializeField]
	private AnimationCurve m_verticalBob;

	private Track.Lane m_currentLane;

	public Destination SpringDestination { get; private set; }

	public Type SpringType { get; private set; }

	public CreateFlags SpringCreateFlags { get; private set; }

	public override void Place(OnEvent onDestroy, Track onTrack, Spline onSpline)
	{
		base.Place(onDestroy, onTrack, onSpline);
		m_currentLane = onTrack.GetLaneOfSpline(onSpline);
	}

	public static Type AnyTypeExcept(Type typeToExclude, System.Random rng)
	{
		return AnyTypeExcept(new Type[1] { typeToExclude }, rng);
	}

	public static Type AnyTypeExcept(IEnumerable<Type> typesToExclude, System.Random rng)
	{
		List<Type> list = new List<Type>(TypeCount);
		for (int i = 0; i < TypeCount; i++)
		{
			if (!typesToExclude.Contains((Type)i))
			{
				list.Add((Type)i);
			}
		}
		return list[rng.Next(list.Count)];
	}

	public static Type AnyType(System.Random rng)
	{
		return (Type)rng.Next(TypeCount);
	}

	public static Destination AnyDestination(System.Random rng)
	{
		return (Destination)rng.Next(DestinationCount);
	}

	public static Type RandomType(System.Random rng)
	{
		return AnyTypeExcept(new Type[4]
		{
			Type.Random,
			Type.DailyChallenge,
			Type.Bank,
			Type.Boss
		}, rng);
	}

	public void PlaceOnSpring(Spring spring, System.Random rng, Type newSpringType, Destination destination, CreateFlags flags)
	{
		spring.SetTV(this);
		PlaceForLane(m_currentLane);
		SpringType = ((newSpringType != Type.Random) ? newSpringType : RandomType(rng));
		if (newSpringType == Type.Random && SpringType == Type.ChangeZone)
		{
			do
			{
				SpringDestination = AnyDestination(rng);
			}
			while (SpringDestination == destination);
		}
		else
		{
			SpringDestination = destination;
		}
		if ((flags & CreateFlags.BossBattle) == CreateFlags.BossBattle && SpringTypeValidForBossBattle())
		{
			SpringCreateFlags |= CreateFlags.BossBattle;
		}
		ShowTVType(newSpringType, SpringDestination);
		StartCoroutine(HoverBob());
	}

	private Type PickNonRandomType(System.Random rng)
	{
		int num = rng.Next(TypeCount - 1);
		if (num == 4)
		{
			num = (num + 1) % TypeCount;
		}
		return (Type)num;
	}

	private bool SpringTypeValidForBossBattle()
	{
		return SpringType == Type.Bank || SpringType == Type.ChangeZone;
	}

	private IEnumerator HoverBob()
	{
		float bobTimer = UnityEngine.Random.value * m_verticalBob.keys[m_verticalBob.length - 1].time;
		float bobSpeed = UnityEngine.Random.Range(0.85f, 1.15f);
		Vector3 origin = base.transform.localPosition;
		while (true)
		{
			bobTimer += Time.deltaTime * bobSpeed;
			base.transform.localPosition = origin + Vector3.up * m_verticalBob.Evaluate(bobTimer);
			yield return null;
		}
	}

	private void ShowTVType(Type typeToShow, Destination destination)
	{
		switch (typeToShow)
		{
		case Type.SetPiece:
			ShowTVType(0, 0);
			break;
		case Type.Random:
			ShowTVType(1, 0);
			break;
		case Type.Bank:
			ShowTVType(2, 0);
			break;
		case Type.DailyChallenge:
			ShowTVType(2, 1);
			break;
		case Type.Boss:
			ShowTVType(1, 0);
			break;
		case Type.ChangeZone:
			switch (destination)
			{
			case Destination.Temple:
				ShowTVType(0, 1);
				break;
			case Destination.Grass:
				ShowTVType(1, 1);
				break;
			case Destination.Beach:
				ShowTVType(0, 2);
				break;
			}
			break;
		}
	}

	private void ShowTVType(int colIndex, int rowIndex)
	{
		float y = 0.33333f - 0.33333f * (float)rowIndex;
		float x = 0.33333f * (float)colIndex;
		m_tvScreen.material.SetTextureOffset("_MainTex", new Vector2(x, y));
	}

	private void PlaceForLane(Track.Lane lane)
	{
		float num = lane switch
		{
			Track.Lane.Middle => 0f, 
			Track.Lane.Left => m_sideLaneOffset, 
			_ => 0f - m_sideLaneOffset, 
		};
		Vector3 localPosition = Vector3.right * num;
		base.transform.localPosition = localPosition;
	}
}
