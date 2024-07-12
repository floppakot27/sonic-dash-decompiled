using UnityEngine;

public class GCDialogManager : MonoBehaviour
{
	private const string m_gcLiveDialogShownSave = "GCLiveDialogShown";

	private const string m_gcInvolvementDialogShownSave = "GCInvolvementDialogShown";

	private const string m_gcFirstTimeVisitDialogShownSave = "GCFirstTimeVisitDialogShown";

	private static GCDialogManager s_singleton;

	private bool m_gcNoConnectionDialogShownThisSession;

	private bool[] m_shownActiveDialog = new bool[2];

	private bool[] m_shownInvolvementDialog = new bool[2];

	private bool[] m_shownFirstTimeVisitDialog = new bool[2];

	private string m_gcLiveDialogShownState = string.Empty;

	private string m_gcInvolvementDialogShownState = string.Empty;

	private string m_gcFirstTimeVisitDialogShownState = string.Empty;

	private void Start()
	{
		s_singleton = this;
		m_gcNoConnectionDialogShownThisSession = false;
		EventDispatch.RegisterInterest("OnGameDataLoaded", this);
		EventDispatch.RegisterInterest("OnGameDataSaveRequest", this);
	}

	public static bool ShouldShowChallenegeActiveDialog()
	{
		if (GCState.ChallengeState(GCState.Challenges.gc3) != GCState.State.Active)
		{
			return false;
		}
		string[] array = s_singleton.m_gcLiveDialogShownState.Split(',');
		int result = 0;
		bool.TryParse(array[0], out s_singleton.m_shownActiveDialog[0]);
		int.TryParse(array[1], out result);
		if (result == 2)
		{
			s_singleton.m_shownActiveDialog[1] = true;
		}
		else
		{
			s_singleton.m_shownActiveDialog[1] = false;
		}
		if (!s_singleton.m_shownActiveDialog[0] || !s_singleton.m_shownActiveDialog[1])
		{
			return true;
		}
		return false;
	}

	public static void ShowChallengeActiveDialog()
	{
		Dialog_GlobalChallengeActive.Display();
		s_singleton.m_shownActiveDialog[0] = true;
		s_singleton.m_shownActiveDialog[1] = true;
		s_singleton.m_gcLiveDialogShownState = s_singleton.m_shownActiveDialog[0] + "," + 2;
		PropertyStore.Save();
	}

	public static bool ShouldShowChallengeInvolvementDialog()
	{
		if (GCState.ChallengeState(GCState.Challenges.gc3) != GCState.State.Active)
		{
			return false;
		}
		string[] array = s_singleton.m_gcInvolvementDialogShownState.Split(',');
		int result = 0;
		bool.TryParse(array[0], out s_singleton.m_shownInvolvementDialog[0]);
		int.TryParse(array[1], out result);
		if (result == 2)
		{
			s_singleton.m_shownInvolvementDialog[1] = true;
		}
		else
		{
			s_singleton.m_shownInvolvementDialog[1] = false;
		}
		if (!s_singleton.m_shownInvolvementDialog[0] || !s_singleton.m_shownInvolvementDialog[1])
		{
			return true;
		}
		return false;
	}

	public static void ShowChallengeInvolvementDialog()
	{
		Dialog_GCPlayerInvolvement.Display();
		s_singleton.m_shownInvolvementDialog[0] = true;
		s_singleton.m_shownInvolvementDialog[1] = true;
		s_singleton.m_gcInvolvementDialogShownState = s_singleton.m_shownInvolvementDialog[0] + "," + 2;
		PropertyStore.Save();
	}

	public static bool ShouldShowFirstTimeVisitDialog()
	{
		if (GCState.ChallengeState(GCState.Challenges.gc3) != GCState.State.Active)
		{
			return false;
		}
		if (GC3Progress.CollectedThisRun() == 0)
		{
			return false;
		}
		string[] array = s_singleton.m_gcFirstTimeVisitDialogShownState.Split(',');
		int result = 0;
		bool.TryParse(array[0], out s_singleton.m_shownFirstTimeVisitDialog[0]);
		int.TryParse(array[1], out result);
		if (result == 2)
		{
			s_singleton.m_shownFirstTimeVisitDialog[1] = true;
		}
		else
		{
			s_singleton.m_shownFirstTimeVisitDialog[1] = false;
		}
		if (!s_singleton.m_shownFirstTimeVisitDialog[0] || !s_singleton.m_shownFirstTimeVisitDialog[1])
		{
			return true;
		}
		return false;
	}

	public static void ShowFirstTimeVisitDialog()
	{
		Dialog_GCFirstTimeVisit.Display();
		s_singleton.m_shownFirstTimeVisitDialog[0] = true;
		s_singleton.m_shownFirstTimeVisitDialog[1] = true;
		s_singleton.m_gcFirstTimeVisitDialogShownState = s_singleton.m_shownFirstTimeVisitDialog[0] + "," + 2;
		PropertyStore.Save();
	}

	public static bool ShouldShowNoConnectionDialog()
	{
		if (Internet.ConnectionAvailable())
		{
			return false;
		}
		if (!s_singleton.m_gcNoConnectionDialogShownThisSession)
		{
			s_singleton.m_gcNoConnectionDialogShownThisSession = true;
			return true;
		}
		return false;
	}

	public static void ShowNoConnectionDialog()
	{
		Dialog_GCNoConnection.Display();
	}

	private void Event_OnGameDataSaveRequest()
	{
		PropertyStore.Store("GCLiveDialogShown", m_gcLiveDialogShownState);
		PropertyStore.Store("GCInvolvementDialogShown", m_gcInvolvementDialogShownState);
		PropertyStore.Store("GCFirstTimeVisitDialogShown", m_gcFirstTimeVisitDialogShownState);
	}

	private void Event_OnGameDataLoaded(ActiveProperties ap)
	{
		m_gcLiveDialogShownState = ap.GetString("GCLiveDialogShown");
		if (m_gcLiveDialogShownState == null || m_gcLiveDialogShownState == string.Empty)
		{
			m_gcLiveDialogShownState = "False," + 2;
		}
		m_gcInvolvementDialogShownState = ap.GetString("GCInvolvementDialogShown");
		if (m_gcInvolvementDialogShownState == null || m_gcInvolvementDialogShownState == string.Empty)
		{
			m_gcInvolvementDialogShownState = "False," + 2;
		}
		m_gcFirstTimeVisitDialogShownState = ap.GetString("GCFirstTimeVisitDialogShown");
		if (m_gcFirstTimeVisitDialogShownState == null || m_gcFirstTimeVisitDialogShownState == string.Empty)
		{
			m_gcFirstTimeVisitDialogShownState = "False," + 2;
		}
	}
}
