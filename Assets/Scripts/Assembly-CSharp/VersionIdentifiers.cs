public class VersionIdentifiers
{
	public enum VersionStatus
	{
		Equal,
		Higher,
		Lower
	}

	public const string Version = "1.8.0";

	public const string Assets = "1.8.0";

	public const string BundleIdentifier = "** Local Build **";

	public const string BundleVersion = "** Local Build **";

	public static VersionStatus CheckVersionNumbers(string versionToCheck, string expectedVersion)
	{
		string[] array = expectedVersion.Split('.');
		string[] array2 = versionToCheck.Split('.');
		int[] array3 = new int[3]
		{
			int.Parse(array[0]),
			int.Parse(array[1]),
			int.Parse(array[2])
		};
		int[] array4 = new int[3]
		{
			int.Parse(array2[0]),
			int.Parse(array2[1]),
			int.Parse(array2[2])
		};
		for (int i = 0; i < 3; i++)
		{
			if (array4[i] > array3[i])
			{
				return VersionStatus.Higher;
			}
			if (array4[i] < array3[i])
			{
				return VersionStatus.Lower;
			}
		}
		return VersionStatus.Equal;
	}
}
