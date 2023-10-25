using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using JsonSchemaConsoleApp.Keywords;
using JsonSchemaConsoleApp.Keywords.interfaces;

namespace JsonSchemaConsoleApp.JsonConverters;

internal class JsonSchemaJsonConverter<T> : JsonConverter<T>
{
    public override bool CanConvert(Type typeToConvert)
    {
        bool canConvert = typeToConvert == typeof(IJsonSchemaDocument) || typeToConvert == typeof(JsonSchema);
        return canConvert;
    }

    public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.True)
        {
            return typeToConvert == typeof(IJsonSchemaDocument) ? (T)BooleanJsonSchemaDocument.True : (T)(object)(BooleanJsonSchema.True);
        }

        if (reader.TokenType == JsonTokenType.False)
        {
            return typeToConvert == typeof(IJsonSchemaDocument) ? (T)BooleanJsonSchemaDocument.False : (T)(object)BooleanJsonSchema.False;
        }

        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException();
        }

        reader.Read();

        var validationKeywords = new List<ValidationNode>();
        JsonSchema? predictEvaluator = null;
        JsonSchema? positiveValidator = null;
        JsonSchema? negativeValidator = null;
        SchemaReference? schemaReference = null;
        SchemaDynamicReference? schemaDynamicReference = null;
        Dictionary<string, JsonSchema>? defs = null;
        Uri? id = null;
        string? anchor = null;
        string? dynamicAnchor = null;

        while (reader.TokenType != JsonTokenType.EndObject)
        {
            string keywordName = reader.GetString()!;

            Type? keywordType = ValidationKeywordRegistry.GetKeyword(keywordName);
            reader.Read();

            if (keywordType is not null)
            {
                ValidationNode? keyword = JsonSerializer.Deserialize(ref reader, keywordType) as ValidationNode;

                Debug.Assert(keyword != null);
                validationKeywords.Add(keyword);
            }
            else if (keywordName == IfKeyword.Keyword)
            {
                predictEvaluator = JsonSerializer.Deserialize<JsonSchema>(ref reader);
            }
            else if (keywordName == ThenKeyword.Keyword)
            {
                positiveValidator = JsonSerializer.Deserialize<JsonSchema>(ref reader);
            }
            else if (keywordName == ElseKeyword.Keyword)
            {
                negativeValidator = JsonSerializer.Deserialize<JsonSchema>(ref reader);
            }
            else if (keywordName == SchemaReference.Keyword)
            {
                schemaReference = new SchemaReference(new Uri(reader.GetString()!, UriKind.RelativeOrAbsolute));
            }
            else if (keywordName == SchemaDynamicReference.Keyword)
            {
                schemaDynamicReference = new SchemaDynamicReference(new Uri(reader.GetString()!, UriKind.RelativeOrAbsolute));
            }
            else if (keywordName == DefsKeyword.Keyword)
            {
                defs = JsonSerializer.Deserialize<Dictionary<string, JsonSchema>>(ref reader)!;
            }
            else if (keywordName == IdKeyword.Keyword)
            {
                id = new Uri(reader.GetString()!);
            }
            else if (keywordName == AnchorKeyword.Keyword)
            {
                anchor = reader.GetString();
            }
            else if (keywordName == DynamicAnchorKeyword.Keyword)
            {
                dynamicAnchor = reader.GetString();
            }
            else
            {
                reader.Skip();
            }

            reader.Read();
        }

        JsonSchema schema;

        DefsKeyword? defsKeyword = defs is null
            ? null
            : new DefsKeyword(defs);

        var conditionalValidator = new ConditionalValidator(predictEvaluator, positiveValidator, negativeValidator);

        if (typeToConvert == typeof(IJsonSchemaDocument))
        {
            schema = new BodyJsonSchemaDocument(validationKeywords, conditionalValidator, schemaReference, schemaDynamicReference, anchor, dynamicAnchor, id, defsKeyword);
        }
        else if (id is not null)
        {
            schema = new JsonSchemaResource(
                id,
                validationKeywords,
                conditionalValidator,
                schemaReference,
                schemaDynamicReference,
                anchor,
                dynamicAnchor,
                defsKeyword);
        }
        else
        {
            schema = new BodyJsonSchema(validationKeywords, conditionalValidator, schemaReference, schemaDynamicReference, anchor, dynamicAnchor);
        }

        return (T)(object)schema;
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }
}