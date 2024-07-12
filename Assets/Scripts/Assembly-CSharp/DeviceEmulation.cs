using UnityEngine;

public class DeviceEmulation : MonoBehaviour
{
	[SerializeField]
	private bool m_emulateDevice;

	[SerializeField]
	private SupportedDevices.iOS m_iosDevice;

	public bool Emulate => m_emulateDevice;

	public SupportedDevices.iOS iOSDevice => m_iosDevice;
}
