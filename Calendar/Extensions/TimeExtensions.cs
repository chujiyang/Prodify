﻿namespace Calendar.Extensions;

internal static class TimeExtensions
{
    public static DateTime ToShortestDateTime(this DateTime time)
    {
        return new DateTime(time.Year, time.Month, time.Day);
    }

    public static DateTime FirstDayOfWeek(this DateTime time)
    {
        var firstDayOfWeek = time.AddDays(-(int)(time.DayOfWeek));
        return firstDayOfWeek.ToShortestDateTime();
    }

    public static DateTime FirstDayOfMonth(this DateTime time)
    {
        return new DateTime(time.Year, time.Month, 1);
    }

    public static DateTime CurrentWeek(this DateTime from, DayOfWeek dayOfWeek)
    {
        int start = (int)from.DayOfWeek;
        int target = (int)dayOfWeek;
        return from.AddDays(target - start);
    }

    public static DateTime NextWeek(this DateTime from, DayOfWeek dayOfWeek)
    {
        int start = (int)from.DayOfWeek;
        int target = (int)dayOfWeek;
        if (target <= start)
            target += 7;
        return from.AddDays(target - start);
    }

    public static DateTime RoundUpToQuarterHour(this DateTime dateTime)
    {
        var newMinute = dateTime.Minute - (dateTime.Minute % 15);
        if (newMinute != 0)
        {
            newMinute += 15;
        }

        return new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, 0, 0, 0, dateTime.Kind).AddMinutes(newMinute);
    }
}
