namespace AblApi.Common.Extensions;

public static class DateTimeExt
{
    public static bool IsWithinSecondsOf(this DateTime dt1, DateTime dt2, double seconds)
    {
        return Math.Abs((dt1 - dt2).TotalSeconds) < seconds;
    }

    public static bool IsWithinHoursOf(this DateTime dt1, DateTime dt2, double hours)
    {
        return Math.Abs((dt1 - dt2).TotalSeconds / 3600) < hours;
    }
}
