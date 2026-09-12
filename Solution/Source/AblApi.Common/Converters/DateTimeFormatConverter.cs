using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AblApi.Common.Converters;

public class DateTimeFormatConverter(string format) : JsonConverter<DateTime>
{
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString();
        return DateTime.ParseExact(value!, format, CultureInfo.InvariantCulture);
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString(format, CultureInfo.InvariantCulture));
    }
}

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class JsonDateTimeFormatAttribute(string format) : JsonConverterAttribute
{
    public override JsonConverter? CreateConverter(Type typeToConvert)
    {
        return new DateTimeFormatConverter(format);
    }
}
