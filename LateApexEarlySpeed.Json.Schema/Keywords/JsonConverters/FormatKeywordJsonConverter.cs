using System.Text.Json;
using System.Text.Json.Serialization;
using LateApexEarlySpeed.Json.Schema.Common;

namespace LateApexEarlySpeed.Json.Schema.Keywords.JsonConverters;

public class FormatKeywordJsonConverter : JsonConverter<FormatKeyword>
{
    public override FormatKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String)
        {
            throw ThrowHelper.CreateKeywordHasInvalidJsonValueKindJsonException<FormatKeyword>(JsonValueKind.String);
        }

        return new FormatKeyword(reader.GetString()!);
    }

    public override void Write(Utf8JsonWriter writer, FormatKeyword value, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }

    public override bool HandleNull => true;
}