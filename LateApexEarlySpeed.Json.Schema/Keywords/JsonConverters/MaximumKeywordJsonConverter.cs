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

        reader.GetNumericValue(out double? doubleValue, out long? longValue, out ulong? unsignedLongValue);

        if (doubleValue.HasValue)
        {
            return new MaximumKeyword(doubleValue.Value);
        }

        if (longValue.HasValue)
        {
            return new MaximumKeyword(longValue.Value);
        }

        Debug.Assert(unsignedLongValue.HasValue);
        return new MaximumKeyword(unsignedLongValue.Value);
    }

    public override void Write(Utf8JsonWriter writer, MaximumKeyword value, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }

    public override bool HandleNull => true;
}