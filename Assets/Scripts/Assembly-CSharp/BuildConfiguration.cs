public class BuildConfiguration
{
	public enum Build
	{
		Development = 1,
		FinalRelease = 2,
		Distribution = 4,
		Review = 8
	}

	public static Build Current => Build.Distribution;
}
