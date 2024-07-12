using System.Globalization;

public class LanguageUtils
{
	private static NumberFormatInfo s_numberFormatter;

	public static string FormatNumber(long numberToFormat)
	{
		if (s_numberFormatter == null)
		{
			InitialiseNumberFormatter();
		}
		if (s_numberFormatter == null)
		{
			return numberToFormat.ToString();
		}
		return numberToFormat.ToString("n", s_numberFormatter);
	}

	private static void InitialiseNumberFormatter()
	{
		if (!(LanguageStrings.First == null))
		{
			s_numberFormatter = (NumberFormatInfo)CultureInfo.InvariantCulture.NumberFormat.Clone();
			string text = LanguageStrings.First.GetString("NUMBER_SEPARATOR");
			if (text == "*")
			{
				text = " ";
			}
			s_numberFormatter.NumberGroupSeparator = text;
			s_numberFormatter.NumberDecimalDigits = 0;
		}
	}
}
