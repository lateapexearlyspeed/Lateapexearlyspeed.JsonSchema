﻿using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using LateApexEarlySpeed.Json.Schema.Common;
using LateApexEarlySpeed.Json.Schema.Common.interfaces;
using LateApexEarlySpeed.Json.Schema.JSchema.interfaces;
using LateApexEarlySpeed.Json.Schema.Keywords;

namespace LateApexEarlySpeed.Json.Schema.JSchema.JsonConverters;

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
            return typeToConvert == typeof(IJsonSchemaDocument) ? (T)BooleanJsonSchemaDocument.True : (T)(object)BooleanJsonSchema.True;
        }

        if (reader.TokenType == JsonTokenType.False)
        {
            return typeToConvert == typeof(IJsonSchemaDocument) ? (T)BooleanJsonSchemaDocument.False : (T)(object)BooleanJsonSchema.False;
        }

        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw ThrowHelper.CreateJsonSchemaHasInvalidJsonValueKindJsonException(JsonValueKind.Object, JsonValueKind.True, JsonValueKind.False);
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

        SchemaReferenceKeyword? schemaReference = null;
        SchemaDynamicReferenceKeyword? schemaDynamicReference = null;
        DefsKeyword? defsKeyword = null;
        Uri? id = null;
        string? anchor = null;
        string? dynamicAnchor = null;

        Dictionary<string, ISchemaContainerElement>? potentialSchemaContainerElements = null;

        while (reader.TokenType != JsonTokenType.EndObject)
        {
            string keywordName = reader.GetString()!;

            Type? keywordType = ValidationKeywordRegistry.GetKeyword(keywordName);
            reader.Read();

            if (keywordType is not null)
            {
                KeywordBase? keyword = JsonSerializer.Deserialize(ref reader, keywordType, options) as KeywordBase;

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
                predictSchema = JsonSerializer.Deserialize<JsonSchema>(ref reader, options);
            }
            else if (keywordName == ConditionalValidator.ThenKeywordName)
            {
                positiveSchema = JsonSerializer.Deserialize<JsonSchema>(ref reader, options);
            }
            else if (keywordName == ConditionalValidator.ElseKeywordName)
            {
                negativeSchema = JsonSerializer.Deserialize<JsonSchema>(ref reader, options);
            }
            else if (keywordName == ArrayContainsValidator.ContainsKeywordName)
            {
                containsSchema = JsonSerializer.Deserialize<JsonSchema>(ref reader, options);
            }
            else if (keywordName == ArrayContainsValidator.MaxContainsKeywordName)
            {
                if (!reader.TryGetUInt32ForJsonSchema(out uint tmp))
                {
                    throw ThrowHelper.CreateKeywordHasInvalidNonNegativeIntegerJsonException(ArrayContainsValidator.MaxContainsKeywordName);
                }

                maxContains = tmp;
            }
            else if (keywordName == ArrayContainsValidator.MinContainsKeywordName)
            {
                if (!reader.TryGetUInt32ForJsonSchema(out uint tmp))
                {
                    throw ThrowHelper.CreateKeywordHasInvalidNonNegativeIntegerJsonException(ArrayContainsValidator.MinContainsKeywordName);
                }

                minContains = tmp;
            }
            else if (keywordName == SchemaReferenceKeyword.Keyword)
            {
                schemaReference = JsonSerializer.Deserialize<SchemaReferenceKeyword>(ref reader, options);
            }
            else if (keywordName == SchemaDynamicReferenceKeyword.Keyword)
            {
                schemaDynamicReference = JsonSerializer.Deserialize<SchemaDynamicReferenceKeyword>(ref reader, options);
            }
            else if (keywordName == DefsKeyword.Keyword)
            {
                defsKeyword = JsonSerializer.Deserialize<DefsKeyword>(ref reader, options)!;
            }
            else if (keywordName == IdKeyword.Keyword)
            {
                id = new Uri(reader.GetString()!, UriKind.RelativeOrAbsolute);
            }
            else if (keywordName == AnchorKeyword.Keyword)
            {
                anchor = reader.GetString();
            }
            else if (keywordName == DynamicAnchorKeyword.Keyword)
            {
                dynamicAnchor = reader.GetString();
            }
            else if (ValidationKeywordRegistry.ContainsIgnoredKeyword(keywordName))
            {
                reader.Skip();
            }
            else if (PotentialSchemaContainerElement.TryDeserialize(ref reader, out ISchemaContainerElement? potentialSchemaContainerElement, options))
            {
                (potentialSchemaContainerElements ??= new Dictionary<string, ISchemaContainerElement>()).Add(keywordName, potentialSchemaContainerElement);
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

        var schemaContainerValidators = new List<ISchemaContainerValidationNode>(2);
        if (containsSchema is not null)
        {
            schemaContainerValidators.Add(new ArrayContainsValidator(containsSchema, minContains, maxContains));
        }

        if (predictSchema is not null || positiveSchema is not null || negativeSchema is not null)
        {
            schemaContainerValidators.Add(new ConditionalValidator(predictSchema, positiveSchema, negativeSchema));
        }

        // Although BodyJsonSchema supports merging duplicated keywords, but it is done by changing json schema tree structure. (That is:
        // when there is duplicated keywords, schema structure will be changed)
        // In json schema deserialization case, it is possible that original schema contains "json path" reference, so here we cannot 
        // dare to change original json schema structure at risk of missing reference.
        ThrowIfKeywordsHaveDuplication(validationKeywords);

        JsonSchema schema;
        if (typeToConvert == typeof(IJsonSchemaDocument))
        {
            schema = new BodyJsonSchemaDocument(validationKeywords, schemaContainerValidators, schemaReference, schemaDynamicReference, anchor, dynamicAnchor, potentialSchemaContainerElements, id, defsKeyword);
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
                defsKeyword, potentialSchemaContainerElements);
        }
        else
        {
            schema = new BodyJsonSchema(validationKeywords, schemaContainerValidators, schemaReference, schemaDynamicReference, anchor, dynamicAnchor, defsKeyword, potentialSchemaContainerElements);
        }

        return (T)(object)schema;
    }

    private static void ThrowIfKeywordsHaveDuplication(ICollection<KeywordBase> keywords)
    {
        KeywordBase? duplicatedKeyword = keywords.FindFirstDuplicatedItem(keyword => keyword.Name);
        if (duplicatedKeyword is not null)
        {
            throw ThrowHelper.CreateJsonSchemaHasDuplicatedKeywordsJsonException(duplicatedKeyword.Name);
        }
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        Debug.Assert(value is not null);

        if (value is BooleanJsonSchemaDocument booleanJsonSchemaDocument)
        {
            writer.WriteBooleanValue(booleanJsonSchemaDocument.AlwaysValid);
        }
        else if (value is BooleanJsonSchema booleanJsonSchema)
        {
            writer.WriteBooleanValue(booleanJsonSchema.AlwaysValid);
        }
        else
        {
            writer.WriteStartObject();

            BodyJsonSchema schema;
            if (value is JsonSchemaResource schemaResource)
            {
                Debug.Assert(value.GetType() == typeof(JsonSchemaResource) || value.GetType() == typeof(BodyJsonSchemaDocument));

                writer.WriteString(IdKeyword.Keyword, schemaResource.BaseUri!.ToString());
                schema = schemaResource;
            }
            else
            {
                Debug.Assert(value.GetType() == typeof(BodyJsonSchema));

                schema = (BodyJsonSchema)(object)value;
            }

            // Keyword part:
            foreach (KeywordBase keyword in schema.Keywords)
            {
                writer.WritePropertyName(keyword.Name);
                JsonSerializer.Serialize(writer, keyword, keyword.GetType(), options);
            }

            // defs part:
            if (schema.DefsKeyword is not null)
            {
                writer.WritePropertyName(DefsKeyword.Keyword);
                JsonSerializer.Serialize(writer, schema.DefsKeyword, options);
            }

            // Reference & DynamicReference part:
            if (schema.SchemaReference is not null)
            {
                writer.WritePropertyName(SchemaReferenceKeyword.Keyword);
                JsonSerializer.Serialize(writer, schema.SchemaReference, options);
            }

            if (schema.SchemaDynamicReference is not null)
            {
                writer.WritePropertyName(SchemaDynamicReferenceKeyword.Keyword);
                JsonSerializer.Serialize(writer, schema.SchemaDynamicReference, options);
            }

            // Anchor & DynamicAnchor part:
            if (schema.Anchor is not null)
            {
                writer.WritePropertyName(AnchorKeyword.Keyword);
                writer.WriteStringValue(schema.Anchor);
            }

            if (schema.DynamicAnchor is not null)
            {
                writer.WritePropertyName(DynamicAnchorKeyword.Keyword);
                writer.WriteStringValue(schema.DynamicAnchor);
            }

            // ArrayContainsValidator & ConditionalValidator part:
            foreach (ISchemaContainerValidationNode schemaContainerValidator in schema.SchemaContainerValidators)
            {
                Debug.Assert(schemaContainerValidator is ArrayContainsValidator || schemaContainerValidator is ConditionalValidator);

                if (schemaContainerValidator is ArrayContainsValidator arrayContainsValidator)
                {
                    WriteArrayContainsValidator(writer, arrayContainsValidator, options);
                }
                else
                {
                    Debug.Assert(schemaContainerValidator is ConditionalValidator);
                    WriteConditionalValidator(writer, (ConditionalValidator)schemaContainerValidator, options);
                }
            }

            // PotentialSchemaContainerElements part:
            if (schema.PotentialSchemaContainerElements is not null)
            {
                foreach ((string propertyName, ISchemaContainerElement potentialSchemaElement) in schema.PotentialSchemaContainerElements)
                {
                    writer.WritePropertyName(propertyName);
                    PotentialSchemaContainerElement.Serialize(writer, potentialSchemaElement, options);
                }
            }

            writer.WriteEndObject();
        }
    }

    private static void WriteConditionalValidator(Utf8JsonWriter writer, ConditionalValidator value, JsonSerializerOptions options)
    {
        if (value.PredictEvaluator is not null)
        {
            writer.WritePropertyName(ConditionalValidator.IfKeywordName);
            JsonSerializer.Serialize(writer, value.PredictEvaluator, options);
        }

        writer.WritePropertyName(ConditionalValidator.ThenKeywordName);
        JsonSerializer.Serialize(writer, value.PositiveValidator, options);

        writer.WritePropertyName(ConditionalValidator.ElseKeywordName);
        JsonSerializer.Serialize(writer, value.NegativeValidator, options);
    }

    private static void WriteArrayContainsValidator(Utf8JsonWriter writer, ArrayContainsValidator value, JsonSerializerOptions options)
    {
        writer.WritePropertyName(ArrayContainsValidator.ContainsKeywordName);
        JsonSerializer.Serialize(writer, value.ContainsSchema, options);

        if (value.MaxContains.HasValue)
        {
            writer.WriteNumber(ArrayContainsValidator.MaxContainsKeywordName, value.MaxContains.Value);
        }

        if (value.MinContains.HasValue)
        {
            writer.WriteNumber(ArrayContainsValidator.MinContainsKeywordName, value.MinContains.Value);
        }
    }

    public override bool HandleNull => true;
}