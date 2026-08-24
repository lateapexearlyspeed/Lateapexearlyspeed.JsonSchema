using System.Text.Json;
using System.Text.Json.Serialization;
using LateApexEarlySpeed.Json.Schema.Common;

namespace LateApexEarlySpeed.Json.Schema.Keywords.Annotations.JsonConverters;

public class BooleanAnnotationJsonConverter<TAnnotationKeyword> : JsonConverter<TAnnotationKeyword> where TAnnotationKeyword : AnnotationKeywordBase, new()
{
    public override TAnnotationKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.True && reader.TokenType != JsonTokenType.False)
        {
            throw ThrowHelper.CreateKeywordHasInvalidJsonValueKindJsonException<TAnnotationKeyword>(JsonValueKind.True, JsonValueKind.False);
        }

        return new TAnnotationKeyword { Value = JsonElement.ParseValue(ref reader) };
    }

    public override void Write(Utf8JsonWriter writer, TAnnotationKeyword value, JsonSerializerOptions options)
    {
        value.Value.WriteTo(writer);
    }

    public override bool HandleNull => true;
}