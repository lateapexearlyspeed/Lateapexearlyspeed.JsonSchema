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

        var validationKeywords = new List<KeywordBase>();

        PropertiesKeyword? propertiesKeyword = null;
        PatternPropertiesKeyword? patternPropertiesKeyword = null;
        AdditionalPropertiesKeyword? additionalPropertiesKeyword = null;

        PrefixItemsKeyword? prefixItemsKeyword = null;
        ItemsKeyword? itemsKeyword = null;

        JsonSchema? predictSchema = null;
        JsonSchema? positiveSchema = null;
        JsonSchema? negativeSchema = null;

        JsonSchema? containsSchema = null;
        uint? maxContains = null;
        uint? minContains = null;

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
                KeywordBase? keyword = JsonSerializer.Deserialize(ref reader, keywordType) as KeywordBase;

                Debug.Assert(keyword != null);
                validationKeywords.Add(keyword);

                // Store dependent keywords
                if (keyword is PropertiesKeyword properties)
                {
                    propertiesKeyword = properties;
                }
                else if (keyword is PatternPropertiesKeyword pattern)
                {
                    patternPropertiesKeyword = pattern;
                }
                else if (keyword is AdditionalPropertiesKeyword additionalProperties)
                {
                    additionalPropertiesKeyword = additionalProperties;
                }
                else if (keyword is PrefixItemsKeyword prefixItems)
                {
                    prefixItemsKeyword = prefixItems;
                }
                else if (keyword is ItemsKeyword items)
                {
                    itemsKeyword = items;
                }
            }
            else if (keywordName == ConditionalValidator.IfKeywordName)
            {
                predictSchema = JsonSerializer.Deserialize<JsonSchema>(ref reader);
            }
            else if (keywordName == ConditionalValidator.ThenKeywordName)
            {
                positiveSchema = JsonSerializer.Deserialize<JsonSchema>(ref reader);
            }
            else if (keywordName == ConditionalValidator.ElseKeywordName)
            {
                negativeSchema = JsonSerializer.Deserialize<JsonSchema>(ref reader);
            }
            else if (keywordName == ArrayContainsValidator.ContainsKeywordName)
            {
                containsSchema = JsonSerializer.Deserialize<JsonSchema>(ref reader);
            }
            else if (keywordName == ArrayContainsValidator.MaxContainsKeywordName)
            {
                maxContains = JsonSerializer.Deserialize<uint>(ref reader);
            }
            else if (keywordName == ArrayContainsValidator.MinContainsKeywordName)
            {
                minContains = JsonSerializer.Deserialize<uint>(ref reader);
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

        // Set dependent keywords
        if (additionalPropertiesKeyword is not null)
        {
            additionalPropertiesKeyword.PropertiesKeyword = propertiesKeyword;
            additionalPropertiesKeyword.PatternPropertiesKeyword = patternPropertiesKeyword;
        }

        if (itemsKeyword is not null)
        {
            itemsKeyword.PrefixItemsKeyword = prefixItemsKeyword;
        }

        JsonSchema schema;

        DefsKeyword? defsKeyword = defs is null
            ? null
            : new DefsKeyword(defs);

        var schemaContainerValidators = new List<ISchemaContainerValidationNode>(2);
        if (containsSchema is not null)
        {
            var arrayContainsValidator = new ArrayContainsValidator(containsSchema, minContains, maxContains);
            schemaContainerValidators.Add(arrayContainsValidator);
        }

        if (predictSchema is not null)
        {
            var conditionalValidator = new ConditionalValidator(predictSchema, positiveSchema, negativeSchema);
            schemaContainerValidators.Add(conditionalValidator);
        }

        if (typeToConvert == typeof(IJsonSchemaDocument))
        {
            schema = new BodyJsonSchemaDocument(validationKeywords, schemaContainerValidators, schemaReference, schemaDynamicReference, anchor, dynamicAnchor, id, defsKeyword);
        }
        else if (id is not null)
        {
            schema = new JsonSchemaResource(
                id,
                validationKeywords,
                schemaContainerValidators,
                schemaReference,
                schemaDynamicReference,
                anchor,
                dynamicAnchor,
                defsKeyword);
        }
        else
        {
            schema = new BodyJsonSchema(validationKeywords, schemaContainerValidators, schemaReference, schemaDynamicReference, anchor, dynamicAnchor);
        }

        return (T)(object)schema;
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }
}