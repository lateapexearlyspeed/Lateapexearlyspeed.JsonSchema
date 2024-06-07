using System.Text.Json;
using System.Text.Json.Serialization;
using LateApexEarlySpeed.Json.Schema.Common;

namespace LateApexEarlySpeed.Json.Schema.Keywords.JsonConverters;

internal class MultipleOfKeywordJsonConverter : JsonConverter<MultipleOfKeyword>
{
    public override MultipleOfKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.Number)
        {
            throw ThrowHelper.CreateKeywordHasInvalidJsonValueKindJsonException<MultipleOfKeyword>(JsonValueKind.Number);
        }

        if (reader.TryGetUInt64(out ulong ulongMultipleOf))
        {
            if (ulongMultipleOf == 0)
            {
                throw ThrowHelper.CreateKeywordHasInvalidPositiveNumberJsonException<MultipleOfKeyword>();
            }

            return new MultipleOfKeyword(ulongMultipleOf);
        }

        if (reader.TryGetDecimal(out decimal decimalMultipleOf))
        {
            if (decimalMultipleOf <= 0)
            {
                throw ThrowHelper.CreateKeywordHasInvalidPositiveNumberJsonException<MultipleOfKeyword>();
            }

            return new MultipleOfKeyword(decimalMultipleOf);
        }

        double multipleOf = reader.GetDouble();
        if (multipleOf <= 0)
        {
            throw ThrowHelper.CreateKeywordHasInvalidPositiveNumberJsonException<MultipleOfKeyword>();
        }

        return new MultipleOfKeyword(multipleOf);
    }

    public override void Write(Utf8JsonWriter writer, MultipleOfKeyword value, JsonSerializerOptions options)
    {
        value.WriteMultipleOfValue(writer);
    }

    public override bool HandleNull => true;
}