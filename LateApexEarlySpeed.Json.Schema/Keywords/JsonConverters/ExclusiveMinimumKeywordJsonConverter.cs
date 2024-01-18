using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using LateApexEarlySpeed.Json.Schema.Common;

namespace LateApexEarlySpeed.Json.Schema.Keywords.JsonConverters;

internal class ExclusiveMinimumKeywordJsonConverter : JsonConverter<ExclusiveMinimumKeyword>
{
    public override ExclusiveMinimumKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.Number)
        {
            throw ThrowHelper.CreateKeywordHasInvalidJsonValueKindJsonException<ExclusiveMinimumKeyword>(JsonValueKind.Number);
        }

        reader.GetNumericValue(out double? doubleValue, out long? longValue, out ulong? unsignedLongValue);

        if (doubleValue.HasValue)
        {
            return new ExclusiveMinimumKeyword(doubleValue.Value);
        }

        if (longValue.HasValue)
        {
            return new ExclusiveMinimumKeyword(longValue.Value);
        }

        Debug.Assert(unsignedLongValue.HasValue);
        return new ExclusiveMinimumKeyword(unsignedLongValue.Value);
    }

    public override void Write(Utf8JsonWriter writer, ExclusiveMinimumKeyword value, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }

    public override bool HandleNull => true;
}