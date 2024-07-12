using System;
using System.Globalization;

public class DCTimeValidation
{
	private const int m_hoursMargin = -13;

	private const string m_propertyPreviousTime = "CDPreviousTime";

	private const string m_propertyTrustedPreviousTime = "CDTrustedPreviousTime";

	private DateTime m_previousCheckTime;

	private bool m_trustedPreviousCheckTime;

	public static bool TrustedTime { get; set; }

	public DCTimeValidation()
	{
		m_previousCheckTime = DCTime.GetCurrentTime();
	}

	public void EnforceValidTime(bool save)
	{
		DateTime currentTime = DCTime.GetCurrentTime();
		TimeSpan timeSpan = currentTime - m_previousCheckTime;
		if (currentTime < m_previousCheckTime && !TrustedTime && !m_trustedPreviousCheckTime && DCTime.TriedNTPTime)
		{
			DCs.SetToCheatingState(save);
		}
		else if (timeSpan.TotalHours < -13.0 && !TrustedTime && m_trustedPreviousCheckTime && DCTime.TriedNTPTime)
		{
			DCs.SetToCheatingState(save);
		}
		else if (TrustedTime && !m_trustedPreviousCheckTime)
		{
			DCs.EnforceDCActualDate();
		}
		m_previousCheckTime = DCTime.GetCurrentTime();
		m_trustedPreviousCheckTime = TrustedTime;
	}

	public void Save()
	{
		CultureInfo cultureInfo = new CultureInfo("en-US");
		PropertyStore.Store("CDPreviousTime", m_previousCheckTime.ToString(cultureInfo.DateTimeFormat));
		PropertyStore.Store("CDTrustedPreviousTime", m_trustedPreviousCheckTime);
	}

	public void Load(ActiveProperties activeProperties)
	{
		CultureInfo provider = new CultureInfo("en-US");
		if (!DateTime.TryParse(activeProperties.GetString("CDPreviousTime"), provider, DateTimeStyles.None, out m_previousCheckTime))
		{
			m_previousCheckTime = DCTime.GetCurrentTime().AddYears(-1).Date;
			m_trustedPreviousCheckTime = TrustedTime;
		}
		else
		{
			m_trustedPreviousCheckTime = activeProperties.GetBool("CDTrustedPreviousTime");
		}
	}
}
