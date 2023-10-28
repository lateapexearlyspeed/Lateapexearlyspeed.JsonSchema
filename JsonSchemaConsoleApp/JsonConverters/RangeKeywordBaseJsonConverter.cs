using System.Text.Json;
using System.Text.Json.Serialization;
using JsonSchemaConsoleApp.Keywords;

namespace JsonSchemaConsoleApp.JsonConverters;

internal class RangeKeywordBaseJsonConverter : JsonConverter<RangeKeywordBase>
{
    public override RangeKeywordBase Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.Number)
        {
            throw ThrowHelper.CreateKeywordHasInvalidJsonValueKindJsonException(typeToConvert, JsonValueKind.Number);
        }

        return (RangeKeywordBase)Activator.CreateInstance(typeToConvert, reader.GetDouble())!;
    }

    public override void Write(Utf8JsonWriter writer, RangeKeywordBase value, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }
}