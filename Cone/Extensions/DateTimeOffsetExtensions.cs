namespace Cone.Extensions;

public static class DateTimeOffsetExtensions
{
    public static DateTimeOffset ToJst(this DateTimeOffset dateTimeOffset)
    {
        var timezoneInfo = TimeZoneInfo.FindSystemTimeZoneById("Asia/Tokyo");
        return TimeZoneInfo.ConvertTime(dateTimeOffset, timezoneInfo);
    }
}