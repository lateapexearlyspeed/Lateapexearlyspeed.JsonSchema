using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using LateApexEarlySpeed.Json.Schema.Common;
using LateApexEarlySpeed.Json.Schema.JSchema;

namespace LateApexEarlySpeed.Json.Schema.Keywords.JsonConverters;

internal class DefsKeywordJsonConverter : JsonConverter<DefsKeyword>
{
    public override DefsKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions? options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw ThrowHelper.CreateKeywordHasInvalidJsonValueKindJsonException(DefsKeyword.Keyword, JsonValueKind.Object);
        }

        Dictionary<string, JsonSchema>? definitions = JsonSerializer.Deserialize<Dictionary<string, JsonSchema>>(ref reader, options);

        Debug.Assert(definitions is not null);

        return new DefsKeyword(definitions);
    }

    public override void Write(Utf8JsonWriter writer, DefsKeyword value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value.Definitions, options);
    }
}