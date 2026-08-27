namespace LateApexEarlySpeed.Json.Schema.Keywords.Annotations;

internal static class AnnotationKeywordRegistry
{
    private static readonly StringKeyedDictionary<Type> Keywords;

    static AnnotationKeywordRegistry()
    {
        var builtInAnnotationTypes = new[]
        {
            typeof(DefaultAnnotation),
            typeof(DeprecatedAnnotation),
            typeof(DescriptionAnnotation),
            typeof(ExamplesAnnotation),
            typeof(ReadOnlyAnnotation),
            typeof(TitleAnnotation),
            typeof(WriteOnlyAnnotation),
            typeof(ContentEncodingAnnotation),
            typeof(ContentMediaTypeAnnotation),
            typeof(ContentSchemaAnnotation),
            typeof(FormatKeyword)
        };

        Keywords = new StringKeyedDictionary<Type>(20);

        foreach (Type annotationType in builtInAnnotationTypes)
        {
            Keywords[KeywordHelper.GetKeywordName(annotationType)] = annotationType;
        }
    }

    public static Type? GetKeyword(ReadOnlySpan<char> keywordName)
    {
        return Keywords.GetValueOrDefault(keywordName);
    }
}