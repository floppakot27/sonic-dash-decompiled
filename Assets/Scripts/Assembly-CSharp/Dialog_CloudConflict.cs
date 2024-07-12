using UnityEngine;

public class Dialog_CloudConflict : MonoBehaviour
{
	[SerializeField]
	private UILabel m_localRingsLabel;

	[SerializeField]
	private UILabel m_localRSRsLabel;

	[SerializeField]
	private UILabel m_localHighScoreLabel;

	[SerializeField]
	private UILabel m_localDescLabel;

	[SerializeField]
	private UILabel m_remoteRingsLabel;

	[SerializeField]
	private UILabel m_remoteRSRsLabel;

	[SerializeField]
	private UILabel m_remoteHighScoreLabel;

	[SerializeField]
	private UILabel m_remoteDescLabel;

	private static bool s_dialogActive;

	public static void Display()
	{
		if (!s_dialogActive)
		{
			DialogStack.ShowDialog("Cloud Conflict Dialog");
		}
	}

	private void OnEnable()
	{
		s_dialogActive = true;
		Populate();
	}

	private void OnDisable()
	{
		s_dialogActive = false;
	}

	private void Populate()
	{
		if (m_localRingsLabel != null)
		{
			m_localRingsLabel.text = RingStorage.TotalBankedRings.ToString();
		}
		if (m_localRSRsLabel != null)
		{
			m_localRSRsLabel.text = RingStorage.TotalStarRings.ToString();
		}
		if (m_localHighScoreLabel != null)
		{
			m_localHighScoreLabel.text = ScoreTracker.HighScore.ToString();
		}
		if (m_localDescLabel != null)
		{
			m_localDescLabel.text = CloudStorage.DeviceDesc;
		}
		if (m_remoteRingsLabel != null)
		{
			m_remoteRingsLabel.text = CloudStorage.GetCloudProperty("Banked Rings Total", fromLocal: false);
		}
		if (m_remoteRSRsLabel != null)
		{
			m_remoteRSRsLabel.text = CloudStorage.GetCloudProperty("Star Rings Total", fromLocal: false);
		}
		if (m_remoteHighScoreLabel != null)
		{
			m_remoteHighScoreLabel.text = CloudStorage.GetCloudProperty(ScoreTracker.HighestScoreSavedProperty, fromLocal: false);
		}
		if (m_remoteDescLabel != null)
		{
			m_remoteDescLabel.text = CloudStorage.GetCloudPropertyDeviceDesc(fromLocal: false);
		}
	}

	private void Trigger_ConflictKeepLocal()
	{
		CloudStorage.Save(overrideCloud: true);
	}

	private void Trigger_ConflictKeepRemote()
	{
		CloudStorage.Load();
	}
}
