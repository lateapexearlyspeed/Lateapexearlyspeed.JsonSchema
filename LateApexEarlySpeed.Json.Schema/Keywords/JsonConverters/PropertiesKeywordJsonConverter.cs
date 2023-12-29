using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using LateApexEarlySpeed.Json.Schema.Common;
using LateApexEarlySpeed.Json.Schema.JSchema;

namespace LateApexEarlySpeed.Json.Schema.Keywords.JsonConverters;

internal class PropertiesKeywordJsonConverter : JsonConverter<PropertiesKeyword>
{
    public override PropertiesKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw ThrowHelper.CreateKeywordHasInvalidJsonValueKindJsonException<PropertiesKeyword>(JsonValueKind.Object);
        }

        reader.Read();

        var propertiesSchemas = new Dictionary<string, JsonSchema>();
        while (reader.TokenType != JsonTokenType.EndObject)
        {
            string propertyName = reader.GetString()!;

            reader.Read();

            JsonSchema? propertySchema = JsonSerializer.Deserialize<JsonSchema>(ref reader);
            
            Debug.Assert(propertySchema is not null);
            propertiesSchemas.Add(propertyName, propertySchema);

            reader.Read();
        }

        return new PropertiesKeyword(propertiesSchemas);
    }

    public override void Write(Utf8JsonWriter writer, PropertiesKeyword value, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }

    public override bool HandleNull => true;
}