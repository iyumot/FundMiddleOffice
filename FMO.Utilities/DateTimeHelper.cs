using System.Text.Json;
using System.Text.Json.Serialization;

namespace FMO.Utilities;

public static class DateTimeHelper
{
    private static DateTime start = new DateTime(1970, 1, 1);

    public static long TimeStampBySeconds(this DateTime dateTime)
    {
        return (long)(dateTime - start).TotalSeconds;
    }


    public static bool TryParse(string s, out DateOnly d)
    {
        if (DateOnly.TryParse(s, out d))
            return true;
        else if (DateOnly.TryParseExact(s, "yyyyMMdd", out d))
            return true;
        else if (DateOnly.TryParseExact(s, "yyyy-MM-dd", out d))
            return true;
        return false;
    }


}



public class DateTimeConverterUsingDateTimeParse : JsonConverter<DateTime>
{
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return DateTime.Parse(reader!.GetString() ?? "");
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString("yyyy-MM-dd HH:mm:ss"));
    }
}