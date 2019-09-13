using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class DateTimeUtils {

	public static float SecondsTillMidnight() {
		DateTime now = UnbiasedTime.Instance.Now();
		int hours = 0, minutes = 0, seconds = 0, totalSeconds = 0;
		hours = (24 - now.Hour) - 1;
		minutes = (60 - now.Minute) - 1;
		seconds = (60 - now.Second - 1);

		totalSeconds = seconds + (minutes * 60) + (hours * 3600);

		return totalSeconds;
	}

	public static long GetCurrentTime()
	{
		var epochStart = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
		long currentEpochTime = (int) (DateTime.UtcNow - epochStart).TotalSeconds;
		return currentEpochTime;
	}

    public static string DateTimeToISO8601(DateTime dateTime) {
        return dateTime.ToString("yyyy-MM-ddTHH:mm:ssZ");
    }
}
