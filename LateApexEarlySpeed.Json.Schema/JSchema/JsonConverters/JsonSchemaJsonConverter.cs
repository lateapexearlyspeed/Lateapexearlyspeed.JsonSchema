using LateApexEarlySpeed.Json.Schema.Common;
using LateApexEarlySpeed.Json.Schema.Common.interfaces;
using LateApexEarlySpeed.Json.Schema.JSchema.interfaces;
using LateApexEarlySpeed.Json.Schema.Keywords;
using LateApexEarlySpeed.Json.Schema.Keywords.Draft7;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using LateApexEarlySpeed.Json.Schema.Keywords.interfaces;

namespace LateApexEarlySpeed.Json.Schema.JSchema.JsonConverters;

internal class JsonSchemaJsonConverter<T> : JsonConverter<T>
{
    /// <summary>
    /// The longest keyword across all JSON Schema drafts is 'unevaluatedProperties' - 21 characters
    /// </summary>
    private const int KeywordNameCharBufferSizeOnStack = 21;

    /// <summary>
    /// The longest schema identifier is <see cref="SchemaKeyword.Draft202012IdentifierString"/> - 44 characters
    /// </summary>
    private const int MaxSchemaIdentifierCharBufferSizeOnStack = 50;

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

        var deserializerContext = new JsonSchemaDeserializerContext(options);

        SchemaKeyword? schemaKeyword = null;

        PropertiesKeyword? propertiesKeyword = null;
        PatternPropertiesKeyword? patternPropertiesKeyword = null;
        AdditionalPropertiesKeyword? additionalPropertiesKeyword = null;

        PrefixItemsKeyword? prefixItemsKeyword = null;
        ItemsKeyword? itemsKeyword = null;

        AdditionalItemsKeyword? additionalItemsKeyword = null;
        ItemsWithMultiSchemasKeyword? itemsWithMultiSchemasKeyword = null;

        JsonSchema? predictSchema = null;
        JsonSchema? positiveSchema = null;
        JsonSchema? negativeSchema = null;

        JsonSchema? containsSchema = null;
        uint? maxContains = null;
        uint? minContains = null;

        SchemaReferenceKeyword? schemaReference = null;
        List<IReferenceKeyword>? referenceKeywords = null;
        List<(string name, DefsKeyword keyword)>? defsKeywords = null;
        Uri? id = null;
        IPlainNameIdentifierKeyword? plainNameIdentifier = null;
        string? dynamicAnchor = null;
        bool recursiveAnchor = false;

        Dictionary<string, ISchemaContainerElement>? potentialSchemaContainerElements = null;
        
        Span<char> keywordNameBuffer = stackalloc char[KeywordNameCharBufferSizeOnStack];

        while (reader.TokenType != JsonTokenType.EndObject)
        {
            var byteCount = reader.HasValueSequence ? reader.ValueSequence.Length : reader.ValueSpan.Length;

            if (byteCount > keywordNameBuffer.Length)
            {
                Debug.WriteLine($"Resize {nameof(keywordNameBuffer)} from {keywordNameBuffer.Length} to {byteCount}, allocate it to char array in gc heap.");
                keywordNameBuffer = new char[byteCount];
            }
        
            int keywordNameLength = reader.CopyString(keywordNameBuffer);

            // int actualLength = CopyKeywordName(ref reader, ref keywordNameBuffer);

            ReadOnlySpan<char> keywordName = keywordNameBuffer.Slice(0, keywordNameLength);

            Type? keywordType = ValidationKeywordRegistry.GetKeyword(keywordName, deserializerContext.Dialect);
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
                else if (keyword is ItemsKeyword items)
                {
                    itemsKeyword = items;
                }
                else if (keyword is PrefixItemsKeyword prefixItems)
                {
                    prefixItemsKeyword = prefixItems;
                }
                else if (keyword is AdditionalItemsKeyword additionalItems)
                {
                    additionalItemsKeyword = additionalItems;
                }
                else if (keyword is ItemsWithMultiSchemasKeyword itemsWithMultiSchemas)
                {
                    itemsWithMultiSchemasKeyword = itemsWithMultiSchemas;
                }
            }
            else if (keywordName.Equals(ConditionalValidator.IfKeywordName, StringComparison.Ordinal))
            {
                predictSchema = JsonSerializer.Deserialize<JsonSchema>(ref reader, options);
            }
            else if (keywordName.Equals(ConditionalValidator.ThenKeywordName, StringComparison.Ordinal))
            {
                positiveSchema = JsonSerializer.Deserialize<JsonSchema>(ref reader, options);
            }
            else if (keywordName.Equals(ConditionalValidator.ElseKeywordName, StringComparison.Ordinal))
            {
                negativeSchema = JsonSerializer.Deserialize<JsonSchema>(ref reader, options);
            }
            else if (keywordName.Equals(ArrayContainsValidator.ContainsKeywordName, StringComparison.Ordinal))
            {
                containsSchema = JsonSerializer.Deserialize<JsonSchema>(ref reader, options);
            }
            else if (keywordName.Equals(ArrayContainsValidator.MaxContainsKeywordName, StringComparison.Ordinal))
            {
                if (!reader.TryGetUInt32ForJsonSchema(out uint tmp))
                {
                    throw ThrowHelper.CreateKeywordHasInvalidNonNegativeIntegerJsonException(ArrayContainsValidator.MaxContainsKeywordName);
                }

                maxContains = tmp;
            }
            else if (keywordName.Equals(ArrayContainsValidator.MinContainsKeywordName, StringComparison.Ordinal))
            {
                if (!reader.TryGetUInt32ForJsonSchema(out uint tmp))
                {
                    throw ThrowHelper.CreateKeywordHasInvalidNonNegativeIntegerJsonException(ArrayContainsValidator.MinContainsKeywordName);
                }

                minContains = tmp;
            }
            else if (keywordName.Equals(SchemaReferenceKeyword.Keyword, StringComparison.Ordinal))
            {
                schemaReference = JsonSerializer.Deserialize<SchemaReferenceKeyword>(ref reader, options)!;
                referenceKeywords ??= new List<IReferenceKeyword>(1);
                referenceKeywords.Add(schemaReference);
            }
            else if (keywordName.Equals(SchemaDynamicReferenceKeyword.Keyword, StringComparison.Ordinal))
            {
                SchemaDynamicReferenceKeyword schemaDynamicReference = JsonSerializer.Deserialize<SchemaDynamicReferenceKeyword>(ref reader, options)!;
                referenceKeywords ??= new List<IReferenceKeyword>(1);
                referenceKeywords.Add(schemaDynamicReference);
            }
            else if (keywordName.Equals(SchemaRecursiveReferenceKeyword.Keyword, StringComparison.Ordinal))
            {
                SchemaRecursiveReferenceKeyword schemaRecursiveReference = JsonSerializer.Deserialize<SchemaRecursiveReferenceKeyword>(ref reader, options)!;
                referenceKeywords ??= new List<IReferenceKeyword>(1);
                referenceKeywords.Add(schemaRecursiveReference);
            }
            else if (keywordName.Equals(DefsKeyword.Keyword, StringComparison.Ordinal) || keywordName.Equals(DefsKeyword.KeywordDraft7, StringComparison.Ordinal))
            {
                string defKeywordName = keywordName.Equals(DefsKeyword.Keyword, StringComparison.Ordinal) ? DefsKeyword.Keyword : DefsKeyword.KeywordDraft7;

                defsKeywords ??= new List<(string name, DefsKeyword keyword)>(1);

                if (defsKeywords.Any(defs => defs.name == defKeywordName))
                {
                    reader.Skip();
                }
                else
                {
                    DefsKeyword defsKeyword = JsonSerializer.Deserialize<DefsKeyword>(ref reader, options)!;
                    defsKeywords.Add((defKeywordName, defsKeyword));                    
                }
            }
            else if (keywordName.Equals(IdKeyword.Keyword, StringComparison.Ordinal))
            {
                string idValue = reader.GetString()!;
                if (idValue.StartsWith('#'))
                {
                    plainNameIdentifier = new PlainNameFragmentIdKeyword(idValue.Substring(1));
                }
                else
                {
                    id = new Uri(idValue, UriKind.RelativeOrAbsolute);    
                }
            }
            else if (keywordName.Equals(SchemaKeyword.Keyword, StringComparison.Ordinal))
            {
                schemaKeyword = GetSchemaKeyword(ref reader);

                deserializerContext.Dialect = schemaKeyword.Dialect;
                options = deserializerContext.ToJsonSerializerOptions();
            }
            else if (keywordName.Equals(AnchorKeyword.Keyword, StringComparison.Ordinal))
            {
                plainNameIdentifier = new AnchorKeyword(reader.GetString()!);
            }
            else if (keywordName.Equals(DynamicAnchorKeyword.Keyword, StringComparison.Ordinal))
            {
                dynamicAnchor = reader.GetString();
            }
            else if (keywordName.Equals(RecursiveAnchorKeyword.Keyword, StringComparison.Ordinal))
            {
                recursiveAnchor = reader.GetBoolean();
            }
            else if (ValidationKeywordRegistry.IsIgnoredKeyword(keywordName))
            {
                reader.Skip();
            }
            else if (PotentialSchemaContainerElement.TryDeserialize(ref reader, out ISchemaContainerElement? potentialSchemaContainerElement, options))
            {
                (potentialSchemaContainerElements ??= new Dictionary<string, ISchemaContainerElement>()).Add(keywordName.ToString(), potentialSchemaContainerElement);
            }

            reader.Read();
        }

        List<ISchemaContainerValidationNode>? schemaContainerValidators = null;

        // Based on spec, in Draft 7, when a schema contains a $ref property, any other properties in that schema will not be treated as JSON Schema keywords and will be ignored.
        // Here when there is $ref property, we still keep existing $schema property for Json Schema Document.
        if (deserializerContext.Dialect == DialectKind.Draft7 && schemaReference is not null)
        {
            validationKeywords.Clear();

            Debug.Assert(referenceKeywords is not null);
            referenceKeywords.Clear();
            referenceKeywords.Add(schemaReference);

            plainNameIdentifier = null;
            dynamicAnchor = null;
            recursiveAnchor = false;
            defsKeywords = null;
            id = null;
            potentialSchemaContainerElements = null;
        }
        else
        {
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

            if (additionalItemsKeyword is not null)
            {
                additionalItemsKeyword.ItemsWithMultiSchemasKeyword = itemsWithMultiSchemasKeyword;
            }

            if (containsSchema is not null)
            {
                schemaContainerValidators ??= new List<ISchemaContainerValidationNode>(1);
                schemaContainerValidators.Add(new ArrayContainsValidator(containsSchema, minContains, maxContains));
            }

            if (predictSchema is not null || positiveSchema is not null || negativeSchema is not null)
            {
                schemaContainerValidators ??= new List<ISchemaContainerValidationNode>(1);
                schemaContainerValidators.Add(new ConditionalValidator(predictSchema, positiveSchema, negativeSchema));
            }

            // Although BodyJsonSchema supports merging duplicated keywords, but it is done by changing json schema tree structure. (That is:
            // when there is duplicated keywords, schema structure will be changed)
            // In json schema deserialization case, it is possible that original schema contains "json path" reference, so here we cannot 
            // dare to change original json schema structure at risk of missing reference.
            ThrowIfKeywordsHaveDuplication(validationKeywords);    
        }

        JsonSchema schema;
        if (typeToConvert == typeof(IJsonSchemaDocument))
        {
            schema = new BodyJsonSchemaDocument(validationKeywords, schemaContainerValidators, referenceKeywords, plainNameIdentifier, dynamicAnchor, recursiveAnchor, potentialSchemaContainerElements, schemaKeyword, id, defsKeywords);
        }
        else if (id is not null)
        {
            schema = new JsonSchemaResource(
                schemaKeyword,
                id,
                validationKeywords,
                schemaContainerValidators,
                referenceKeywords,
                plainNameIdentifier,
                dynamicAnchor,
                recursiveAnchor,
                defsKeywords, potentialSchemaContainerElements);
        }
        else if (schemaKeyword is not null)
        {
            throw ThrowHelper.CreateJsonSchemaContainsSchemaKeywordJsonException();
        }
        else
        {
            schema = new BodyJsonSchema(validationKeywords, schemaContainerValidators, referenceKeywords, plainNameIdentifier, dynamicAnchor, defsKeywords, potentialSchemaContainerElements);
        }

        return (T)(object)schema;
    }

    private static SchemaKeyword GetSchemaKeyword(ref Utf8JsonReader reader)
    { 
        long bytes = reader.HasValueSequence ? reader.ValueSequence.Length : reader.ValueSpan.Length;

        scoped Span<char> schemaIdentifierBuffer;

        if (bytes > MaxSchemaIdentifierCharBufferSizeOnStack)
        {
            Debug.WriteLine($"Allocate {nameof(schemaIdentifierBuffer)} to char array with length {bytes} in gc heap.");
            schemaIdentifierBuffer = new char[bytes];
        }
        else
        {
            schemaIdentifierBuffer = stackalloc char[(int)bytes];
        }

        int schemaIdentifierLength = reader.CopyString(schemaIdentifierBuffer);

        ReadOnlySpan<char> schemaIdentifier = schemaIdentifierBuffer.Slice(0, schemaIdentifierLength);

        return SchemaKeyword.Create(schemaIdentifier);
    }

    private static int CopyKeywordName(ref Utf8JsonReader reader, ref Span<char> keywordNameBuffer)
    {
        long bytes = reader.HasValueSequence ? reader.ValueSequence.Length : reader.ValueSpan.Length;

        if (keywordNameBuffer.Length < bytes)
        {
            keywordNameBuffer = new char[bytes];
        }
        
        return reader.CopyString(keywordNameBuffer);
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

                if (schemaResource.SchemaKeyword is not null)
                {
                    writer.WriteString(SchemaKeyword.Keyword, schemaResource.SchemaKeyword.DraftIdentifier.ToString());
                }

                writer.WriteString(IdKeyword.Keyword, schemaResource.BaseUri!.ToString());

                // RecursiveAnchor part:
                if (schemaResource.RecursiveAnchor)
                {
                    writer.WriteBoolean(RecursiveAnchorKeyword.Keyword, RecursiveAnchorKeyword.EnabledValue);
                }

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
            if (schema.DefsKeywords is not null)
            {
                foreach ((string name, DefsKeyword defsKeyword) in schema.DefsKeywords)
                {
                    writer.WritePropertyName(name);
                    JsonSerializer.Serialize(writer, defsKeyword, options);                    
                }
            }

            // Reference & DynamicReference & RecursiveReference part:
            if (schema.ReferenceKeywords is not null)
            {
                foreach (IReferenceKeyword referenceKeyword in schema.ReferenceKeywords)
                {
                    writer.WritePropertyName(referenceKeyword.Name);
                    JsonSerializer.Serialize(writer, referenceKeyword, referenceKeyword.GetType(), options);
                }
            }

            // Plain named identifier part ($id or anchor):
            if (schema.PlainNameIdentifierKeyword is not null)
            {
                writer.WritePropertyName(schema.PlainNameIdentifierKeyword.KeywordName);
                writer.WriteStringValue(schema.PlainNameIdentifierKeyword.SerializedValue);
            }

            // DynamicAnchor part:
            if (schema.DynamicAnchor is not null)
            {
                writer.WritePropertyName(DynamicAnchorKeyword.Keyword);
                writer.WriteStringValue(schema.DynamicAnchor);
            }

            // ArrayContainsValidator & ConditionalValidator part:
            if (schema.SchemaContainerValidators is not null)
            {
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