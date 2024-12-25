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