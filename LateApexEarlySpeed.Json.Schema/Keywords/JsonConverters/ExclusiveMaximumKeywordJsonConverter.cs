using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using LateApexEarlySpeed.Json.Schema.Common;

namespace LateApexEarlySpeed.Json.Schema.Keywords.JsonConverters;

internal class ExclusiveMaximumKeywordJsonConverter : JsonConverter<ExclusiveMaximumKeyword>
{
    public override ExclusiveMaximumKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.Number)
        {
            throw ThrowHelper.CreateKeywordHasInvalidJsonValueKindJsonException<ExclusiveMaximumKeyword>(JsonValueKind.Number);
        }

        reader.GetNumericValue(out long? longValue, out ulong? unsignedLongValue, out decimal? decimalValue, out double? doubleValue);

        if (longValue.HasValue)
        {
            return new ExclusiveMaximumKeyword(longValue.Value);
        }

        if (unsignedLongValue.HasValue)
        {
            return new ExclusiveMaximumKeyword(unsignedLongValue.Value);
        }

        if (decimalValue.HasValue)
        {
            return new ExclusiveMaximumKeyword(decimalValue.Value);
        }

        Debug.Assert(doubleValue.HasValue);
        return new ExclusiveMaximumKeyword(doubleValue.Value);
    }

    public override void Write(Utf8JsonWriter writer, ExclusiveMaximumKeyword value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value.BenchmarkValue, options);
    }

    public override bool HandleNull => true;
}