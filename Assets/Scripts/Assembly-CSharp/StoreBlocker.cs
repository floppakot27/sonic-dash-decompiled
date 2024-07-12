using UnityEngine;

public class StoreBlocker : MonoBehaviour
{
	public enum BlockingMode
	{
		AutoBlock,
		ManualBlock
	}

	[SerializeField]
	private GameObject m_storeActive;

	[SerializeField]
	private GameObject m_storeIdle;

	[SerializeField]
	private BlockingMode m_blockingMode;

	private void Update()
	{
		bool flag = StoreUtils.IsStoreActive();
		EnableStoreAccess(!flag);
	}

	private void EnableStoreAccess(bool accessEnabled)
	{
		if (m_blockingMode == BlockingMode.AutoBlock)
		{
			GuiButtonBlocker component = GetComponent<GuiButtonBlocker>();
			if (component != null)
			{
				component.Blocked = !accessEnabled;
			}
		}
		m_storeActive.SetActive(!accessEnabled);
		m_storeIdle.SetActive(accessEnabled);
	}

	public void ChangeIdleObject(GameObject newIdle)
	{
		m_storeIdle = newIdle;
	}
}
