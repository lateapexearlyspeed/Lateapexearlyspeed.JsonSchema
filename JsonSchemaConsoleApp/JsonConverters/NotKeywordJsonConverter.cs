using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using JsonSchemaConsoleApp.Keywords;

namespace JsonSchemaConsoleApp.JsonConverters;

public class NotKeywordJsonConverter : JsonConverter<NotKeyword>
{
    public override NotKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        JsonSchema? jsonSchema = JsonSerializer.Deserialize<JsonSchema>(ref reader);
        
        Debug.Assert(jsonSchema is not null);
        return new NotKeyword { SubSchema = jsonSchema };
    }

    public override void Write(Utf8JsonWriter writer, NotKeyword value, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }
}