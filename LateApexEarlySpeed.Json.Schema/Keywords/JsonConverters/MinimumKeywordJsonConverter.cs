using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using LateApexEarlySpeed.Json.Schema.Common;

namespace LateApexEarlySpeed.Json.Schema.Keywords.JsonConverters;

internal class MinimumKeywordJsonConverter : JsonConverter<MinimumKeyword>
{
    public override MinimumKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.Number)
        {
            throw ThrowHelper.CreateKeywordHasInvalidJsonValueKindJsonException<MinimumKeyword>(JsonValueKind.Number);
        }

        reader.GetNumericValue(out double? doubleValue, out long? longValue, out ulong? unsignedLongValue);

        if (doubleValue.HasValue)
        {
            return new MinimumKeyword(doubleValue.Value);
        }

        if (longValue.HasValue)
        {
            return new MinimumKeyword(longValue.Value);
        }

        Debug.Assert(unsignedLongValue.HasValue);
        return new MinimumKeyword(unsignedLongValue.Value);
    }

    public override void Write(Utf8JsonWriter writer, MinimumKeyword value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value.BenchmarkValue, options);
    }

    public override bool HandleNull => true;
}