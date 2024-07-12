using UnityEngine;

public class CharacterSelection : MonoBehaviour
{
	public static string[] m_storeEntries = new string[9]
	{
		string.Empty,
		"Character Tails",
		"Character Knuckles",
		"Character Amy",
		"Character Shadow",
		"Character Blaze",
		"Character Silver",
		"Character Rouge",
		"Character Cream"
	};

	private object[] m_eventParams = new object[1];

	private StoreContent.StoreEntry m_pendingPurchase;

	private Characters.Type m_currentDisplayedCharacter;

	[SerializeField]
	private GameObject m_playGroup;

	[SerializeField]
	private GameObject m_costGroup;

	[SerializeField]
	private GameObject m_activeGroup;

	[SerializeField]
	private GuiButtonBlocker[] m_buttonBlockers;

	[SerializeField]
	private GameObject m_startGameTrigger;

	[SerializeField]
	private GameObject m_nextMenuTrigger;

	[SerializeField]
	private UILabel m_costLabel;

	private SimpleGestureMonitor m_simpleGestureMonitor;

	private void Start()
	{
		m_simpleGestureMonitor = new SimpleGestureMonitor();
	}

	private void OnEnable()
	{
		EventDispatch.GenerateEvent("OnCharacterSelectEnabled");
		if (m_simpleGestureMonitor != null)
		{
			m_simpleGestureMonitor.reset();
		}
		EventDispatch.RegisterInterest("OnStorePurchaseCompleted", this);
	}

	private void OnDisable()
	{
		m_pendingPurchase = null;
		EventDispatch.UnregisterInterest("OnStorePurchaseCompleted", this);
	}

	private void Update()
	{
		UpdateCharacterCost();
		bool buttonBlocked = UpdateButtonDisplay();
		UpdateButtonState(buttonBlocked);
		m_simpleGestureMonitor.Update();
		if (m_simpleGestureMonitor.swipeLeftDetected() && m_simpleGestureMonitor.GestureStartPosition.x > (float)Screen.width * 0.5f)
		{
			Trigger_CharacterNext();
		}
		else if (m_simpleGestureMonitor.swipeRightDetected() && m_simpleGestureMonitor.GestureStartPosition.x < (float)Screen.width * 0.5f)
		{
			Trigger_CharacterPrev();
		}
	}

	private void UpdateCharacterCost()
	{
		Characters.Type currentCharacterSelection = CharacterManager.Singleton.GetCurrentCharacterSelection();
		bool flag = Characters.CharacterUnlocked(currentCharacterSelection);
		if (currentCharacterSelection != m_currentDisplayedCharacter && !flag)
		{
			StoreContent.StoreEntry storeEntry = StoreContent.GetStoreEntry(m_storeEntries[(int)currentCharacterSelection], StoreContent.Identifiers.Name);
			m_costLabel.text = StoreUtils.GetItemCost(storeEntry, StoreUtils.EntryType.Player).ToString();
		}
		m_currentDisplayedCharacter = currentCharacterSelection;
	}

	private bool UpdateButtonDisplay()
	{
		bool result = false;
		Characters.Type currentCharacterSelection = CharacterManager.Singleton.GetCurrentCharacterSelection();
		if (Characters.CharacterUnlocked(currentCharacterSelection))
		{
			m_playGroup.SetActive(value: true);
			m_costGroup.SetActive(value: false);
			m_activeGroup.SetActive(value: false);
		}
		else
		{
			m_playGroup.SetActive(value: false);
			if (StoreUtils.IsStoreActive())
			{
				m_costGroup.SetActive(value: false);
				m_activeGroup.SetActive(value: true);
				result = true;
			}
			else
			{
				m_costGroup.SetActive(value: true);
				m_activeGroup.SetActive(value: false);
			}
		}
		return result;
	}

	private void UpdateButtonState(bool buttonBlocked)
	{
		for (int i = 0; i < m_buttonBlockers.Length; i++)
		{
			m_buttonBlockers[i].Blocked = buttonBlocked;
		}
	}

	private void StartCharacterPurchase(Characters.Type currentCharacter)
	{
		m_pendingPurchase = StoreContent.GetStoreEntry(m_storeEntries[(int)currentCharacter], StoreContent.Identifiers.Name);
		StorePurchases.RequestPurchase(m_storeEntries[(int)currentCharacter], StorePurchases.LowCurrencyResponse.PurchaseCurrencyAndItem);
	}

	private void MoveToNextMenu()
	{
		if (TutorialSystem.instance().isTrackTutorialEnabled())
		{
			m_startGameTrigger.SendMessage("OnClick");
		}
		else
		{
			m_nextMenuTrigger.SendMessage("OnClick");
		}
		m_pendingPurchase = null;
	}

	private void Trigger_CharacterScrolling()
	{
		m_eventParams[0] = true;
		EventDispatch.GenerateEvent("OnCharacterSelection", m_eventParams);
		m_pendingPurchase = null;
	}

	private void Trigger_CharacterNext()
	{
		CharacterManager.Singleton.Next();
		Trigger_CharacterIdle();
	}

	private void Trigger_CharacterPrev()
	{
		CharacterManager.Singleton.Previous();
		Trigger_CharacterIdle();
	}

	private void Trigger_CharacterIdle()
	{
		m_eventParams[0] = false;
		EventDispatch.GenerateEvent("OnCharacterSelection", m_eventParams);
	}

	private void Trigger_OnPlaySelected()
	{
		Characters.Type currentCharacterSelection = CharacterManager.Singleton.GetCurrentCharacterSelection();
		if (!Characters.CharacterUnlocked(currentCharacterSelection))
		{
			StartCharacterPurchase(currentCharacterSelection);
		}
		else
		{
			MoveToNextMenu();
		}
	}

	private void Event_OnStorePurchaseCompleted(StoreContent.StoreEntry thisEntry, StorePurchases.Result result)
	{
		if (thisEntry == m_pendingPurchase && thisEntry != null)
		{
			m_pendingPurchase = null;
			if (result == StorePurchases.Result.Success)
			{
				MoveToNextMenu();
			}
		}
	}
}
