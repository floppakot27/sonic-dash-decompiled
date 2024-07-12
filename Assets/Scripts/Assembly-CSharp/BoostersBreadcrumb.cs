using UnityEngine;

public class BoostersBreadcrumb : MonoBehaviour
{
	public const int BreadcrumbCount = 5;

	private PowerUps.Type[] m_breadcrumbOrder = new PowerUps.Type[5]
	{
		PowerUps.Type.Booster_SpringBonus,
		PowerUps.Type.Booster_EnemyComboBonus,
		PowerUps.Type.Booster_RingStreakBonus,
		PowerUps.Type.Booster_ScoreMultiplier,
		PowerUps.Type.Booster_GoldenEnemy
	};

	private bool[] m_breadcrumbShown = new bool[5];

	public static BoostersBreadcrumb Instance { get; private set; }

	public int CurrentBreadcrumb { get; private set; }

	public StoreContent.StoreEntry CurrentBreadcrumbBooster { get; private set; }

	public bool BoostersHaveArrivedDialogShown { get; private set; }

	private static string CurrentBreadcrumbSaveProperty => "BoostersCurrentBreadcrumb";

	private static string BreadcrumbShownSaveProperty => "BoostersBreadcrumbShown";

	private static string AnnouncementShownSaveProperty => "BoosterAnnouncementShown";

	private void Awake()
	{
		if (Instance != null && Instance != this)
		{
			Object.Destroy(this);
		}
		else
		{
			Instance = this;
		}
		EventDispatch.RegisterInterest("OnGameDataLoaded", this);
		EventDispatch.RegisterInterest("OnGameDataSaveRequest", this);
	}

	public void ActivateBreadcrumb(PowerUps.Type booster)
	{
		int num = 0;
		for (int i = 0; i < 5; i++)
		{
			if (booster == m_breadcrumbOrder[i])
			{
				num = i;
			}
		}
		if (!m_breadcrumbShown[num])
		{
			string identifier = MenuBoosters.StoreEntry(booster);
			CurrentBreadcrumbBooster = StoreContent.GetStoreEntry(identifier, StoreContent.Identifiers.Name);
			Dialog_BoosterBreadcrumb.Display(CurrentBreadcrumbBooster);
			CurrentBreadcrumb++;
			m_breadcrumbShown[num] = true;
		}
	}

	public void ShowBoosterAnouncement()
	{
		if (!BoostersHaveArrivedDialogShown)
		{
			for (int i = 0; i < m_breadcrumbOrder.Length; i++)
			{
				PowerUpsInventory.ModifyPowerUpStock(m_breadcrumbOrder[i], 3);
			}
			Dialog_BoosterAnounce.Display();
			BoostersHaveArrivedDialogShown = true;
			PropertyStore.Save();
		}
	}

	public bool IsBoosterDiscovered(int index)
	{
		return m_breadcrumbShown[index];
	}

	public string GetBreadcrumbsShownSaveProperty()
	{
		string text = string.Empty;
		for (int i = 0; i < 5; i++)
		{
			text += m_breadcrumbShown[i];
			if (i != 4)
			{
				text += ",";
			}
		}
		return text;
	}

	private void Event_OnGameDataSaveRequest()
	{
		PropertyStore.Store(CurrentBreadcrumbSaveProperty, CurrentBreadcrumb);
		string breadcrumbsShownSaveProperty = GetBreadcrumbsShownSaveProperty();
		PropertyStore.Store(BreadcrumbShownSaveProperty, breadcrumbsShownSaveProperty);
		PropertyStore.Store(AnnouncementShownSaveProperty, BoostersHaveArrivedDialogShown);
	}

	private void Event_OnGameDataLoaded(ActiveProperties activeProperties)
	{
		CurrentBreadcrumb = activeProperties.GetInt("BoostersCurrentBreadcrumb");
		if (activeProperties.DoesPropertyExist("BoosterAnnouncementShown"))
		{
			BoostersHaveArrivedDialogShown = activeProperties.GetBool("BoosterAnnouncementShown");
		}
		else
		{
			BoostersHaveArrivedDialogShown = false;
		}
		string @string = activeProperties.GetString("BoostersBreadcrumbShown");
		if (@string != null)
		{
			string[] array = @string.Split(',');
			for (int i = 0; i < array.Length; i++)
			{
				m_breadcrumbShown[i] = bool.Parse(array[i]);
			}
		}
		else
		{
			m_breadcrumbShown = new bool[5];
		}
	}
}
