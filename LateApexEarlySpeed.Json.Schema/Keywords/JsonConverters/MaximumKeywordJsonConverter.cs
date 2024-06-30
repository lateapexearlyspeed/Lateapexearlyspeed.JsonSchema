using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using LateApexEarlySpeed.Json.Schema.Common;

namespace LateApexEarlySpeed.Json.Schema.Keywords.JsonConverters;

internal class MaximumKeywordJsonConverter : JsonConverter<MaximumKeyword>
{
    public override MaximumKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.Number)
        {
            throw ThrowHelper.CreateKeywordHasInvalidJsonValueKindJsonException<MaximumKeyword>(JsonValueKind.Number);
        }

        reader.GetNumericValue(out long? longValue, out ulong? unsignedLongValue, out decimal? decimalValue, out double? doubleValue);

        if (longValue.HasValue)
        {
            return new MaximumKeyword(longValue.Value);
        }

        if (unsignedLongValue.HasValue)
        {
            return new MaximumKeyword(unsignedLongValue.Value);
        }

        if (decimalValue.HasValue)
        {
            return new MaximumKeyword(decimalValue.Value);
        }

        Debug.Assert(doubleValue.HasValue);
        return new MaximumKeyword(doubleValue.Value);
    }

    public override void Write(Utf8JsonWriter writer, MaximumKeyword value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value.BenchmarkValue, options);
    }

    public override bool HandleNull => true;
}