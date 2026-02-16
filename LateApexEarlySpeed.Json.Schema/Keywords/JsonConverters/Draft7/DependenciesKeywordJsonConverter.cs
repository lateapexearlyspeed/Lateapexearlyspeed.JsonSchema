using System.Text.Json;
using System.Text.Json.Serialization;
using LateApexEarlySpeed.Json.Schema.Common;
using LateApexEarlySpeed.Json.Schema.JSchema;
using LateApexEarlySpeed.Json.Schema.Keywords.Draft7;

namespace LateApexEarlySpeed.Json.Schema.Keywords.JsonConverters.Draft7;

internal class DependenciesKeywordJsonConverter : JsonConverter<DependenciesKeyword>
{
    public override DependenciesKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw ThrowHelper.CreateKeywordHasInvalidJsonValueKindJsonException<DependenciesKeyword>(JsonValueKind.Object);
        }

        Dictionary<string, JsonSchema>? dependenciesSchema = null;
        Dictionary<string, string[]>? dependenciesProperty = null;

        reader.Read();

        while (reader.TokenType != JsonTokenType.EndObject)
        {
            string propertyName = reader.GetString()!;

            reader.Read();

            if (reader.TokenType == JsonTokenType.StartArray)
            {
                string[] properties = JsonSerializer.Deserialize<string[]>(ref reader, options)
                                      ?? throw ThrowHelper.CreateKeywordHasInvalidJsonValueKindJsonException<DependenciesKeyword>(JsonValueKind.Array);

                dependenciesProperty ??= new();
                dependenciesProperty[propertyName] = properties;
            }
            else
            {
                dependenciesSchema ??= new();
                dependenciesSchema[propertyName] = JsonSerializer.Deserialize<JsonSchema>(ref reader, options)!;
            }

            reader.Read();
        }

        return new DependenciesKeyword(dependenciesSchema, dependenciesProperty, new JsonSchemaDeserializerContext(options).PropertyNameCaseInsensitive);
    }

    public override void Write(Utf8JsonWriter writer, DependenciesKeyword value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        if (value.DependenciesSchema is not null)
        {
            foreach ((string prop, JsonSchema schema) in value.DependenciesSchema)
            {
                writer.WritePropertyName(prop);
                JsonSerializer.Serialize(writer, schema, options);
            }
        }

        if (value.DependenciesProperty is not null)
        {
            foreach ((string prop, string[] dependentProperties) in value.DependenciesProperty)
            {
                writer.WritePropertyName(prop);
                JsonSerializer.Serialize(writer, dependentProperties, options);
            }
        }

        writer.WriteEndObject();
    }

    public override bool HandleNull => true;
}