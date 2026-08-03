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

        JsonElement[] examples = JsonSerializer.Deserialize<JsonElement[]>(ref reader, options)!;

        return new ExamplesAnnotation { Value = examples };
    }

    public override void Write(Utf8JsonWriter writer, ExamplesAnnotation value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value.Value, options);
    }
}