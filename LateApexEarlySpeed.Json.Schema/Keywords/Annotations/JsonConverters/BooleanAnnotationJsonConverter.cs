using System.Text.Json;
using System.Text.Json.Serialization;
using LateApexEarlySpeed.Json.Schema.Common;

namespace LateApexEarlySpeed.Json.Schema.Keywords.Annotations.JsonConverters;

public class BooleanAnnotationJsonConverter<TAnnotationKeyword> : JsonConverter<TAnnotationKeyword> where TAnnotationKeyword : AnnotationKeyword<bool>, new()
{
    public override TAnnotationKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.True:
                return new TAnnotationKeyword { Value = true };
            case JsonTokenType.False:
                return new TAnnotationKeyword { Value = false };
            default:
                throw ThrowHelper.CreateKeywordHasInvalidJsonValueKindJsonException<TAnnotationKeyword>(JsonValueKind.True, JsonValueKind.False);
        }
    }

    public override void Write(Utf8JsonWriter writer, TAnnotationKeyword value, JsonSerializerOptions options)
    {
        writer.WriteBooleanValue(value.Value);
    }

    public override bool HandleNull => true;
}