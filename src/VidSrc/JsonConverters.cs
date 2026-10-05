using System.Text.Json;
using System.Text.Json.Serialization;

namespace VidSrc.Models;

/// <summary>
/// VidSrc returns some numeric ids as JSON strings (<c>"tmdb_id":"68721"</c>) and others
/// as numbers depending on the endpoint. This converter accepts both.
/// </summary>
internal sealed class FlexibleInt32Converter : JsonConverter<int?>
{
    public override int? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => reader.TokenType switch
        {
            JsonTokenType.Number when reader.TryGetInt32(out var n) => n,
            JsonTokenType.String when int.TryParse(reader.GetString(), out var n) => n,
            JsonTokenType.Null => null,
            _ => null,
        };

    public override void Write(Utf8JsonWriter writer, int? value, JsonSerializerOptions options)
    {
        if (value is { } n) writer.WriteNumberValue(n);
        else writer.WriteNullValue();
    }
}

/// <summary>Parses VidSrc's <c>time_added</c> strings (<c>"yyyy-MM-dd HH:mm:ss"</c>, UTC).</summary>
internal static class VidSrcTime
{
    public static DateTimeOffset? Parse(string? value) =>
        DateTimeOffset.TryParseExact(value, "yyyy-MM-dd HH:mm:ss",
            System.Globalization.CultureInfo.InvariantCulture,
            System.Globalization.DateTimeStyles.AssumeUniversal, out var dt)
            ? dt : null;
}
