using System.Text.Json;
using System.Text.Json.Serialization;
using LateApexEarlySpeed.Json.Schema.Common;

namespace LateApexEarlySpeed.Json.Schema.Keywords.Annotations.JsonConverters;

public class StringAnnotationJsonConverter<TAnnotationKeyword> : JsonConverter<TAnnotationKeyword> where TAnnotationKeyword : AnnotationKeyword<string>, new()
{
    public override TAnnotationKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String)
        {
            throw ThrowHelper.CreateKeywordHasInvalidJsonValueKindJsonException<TAnnotationKeyword>(JsonValueKind.String);
        }

        return new TAnnotationKeyword { Value = reader.GetString()! };
    }

    public override void Write(Utf8JsonWriter writer, TAnnotationKeyword value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.Value);
    }

    public override bool HandleNull => true;
}