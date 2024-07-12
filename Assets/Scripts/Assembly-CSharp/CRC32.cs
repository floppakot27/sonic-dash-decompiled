public class CRC32
{
	public enum Case
	{
		Upper,
		Lower,
		AsIs
	}

	private const uint Polynomial = 3988292384u;

	private const uint CRCSeed = uint.MaxValue;

	private const uint TableSize = 256u;

	private static uint[] s_table;

	static CRC32()
	{
		InitialiseTable();
	}

	public static uint Generate(string crcSource, Case stringCase)
	{
		string text = crcSource;
		switch (stringCase)
		{
		case Case.Lower:
			text = text.ToLower();
			break;
		case Case.Upper:
			text = text.ToUpper();
			break;
		}
		uint num = uint.MaxValue;
		string text2 = text;
		for (int i = 0; i < text2.Length; i++)
		{
			byte b = (byte)text2[i];
			num = (num >> 8) ^ s_table[b ^ (num & 0xFF)];
		}
		return num;
	}

	public static uint Generate(byte[] crcSource)
	{
		uint num = uint.MaxValue;
		foreach (byte b in crcSource)
		{
			num = (num >> 8) ^ s_table[b ^ (num & 0xFF)];
		}
		return num;
	}

	private static void InitialiseTable()
	{
		s_table = new uint[256];
		for (int i = 0; (long)i < 256L; i++)
		{
			uint num = (uint)i;
			for (int j = 0; j < 8; j++)
			{
				num = (((num & (true ? 1u : 0u)) != 0) ? ((num >> 1) ^ 0xEDB88320u) : (num >> 1));
			}
			s_table[i] = num;
		}
	}
}
