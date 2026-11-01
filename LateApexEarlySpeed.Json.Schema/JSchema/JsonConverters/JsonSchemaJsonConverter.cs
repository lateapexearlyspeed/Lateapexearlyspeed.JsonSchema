using LateApexEarlySpeed.Json.Schema.Common;
using LateApexEarlySpeed.Json.Schema.Common.interfaces;
using LateApexEarlySpeed.Json.Schema.JSchema.interfaces;
using LateApexEarlySpeed.Json.Schema.Keywords;
using LateApexEarlySpeed.Json.Schema.Keywords.Annotations;
using LateApexEarlySpeed.Json.Schema.Keywords.Draft7;
using LateApexEarlySpeed.Json.Schema.Keywords.interfaces;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace LateApexEarlySpeed.Json.Schema.JSchema.JsonConverters;

internal class JsonSchemaJsonConverter<T> : JsonConverter<T>
{
    private delegate bool TryTurnReadPart(ref BodyJsonSchemaReadContext bodyJsonSchemaReadContext, ref Utf8JsonReader reader, scoped Span<char> keywordNameBuffer, ref char[]? bigKeywordNameBuffer, JsonSerializerOptions options);

    /// <summary>
    /// The longest keyword across all JSON Schema drafts is 'unevaluatedProperties' - 21 characters
    /// </summary>
    private const int KeywordNameCharBufferSizeOnStack = 21;

    /// <summary>
    /// The longest standard schema identifier is <see cref="SchemaKeyword.Draft202012IdentifierString"/> - 44 characters
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

        Span<char> keywordNameBuffer = stackalloc char[KeywordNameCharBufferSizeOnStack];
        char[]? bigKeywordNameBuffer = null;
        var bodyJsonSchemaReadContext = new BodyJsonSchemaReadContext(options);

        if (bodyJsonSchemaReadContext.DeserializerContext.IsCustomMetaSchema)
        {
            ReadForCustomMetaSchema(ref bodyJsonSchemaReadContext, ref reader, keywordNameBuffer, ref bigKeywordNameBuffer, options);
        }
        else
        {
            ReadForStandardSchemaDialect(ref bodyJsonSchemaReadContext, ref reader, keywordNameBuffer, ref bigKeywordNameBuffer, options);
        }
        

        return GenerateSchema(ref bodyJsonSchemaReadContext, typeToConvert);
    }

    private static void ReadForStandardSchemaDialect(ref BodyJsonSchemaReadContext bodyJsonSchemaReadContext, ref Utf8JsonReader reader, scoped Span<char> keywordNameBuffer, ref char[]? bigKeywordNameBuffer, JsonSerializerOptions options)
    {
        while (reader.TokenType != JsonTokenType.EndObject)
        {
            ReadOnlySpan<char> keywordName = ReadKeywordName(in reader, keywordNameBuffer, ref bigKeywordNameBuffer);

            Type? keywordType = bodyJsonSchemaReadContext.DeserializerContext.GetValidationKeyword(keywordName);
            reader.Read();

            if (keywordType is not null)
            {
                ReadValidationKeyword(ref bodyJsonSchemaReadContext, ref reader, keywordType, options);
            }
            else
            {
                if (!ReadComplexKeywords(keywordName, ref bodyJsonSchemaReadContext, ref reader, keywordNameBuffer, ref bigKeywordNameBuffer, options, TryTurnToCustomMetaSchemaReadPart))
                {
                    return;
                }
            }

            reader.Read();
        }

        static bool TryTurnToCustomMetaSchemaReadPart(ref BodyJsonSchemaReadContext bodyJsonSchemaReadContext, ref Utf8JsonReader reader, scoped Span<char> keywordNameBuffer, ref char[]? bigKeywordNameBuffer, JsonSerializerOptions options)
        {
            if (!bodyJsonSchemaReadContext.DeserializerContext.IsCustomMetaSchema)
            {
                return false;
            }

            reader.Read();

            ReadForCustomMetaSchema(ref bodyJsonSchemaReadContext, ref reader, keywordNameBuffer, ref bigKeywordNameBuffer, options);
            return true;
        }
    }

    private static void ReadForCustomMetaSchema(ref BodyJsonSchemaReadContext bodyJsonSchemaReadContext, ref Utf8JsonReader reader, scoped Span<char> keywordNameBuffer, ref char[]? bigKeywordNameBuffer, JsonSerializerOptions options)
    {
        while (reader.TokenType != JsonTokenType.EndObject)
        {
            ReadOnlySpan<char> keywordName = ReadKeywordName(in reader, keywordNameBuffer, ref bigKeywordNameBuffer);

            Type? keywordType = bodyJsonSchemaReadContext.DeserializerContext.GetValidationKeywordFromVocabulary(keywordName);
            reader.Read();

            if (keywordType is not null)
            {
                ReadValidationKeyword(ref bodyJsonSchemaReadContext, ref reader, keywordType, options);
            }
            else
            {
                if (!bodyJsonSchemaReadContext.DeserializerContext.ContainsKeywordInVocabulary(keywordName))
                {
                    reader.HandleFinalBlockAndSkip();
                }
                else
                {
                    if (!ReadComplexKeywords(keywordName, ref bodyJsonSchemaReadContext, ref reader, keywordNameBuffer, ref bigKeywordNameBuffer, options, TryTurnToStandardSchemaDialectReadPart))
                    {
                        return;
                    }
                }
            }

            reader.Read();
        }

        static bool TryTurnToStandardSchemaDialectReadPart(ref BodyJsonSchemaReadContext bodyJsonSchemaReadContext, ref Utf8JsonReader reader, scoped Span<char> keywordNameBuffer, ref char[]? bigKeywordNameBuffer, JsonSerializerOptions options)
        {
            if (bodyJsonSchemaReadContext.DeserializerContext.IsCustomMetaSchema)
            {
                return false;
            }

            reader.Read();

            ReadForStandardSchemaDialect(ref bodyJsonSchemaReadContext, ref reader, keywordNameBuffer, ref bigKeywordNameBuffer, options);
            return true;
        }
    }

    private static bool ReadComplexKeywords(scoped ReadOnlySpan<char> keywordName, ref BodyJsonSchemaReadContext bodyJsonSchemaReadContext, ref Utf8JsonReader reader, scoped Span<char> keywordNameBuffer, ref char[]? bigKeywordNameBuffer, JsonSerializerOptions options, TryTurnReadPart tryTurnReadPart)
    {
        Type? annotationType;

        if (keywordName.Equals(ConditionalValidator.IfKeywordName, StringComparison.Ordinal))
        {
            bodyJsonSchemaReadContext.PredictSchema = JsonSerializer.Deserialize<JsonSchema>(ref reader, options);
        }
        else if (keywordName.Equals(ConditionalValidator.ThenKeywordName, StringComparison.Ordinal))
        {
            bodyJsonSchemaReadContext.PositiveSchema = JsonSerializer.Deserialize<JsonSchema>(ref reader, options);
        }
        else if (keywordName.Equals(ConditionalValidator.ElseKeywordName, StringComparison.Ordinal))
        {
            bodyJsonSchemaReadContext.NegativeSchema = JsonSerializer.Deserialize<JsonSchema>(ref reader, options);
        }
        else if (keywordName.Equals(ArrayContainsValidator.ContainsKeywordName, StringComparison.Ordinal))
        {
            bodyJsonSchemaReadContext.ContainsSchema = JsonSerializer.Deserialize<JsonSchema>(ref reader, options);
        }
        else if (keywordName.Equals(ArrayContainsValidator.MaxContainsKeywordName, StringComparison.Ordinal))
        {
            if (!reader.TryGetUInt32ForJsonSchema(out uint tmp))
            {
                throw ThrowHelper.CreateKeywordHasInvalidNonNegativeIntegerJsonException(ArrayContainsValidator.MaxContainsKeywordName);
            }

            bodyJsonSchemaReadContext.MaxContains = tmp;
        }
        else if (keywordName.Equals(ArrayContainsValidator.MinContainsKeywordName, StringComparison.Ordinal))
        {
            if (!reader.TryGetUInt32ForJsonSchema(out uint tmp))
            {
                throw ThrowHelper.CreateKeywordHasInvalidNonNegativeIntegerJsonException(ArrayContainsValidator.MinContainsKeywordName);
            }

            bodyJsonSchemaReadContext.MinContains = tmp;
        }
        else if (keywordName.Equals(SchemaReferenceKeyword.Keyword, StringComparison.Ordinal))
        {
            bodyJsonSchemaReadContext.SchemaReference = JsonSerializer.Deserialize<SchemaReferenceKeyword>(ref reader, options)!;
            bodyJsonSchemaReadContext.ReferenceKeywords ??= new List<IReferenceKeyword>(1);
            bodyJsonSchemaReadContext.ReferenceKeywords.Add(bodyJsonSchemaReadContext.SchemaReference);
        }
        else if (keywordName.Equals(SchemaDynamicReferenceKeyword.Keyword, StringComparison.Ordinal))
        {
            SchemaDynamicReferenceKeyword schemaDynamicReference = JsonSerializer.Deserialize<SchemaDynamicReferenceKeyword>(ref reader, options)!;
            bodyJsonSchemaReadContext.ReferenceKeywords ??= new List<IReferenceKeyword>(1);
            bodyJsonSchemaReadContext.ReferenceKeywords.Add(schemaDynamicReference);
        }
        else if (keywordName.Equals(SchemaRecursiveReferenceKeyword.Keyword, StringComparison.Ordinal))
        {
            SchemaRecursiveReferenceKeyword schemaRecursiveReference = JsonSerializer.Deserialize<SchemaRecursiveReferenceKeyword>(ref reader, options)!;
            bodyJsonSchemaReadContext.ReferenceKeywords ??= new List<IReferenceKeyword>(1);
            bodyJsonSchemaReadContext.ReferenceKeywords.Add(schemaRecursiveReference);
        }
        else if (keywordName.Equals(DefsKeyword.Keyword, StringComparison.Ordinal) || keywordName.Equals(DefsKeyword.KeywordDraft7, StringComparison.Ordinal))
        {
            string defKeywordName = keywordName.Equals(DefsKeyword.Keyword, StringComparison.Ordinal) ? DefsKeyword.Keyword : DefsKeyword.KeywordDraft7;

            bodyJsonSchemaReadContext.DefsKeywords ??= new List<(string name, DefsKeyword keyword)>(1);

            if (bodyJsonSchemaReadContext.DefsKeywords.Any(defs => defs.name == defKeywordName))
            {
                reader.HandleFinalBlockAndSkip();
            }
            else
            {
                DefsKeyword defsKeyword = JsonSerializer.Deserialize<DefsKeyword>(ref reader, options)!;
                bodyJsonSchemaReadContext.DefsKeywords.Add((defKeywordName, defsKeyword));
            }
        }
        else if (keywordName.Equals(IdKeyword.Keyword, StringComparison.Ordinal))
        {
            string idValue = reader.GetString()!;
            if (idValue.StartsWith('#'))
            {
                bodyJsonSchemaReadContext.PlainNameIdentifier = new PlainNameFragmentIdKeyword(idValue.Substring(1));
            }
            else
            {
                bodyJsonSchemaReadContext.Id = new Uri(idValue, UriKind.RelativeOrAbsolute);
            }
        }
        else if (keywordName.Equals(SchemaKeyword.Keyword, StringComparison.Ordinal))
        {
            bodyJsonSchemaReadContext.SchemaKeyword = GetSchemaKeyword(ref reader, bodyJsonSchemaReadContext.DeserializerContext.InternalMetaSchemaRegistry);

            bodyJsonSchemaReadContext.DeserializerContext.Dialect = bodyJsonSchemaReadContext.SchemaKeyword.Dialect;
            bodyJsonSchemaReadContext.DeserializerContext.MetaSchemaId = bodyJsonSchemaReadContext.SchemaKeyword.Identifier;
            options = bodyJsonSchemaReadContext.DeserializerContext.ToJsonSerializerOptions();

            if (tryTurnReadPart(ref bodyJsonSchemaReadContext, ref reader, keywordNameBuffer, ref bigKeywordNameBuffer, options))
            {
                return false;
            }
        }
        else if (keywordName.Equals(AnchorKeyword.Keyword, StringComparison.Ordinal))
        {
            bodyJsonSchemaReadContext.PlainNameIdentifier = new AnchorKeyword(reader.GetString()!);
        }
        else if (keywordName.Equals(DynamicAnchorKeyword.Keyword, StringComparison.Ordinal))
        {
            bodyJsonSchemaReadContext.DynamicAnchor = reader.GetString();
        }
        else if (keywordName.Equals(RecursiveAnchorKeyword.Keyword, StringComparison.Ordinal))
        {
            bodyJsonSchemaReadContext.RecursiveAnchor = reader.GetBoolean();
        }
        else if ((annotationType = AnnotationKeywordRegistry.GetKeyword(keywordName)) is not null)
        {
            if (bodyJsonSchemaReadContext.DeserializerContext.CollectAnnotations)
            {
                IAnnotationKeyword? annotationKeyword = JsonSerializer.Deserialize(ref reader, annotationType, options) as IAnnotationKeyword;
                Debug.Assert(annotationKeyword is not null);

                bodyJsonSchemaReadContext.AnnotationKeywords ??= new();
                bodyJsonSchemaReadContext.AnnotationKeywords.Add(annotationKeyword);
            }
            else
            {
                reader.HandleFinalBlockAndSkip();
            }
        }
        else if (keywordName.Equals(UnevaluatedPropertiesKeyword.Keyword, StringComparison.Ordinal))
        {
            bodyJsonSchemaReadContext.UnevaluatedPropertiesKeyword = JsonSerializer.Deserialize<UnevaluatedPropertiesKeyword>(ref reader, options)!;
        }
        else if (keywordName.Equals(UnevaluatedItemsKeyword.Keyword, StringComparison.Ordinal))
        {
            bodyJsonSchemaReadContext.UnevaluatedItemsKeyword = JsonSerializer.Deserialize<UnevaluatedItemsKeyword>(ref reader, options)!;
        }
        else if (ValidationKeywordRegistry.IsIgnoredKeyword(keywordName))
        {
            reader.HandleFinalBlockAndSkip();
        }
        else if (PotentialSchemaContainerElement.TryDeserialize(ref reader, out ISchemaContainerElement? potentialSchemaContainerElement, options))
        {
            (bodyJsonSchemaReadContext.PotentialSchemaContainerElements ??= new Dictionary<string, ISchemaContainerElement>()).Add(keywordName.ToString(), potentialSchemaContainerElement);
        }

        return true;
    }

    private static ReadOnlySpan<char> ReadKeywordName(in Utf8JsonReader reader, Span<char> keywordNameBuffer, ref char[]? bigKeywordNameBuffer)
    {
        var byteCount = reader.HasValueSequence ? reader.ValueSequence.Length : reader.ValueSpan.Length;

        if (byteCount > keywordNameBuffer.Length)
        {
            if (bigKeywordNameBuffer is null || byteCount > bigKeywordNameBuffer.Length)
            {
                Debug.WriteLine($"Resize {nameof(keywordNameBuffer)} from {GetPreviousBufferLength(keywordNameBuffer, bigKeywordNameBuffer)} to {byteCount}, allocate it to char array in gc heap.");
                bigKeywordNameBuffer = new char[byteCount];
            }
            
            keywordNameBuffer = bigKeywordNameBuffer;
        }
        
        int keywordNameLength = reader.CopyString(keywordNameBuffer);
        // int actualLength = CopyKeywordName(ref reader, ref keywordNameBuffer);

        return keywordNameBuffer.Slice(0, keywordNameLength);

        static int GetPreviousBufferLength(Span<char> keywordNameBuffer, char[]? bigKeywordNameBuffer) => bigKeywordNameBuffer?.Length ?? keywordNameBuffer.Length;
    }

    private static T GenerateSchema(ref BodyJsonSchemaReadContext bodyJsonSchemaReadContext, Type typeToConvert)
    {
        List<ISchemaContainerValidationNode>? schemaContainerValidators = null;

        // Based on spec, in Draft 7, when a schema contains a $ref property, any other properties in that schema will not be treated as JSON Schema keywords and will be ignored.
        // Here when there is $ref property, we still keep existing $schema property for Json Schema Document.
        if (bodyJsonSchemaReadContext.DeserializerContext.Dialect == DialectKind.Draft7 && bodyJsonSchemaReadContext.SchemaReference is not null)
        {
            bodyJsonSchemaReadContext.ValidationKeywords.Clear();
            bodyJsonSchemaReadContext.AnnotationKeywords = null;
            bodyJsonSchemaReadContext.UnevaluatedPropertiesKeyword = null;
            bodyJsonSchemaReadContext.UnevaluatedItemsKeyword = null;

            Debug.Assert(bodyJsonSchemaReadContext.ReferenceKeywords is not null);
            bodyJsonSchemaReadContext.ReferenceKeywords.Clear();
            bodyJsonSchemaReadContext.ReferenceKeywords.Add(bodyJsonSchemaReadContext.SchemaReference);

            bodyJsonSchemaReadContext.PlainNameIdentifier = null;
            bodyJsonSchemaReadContext.DynamicAnchor = null;
            bodyJsonSchemaReadContext.RecursiveAnchor = false;
            bodyJsonSchemaReadContext.DefsKeywords = null;
            bodyJsonSchemaReadContext.Id = null;
            bodyJsonSchemaReadContext.PotentialSchemaContainerElements = null;
        }
        else
        {
            // Set dependent keywords
            if (bodyJsonSchemaReadContext.AdditionalPropertiesKeyword is not null)
            {
                bodyJsonSchemaReadContext.AdditionalPropertiesKeyword.PropertiesKeyword = bodyJsonSchemaReadContext.PropertiesKeyword;
                bodyJsonSchemaReadContext.AdditionalPropertiesKeyword.PatternPropertiesKeyword = bodyJsonSchemaReadContext.PatternPropertiesKeyword;
            }

            if (bodyJsonSchemaReadContext.ItemsKeyword is not null)
            {
                bodyJsonSchemaReadContext.ItemsKeyword.PrefixItemsKeyword = bodyJsonSchemaReadContext.PrefixItemsKeyword;
            }

            if (bodyJsonSchemaReadContext.AdditionalItemsKeyword is not null)
            {
                bodyJsonSchemaReadContext.AdditionalItemsKeyword.ItemsWithMultiSchemasKeyword = bodyJsonSchemaReadContext.ItemsWithMultiSchemasKeyword;
            }

            if (bodyJsonSchemaReadContext.ContainsSchema is not null)
            {
                schemaContainerValidators ??= new List<ISchemaContainerValidationNode>(1);
                schemaContainerValidators.Add(new ArrayContainsValidator(bodyJsonSchemaReadContext.ContainsSchema, bodyJsonSchemaReadContext.MinContains, bodyJsonSchemaReadContext.MaxContains));
            }

            if (bodyJsonSchemaReadContext.PredictSchema is not null || bodyJsonSchemaReadContext.PositiveSchema is not null || bodyJsonSchemaReadContext.NegativeSchema is not null)
            {
                schemaContainerValidators ??= new List<ISchemaContainerValidationNode>(1);
                schemaContainerValidators.Add(new ConditionalValidator(bodyJsonSchemaReadContext.PredictSchema, bodyJsonSchemaReadContext.PositiveSchema, bodyJsonSchemaReadContext.NegativeSchema));
            }

            if (bodyJsonSchemaReadContext.UnevaluatedPropertiesKeyword is not null)
            {
                IEnumerable<IPropertyEvaluator> propertyEvaluators = bodyJsonSchemaReadContext.ValidationKeywords.OfType<IPropertyEvaluator>();

                if (schemaContainerValidators is not null)
                {
                    propertyEvaluators = propertyEvaluators.Concat(schemaContainerValidators.OfType<IPropertyEvaluator>());
                }

                if (bodyJsonSchemaReadContext.ReferenceKeywords is not null)
                {
                    propertyEvaluators = propertyEvaluators.Concat(bodyJsonSchemaReadContext.ReferenceKeywords);
                }

                bodyJsonSchemaReadContext.UnevaluatedPropertiesKeyword.PropertyEvaluators = propertyEvaluators.ToArray();
            }

            if (bodyJsonSchemaReadContext.UnevaluatedItemsKeyword is not null)
            {
                IEnumerable<IItemEvaluator> itemEvaluators = bodyJsonSchemaReadContext.ValidationKeywords.OfType<IItemEvaluator>();

                if (schemaContainerValidators is not null)
                {
                    itemEvaluators = itemEvaluators.Concat(schemaContainerValidators.OfType<IItemEvaluator>());
                }

                if (bodyJsonSchemaReadContext.ReferenceKeywords is not null)
                {
                    itemEvaluators = itemEvaluators.Concat(bodyJsonSchemaReadContext.ReferenceKeywords);
                }

                bodyJsonSchemaReadContext.UnevaluatedItemsKeyword.ItemEvaluators = itemEvaluators.ToArray();
            }

            RemoveIgnoredContentSchemaAnnotation(ref bodyJsonSchemaReadContext.AnnotationKeywords);

            // Although BodyJsonSchema supports merging duplicated keywords, but it is done by changing json schema tree structure. (That is:
            // when there is duplicated keywords, schema structure will be changed)
            // In json schema deserialization case, it is possible that original schema contains "json path" reference, so here we cannot 
            // dare to change original json schema structure at risk of missing reference.
            ThrowIfKeywordsHaveDuplication(bodyJsonSchemaReadContext.ValidationKeywords);    
        }

        JsonSchema schema;
        if (typeToConvert == typeof(IJsonSchemaDocument))
        {
            schema = new BodyJsonSchemaDocument(bodyJsonSchemaReadContext.ValidationKeywords, bodyJsonSchemaReadContext.AnnotationKeywords, schemaContainerValidators, bodyJsonSchemaReadContext.ReferenceKeywords, bodyJsonSchemaReadContext.PlainNameIdentifier, bodyJsonSchemaReadContext.DynamicAnchor, bodyJsonSchemaReadContext.RecursiveAnchor, new UnevaluatedKeywords{ UnevaluatedPropertiesKeyword = bodyJsonSchemaReadContext.UnevaluatedPropertiesKeyword, UnevaluatedItemsKeyword = bodyJsonSchemaReadContext.UnevaluatedItemsKeyword }, bodyJsonSchemaReadContext.PotentialSchemaContainerElements, bodyJsonSchemaReadContext.SchemaKeyword, bodyJsonSchemaReadContext.Id, bodyJsonSchemaReadContext.DefsKeywords);
        }
        else if (bodyJsonSchemaReadContext.Id is not null)
        {
            schema = new JsonSchemaResource(
                bodyJsonSchemaReadContext.SchemaKeyword,
                bodyJsonSchemaReadContext.Id,
                bodyJsonSchemaReadContext.ValidationKeywords,
                bodyJsonSchemaReadContext.AnnotationKeywords,
                schemaContainerValidators,
                bodyJsonSchemaReadContext.ReferenceKeywords,
                bodyJsonSchemaReadContext.PlainNameIdentifier,
                bodyJsonSchemaReadContext.DynamicAnchor,
                bodyJsonSchemaReadContext.RecursiveAnchor,
                bodyJsonSchemaReadContext.DefsKeywords, bodyJsonSchemaReadContext.PotentialSchemaContainerElements, new UnevaluatedKeywords{ UnevaluatedPropertiesKeyword = bodyJsonSchemaReadContext.UnevaluatedPropertiesKeyword, UnevaluatedItemsKeyword = bodyJsonSchemaReadContext.UnevaluatedItemsKeyword });
        }
        else
        {
            schema = new BodyJsonSchema(bodyJsonSchemaReadContext.ValidationKeywords, bodyJsonSchemaReadContext.AnnotationKeywords, schemaContainerValidators, bodyJsonSchemaReadContext.ReferenceKeywords, bodyJsonSchemaReadContext.PlainNameIdentifier, bodyJsonSchemaReadContext.DynamicAnchor, bodyJsonSchemaReadContext.DefsKeywords, bodyJsonSchemaReadContext.PotentialSchemaContainerElements, new UnevaluatedKeywords{ UnevaluatedPropertiesKeyword = bodyJsonSchemaReadContext.UnevaluatedPropertiesKeyword, UnevaluatedItemsKeyword = bodyJsonSchemaReadContext.UnevaluatedItemsKeyword }, bodyJsonSchemaReadContext.SchemaKeyword);
        }

        return (T)(object)schema;
    }

    private static void ReadValidationKeyword(ref BodyJsonSchemaReadContext bodyJsonSchemaReadContext, ref Utf8JsonReader reader, Type keywordType, JsonSerializerOptions options)
    {
        ValidationKeywordBase? keyword = JsonSerializer.Deserialize(ref reader, keywordType, options) as ValidationKeywordBase;
        Debug.Assert(keyword is not null);

        bodyJsonSchemaReadContext.ValidationKeywords.Add(keyword);

        switch (keyword)
        {
            // Store dependent keywords
            case PropertiesKeyword properties:
                bodyJsonSchemaReadContext.PropertiesKeyword = properties;
                break;
            case PatternPropertiesKeyword pattern:
                bodyJsonSchemaReadContext.PatternPropertiesKeyword = pattern;
                break;
            case AdditionalPropertiesKeyword additionalProperties:
                bodyJsonSchemaReadContext.AdditionalPropertiesKeyword = additionalProperties;
                break;
            case ItemsKeyword items:
                bodyJsonSchemaReadContext.ItemsKeyword = items;
                break;
            case PrefixItemsKeyword prefixItems:
                bodyJsonSchemaReadContext.PrefixItemsKeyword = prefixItems;
                break;
            case AdditionalItemsKeyword additionalItems:
                bodyJsonSchemaReadContext.AdditionalItemsKeyword = additionalItems;
                break;
            case ItemsWithMultiSchemasKeyword itemsWithMultiSchemas:
                bodyJsonSchemaReadContext.ItemsWithMultiSchemasKeyword = itemsWithMultiSchemas;
                break;
            // This is for 'format' keyword which is both validation keyword and annotation keyword
            case IAnnotationKeyword annotationKeyword:
                if (bodyJsonSchemaReadContext.DeserializerContext.CollectAnnotations)
                {
                    bodyJsonSchemaReadContext.AnnotationKeywords ??= new();
                    bodyJsonSchemaReadContext.AnnotationKeywords.Add(annotationKeyword);
                }
                break;
        }
    }

    private ref struct BodyJsonSchemaReadContext
    {
        public JsonSchemaDeserializerContext DeserializerContext;

        public readonly List<ValidationKeywordBase> ValidationKeywords = new();
        public List<IAnnotationKeyword>? AnnotationKeywords;

        public SchemaKeyword? SchemaKeyword;

        public PropertiesKeyword? PropertiesKeyword;
        public PatternPropertiesKeyword? PatternPropertiesKeyword;
        public AdditionalPropertiesKeyword? AdditionalPropertiesKeyword;
        public UnevaluatedPropertiesKeyword? UnevaluatedPropertiesKeyword;

        public PrefixItemsKeyword? PrefixItemsKeyword;
        public ItemsKeyword? ItemsKeyword;

        public AdditionalItemsKeyword? AdditionalItemsKeyword;
        public ItemsWithMultiSchemasKeyword? ItemsWithMultiSchemasKeyword;
        public UnevaluatedItemsKeyword? UnevaluatedItemsKeyword;

        public JsonSchema? PredictSchema;
        public JsonSchema? PositiveSchema;
        public JsonSchema? NegativeSchema;
        public JsonSchema? ContainsSchema;
        public uint? MaxContains;
        public uint? MinContains;

        public SchemaReferenceKeyword? SchemaReference;
        public List<IReferenceKeyword>? ReferenceKeywords;
        public List<(string name, DefsKeyword keyword)>? DefsKeywords;
        public Uri? Id;
        public IPlainNameIdentifierKeyword? PlainNameIdentifier;
        public string? DynamicAnchor;
        public bool RecursiveAnchor;

        public Dictionary<string, ISchemaContainerElement>? PotentialSchemaContainerElements;
        
        public BodyJsonSchemaReadContext(JsonSerializerOptions options)
        {
            DeserializerContext = new JsonSchemaDeserializerContext(options);
        }
    }

    /// <summary>
    /// Remove the 'contentSchema' annotation keyword because it is ignored when adjacent 'contentMediaType' is absent.
    /// </summary>
    /// <param name="annotationKeywords">reference may be set to <see langword="null"/>
    /// when the removed 'contentSchema' annotation was the only annotation keyword.
    /// </param>
    private static void RemoveIgnoredContentSchemaAnnotation(ref List<IAnnotationKeyword>? annotationKeywords)
    {
        if (annotationKeywords is null)
        {
            return;
        }

        int? contentSchemaIdx = null;

        for (int i = 0; i < annotationKeywords.Count; i++)
        {
            IAnnotationKeyword keyword = annotationKeywords[i];
            
            if (keyword.Name == ContentMediaTypeAnnotation.Keyword)
            {
                return;
            }

            if (keyword.Name == ContentSchemaAnnotation.Keyword)
            {
                contentSchemaIdx = i;
            }
        }

        if (!contentSchemaIdx.HasValue)
        {
            return;
        }

        // From now, we should remove the 'contentSchema' annotation keyword, because it is ignored without adjacent 'contentMediaType'.
        if (annotationKeywords.Count == 1) // here we can fast return null, because the only annotation keyword in list is 'contentSchema'.
        {
            annotationKeywords = null;
            return;
        }

        annotationKeywords.RemoveAt(contentSchemaIdx.Value);
    }

    private static SchemaKeyword GetSchemaKeyword(ref Utf8JsonReader reader, MetaSchemaRegistry? metaSchemaRegistry)
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

        return SchemaKeyword.Create(schemaIdentifier, metaSchemaRegistry);
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

    private static void ThrowIfKeywordsHaveDuplication(ICollection<ValidationKeywordBase> keywords)
    {
        ValidationKeywordBase? duplicatedKeyword = keywords.FindFirstDuplicatedItem(keyword => keyword.Name);
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

            BodyJsonSchema schema = (BodyJsonSchema)(object)value;

            if (schema.SchemaKeyword is not null)
            {
                writer.WriteString(SchemaKeyword.Keyword, schema.SchemaKeyword.Identifier.ToString());
            }

            if (value is JsonSchemaResource schemaResource)
            {
                Debug.Assert(value.GetType() == typeof(JsonSchemaResource) || value.GetType() == typeof(BodyJsonSchemaDocument));

                writer.WriteString(IdKeyword.Keyword, schemaResource.BaseUri!.ToString());

                // RecursiveAnchor part:
                if (schemaResource.RecursiveAnchor)
                {
                    writer.WriteBoolean(RecursiveAnchorKeyword.Keyword, RecursiveAnchorKeyword.EnabledValue);
                }
            }

            // Validation keyword part:
            foreach (ValidationKeywordBase keyword in schema.ValidationKeywords)
            {
                writer.WritePropertyName(keyword.Name);
                JsonSerializer.Serialize(writer, keyword, keyword.GetType(), options);
            }

            // Annotation keyword part:
            if (schema.AnnotationKeywords is not null)
            {
                foreach (IAnnotationKeyword annotationKeyword in schema.AnnotationKeywords)
                {
                    if (annotationKeyword is not ValidationKeywordBase)
                    {
                        writer.WritePropertyName(annotationKeyword.Name);
                        JsonSerializer.Serialize(writer, annotationKeyword, annotationKeyword.GetType(), options);
                    }
                }
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

            // UnevaluatedProperties & UnevaluatedItems
            if (schema.UnevaluatedPropertiesKeyword is not null)
            {
                writer.WritePropertyName(UnevaluatedPropertiesKeyword.Keyword);
                JsonSerializer.Serialize(writer, schema.UnevaluatedPropertiesKeyword, options);
            }

            if (schema.UnevaluatedItemsKeyword is not null)
            {
                writer.WritePropertyName(UnevaluatedItemsKeyword.Keyword);
                JsonSerializer.Serialize(writer, schema.UnevaluatedItemsKeyword, options);
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