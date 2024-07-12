using System.Runtime.InteropServices;

public struct ImageData
{
	[MarshalAs(UnmanagedType.Bool)]
	private bool m_loaded;

	[MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)]
	public sbyte[] m_image;
}
