using System;
using System.Collections;
using UnityEngine;

public class DCTime : MonoBehaviour
{
	private static DateTime s_currentTime;

	private static bool s_updateTime;

	public static bool TriedNTPTime { get; set; }

	public static DateTime GetCurrentTime()
	{
		if (DCTimeValidation.TrustedTime)
		{
			return s_currentTime.AddHours(DCServerTimeModification.HoursToAddToServerTime);
		}
		return s_currentTime;
	}

	private void Start()
	{
		s_currentTime = DateTime.UtcNow;
		s_updateTime = true;
		TriedNTPTime = false;
		SetInternalTime();
	}

	private void Update()
	{
		if (s_updateTime)
		{
			s_currentTime = s_currentTime.AddTicks((long)(IndependantTimeDelta.Delta * 10000000f));
		}
	}

	private void OnApplicationPause(bool paused)
	{
		if (paused)
		{
			TriedNTPTime = false;
		}
		else
		{
			SetInternalTime();
		}
	}

	private void SetInternalTime()
	{
		s_updateTime = true;
		StartCoroutine(GetWWWServerTime());
	}

	private IEnumerator GetWWWServerTime()
	{
		FileDownloader request = new FileDownloader(FileDownloader.Files.SegaTime, keepAndUseLocalCopy: false);
		yield return request.Loading;
		TriedNTPTime = true;
		if (request.Error == null && long.TryParse(request.Text, out var seconds))
		{
			s_currentTime = new DateTime(1970, 1, 1).AddSeconds(seconds);
			DCTimeValidation.TrustedTime = true;
		}
		else
		{
			s_currentTime = DateTime.UtcNow;
			DCTimeValidation.TrustedTime = false;
		}
	}
}
