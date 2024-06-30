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

        reader.GetNumericValue(out long? longValue, out ulong? unsignedLongValue, out decimal? decimalValue, out double? doubleValue);

        if (longValue.HasValue)
        {
            return new MinimumKeyword(longValue.Value);
        }

        if (unsignedLongValue.HasValue)
        {
            return new MinimumKeyword(unsignedLongValue.Value);
        }

        if (decimalValue.HasValue)
        {
            return new MinimumKeyword(decimalValue.Value);
        }

        Debug.Assert(doubleValue.HasValue);
        return new MinimumKeyword(doubleValue.Value);
    }

    public override void Write(Utf8JsonWriter writer, MinimumKeyword value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value.BenchmarkValue, options);
    }

    public override bool HandleNull => true;
}