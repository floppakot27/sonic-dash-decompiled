using UnityEngine;

public class DCNotificationIcon : MonoBehaviour
{
	private enum NotificationID
	{
		None,
		DailyChalenge,
		WheelOfFortune,
		MainMenu
	}

	[SerializeField]
	private NotificationID m_notificationIdentifier;

	private bool m_showNotification;

	private void Update()
	{
		switch (m_notificationIdentifier)
		{
		case NotificationID.DailyChalenge:
			m_showNotification = !DCs.IsChallengeCompleted();
			break;
		case NotificationID.WheelOfFortune:
			m_showNotification = !WheelOfFortuneSettings.Instance.KnowAboutFreeSpin && WheelOfFortuneSettings.Instance.HasFreeSpin;
			break;
		case NotificationID.MainMenu:
			m_showNotification = (!WheelOfFortuneSettings.Instance.KnowAboutFreeSpin && WheelOfFortuneSettings.Instance.HasFreeSpin) || !DCs.IsChallengeCompleted();
			break;
		default:
			m_showNotification = false;
			break;
		}
		ShowChildren(m_showNotification);
	}

	private void ShowChildren(bool show)
	{
		UIWidget[] componentsInChildren = GetComponentsInChildren<UIWidget>(includeInactive: true);
		if (componentsInChildren != null)
		{
			UIWidget[] array = componentsInChildren;
			foreach (UIWidget uIWidget in array)
			{
				uIWidget.gameObject.SetActive(show);
			}
		}
	}
}
