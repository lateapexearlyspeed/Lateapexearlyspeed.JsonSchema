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

        reader.GetNumericValue(out double? doubleValue, out long? longValue, out ulong? unsignedLongValue);

        if (doubleValue.HasValue)
        {
            return new ExclusiveMaximumKeyword(doubleValue.Value);
        }

        if (longValue.HasValue)
        {
            return new ExclusiveMaximumKeyword(longValue.Value);
        }

        Debug.Assert(unsignedLongValue.HasValue);
        return new ExclusiveMaximumKeyword(unsignedLongValue.Value);
    }

    public override void Write(Utf8JsonWriter writer, ExclusiveMaximumKeyword value, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }

    public override bool HandleNull => true;
}