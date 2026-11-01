using System.Text.Json;
using System.Text.Json.Serialization;
using LateApexEarlySpeed.Json.Schema.Common;
using LateApexEarlySpeed.Json.Schema.Keywords;
using LateApexEarlySpeed.Json.Schema.Keywords.Draft7;

namespace LateApexEarlySpeed.Json.Schema.JSchema;

[JsonConverter(typeof(MetaSchemaJsonConverter))]
public class MetaSchema
{
    public Uri Id { get; }
    private readonly Vocabulary[] _vocabularies;

    public MetaSchema(Uri id, Vocabulary[] vocabularies)
    {
        Id = id;
        _vocabularies = vocabularies;
    }

    public bool ContainsKeyword(ReadOnlySpan<char> keyword)
    {
        foreach (Vocabulary vocabulary in _vocabularies)
        {
            if (vocabulary.ContainsKeyword(keyword))
            {
                return true;
            }
        }

        return false;
    }

    public Type? GetKeyword(ReadOnlySpan<char> keyword)
    {
        foreach (Vocabulary vocabulary in _vocabularies)
        {
            Type? type = vocabulary.GetKeyword(keyword);
            if (type is not null)
            {
                return type;
            }
        }

        return null;
    }
}

internal class MetaSchemaJsonConverter : JsonConverter<MetaSchema>
{
    public const string VocabularyKeywordName = "$vocabulary";

    public override MetaSchema Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw ThrowHelper.CreateMetaSchemaHasInvalidJsonValueKindJsonException();
        }

        reader.Read();

        Uri? id = null;
        Vocabulary[]? vocabularies = null;

        while (reader.TokenType != JsonTokenType.EndObject)
        {
            if (reader.ValueTextEquals(IdKeyword.Keyword))
            {
                reader.Read();
                id = new Uri(reader.GetString()!);
            }
            else if (reader.ValueTextEquals(VocabularyKeywordName))
            {
                reader.Read();
                vocabularies = DeserializeVocabularies(ref reader);
            }
            else
            {
                reader.HandleFinalBlockAndSkip();
            }

            reader.Read();
        }

        if (id is null)
        {
            throw new JsonException($"Meta schema {IdKeyword.Keyword} is missing.");
        }

        return new MetaSchema(id, vocabularies ?? Array.Empty<Vocabulary>());
    }

    private static Vocabulary[] DeserializeVocabularies(ref Utf8JsonReader reader)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw ThrowHelper.CreateVocabularyHasInvalidJsonValueKindJsonException();
        }

        var vocabularies = new List<Vocabulary>();
        
        reader.Read();
        while (reader.TokenType != JsonTokenType.EndObject)
        {
            string vocabularyValue = reader.GetString()!;
            Vocabulary? vocabulary = Vocabulary.GetVocabulary(new Uri(vocabularyValue));
            if (vocabulary is not null)
            {
                vocabularies.Add(vocabulary);
            }

            reader.HandleFinalBlockAndSkip();
            reader.Read();
        }

        return vocabularies.ToArray();
    }

    public override void Write(Utf8JsonWriter writer, MetaSchema value, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }

    public override bool HandleNull => true;
}

public class Vocabulary
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
            new[]{"$id", "$schema", "$ref", "$anchor", "$dynamicRef", "$dynamicAnchor", "$defs" });
    }

    private static Vocabulary CreateApplicator2020()
    {
        return new(new Uri("https://json-schema.org/draft/2020-12/vocab/applicator"), 
            new[]{typeof(PrefixItemsKeyword), typeof(ItemsKeyword), typeof(AdditionalPropertiesKeyword), typeof(PropertiesKeyword), typeof(PatternPropertiesKeyword), typeof(DependentSchemasKeyword), typeof(PropertyNamesKeyword), typeof(AllOfKeyword), typeof(AnyOfKeyword), typeof(OneOfKeyword), typeof(NotKeyword)},
            new[]{"contains", "if", "then", "else" });
    }
    
    private static Vocabulary CreateUnevaluated2020()
    {
        return new(new Uri("https://json-schema.org/draft/2020-12/vocab/unevaluated"), 
            Array.Empty<Type>(),
            new[]{"unevaluatedItems", "unevaluatedProperties" });
    }

    private static Vocabulary CreateValidation2020()
    {
        return new(new Uri("https://json-schema.org/draft/2020-12/vocab/validation"), 
            new[]{typeof(TypeKeyword), typeof(ConstKeyword), typeof(EnumKeyword), typeof(MultipleOfKeyword), typeof(MaximumKeyword), typeof(ExclusiveMaximumKeyword), typeof(MinimumKeyword), typeof(ExclusiveMinimumKeyword), typeof(MaxLengthKeyword), typeof(MinLengthKeyword), typeof(PatternKeyword), typeof(MaxItemsKeyword), typeof(MinItemsKeyword), typeof(UniqueItemsKeyword), typeof(MaxPropertiesKeyword), typeof(MinPropertiesKeyword), typeof(RequiredKeyword), typeof(DependentRequiredKeyword)},
            new[]{"maxContains", "minContains"});
    }

    private static Vocabulary CreateMetaData2020()
    {
        return new(new Uri("https://json-schema.org/draft/2020-12/vocab/meta-data"),
            Array.Empty<Type>(),
            new[]{"title", "description", "default", "deprecated", "readOnly", "writeOnly", "examples" });
    }

    private static Vocabulary CreateContent2020()
    {
        return new(new Uri("https://json-schema.org/draft/2020-12/vocab/content"),
            Array.Empty<Type>(),
            new[]{"contentEncoding", "contentMediaType", "contentSchema" });
    }

    // Keeping 'format' out of validationKeywordTypes means it can never reach ValidationKeywords, so it is structurally annotation-only here.
    private static Vocabulary CreateFormatAnnotation2020()
    {
        return new(new Uri("https://json-schema.org/draft/2020-12/vocab/format-annotation"),
            Array.Empty<Type>(),
            new[]{"format" });
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
            new[]{"$id", "$schema", "$anchor", "$ref", "$recursiveRef", "$recursiveAnchor", "$defs" });
    }

    private static Vocabulary CreateApplicator2019()
    {
        return new(new Uri("https://json-schema.org/draft/2019-09/vocab/applicator"), 
            new[]{typeof(ItemsDraft7Keyword), typeof(AdditionalItemsKeyword), typeof(AdditionalPropertiesKeyword), typeof(PropertiesKeyword), typeof(PatternPropertiesKeyword), typeof(DependentSchemasKeyword), typeof(PropertyNamesKeyword), typeof(AllOfKeyword), typeof(AnyOfKeyword), typeof(OneOfKeyword), typeof(NotKeyword)},
            new[]{"unevaluatedItems", "unevaluatedProperties", "contains", "if", "then", "else" });
    }

    private static Vocabulary CreateValidation2019()
    {
        return new(new Uri("https://json-schema.org/draft/2019-09/vocab/validation"), 
            new[]{typeof(TypeKeyword), typeof(ConstKeyword), typeof(EnumKeyword), typeof(MultipleOfKeyword), typeof(MaximumKeyword), typeof(ExclusiveMaximumKeyword), typeof(MinimumKeyword), typeof(ExclusiveMinimumKeyword), typeof(MaxLengthKeyword), typeof(MinLengthKeyword), typeof(PatternKeyword), typeof(MaxItemsKeyword), typeof(MinItemsKeyword), typeof(UniqueItemsKeyword), typeof(MaxPropertiesKeyword), typeof(MinPropertiesKeyword), typeof(RequiredKeyword), typeof(DependentRequiredKeyword)},
            new[]{"maxContains", "minContains"});
    }

    private static Vocabulary CreateMetaData2019()
    {
        return new(new Uri("https://json-schema.org/draft/2019-09/vocab/meta-data"),
            Array.Empty<Type>(),
            new[]{"title", "description", "default", "deprecated", "readOnly", "writeOnly", "examples" });
    }

    private static Vocabulary CreateContent2019()
    {
        return new(new Uri("https://json-schema.org/draft/2019-09/vocab/content"),
            Array.Empty<Type>(),
            new[]{"contentEncoding", "contentMediaType", "contentSchema" });
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