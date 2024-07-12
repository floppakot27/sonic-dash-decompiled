using System.Collections.Generic;
using UnityEngine;

public class RingStorage : MonoBehaviour
{
	private const float AdditionalCollectionHeight = 0.5f;

	public const string BankedRingsSaveProperty = "Banked Rings Total";

	public const string StarRingsSaveProperty = "Star Rings Total";

	private static bool s_storageCreated;

	[SerializeField]
	private float m_maximumCollectionRange = 1.5f;

	private RingGenerator m_ringGenerator;

	private IList<TrackEntity> m_trackInfoScratchList = new List<TrackEntity>();

	public static int TotalStarRings { get; private set; }

	public static int TotalBankedRings { get; private set; }

	public static int RunBankedRings { get; private set; }

	public static int HeldRings { get; private set; }

	public static bool Banked { get; private set; }

	private void Start()
	{
		s_storageCreated = true;
		EventDispatch.RegisterInterest("OnNewGameStarted", this);
		EventDispatch.RegisterInterest("OnGameFinished", this);
		EventDispatch.RegisterInterest("OnGameDataLoaded", this);
		EventDispatch.RegisterInterest("OnGameDataSaveRequest", this);
		EventDispatch.RegisterInterest("OnRingsAwarded", this);
		EventDispatch.RegisterInterest("OnStarRingsAwarded", this);
		EventDispatch.RegisterInterest("OnRingExplosion", this);
		EventDispatch.RegisterInterest("OnRingBankRequest", this);
		m_ringGenerator = GetComponent<RingGenerator>();
		Sonic.OnMovementCallback += OnSonicMovement;
		TotalBankedRings = 0;
		RunBankedRings = 0;
		HeldRings = 0;
	}

	private void OnDestroy()
	{
		s_storageCreated = false;
	}

	private void RegisterRingPickup(RingSequence seq, RingSequence.Ring ring, ref int ringsCollectedThisRun)
	{
		seq.RegisterCollection(ring);
		ringsCollectedThisRun++;
	}

	private void CommitRingPickups(int ringsCollectedThisRun)
	{
		RingPickupMonitor.instance().PickupRings(ringsCollectedThisRun);
	}

	private void OnSonicMovement(SonicSplineTracker.MovementInfo info)
	{
		if (Sonic.Tracker.GetIsGhosted())
		{
			return;
		}
		int ringsCollectedThisRun = 0;
		float y = Sonic.Tracker.transform.position.y;
		float prevTrackPosition = info.PrevTrackPosition;
		float maxInclusive = info.NewTrackPosition + m_maximumCollectionRange;
		TrackInfo info2 = Sonic.Tracker.Track.Info;
		info2.EntitiesInRange(prevTrackPosition, maxInclusive, 256u, info.Lane, ref m_trackInfoScratchList);
		for (int i = 0; i < m_trackInfoScratchList.Count; i++)
		{
			TrackEntity trackEntity = m_trackInfoScratchList[i];
			if (trackEntity.IsValid)
			{
				RingID iD = (trackEntity as TrackRing).ID;
				float num = Mathf.Abs(y - iD.Ring.m_position.y);
				if (!(num > m_maximumCollectionRange))
				{
					RegisterRingPickup(iD.Sequence, iD.Ring, ref ringsCollectedThisRun);
				}
			}
		}
		CommitRingPickups(ringsCollectedThisRun);
	}

	private void Update()
	{
		if (RingPickupMonitor.instance() == null)
		{
			return;
		}
		RingSequence[] sequences = m_ringGenerator.GetSequences();
		if (sequences == null)
		{
			return;
		}
		Banked = false;
		int ringsCollectedThisRun = 0;
		foreach (RingSequence ringSequence in sequences)
		{
			if (!ringSequence.Collectable)
			{
				continue;
			}
			if (!ringSequence.RingsAvailable)
			{
				ringSequence.Reset();
				continue;
			}
			for (int j = 0; j < ringSequence.Length; j++)
			{
				RingSequence.Ring ring = ringSequence.GetRing(j);
				if (ring.m_occupied && !ring.m_collected && ring.m_forceCollecion)
				{
					RegisterRingPickup(ringSequence, ring, ref ringsCollectedThisRun);
				}
			}
		}
		CommitRingPickups(ringsCollectedThisRun);
		TallyRingPickups();
	}

	private void TallyRingPickups()
	{
		int pickupCount = RingPickupMonitor.instance().GetPickupCount();
		if (pickupCount != 0)
		{
			RingPickupMonitor.instance().ResetPickupCount();
			HeldRings += pickupCount;
			PlayerStats.IncreaseStat(PlayerStats.StatNames.RingsCollected_Total, pickupCount);
			PlayerStats.IncreaseStat(PlayerStats.StatNames.RingsCollected_Session, pickupCount);
			PlayerStats.IncreaseStat(PlayerStats.StatNames.RingsCollected_Run, pickupCount);
		}
	}

	private void BankRings()
	{
		RunBankedRings += HeldRings;
		HeldRings = 0;
		Banked = true;
	}

	private void Event_OnNewGameStarted()
	{
		HeldRings = 0;
		RunBankedRings = 0;
		Banked = false;
	}

	private void Event_OnGameFinished()
	{
		RunBankedRings += HeldRings;
		if (PowerUpsInventory.GetPowerUpCount(PowerUps.Type.DoubleRing) > 0)
		{
			RunBankedRings *= 2;
		}
		GameAnalytics.RingsGiven(RunBankedRings, TotalBankedRings, GameAnalytics.RingsRecievedReason.CollectedInRun);
		TotalBankedRings += RunBankedRings;
	}

	private void Event_OnGameDataSaveRequest()
	{
		PropertyStore.Store("Banked Rings Total", TotalBankedRings);
		PropertyStore.Store("Star Rings Total", TotalStarRings);
	}

	private void Event_OnGameDataLoaded(ActiveProperties activeProperties)
	{
		TotalBankedRings = activeProperties.GetInt("Banked Rings Total");
		TotalStarRings = activeProperties.GetInt("Star Rings Total");
	}

	private void Event_OnRingsAwarded(int ringCount)
	{
		TotalBankedRings += ringCount;
	}

	private void Event_OnStarRingsAwarded(GameAnalytics.RSRAnalyticsParam param)
	{
		if (param.Amount > 0)
		{
			GameAnalytics.RSRGiven(param.Amount, TotalStarRings, param.Reason);
		}
		else
		{
			GameAnalytics.RSRTaken(-param.Amount, TotalStarRings);
		}
		TotalStarRings += param.Amount;
		PropertyStore.Save();
	}

	private void Event_OnRingExplosion()
	{
		HeldRings = 0;
	}

	private void Event_OnRingBankRequest()
	{
		BankRings();
	}
}
