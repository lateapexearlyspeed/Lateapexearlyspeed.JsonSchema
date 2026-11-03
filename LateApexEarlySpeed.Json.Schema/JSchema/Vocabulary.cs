using LateApexEarlySpeed.Json.Schema.Keywords;
using LateApexEarlySpeed.Json.Schema.Keywords.Annotations;
using LateApexEarlySpeed.Json.Schema.Keywords.Draft7;

namespace LateApexEarlySpeed.Json.Schema.JSchema;

internal class Vocabulary
{
    private static readonly Dictionary<Uri, Vocabulary> Vocabularies;

    static Vocabulary()
    {
        Vocabulary[] vocabularies = new[]
        {
            CreateCore2020(),
            CreateApplicator2020(),
            CreateUnevaluated2020(),
            CreateValidation2020(),
            CreateMetaData2020(),
            CreateContent2020(),
            CreateFormatAnnotation2020(),
            CreateFormatAssertion2020(),
            CreateCore2019(),
            CreateApplicator2019(),
            CreateValidation2019(),
            CreateMetaData2019(),
            CreateContent2019(),
            CreateFormat2019()
        };

        Vocabularies = vocabularies.ToDictionary(v => v.Id);
    }

    private static Vocabulary CreateCore2020()
    {
        return new(new Uri("https://json-schema.org/draft/2020-12/vocab/core"), 
            Array.Empty<Type>(),
            new[]{IdKeyword.Keyword, SchemaKeyword.Keyword, SchemaReferenceKeyword.Keyword, AnchorKeyword.Keyword, SchemaDynamicReferenceKeyword.Keyword, DynamicAnchorKeyword.Keyword, DefsKeyword.Keyword });
    }

    private static Vocabulary CreateApplicator2020()
    {
        return new(new Uri("https://json-schema.org/draft/2020-12/vocab/applicator"), 
            new[]{typeof(PrefixItemsKeyword), typeof(ItemsKeyword), typeof(AdditionalPropertiesKeyword), typeof(PropertiesKeyword), typeof(PatternPropertiesKeyword), typeof(DependentSchemasKeyword), typeof(PropertyNamesKeyword), typeof(AllOfKeyword), typeof(AnyOfKeyword), typeof(OneOfKeyword), typeof(NotKeyword)},
            new[]{ArrayContainsValidator.ContainsKeywordName, ConditionalValidator.IfKeywordName, ConditionalValidator.ThenKeywordName, ConditionalValidator.ElseKeywordName });
    }
    
    private static Vocabulary CreateUnevaluated2020()
    {
        return new(new Uri("https://json-schema.org/draft/2020-12/vocab/unevaluated"), 
            Array.Empty<Type>(),
            new[]{UnevaluatedItemsKeyword.Keyword, UnevaluatedPropertiesKeyword.Keyword });
    }

    private static Vocabulary CreateValidation2020()
    {
        return new(new Uri("https://json-schema.org/draft/2020-12/vocab/validation"), 
            new[]{typeof(TypeKeyword), typeof(ConstKeyword), typeof(EnumKeyword), typeof(MultipleOfKeyword), typeof(MaximumKeyword), typeof(ExclusiveMaximumKeyword), typeof(MinimumKeyword), typeof(ExclusiveMinimumKeyword), typeof(MaxLengthKeyword), typeof(MinLengthKeyword), typeof(PatternKeyword), typeof(MaxItemsKeyword), typeof(MinItemsKeyword), typeof(UniqueItemsKeyword), typeof(MaxPropertiesKeyword), typeof(MinPropertiesKeyword), typeof(RequiredKeyword), typeof(DependentRequiredKeyword)},
            new[]{ArrayContainsValidator.MaxContainsKeywordName, ArrayContainsValidator.MinContainsKeywordName});
    }

    private static Vocabulary CreateMetaData2020()
    {
        return new(new Uri("https://json-schema.org/draft/2020-12/vocab/meta-data"),
            Array.Empty<Type>(),
            new[]{TitleAnnotation.Keyword, DescriptionAnnotation.Keyword, DefaultAnnotation.Keyword, DeprecatedAnnotation.Keyword, ReadOnlyAnnotation.Keyword, WriteOnlyAnnotation.Keyword, ExamplesAnnotation.Keyword });
    }

    private static Vocabulary CreateContent2020()
    {
        return new(new Uri("https://json-schema.org/draft/2020-12/vocab/content"),
            Array.Empty<Type>(),
            new[]{ContentEncodingAnnotation.Keyword, ContentMediaTypeAnnotation.Keyword, ContentSchemaAnnotation.Keyword });
    }

    // Keeping 'format' out of validationKeywordTypes means it can never reach ValidationKeywords, so it is structurally annotation-only here.
    private static Vocabulary CreateFormatAnnotation2020()
    {
        return new(new Uri("https://json-schema.org/draft/2020-12/vocab/format-annotation"),
            Array.Empty<Type>(),
            new[]{FormatKeyword.Keyword });
    }

    private static Vocabulary CreateFormatAssertion2020()
    {
        return new(new Uri("https://json-schema.org/draft/2020-12/vocab/format-assertion"),
            new[]{typeof(FormatKeyword)},
            Array.Empty<string>());
    }

    private static Vocabulary CreateCore2019()
    {
        return new(new Uri("https://json-schema.org/draft/2019-09/vocab/core"), 
            Array.Empty<Type>(),
            new[]{IdKeyword.Keyword, SchemaKeyword.Keyword, AnchorKeyword.Keyword, SchemaReferenceKeyword.Keyword, SchemaRecursiveReferenceKeyword.Keyword, RecursiveAnchorKeyword.Keyword, DefsKeyword.Keyword });
    }

    private static Vocabulary CreateApplicator2019()
    {
        return new(new Uri("https://json-schema.org/draft/2019-09/vocab/applicator"), 
            new[]{typeof(ItemsDraft7Keyword), typeof(AdditionalItemsKeyword), typeof(AdditionalPropertiesKeyword), typeof(PropertiesKeyword), typeof(PatternPropertiesKeyword), typeof(DependentSchemasKeyword), typeof(PropertyNamesKeyword), typeof(AllOfKeyword), typeof(AnyOfKeyword), typeof(OneOfKeyword), typeof(NotKeyword)},
            new[]{UnevaluatedItemsKeyword.Keyword, UnevaluatedPropertiesKeyword.Keyword, ArrayContainsValidator.ContainsKeywordName, ConditionalValidator.IfKeywordName, ConditionalValidator.ThenKeywordName, ConditionalValidator.ElseKeywordName });
    }

    private static Vocabulary CreateValidation2019()
    {
        return new(new Uri("https://json-schema.org/draft/2019-09/vocab/validation"), 
            new[]{typeof(TypeKeyword), typeof(ConstKeyword), typeof(EnumKeyword), typeof(MultipleOfKeyword), typeof(MaximumKeyword), typeof(ExclusiveMaximumKeyword), typeof(MinimumKeyword), typeof(ExclusiveMinimumKeyword), typeof(MaxLengthKeyword), typeof(MinLengthKeyword), typeof(PatternKeyword), typeof(MaxItemsKeyword), typeof(MinItemsKeyword), typeof(UniqueItemsKeyword), typeof(MaxPropertiesKeyword), typeof(MinPropertiesKeyword), typeof(RequiredKeyword), typeof(DependentRequiredKeyword)},
            new[]{ArrayContainsValidator.MaxContainsKeywordName, ArrayContainsValidator.MinContainsKeywordName});
    }

    private static Vocabulary CreateMetaData2019()
    {
        return new(new Uri("https://json-schema.org/draft/2019-09/vocab/meta-data"),
            Array.Empty<Type>(),
            new[]{TitleAnnotation.Keyword, DescriptionAnnotation.Keyword, DefaultAnnotation.Keyword, DeprecatedAnnotation.Keyword, ReadOnlyAnnotation.Keyword, WriteOnlyAnnotation.Keyword, ExamplesAnnotation.Keyword });
    }

    private static Vocabulary CreateContent2019()
    {
        return new(new Uri("https://json-schema.org/draft/2019-09/vocab/content"),
            Array.Empty<Type>(),
            new[]{ContentEncodingAnnotation.Keyword, ContentMediaTypeAnnotation.Keyword, ContentSchemaAnnotation.Keyword });
    }

    // Single 2019-09 format vocabulary always requires annotation collection; ReadValidationKeyword already does this for FormatKeyword via IAnnotationKeyword.
    private static Vocabulary CreateFormat2019()
    {
        return new(new Uri("https://json-schema.org/draft/2019-09/vocab/format"),
            new[]{typeof(FormatKeyword)},
            Array.Empty<string>());
    }

    public static Vocabulary? GetVocabulary(Uri id) => Vocabularies.GetValueOrDefault(id);

    public Uri Id { get; }

    private readonly StringKeyedDictionary<Type> _validationKeywordTypes;
    private readonly StringKeyedHashSet _keywordNames;

    public Vocabulary(Uri id, Type[] validationKeywordTypes, string[] keywordNames)
    {
        Id = id;

        int bucketCount = validationKeywordTypes.Length == 0 ? 1 : validationKeywordTypes.Length;
        _validationKeywordTypes = new StringKeyedDictionary<Type>(bucketCount);

        foreach (Type keywordType in validationKeywordTypes)
        {
            _validationKeywordTypes[KeywordHelper.GetKeywordName(keywordType)] = keywordType;
        }

        _keywordNames = new StringKeyedHashSet(keywordNames);
    }

    public bool ContainsKeyword(ReadOnlySpan<char> keyword)
    {
        return _validationKeywordTypes.TryGetValue(keyword, out _) || _keywordNames.Contains(keyword);
    }

    public Type? GetKeyword(ReadOnlySpan<char> keyword)
    {
        return _validationKeywordTypes.GetValueOrDefault(keyword);
    }
}