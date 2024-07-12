using System.Collections.Generic;
using UnityEngine;

public class MenuBoosters : MonoBehaviour
{
	private class BoosterDesc
	{
		public PowerUps.Type m_type;

		public UILabel m_countLabel;

		public UILabel m_costLabel;

		public UIButton m_removeBtn;

		public MeshFilter m_mesh;

		public GameObject m_sparkles;

		public UIButton m_toggleBtn;

		public GameObject m_btnEnabled;

		public GameObject m_btnDisabled;

		public GameObject m_spinner;

		public void SetActive(bool active)
		{
			m_countLabel.gameObject.SetActive(active);
			m_costLabel.transform.parent.gameObject.SetActive(active);
			m_mesh.gameObject.SetActive(active);
			m_sparkles.SetActive(active);
			m_toggleBtn.GetComponent<GuiButtonBlocker>().Blocked = !active;
			m_btnEnabled.SetActive(active);
			m_btnDisabled.SetActive(!active);
			m_spinner.SetActive(!active);
		}
	}

	private class SlotDesc
	{
		public MeshFilter m_mesh;

		public UIButton m_removeBtn;

		public PowerUps.Type? m_type;

		public int m_ringCost;
	}

	[SerializeField]
	private UILabel m_descriptionLabel;

	[SerializeField]
	private GameObject[] m_slot = new GameObject[3];

	[SerializeField]
	private GameObject[] m_booster = new GameObject[5];

	[SerializeField]
	private ParticleSystem m_particleAssigned;

	[SerializeField]
	private UILabel m_ringCountLabel;

	[SerializeField]
	private GameObject m_startGameTrigger;

	[SerializeField]
	private GameObject m_restartGameTrigger;

	[SerializeField]
	private AudioClip m_slotAvailableChoice;

	[SerializeField]
	private AudioClip m_slotUnavailableChoice;

	[SerializeField]
	private AudioClip m_slotRemove;

	[SerializeField]
	private BoostersHandHolder m_handHolder;

	[SerializeField]
	private GuiButtonBlocker[] m_blockers;

	[SerializeField]
	private GuiButtonBlocker m_homeBlocker;

	[SerializeField]
	private MenuTriggers m_menuTriggersRef;

	private bool m_homePressed;

	private bool m_playPressed;

	private List<BoosterDesc> m_boosterDesc = new List<BoosterDesc>();

	private List<SlotDesc> m_slotDesc = new List<SlotDesc>();

	private bool m_overlayPending;

	private bool m_overlayShowing;

	private PowerUps.Type? m_requestedType;

	private int m_requestedRingCost;

	private int m_totalRingCost;

	private static string[] StoreEntries = new string[5] { "Booster Spring Bonus", "Booster Enemy Combo", "Booster Ring Streak", "Booster Score Multiplier", "Booster Golden Enemy" };

	private static string[] s_boosterDescs = new string[5] { "BOOSTERS_DESC_SPRING_BOOSTER_SHORT", "BOOSTERS_DESC_COMBO_BOOSTER_SHORT", "BOOSTERS_DESC_STREAK_BOOSTER_SHORT", "BOOSTERS_DESC_SCORE_BOOSTER_SHORT", "BOOSTERS_DESC_GOLDEN_BADNIK_SHORT" };

	private static string s_emptyDesc = "BOOSTERS_INFO";

	public static string StoreEntry(PowerUps.Type type)
	{
		return StoreEntries[GetBoosterIndex(type)];
	}

	private void Awake()
	{
		for (int i = 0; i < m_slot.Length; i++)
		{
			GameObject gameObject = m_slot[i];
			SlotDesc slotDesc = new SlotDesc();
			slotDesc.m_mesh = gameObject.transform.FindChild("Booster Mesh").GetComponent<MeshFilter>();
			slotDesc.m_removeBtn = gameObject.transform.FindChild("Button (Remove Booster)").GetComponent<UIButton>();
			m_slotDesc.Add(slotDesc);
		}
		for (int j = 0; j < m_booster.Length; j++)
		{
			GameObject gameObject2 = m_booster[j];
			BoosterDesc boosterDesc = new BoosterDesc();
			boosterDesc.m_countLabel = gameObject2.transform.FindChild("Booster Count [label]").GetComponent<UILabel>();
			boosterDesc.m_costLabel = gameObject2.transform.FindChild("Booster Cost").GetComponentInChildren<UILabel>();
			boosterDesc.m_removeBtn = gameObject2.transform.FindChild("Button (Remove Booster)").GetComponent<UIButton>();
			boosterDesc.m_mesh = gameObject2.transform.FindChild("Booster Mesh").GetComponent<MeshFilter>();
			boosterDesc.m_sparkles = gameObject2.transform.FindChild("Booster Highlight").gameObject;
			boosterDesc.m_spinner = gameObject2.transform.FindChild("Booster Spinner").gameObject;
			GameObject gameObject3 = gameObject2.transform.FindChild("Button (Toggle Booster)").gameObject;
			boosterDesc.m_toggleBtn = gameObject3.GetComponent<UIButton>();
			boosterDesc.m_btnEnabled = gameObject3.transform.FindChild("Background (Enabled)").gameObject;
			boosterDesc.m_btnDisabled = gameObject3.transform.FindChild("Background (Disabled)").gameObject;
			m_boosterDesc.Add(boosterDesc);
		}
	}

	private void OnEnable()
	{
		Boosters.ClearSelected();
		BoostersBreadcrumb.Instance.ShowBoosterAnouncement();
		m_homeBlocker.Blocked = false;
		for (int i = 0; i < m_slotDesc.Count; i++)
		{
			SlotDesc slotDesc = m_slotDesc[i];
			slotDesc.m_mesh.mesh = null;
			slotDesc.m_removeBtn.gameObject.SetActive(value: false);
			slotDesc.m_type = null;
		}
		SetBooster(PowerUps.Type.Booster_SpringBonus, enable: true);
		SetBooster(PowerUps.Type.Booster_ScoreMultiplier, enable: true);
		SetBooster(PowerUps.Type.Booster_GoldenEnemy, enable: true);
		SetBooster(PowerUps.Type.Booster_EnemyComboBonus, enable: true);
		SetBooster(PowerUps.Type.Booster_RingStreakBonus, enable: true);
		BlockAllButtons(blockVal: false);
		m_totalRingCost = 0;
		UpdateRingCount(0);
		SetLocalisedLabel(m_descriptionLabel, -1);
		EventDispatch.RegisterInterest("OnStorePurchaseCompleted", this, EventDispatch.Priority.Lowest);
		m_handHolder.UpdatePromptButtonText(GetNumberOfFreeSlots());
		m_handHolder.ShowCorrectButton();
		m_homePressed = false;
		m_playPressed = false;
	}

	private void OnDisable()
	{
		EventDispatch.UnregisterInterest("OnStorePurchaseCompleted", this);
	}

	private void Trigger_HomeButtonPressed()
	{
		if (!m_playPressed)
		{
			m_homeBlocker.Blocked = true;
			m_homePressed = true;
			m_menuTriggersRef.SendMessage("Trigger_MoveToPage", base.gameObject, SendMessageOptions.DontRequireReceiver);
		}
	}

	private void UseBreadcrumbOfType(PowerUps.Type booster)
	{
		if (BoostersBreadcrumb.Instance.CurrentBreadcrumb < 5)
		{
			BoostersBreadcrumb.Instance.ActivateBreadcrumb(booster);
		}
	}

	private static int GetBoosterIndex(PowerUps.Type boosterType)
	{
		return (int)(boosterType - 11);
	}

	private int SetBooster(PowerUps.Type boosterType, bool enable)
	{
		int boosterIndex = GetBoosterIndex(boosterType);
		int boosterCost = GetBoosterCost(StoreEntry(boosterType));
		int powerUpCount = PowerUpsInventory.GetPowerUpCount(boosterType);
		EnableBooster(boosterType, enable);
		m_boosterDesc[boosterIndex].m_type = boosterType;
		BoosterDesc boosterDesc = m_boosterDesc[boosterIndex];
		if (powerUpCount == 0 && enable)
		{
			boosterDesc.m_countLabel.transform.gameObject.SetActive(value: false);
			boosterDesc.m_costLabel.transform.parent.gameObject.SetActive(value: true);
			boosterDesc.m_costLabel.text = boosterCost.ToString();
		}
		else if (powerUpCount > 0)
		{
			boosterDesc.m_countLabel.transform.gameObject.SetActive(value: true);
			boosterDesc.m_countLabel.text = "x" + powerUpCount;
			boosterDesc.m_costLabel.transform.parent.gameObject.SetActive(value: false);
		}
		else
		{
			boosterDesc.m_countLabel.transform.gameObject.SetActive(value: false);
			boosterDesc.m_costLabel.transform.parent.gameObject.SetActive(value: false);
		}
		bool active = enable && !BoostersBreadcrumb.Instance.IsBoosterDiscovered(boosterIndex);
		boosterDesc.m_sparkles.SetActive(active);
		return powerUpCount;
	}

	private int GetBoosterFreeSlot()
	{
		int i;
		for (i = 0; i < m_slotDesc.Count; i++)
		{
			PowerUps.Type? type = m_slotDesc[i].m_type;
			if (!type.HasValue)
			{
				break;
			}
		}
		if (i < m_slotDesc.Count)
		{
			return i;
		}
		return -1;
	}

	private int GetNumberOfFreeSlots()
	{
		int num = 0;
		for (int i = 0; i < m_slotDesc.Count; i++)
		{
			PowerUps.Type? type = m_slotDesc[i].m_type;
			if (!type.HasValue)
			{
				num++;
			}
		}
		return num;
	}

	private void AddBoosterToSlot(PowerUps.Type boosterType, int ringCost)
	{
		int boosterFreeSlot = GetBoosterFreeSlot();
		if (boosterFreeSlot >= 0)
		{
			int boosterIndex = GetBoosterIndex(boosterType);
			m_slotDesc[boosterFreeSlot].m_removeBtn.gameObject.SetActive(value: true);
			m_particleAssigned.transform.position = m_slotDesc[boosterFreeSlot].m_mesh.transform.position;
			ParticlePlayer.Play(m_particleAssigned);
			int num = SetBooster(boosterType, enable: false) - 1;
			m_boosterDesc[boosterIndex].m_countLabel.text = "x" + num;
			m_slotDesc[boosterFreeSlot].m_type = boosterType;
			m_slotDesc[boosterFreeSlot].m_mesh.sharedMesh = m_boosterDesc[boosterIndex].m_mesh.sharedMesh;
			m_slotDesc[boosterFreeSlot].m_ringCost = ringCost;
			SetLocalisedLabel(m_descriptionLabel, boosterIndex);
			if (ringCost > 0)
			{
				UpdateRingCount(ringCost);
			}
			UseBreadcrumbOfType(boosterType);
		}
		m_handHolder.UpdatePromptButtonText(GetNumberOfFreeSlots());
	}

	private void EnableBooster(PowerUps.Type boosterType, bool enable)
	{
		int boosterIndex = GetBoosterIndex(boosterType);
		BoosterDesc boosterDesc = m_boosterDesc[boosterIndex];
		int powerUpCount = PowerUpsInventory.GetPowerUpCount(boosterType);
		boosterDesc.m_countLabel.text = "x" + powerUpCount;
		boosterDesc.m_removeBtn.gameObject.SetActive(value: false);
		boosterDesc.m_btnEnabled.SetActive(enable);
		boosterDesc.m_btnDisabled.SetActive(!enable);
		boosterDesc.m_sparkles.SetActive(!BoostersBreadcrumb.Instance.IsBoosterDiscovered(boosterIndex));
	}

	private int GetBoosterCost(string storeId)
	{
		StoreContent.StoreEntry storeEntry = StoreContent.GetStoreEntry(storeId, StoreContent.Identifiers.Name);
		return StoreUtils.GetItemCost(storeEntry, StoreUtils.EntryType.Player);
	}

	private void RequestBooster(PowerUps.Type boosterType)
	{
		string identifier = StoreEntry(boosterType);
		int boosterFreeSlot = GetBoosterFreeSlot();
		if (boosterFreeSlot >= 0)
		{
			int powerUpCount = PowerUpsInventory.GetPowerUpCount(boosterType);
			m_requestedType = null;
			m_requestedRingCost = 0;
			if (powerUpCount > 0)
			{
				AddBoosterToSlot(boosterType, 0);
			}
			else
			{
				StoreContent.StoreEntry storeEntry = StoreContent.GetStoreEntry(identifier, StoreContent.Identifiers.Name);
				int itemCost = StoreUtils.GetItemCost(storeEntry, StoreUtils.EntryType.Player);
				int num = RingStorage.TotalBankedRings - m_totalRingCost - itemCost;
				if (num < 0)
				{
					m_overlayPending = false;
					m_overlayShowing = false;
					m_requestedType = boosterType;
					m_requestedRingCost = itemCost;
					StorePurchases.BuyBestRingBundle(storeEntry.m_payment, -num);
					BlockAllButtons(blockVal: true);
				}
				else
				{
					AddBoosterToSlot(boosterType, itemCost);
				}
			}
			Audio.PlayClip(m_slotAvailableChoice, loop: false);
		}
		else
		{
			Audio.PlayClip(m_slotUnavailableChoice, loop: false);
		}
	}

	private void UpdateRingCount(int ringCost)
	{
		m_totalRingCost += ringCost;
		m_ringCountLabel.text = LanguageUtils.FormatNumber(RingStorage.TotalBankedRings - m_totalRingCost);
	}

	private void Event_OnStorePurchaseCompleted(StoreContent.StoreEntry thisEntry, StorePurchases.Result result)
	{
		PowerUps.Type? requestedType = m_requestedType;
		if (requestedType.HasValue)
		{
			if (result == StorePurchases.Result.Success)
			{
				AddBoosterToSlot(m_requestedType.GetValueOrDefault(), m_requestedRingCost);
			}
			if (m_overlayShowing)
			{
				DialogStack.HideDialog();
			}
			m_requestedType = null;
		}
		BlockAllButtons(blockVal: false);
	}

	private void Trigger_RequestStartGame()
	{
		if (m_homePressed)
		{
			return;
		}
		m_playPressed = true;
		if (m_handHolder.CanRun())
		{
			if (GCState.IsCurrentChallengeActive())
			{
				if (GCDialogManager.ShouldShowChallengeInvolvementDialog())
				{
					GCDialogManager.ShowChallengeInvolvementDialog();
				}
				else if (GCDialogManager.ShouldShowNoConnectionDialog())
				{
					GCDialogManager.ShowNoConnectionDialog();
				}
				else
				{
					StartTheGame();
				}
			}
			else
			{
				StartTheGame();
			}
		}
		else
		{
			Audio.PlayClip(m_slotUnavailableChoice, loop: false);
		}
	}

	private void StartTheGame()
	{
		for (int i = 0; i < m_slotDesc.Count; i++)
		{
			SlotDesc slotDesc = m_slotDesc[i];
			PowerUps.Type? type = slotDesc.m_type;
			if (type.HasValue)
			{
				if (slotDesc.m_ringCost > 0)
				{
					PowerUps.Type valueOrDefault = slotDesc.m_type.GetValueOrDefault();
					string entryID = StoreEntry(valueOrDefault);
					StorePurchases.RequestPurchase(entryID, StorePurchases.LowCurrencyResponse.PurchaseCurrencyAndItem);
				}
				Boosters.SelectBooster(slotDesc.m_type.GetValueOrDefault());
			}
		}
		if (GameState.GetMode() == GameState.Mode.PauseMenu)
		{
			m_restartGameTrigger.SendMessage("OnClick");
			m_handHolder.CompleteHandholding();
		}
		else
		{
			m_startGameTrigger.SendMessage("OnClick");
			m_handHolder.CompleteHandholding();
		}
		Audio.PlayClip(m_slotAvailableChoice, loop: false);
	}

	private void RemoveBooster(PowerUps.Type boosterType)
	{
		for (int i = 0; i < m_slotDesc.Count; i++)
		{
			PowerUps.Type? type = m_slotDesc[i].m_type;
			if (type.HasValue && m_slotDesc[i].m_type == boosterType)
			{
				RemoveBoosterSlot(i);
			}
		}
		Audio.PlayClip(m_slotRemove, loop: false);
		m_handHolder.UpdatePromptButtonText(GetNumberOfFreeSlots());
	}

	private void RemoveBoosterSlot(int slot)
	{
		if (slot < m_slotDesc.Count)
		{
			PowerUps.Type? type = m_slotDesc[slot].m_type;
			if (type.HasValue)
			{
				PowerUps.Type valueOrDefault = m_slotDesc[slot].m_type.GetValueOrDefault();
				SetBooster(valueOrDefault, enable: true);
				SlotDesc slotDesc = m_slotDesc[slot];
				if (slotDesc.m_ringCost > 0)
				{
					UpdateRingCount(-slotDesc.m_ringCost);
				}
				slotDesc.m_mesh.mesh = null;
				slotDesc.m_type = null;
				slotDesc.m_ringCost = 0;
				slotDesc.m_removeBtn.gameObject.SetActive(value: false);
				SetLocalisedLabel(m_descriptionLabel, -1);
			}
		}
		Audio.PlayClip(m_slotRemove, loop: false);
		m_handHolder.UpdatePromptButtonText(GetNumberOfFreeSlots());
	}

	private void BlockAllButtons(bool blockVal)
	{
		int num = 0;
		int num2 = 0;
		for (int i = 0; i < m_boosterDesc.Count; i++)
		{
			m_boosterDesc[i].m_spinner.SetActive(blockVal);
			if (blockVal)
			{
				m_boosterDesc[i].m_countLabel.transform.gameObject.SetActive(value: false);
				m_boosterDesc[i].m_costLabel.transform.parent.gameObject.SetActive(value: false);
				continue;
			}
			num = GetBoosterCost(StoreEntry(m_boosterDesc[i].m_type));
			num2 = PowerUpsInventory.GetPowerUpCount(m_boosterDesc[i].m_type);
			if (num2 == 0)
			{
				m_boosterDesc[i].m_countLabel.transform.gameObject.SetActive(value: false);
				m_boosterDesc[i].m_costLabel.transform.parent.gameObject.SetActive(value: true);
				m_boosterDesc[i].m_costLabel.text = num.ToString();
			}
			else if (num2 > 0)
			{
				m_boosterDesc[i].m_countLabel.transform.gameObject.SetActive(value: true);
				m_boosterDesc[i].m_countLabel.text = "x" + num2;
				m_boosterDesc[i].m_costLabel.transform.parent.gameObject.SetActive(value: false);
			}
			else
			{
				m_boosterDesc[i].m_countLabel.transform.gameObject.SetActive(value: false);
				m_boosterDesc[i].m_costLabel.transform.parent.gameObject.SetActive(value: false);
			}
		}
		for (int j = 0; j < m_blockers.Length; j++)
		{
			m_blockers[j].Blocked = blockVal;
		}
	}

	private void Trigger_AddSpringBonus()
	{
		if (m_boosterDesc[GetBoosterIndex(PowerUps.Type.Booster_SpringBonus)].m_btnEnabled.activeSelf)
		{
			RequestBooster(PowerUps.Type.Booster_SpringBonus);
		}
		else
		{
			Trigger_RemoveSpringBonus();
		}
	}

	private void Trigger_AddRingStreakCombo()
	{
		if (m_boosterDesc[GetBoosterIndex(PowerUps.Type.Booster_RingStreakBonus)].m_btnEnabled.activeSelf)
		{
			RequestBooster(PowerUps.Type.Booster_RingStreakBonus);
		}
		else
		{
			Trigger_RemoveRingStreakCombo();
		}
	}

	private void Trigger_AddEnemyCombo()
	{
		if (m_boosterDesc[GetBoosterIndex(PowerUps.Type.Booster_EnemyComboBonus)].m_btnEnabled.activeSelf)
		{
			RequestBooster(PowerUps.Type.Booster_EnemyComboBonus);
		}
		else
		{
			Trigger_RemoveEnemyCombo();
		}
	}

	private void Trigger_AddScoreMultiplier()
	{
		if (m_boosterDesc[GetBoosterIndex(PowerUps.Type.Booster_ScoreMultiplier)].m_btnEnabled.activeSelf)
		{
			RequestBooster(PowerUps.Type.Booster_ScoreMultiplier);
		}
		else
		{
			Trigger_RemoveScoreMultiplier();
		}
	}

	private void Trigger_AddGoldenEnemy()
	{
		if (m_boosterDesc[GetBoosterIndex(PowerUps.Type.Booster_GoldenEnemy)].m_btnEnabled.activeSelf)
		{
			RequestBooster(PowerUps.Type.Booster_GoldenEnemy);
		}
		else
		{
			Trigger_RemoveGoldenEnemy();
		}
	}

	private void Trigger_RemoveSpringBonus()
	{
		RemoveBooster(PowerUps.Type.Booster_SpringBonus);
	}

	private void Trigger_RemoveRingStreakCombo()
	{
		RemoveBooster(PowerUps.Type.Booster_RingStreakBonus);
	}

	private void Trigger_RemoveEnemyCombo()
	{
		RemoveBooster(PowerUps.Type.Booster_EnemyComboBonus);
	}

	private void Trigger_RemoveScoreMultiplier()
	{
		RemoveBooster(PowerUps.Type.Booster_ScoreMultiplier);
	}

	private void Trigger_RemoveGoldenEnemy()
	{
		RemoveBooster(PowerUps.Type.Booster_GoldenEnemy);
	}

	private void Trigger_RemoveSlot1()
	{
		RemoveBoosterSlot(0);
	}

	private void Trigger_RemoveSlot2()
	{
		RemoveBoosterSlot(1);
	}

	private void Trigger_RemoveSlot3()
	{
		RemoveBoosterSlot(2);
	}

	private void Trigger_DisplayHelpDialog()
	{
		DialogStack.ShowDialog("Boosters Help");
	}

	private void SetLocalisedLabel(UILabel descriptionLabel, int boosterIndex)
	{
		LocalisedStringProperties component = descriptionLabel.GetComponent<LocalisedStringProperties>();
		if (boosterIndex == -1)
		{
			component.SetLocalisationID(s_emptyDesc);
		}
		else if (boosterIndex == GetBoosterIndex(PowerUps.Type.Booster_ScoreMultiplier))
		{
			string @string = LanguageStrings.First.GetString(s_boosterDescs[boosterIndex]);
			string text = string.Format(@string, Boosters.ScoreMultiplier * 100f);
			descriptionLabel.text = text;
			component.SetLocalisationID(null);
		}
		else if (boosterIndex == GetBoosterIndex(PowerUps.Type.Booster_GoldenEnemy))
		{
			string string2 = LanguageStrings.First.GetString(s_boosterDescs[boosterIndex]);
			string text2 = string.Format(string2, Boosters.GoldenEnemyScoreMultipler);
			descriptionLabel.text = text2;
			component.SetLocalisationID(null);
		}
		else
		{
			component.SetLocalisationID(s_boosterDescs[boosterIndex]);
		}
		LocalisedStringStatic component2 = descriptionLabel.GetComponent<LocalisedStringStatic>();
		component2.ForceStringUpdate();
	}
}
