using System.Text.Json;
using System.Text.Json.Serialization;
using LateApexEarlySpeed.Json.Schema.Common;

namespace LateApexEarlySpeed.Json.Schema.Keywords.Annotations.JsonConverters;

public class ExamplesAnnotationJsonConverter : JsonConverter<ExamplesAnnotation>
{
    public override ExamplesAnnotation Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartArray)
        {
            throw ThrowHelper.CreateKeywordHasInvalidJsonValueKindJsonException<ExamplesAnnotation>(JsonValueKind.Array);
        }

        return new ExamplesAnnotation { Value = JsonElement.ParseValue(ref reader) };
    }

    public override void Write(Utf8JsonWriter writer, ExamplesAnnotation value, JsonSerializerOptions options)
    {
        value.Value.WriteTo(writer);
    }

    public override bool HandleNull => true;
}