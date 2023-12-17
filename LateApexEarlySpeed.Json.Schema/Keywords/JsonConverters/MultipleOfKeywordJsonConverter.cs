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

        double multipleOf = reader.GetDouble();
        if (multipleOf <= 0)
        {
            throw ThrowHelper.CreateKeywordHasInvalidPositiveNumberJsonException<MultipleOfKeyword>();
        }

        return new MultipleOfKeyword(multipleOf);
    }

    public override void Write(Utf8JsonWriter writer, MultipleOfKeyword value, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }

    public override bool HandleNull => true;
}