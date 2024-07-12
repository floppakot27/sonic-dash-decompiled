using UnityEngine;

public class Characters : MonoBehaviour
{
	public enum Type
	{
		Sonic,
		Tails,
		Knuckles,
		Amy,
		Shadow,
		Blaze,
		Silver,
		Rouge,
		Cream
	}

	public const int NoCharacter = -1;

	private const string CharacterStateSaveProperty = "CharacterState";

	public static string[] StoreEntries = new string[9] { null, "Character Tails", "Character Knuckles", "Character Amy", "Character Shadow", "Character Blaze", "Character Silver", "Character Rouge", "Character Cream" };

	public static string[] IDStrings = new string[9] { "character_sonic", "character_tails", "character_knuckles", "character_amy", "character_shadow", "character_blaze", "character_silver", "character_rouge", "character_cream" };

	private static bool[] s_unlockState = null;

	public static int WhatCharacterIs(string id)
	{
		for (int i = 0; i < IDStrings.Length; i++)
		{
			if (id == IDStrings[i])
			{
				return i;
			}
		}
		return -1;
	}

	public static void UnlockCharacter(Type character)
	{
		s_unlockState[(int)character] = true;
	}

	public static bool CharacterUnlocked(Type character)
	{
		return s_unlockState[(int)character];
	}

	public static string GetCharacterSaveState()
	{
		string text = string.Empty;
		bool[] array = s_unlockState;
		foreach (bool flag in array)
		{
			text += ((!flag) ? '0' : '1');
		}
		return text;
	}

	private void Start()
	{
		EventDispatch.RegisterInterest("OnGameDataSaveRequest", this);
		EventDispatch.RegisterInterest("OnGameDataLoaded", this);
		EventDispatch.RegisterInterest("MainMenuActive", this, EventDispatch.Priority.High);
		InitialisePurchaseState();
	}

	private static void InitialisePurchaseState()
	{
		int enumCount = Utils.GetEnumCount<Type>();
		if (s_unlockState == null)
		{
			s_unlockState = new bool[enumCount];
		}
		ActiveProperties activeProperties = PropertyStore.ActiveProperties();
		if (!activeProperties.DoesPropertyExist("CharacterState"))
		{
			for (int i = 0; i < enumCount; i++)
			{
				s_unlockState[i] = false;
			}
		}
		else
		{
			string @string = activeProperties.GetString("CharacterState");
			char[] array = @string.ToCharArray();
			for (int j = 0; j < array.Length; j++)
			{
				s_unlockState[j] = array[j] == '1';
			}
		}
		s_unlockState[0] = true;
	}

	private void UpdateCharacterVisibility()
	{
		CheckShadowVisibility();
		CheckBlazeVisibility();
	}

	private void CheckShadowVisibility()
	{
		bool flag = CharacterUnlocked(Type.Shadow);
		StoreContent.StoreEntry storeEntry = StoreContent.GetStoreEntry(StoreEntries[4], StoreContent.Identifiers.Name);
		GCState.State state = GCState.ChallengeState(GCState.Challenges.gc1);
		bool flag2 = true;
		if (flag2 || flag || StoreContent.ShowHiddenStoreItems)
		{
			storeEntry.m_state &= ~StoreContent.StoreEntry.State.Hidden;
		}
		else
		{
			storeEntry.m_state |= StoreContent.StoreEntry.State.Hidden;
		}
		bool flag3 = GCState.IsChallengeParticipated(GCState.Challenges.gc1);
		if (!flag && flag3 && flag2)
		{
			StorePurchases.RequestReward(storeEntry.m_identifier, 1, 16, StorePurchases.ShowDialog.Yes);
		}
	}

	private void CheckBlazeVisibility()
	{
		bool flag = CharacterUnlocked(Type.Blaze);
		StoreContent.StoreEntry storeEntry = StoreContent.GetStoreEntry(StoreEntries[5], StoreContent.Identifiers.Name);
		GCState.State state = GCState.ChallengeState(GCState.Challenges.gc2);
		if (true || flag || StoreContent.ShowHiddenStoreItems)
		{
			storeEntry.m_state &= ~StoreContent.StoreEntry.State.Hidden;
		}
		else
		{
			storeEntry.m_state |= StoreContent.StoreEntry.State.Hidden;
		}
	}

	private void Event_OnGameDataSaveRequest()
	{
		string characterSaveState = GetCharacterSaveState();
		PropertyStore.Store("CharacterState", characterSaveState);
	}

	private void Event_OnGameDataLoaded(ActiveProperties activeProperties)
	{
		InitialisePurchaseState();
	}

	private void Event_MainMenuActive()
	{
		UpdateCharacterVisibility();
	}
}
