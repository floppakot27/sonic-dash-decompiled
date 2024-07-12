using System;
using System.Collections.Generic;
using UnityEngine;

public class DialogContent_Hints : MonoBehaviour
{
	public enum Reason
	{
		General,
		Featured,
		LowRings,
		LowScore
	}

	[Serializable]
	public class Hint
	{
		public enum State
		{
			UseStore = 1
		}

		public const int AvailableStoreEntries = 4;

		public Reason m_reason;

		public string m_title = string.Empty;

		public string m_description = string.Empty;

		public Mesh m_mesh;

		public string[] m_storeEntry = new string[4]
		{
			string.Empty,
			string.Empty,
			string.Empty,
			string.Empty
		};

		public State m_state;
	}

	public const int InvalidStoreIndex = -1;

	private const int m_runsWithoutReuse = 5;

	[SerializeField]
	public int m_percentGeneral = 15;

	[SerializeField]
	public int m_percentFeatured = 30;

	[SerializeField]
	public int m_percentSpecific = 55;

	private static DialogContent_Hints s_hintContent;

	[SerializeField]
	private List<Hint> m_currentHintList;

	private List<Hint> m_loadingHintList;

	private int[] m_useOfHints;

	public static List<Hint> HintList
	{
		get
		{
			s_hintContent.InitialiseHintList();
			return s_hintContent.m_currentHintList;
		}
	}

	public static void Display()
	{
		Hint hint = GetHint();
		int storeEntry = GetStoreEntry(hint);
		Display(hint, storeEntry);
	}

	public static void Display(Hint hintToUse, int storeIndex)
	{
		GuiTrigger guiTrigger = null;
		guiTrigger = ((storeIndex != -1) ? DialogStack.ShowDialog("Hint (With Store)") : DialogStack.ShowDialog("Hint (No Store)"));
		Dialog_Hint componentInChildren = Utils.GetComponentInChildren<Dialog_Hint>(guiTrigger.Trigger.gameObject);
		Dialog_Hint.SetNextContent(hintToUse, storeIndex);
	}

	public static Hint GetHint()
	{
		return s_hintContent.GetSuggestedHint(isForLoading: false);
	}

	public static Hint GetLoadingHint()
	{
		return s_hintContent.GetSuggestedHint(isForLoading: true);
	}

	public static int GetStoreEntry(Hint hintToUse)
	{
		return s_hintContent.GetRandomStoreEntry(hintToUse);
	}

	private void Awake()
	{
		InitialiseHintList();
		m_useOfHints = new int[m_currentHintList.Count];
	}

	private void Start()
	{
		s_hintContent = this;
	}

	private Hint GetSuggestedHint(bool isForLoading)
	{
		List<Hint> outGeneralHints = new List<Hint>(m_currentHintList);
		List<Hint> list = null;
		for (int num = outGeneralHints.Count - 1; num >= 0; num--)
		{
			if (m_useOfHints[num] > 0)
			{
				outGeneralHints.RemoveAt(num);
			}
		}
		GenerateSpecifics(out var adding, out var removing);
		BuildHintsLists(outGeneralHints, out outGeneralHints, out var featuredHints, out var specificHints, adding, removing, isForLoading);
		int num2 = UnityEngine.Random.Range(0, 100);
		bool flag = false;
		if (num2 > 100 - m_percentSpecific)
		{
			if (specificHints.Count == 0)
			{
				flag = true;
			}
			else
			{
				list = specificHints;
			}
		}
		if ((num2 > m_percentGeneral && num2 <= 100 - m_percentSpecific) || flag)
		{
			if (featuredHints.Count == 0)
			{
				flag = true;
			}
			else
			{
				list = featuredHints;
			}
		}
		if (num2 <= m_percentGeneral || flag)
		{
			list = outGeneralHints;
		}
		for (int i = 0; i < m_useOfHints.Length; i++)
		{
			if (m_useOfHints[i] > 0)
			{
				m_useOfHints[i]--;
			}
		}
		int count = list.Count;
		int index = UnityEngine.Random.Range(0, count);
		Hint hintToUse = list[index];
		int num3 = m_currentHintList.FindIndex((Hint h) => h.m_description == hintToUse.m_description);
		m_useOfHints[num3] += 5;
		return hintToUse;
	}

	private void GenerateSpecifics(out List<string> adding, out List<string> removing)
	{
		adding = new List<string>();
		removing = new List<string>();
		PlayerStats.Stats currentStats = PlayerStats.GetCurrentStats();
		if ((double)((float)currentStats.m_trackedStats[48] / (float)currentStats.m_trackedStats[1]) < 0.2)
		{
			adding.Add("HINT_4");
		}
		int min;
		List<PowerUps.Type> lowestLevelHintablePowerUps = PowerUpsInventory.GetLowestLevelHintablePowerUps(out min);
		using (List<PowerUps.Type>.Enumerator enumerator = lowestLevelHintablePowerUps.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				switch (enumerator.Current)
				{
				case PowerUps.Type.Magnet:
					adding.Add("HINT_11");
					break;
				case PowerUps.Type.HeadStart:
					adding.Add("HINT_12");
					break;
				case PowerUps.Type.DashLength:
					adding.Add("HINT_29");
					break;
				case PowerUps.Type.DashIncrease:
					adding.Add("HINT_30");
					break;
				case PowerUps.Type.Shield:
					adding.Add("HINT_41");
					break;
				}
			}
		}
		if (min == 0)
		{
			adding.Add("HINT_33");
		}
		if (!DCs.AllPiecesCollected())
		{
			adding.Add("HINT_43");
		}
		if (GCState.IsCurrentChallengeActive())
		{
			adding.Add("HINT_45");
			adding.Add("HINT_46");
		}
		if (currentStats.m_trackedStats[62] == 57)
		{
			removing.Add("HINT_3");
			removing.Add("HINT_5");
			removing.Add("HINT_10");
			removing.Add("HINT_27");
			removing.Add("HINT_47");
		}
		if (currentStats.m_trackedStats[49] > 0)
		{
			removing.Add("HINT_4");
			adding.Remove("HINT_4");
		}
		if (PowerUpsInventory.GetPowerUpCount(PowerUps.Type.DoubleRing) > 0)
		{
			removing.Add("HINT_6");
		}
		if (PowerUpsInventory.GetPowerUpLevel(PowerUps.Type.Magnet) == 6)
		{
			removing.Add("HINT_11");
			adding.Remove("HINT_11");
		}
		if (PowerUpsInventory.GetPowerUpLevel(PowerUps.Type.HeadStart) == 6)
		{
			removing.Add("HINT_12");
			adding.Remove("HINT_12");
		}
		if (PowerUpsInventory.GetPowerUpLevel(PowerUps.Type.DashLength) == 6)
		{
			removing.Add("HINT_24");
			removing.Add("HINT_29");
			adding.Remove("HINT_29");
		}
		if (PowerUpsInventory.GetPowerUpLevel(PowerUps.Type.DashIncrease) == 6)
		{
			removing.Add("HINT_22");
			removing.Add("HINT_23");
			removing.Add("HINT_30");
			adding.Remove("HINT_30");
		}
		if (PowerUpsInventory.GetPowerUpLevel(PowerUps.Type.Shield) == 6)
		{
			removing.Add("HINT_41");
			adding.Remove("HINT_41");
		}
		if (min == 6)
		{
			removing.Add("HINT_33");
			adding.Remove("HINT_33");
		}
		if (Characters.CharacterUnlocked(Characters.Type.Tails))
		{
			removing.Add("HINT_37");
		}
		if (Characters.CharacterUnlocked(Characters.Type.Amy))
		{
			removing.Add("HINT_38");
		}
		if (Characters.CharacterUnlocked(Characters.Type.Knuckles))
		{
			removing.Add("HINT_39");
		}
		if (StoreContent.StoreInitialised())
		{
			StoreContent.StoreEntry storeEntry = StoreContent.GetStoreEntry(Characters.StoreEntries[4], StoreContent.Identifiers.Name);
			if (Characters.CharacterUnlocked(Characters.Type.Shadow) || (storeEntry.m_state & StoreContent.StoreEntry.State.Hidden) == StoreContent.StoreEntry.State.Hidden)
			{
				removing.Add("HINT_44");
			}
		}
		else
		{
			removing.Add("HINT_44");
		}
		if (!GCState.IsCurrentChallengeActive())
		{
			removing.Add("HINT_45");
			adding.Remove("HINT_45");
			removing.Add("HINT_46");
			adding.Remove("HINT_46");
		}
		if (!WheelOfFortuneSettings.Instance.HasFreeSpin)
		{
			removing.Add("HINT_48");
		}
		if (Boosters.GetBoostersSelected[0] != -1)
		{
			removing.Add("HINT_49");
			removing.Add("HINT_50");
			removing.Add("HINT_51");
			removing.Add("HINT_52");
			removing.Add("HINT_53");
			removing.Add("HINT_54");
		}
	}

	private void BuildHintsLists(List<Hint> generalHints, out List<Hint> outGeneralHints, out List<Hint> featuredHints, out List<Hint> specificHints, List<string> adding, List<string> removing, bool isForLoading)
	{
		featuredHints = new List<Hint>();
		specificHints = new List<Hint>();
		string hintDesc;
		foreach (string item in removing)
		{
			hintDesc = item;
			Hint hint = generalHints.Find((Hint h) => h.m_description == hintDesc);
			if (hint != null)
			{
				generalHints.Remove(hint);
			}
		}
		if (!isForLoading)
		{
			if (RingPerMinute.Current < 30f)
			{
				foreach (Hint generalHint in generalHints)
				{
					if (generalHint.m_reason == Reason.LowRings)
					{
						featuredHints.Add(generalHint);
					}
				}
			}
			if ((double)((float)ScoreTracker.CurrentScore / (float)ScoreTracker.HighScore) < 0.75)
			{
				foreach (Hint generalHint2 in generalHints)
				{
					if (generalHint2.m_reason == Reason.LowScore)
					{
						featuredHints.Add(generalHint2);
					}
				}
			}
		}
		foreach (Hint generalHint3 in generalHints)
		{
			if (generalHint3.m_reason == Reason.Featured)
			{
				featuredHints.Add(generalHint3);
			}
		}
		foreach (string item2 in adding)
		{
			hintDesc = item2;
			Hint hint = generalHints.Find((Hint h) => h.m_description == hintDesc);
			if (hint != null)
			{
				specificHints.Add(hint);
			}
		}
		outGeneralHints = generalHints;
	}

	private Hint GetRandomHint()
	{
		int count = m_currentHintList.Count;
		int index = UnityEngine.Random.Range(0, count);
		return m_currentHintList[index];
	}

	private int GetRandomStoreEntry(Hint hintToUse)
	{
		int num = -1;
		if ((hintToUse.m_state & Hint.State.UseStore) == Hint.State.UseStore)
		{
			int num2 = 0;
			string[] storeEntry = hintToUse.m_storeEntry;
			foreach (string text in storeEntry)
			{
				if (text != null && text.Length != 0)
				{
					StoreContent.StoreEntry storeEntry2 = StoreContent.GetStoreEntry(text, StoreContent.Identifiers.Name);
					if (storeEntry2 != null)
					{
						num2++;
					}
				}
			}
			if (num2 > 0)
			{
				bool flag = false;
				do
				{
					num = UnityEngine.Random.Range(0, num2);
					string text2 = hintToUse.m_storeEntry[num];
					if (text2 != null && text2.Length > 0)
					{
						StoreContent.StoreEntry storeEntry3 = StoreContent.GetStoreEntry(text2, StoreContent.Identifiers.Name);
						if (storeEntry3 != null)
						{
							flag = true;
						}
					}
				}
				while (!flag);
			}
		}
		return num;
	}

	private void InitialiseHintList()
	{
		if (m_currentHintList == null)
		{
			m_currentHintList = new List<Hint>();
		}
	}
}
