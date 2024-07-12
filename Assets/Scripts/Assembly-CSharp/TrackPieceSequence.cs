using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class TrackPieceSequence
{
	private enum ElevationType
	{
		Low,
		High,
		Hilly
	}

	[Flags]
	public enum SequenceFlags
	{
		None = 0,
		UseFirstSubzone = 1
	}

	public enum GenericPieceType
	{
		Straight,
		SetPiece,
		GapLeft,
		GapLeftEnd,
		GapRight,
		GapRightEnd,
		TrackCap,
		EmptyAir,
		GameStart
	}

	[Flags]
	public enum Flags
	{
		None = 0,
		NoElevationChanges = 2,
		ForceToLowElevation = 3
	}

	private struct Context
	{
		public TrackDatabase m_database;

		public Random m_rng;

		public TrackGenerationParameters m_generationParams;

		public uint m_excludedTemplates;
	}

	public enum SubzoneType
	{
		ForceToFirstSubzone,
		UseRandomSubzone
	}

	private enum ElevationChanges
	{
		FollowSequence,
		StayFlat,
		ForceToLow
	}

	private class SequenceLooper<T> : IEnumerator, IDisposable, IEnumerable, IEnumerable<T>, IEnumerator<T>
	{
		private IEnumerator<IEnumerable<T>> m_loopGenerator;

		private IEnumerator<T> m_currentRun;

		object IEnumerator.Current => Current;

		public T Current => m_currentRun.Current;

		public SequenceLooper(IEnumerator<IEnumerable<T>> sequenceToLoop)
		{
			m_loopGenerator = sequenceToLoop;
			m_loopGenerator.MoveNext();
			m_currentRun = m_loopGenerator.Current.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return this;
		}

		void IDisposable.Dispose()
		{
		}

		public IEnumerator<T> GetEnumerator()
		{
			return this;
		}

		public bool MoveNext()
		{
			if (!m_currentRun.MoveNext())
			{
				m_loopGenerator.MoveNext();
				m_currentRun = m_loopGenerator.Current.GetEnumerator();
				bool flag = m_currentRun.MoveNext();
			}
			return true;
		}

		public void Reset()
		{
			throw new NotSupportedException();
		}
	}

	private Context m_context = default(Context);

	private int m_previousElevation;

	private IEnumerator<int> m_currentElevationSequence;

	private IEnumerator<TrackDatabase.TrackPiece> m_corneringSequence;

	private GenericPieceType m_lastRequestedType = GenericPieceType.EmptyAir;

	private static int m_setpieceIndex;

	public TrackPieceSequence(Random rng, TrackDatabase database, TrackGenerationParameters generationParams, uint excludedTemplates)
	{
		m_context.m_database = database;
		m_context.m_rng = rng;
		m_context.m_generationParams = generationParams;
		m_context.m_excludedTemplates = excludedTemplates;
		m_currentElevationSequence = ElevationSequence();
		m_currentElevationSequence.MoveNext();
		m_corneringSequence = CorneringSequence();
	}

	private IEnumerator<IEnumerable<int>> ElevationSequenceGenerator()
	{
		ElevationType currentElevation = ElevationType.Low;
		TrackGenerationParameters generationParams = m_context.m_generationParams;
		bool allowElevation = (m_context.m_excludedTemplates & 4) == 0;
		while (true)
		{
			switch (currentElevation)
			{
			case ElevationType.Low:
				yield return FlatSequence(0, generationParams.LowElevationMinSegmentCount, generationParams.LowElevationMaxSegmentCount);
				if (allowElevation)
				{
					currentElevation = ((m_context.m_rng.NextDouble() < (double)generationParams.ChanceStayOnLowElevation) ? currentElevation : ((m_context.m_rng.NextDouble() < (double)generationParams.ChanceSwitchToHighElevation) ? ElevationType.High : ElevationType.Hilly));
				}
				break;
			case ElevationType.High:
				yield return FlatSequence(1, generationParams.HighElevationMinSegmentCount, generationParams.HighElevationMaxSegmentCount);
				currentElevation = ElevationType.Low;
				break;
			case ElevationType.Hilly:
				yield return HillySequence();
				currentElevation = ElevationType.Low;
				break;
			}
		}
	}

	private IEnumerable<int> FlatSequence(int elevation, int minLength, int maxLength)
	{
		int count = m_context.m_rng.Next(minLength, maxLength);
		return Enumerable.Repeat(elevation, count);
	}

	private IEnumerable<int> HillySequence()
	{
		int elevation = 1;
		TrackGenerationParameters generationParams = m_context.m_generationParams;
		for (int sequenceLength = m_context.m_rng.Next(generationParams.HillsMinSegmentCount, generationParams.HillsMaxSegmentCount); sequenceLength > 0; sequenceLength--)
		{
			yield return elevation;
			elevation = ((!(m_context.m_rng.NextDouble() < (double)generationParams.ChanceHillyElevationChange)) ? elevation : ((elevation + 1) % 2));
		}
	}

	public IEnumerable<TrackDatabase.TrackPiece> NextCorner()
	{
		m_corneringSequence.MoveNext();
		yield return m_corneringSequence.Current;
	}

	public TrackDatabase.TrackPiecePrefab GetNext(GenericPieceType nextType, Flags pieceFlags)
	{
		TrackDatabase.PieceType pieceTypeFromGenericRequest = GetPieceTypeFromGenericRequest(nextType);
		m_lastRequestedType = nextType;
		ElevationChanges changesAllowed = (((pieceFlags & Flags.ForceToLowElevation) == Flags.ForceToLowElevation) ? ElevationChanges.ForceToLow : (((pieceFlags & Flags.NoElevationChanges) == Flags.NoElevationChanges) ? ElevationChanges.StayFlat : ElevationChanges.FollowSequence));
		IList<TrackDatabase.TrackPiecePrefab> subzonePrefabs = m_context.m_database.SubzonePrefabs;
		return PickRandomPrefab(subzonePrefabs, pieceTypeFromGenericRequest, changesAllowed);
	}

	private TrackDatabase.PieceType GetPieceTypeFromGenericRequest(GenericPieceType requestedType)
	{
		switch (requestedType)
		{
		case GenericPieceType.Straight:
			return (m_context.m_rng.NextDouble() < (double)m_context.m_generationParams.ChanceSBend) ? TrackDatabase.PieceType.SBend : TrackDatabase.PieceType.Standard;
		case GenericPieceType.EmptyAir:
			return TrackDatabase.PieceType.EmptyAir;
		case GenericPieceType.GameStart:
			return TrackDatabase.PieceType.GameStart;
		case GenericPieceType.SetPiece:
		{
			IList<TrackDatabase.TrackPiecePrefab> subzonePrefabs = m_context.m_database.SubzonePrefabs;
			TrackDatabase.ElevationType atElevation = TrackDatabase.ElevationType.Low;
			List<TrackDatabase.PieceType> list = new List<TrackDatabase.PieceType>();
			if (IsAny(subzonePrefabs, atElevation, TrackDatabase.PieceType.SetPieceLoop))
			{
				list.Add(TrackDatabase.PieceType.SetPieceLoop);
			}
			if (IsAny(subzonePrefabs, atElevation, TrackDatabase.PieceType.SetPieceCorkscrew))
			{
				list.Add(TrackDatabase.PieceType.SetPieceCorkscrew);
			}
			if (IsAny(subzonePrefabs, atElevation, TrackDatabase.PieceType.SetPieceBend))
			{
				list.Add(TrackDatabase.PieceType.SetPieceBend);
			}
			m_setpieceIndex = (m_setpieceIndex + 1) % list.Count;
			return list[m_setpieceIndex];
		}
		case GenericPieceType.GapLeft:
		case GenericPieceType.GapRight:
			if (m_lastRequestedType != requestedType)
			{
				return (requestedType != GenericPieceType.GapLeft) ? TrackDatabase.PieceType.GapRightStart : TrackDatabase.PieceType.GapLeftStart;
			}
			return (requestedType != GenericPieceType.GapLeft) ? TrackDatabase.PieceType.GapRight : TrackDatabase.PieceType.GapLeft;
		case GenericPieceType.GapLeftEnd:
			return TrackDatabase.PieceType.GapLeftEnd;
		case GenericPieceType.GapRightEnd:
			return TrackDatabase.PieceType.GapRightEnd;
		case GenericPieceType.TrackCap:
			return (m_lastRequestedType != GenericPieceType.EmptyAir) ? TrackDatabase.PieceType.TrackEnd : TrackDatabase.PieceType.TrackStart;
		default:
			return TrackDatabase.PieceType.Standard;
		}
	}

	private IEnumerator<TrackDatabase.TrackPiece> CorneringSequence()
	{
		bool isLeftFirst = m_context.m_rng.Next(2) == 0;
		TrackDatabase.PieceType firstTurn = (isLeftFirst ? TrackDatabase.PieceType.Left : TrackDatabase.PieceType.Right);
		TrackDatabase.PieceType secondTurn = ((!isLeftFirst) ? TrackDatabase.PieceType.Left : TrackDatabase.PieceType.Right);
		bool isPrevAZigZag = false;
		while (true)
		{
			IList<TrackDatabase.TrackPiecePrefab> subzonePrefabs = m_context.m_database.SubzonePrefabs;
			bool isNextAZigZag = !isPrevAZigZag || m_context.m_rng.Next(3) > 0;
			if (isNextAZigZag)
			{
				yield return PickRandomPrefab(subzonePrefabs, firstTurn);
				yield return PickRandomPrefab(subzonePrefabs, secondTurn);
			}
			else
			{
				yield return PickRandomPrefab(subzonePrefabs, secondTurn);
			}
			isPrevAZigZag = isNextAZigZag;
		}
	}

	private void NextElevation()
	{
		m_previousElevation = m_currentElevationSequence.Current;
		m_currentElevationSequence.MoveNext();
	}

	private TrackDatabase.ElevationType GetCurrentElevationType()
	{
		int current = m_currentElevationSequence.Current;
		return (m_previousElevation > current) ? TrackDatabase.ElevationType.RampDown : ((m_previousElevation < current) ? TrackDatabase.ElevationType.RampUp : ((current > 0) ? TrackDatabase.ElevationType.High : TrackDatabase.ElevationType.Low));
	}

	private IEnumerator<int> ElevationSequence()
	{
		return new SequenceLooper<int>(ElevationSequenceGenerator());
	}

	private TrackDatabase.TrackPiecePrefab PickRandomPrefab(IList<TrackDatabase.TrackPiecePrefab> source, TrackDatabase.PieceType typeToPick)
	{
		return PickRandomPrefab(source, typeToPick, ElevationChanges.FollowSequence);
	}

	private TrackDatabase.TrackPiecePrefab PickRandomPrefab(IList<TrackDatabase.TrackPiecePrefab> source, TrackDatabase.PieceType typeToPick, ElevationChanges changesAllowed)
	{
		if (typeToPick == TrackDatabase.PieceType.EmptyAir)
		{
			return m_context.m_database.SubzoneTransitionPrefab;
		}
		TrackDatabase.ElevationType elevationType = changesAllowed switch
		{
			ElevationChanges.ForceToLow => (m_previousElevation > 0) ? TrackDatabase.ElevationType.RampDown : TrackDatabase.ElevationType.Low, 
			ElevationChanges.FollowSequence => GetCurrentElevationType(), 
			_ => (m_previousElevation > 0) ? TrackDatabase.ElevationType.High : TrackDatabase.ElevationType.Low, 
		};
		if (changesAllowed == ElevationChanges.ForceToLow)
		{
			m_previousElevation = 0;
		}
		if (IsAny(source, elevationType, typeToPick))
		{
			if (changesAllowed == ElevationChanges.FollowSequence)
			{
				NextElevation();
			}
		}
		else
		{
			elevationType = ((elevationType == TrackDatabase.ElevationType.RampDown) ? TrackDatabase.ElevationType.High : TrackDatabase.ElevationType.Low);
		}
		int maxValue = CountPrefabsMatching(source, elevationType, typeToPick);
		int indexToFind = m_context.m_rng.Next(maxValue);
		return FindNthPrefab(source, elevationType, typeToPick, indexToFind);
	}

	private TrackDatabase.TrackPiecePrefab FindNthPrefab(IList<TrackDatabase.TrackPiecePrefab> prefabSource, TrackDatabase.ElevationType atElevation, TrackDatabase.PieceType ofType, int indexToFind)
	{
		int num = 0;
		for (int i = 0; i < prefabSource.Count; i++)
		{
			TrackDatabase.TrackPiecePrefab trackPiecePrefab = prefabSource[i];
			if (trackPiecePrefab.PieceType.Elevation == atElevation && trackPiecePrefab.PieceType.Type == ofType)
			{
				if (num == indexToFind)
				{
					return trackPiecePrefab;
				}
				num++;
			}
		}
		return null;
	}

	private int CountPrefabsMatching(IList<TrackDatabase.TrackPiecePrefab> prefabSource, TrackDatabase.ElevationType atElevation, TrackDatabase.PieceType ofType)
	{
		int num = 0;
		for (int i = 0; i < prefabSource.Count; i++)
		{
			TrackDatabase.TrackPiecePrefab trackPiecePrefab = prefabSource[i];
			if (trackPiecePrefab.PieceType.Elevation == atElevation && trackPiecePrefab.PieceType.Type == ofType)
			{
				num++;
			}
		}
		return num;
	}

	private bool IsAny(IList<TrackDatabase.TrackPiecePrefab> prefabSource, TrackDatabase.ElevationType atElevation, TrackDatabase.PieceType ofType)
	{
		return FindNthPrefab(prefabSource, atElevation, ofType, 0) != null;
	}

	private TrackDatabase.TrackPiecePrefab PickFirstPrefabOfType(List<TrackDatabase.TrackPiecePrefab> prefabSourceList, TrackDatabase.PieceType typeToPick)
	{
		for (int i = 0; i < prefabSourceList.Count; i++)
		{
			TrackDatabase.TrackPiecePrefab trackPiecePrefab = prefabSourceList[i];
			if (trackPiecePrefab.PieceType.Type == typeToPick)
			{
				return trackPiecePrefab;
			}
		}
		return null;
	}
}
