using System.Text.Json;
using System.Text.Json.Serialization;
using JsonSchemaConsoleApp.Keywords;

namespace JsonSchemaConsoleApp.JsonConverters;

internal class TypeKeywordJsonConverter : JsonConverter<TypeKeyword>
{
    public override TypeKeyword? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String)
        {
            throw new JsonException();
        }

        return new TypeKeyword { SchemaType = Enum.Parse<SchemaType>(reader.GetString()!, true) };
    }

    public override void Write(Utf8JsonWriter writer, TypeKeyword value, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }
}