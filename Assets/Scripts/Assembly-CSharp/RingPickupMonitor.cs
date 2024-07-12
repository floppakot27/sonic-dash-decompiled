using UnityEngine;

public class RingPickupMonitor : MonoBehaviour
{
	private static RingPickupMonitor m_instance;

	private int m_pickupCount;

	private void Start()
	{
		m_instance = this;
		EventDispatch.RegisterInterest("OnNewGameStarted", this);
	}

	private void Event_OnNewGameStarted()
	{
		m_pickupCount = 0;
	}

	public static RingPickupMonitor instance()
	{
		return m_instance;
	}

	public int GetPickupCount()
	{
		return m_pickupCount;
	}

	public void ResetPickupCount()
	{
		m_pickupCount = 0;
	}

	public void PickupRings(int rings)
	{
		m_pickupCount += rings;
	}
}
