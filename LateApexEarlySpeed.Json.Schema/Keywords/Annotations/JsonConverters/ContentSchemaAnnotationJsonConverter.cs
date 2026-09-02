using System.Text.Json;
using System.Text.Json.Serialization;
using LateApexEarlySpeed.Json.Schema.Common;

namespace LateApexEarlySpeed.Json.Schema.Keywords.Annotations.JsonConverters;

public class ContentSchemaAnnotationJsonConverter : JsonConverter<ContentSchemaAnnotation>
{
    public override ContentSchemaAnnotation Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject && reader.TokenType != JsonTokenType.True && reader.TokenType != JsonTokenType.False)
        {
            throw ThrowHelper.CreateKeywordHasInvalidJsonValueKindJsonException<ContentSchemaAnnotation>(JsonValueKind.Object, JsonValueKind.True, JsonValueKind.False);
        }

        return new ContentSchemaAnnotation { Value = JsonElement.ParseValue(ref reader) };
    }

    public override void Write(Utf8JsonWriter writer, ContentSchemaAnnotation value, JsonSerializerOptions options)
    {
        value.Value.WriteTo(writer);
    }

    public override bool HandleNull => true;
}