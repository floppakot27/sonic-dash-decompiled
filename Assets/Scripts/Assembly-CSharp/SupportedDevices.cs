using System;
using UnityEngine;

public class SupportedDevices : MonoBehaviour
{
	[Flags]
	public enum iOS
	{
		iUnsupported = 1,
		iPhone3GS = 2,
		iPod3 = 4,
		iPad1 = 8,
		iPhone4 = 0x10,
		iPod4 = 0x20,
		iPad2 = 0x40,
		iPhone4S = 0x80,
		iPad3 = 0x100,
		iPhone5 = 0x200,
		iPod5 = 0x400,
		iPad4 = 0x800,
		iFuture = 0x1000
	}

	[Serializable]
	public class Support
	{
		public string m_supportID = string.Empty;

		public uint m_supportIdCRC;

		public bool m_editorSupport = true;

		public iOS m_iosSupport;

		public bool m_androidSupport = true;
	}
}
