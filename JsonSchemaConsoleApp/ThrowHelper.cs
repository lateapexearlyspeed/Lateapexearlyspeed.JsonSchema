using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Text.Json;
using JsonSchemaConsoleApp.Keywords;

namespace JsonSchemaConsoleApp;

internal class ThrowHelper
{
    [Pure]
    public static JsonException CreateKeywordHasInvalidJsonValueKindJsonException<TKeyword>(JsonValueKind expectedJsonKind) where TKeyword : KeywordBase
    {
        return new JsonException(CreateKeywordPrefixContent<TKeyword>() + $" expects json kind: {expectedJsonKind}");
    }

    [Pure]
    public static JsonException CreateKeywordHasInvalidJsonValueKindJsonException<TKeyword>(params JsonValueKind[] expectedJsonKinds) where TKeyword : KeywordBase
    {
        return new JsonException(CreateKeywordPrefixContent<TKeyword>() + $" expects json kind: \"{string.Join(',', expectedJsonKinds)}\"");
    }

    [Pure]
    public static JsonException CreateKeywordHasInvalidJsonValueKindJsonException(Type keywordType, params JsonValueKind[] expectedJsonKinds)
    {
        return new JsonException(CreateKeywordPrefixContent(keywordType) + $" expects json kind: \"{string.Join(',', expectedJsonKinds)}\"");
    }

    [Pure]
    public static JsonException CreateKeywordHasEmptyJsonArrayJsonException<TKeyword>() where TKeyword : KeywordBase
    {
        return new JsonException(CreateKeywordPrefixContent<TKeyword>() + " expects non-empty json array.");
    }

    [Pure]
    public static JsonException CreateKeywordHasDuplicatedJsonArrayElementsJsonException<TKeyword>() where TKeyword : KeywordBase
    {
        return new JsonException(CreateKeywordPrefixContent<TKeyword>() + " expects that elements in array must be unique.");
    }

    [Pure]
    public static JsonException CreateKeywordHasInvalidRegexJsonException<TKeyword>(Exception innerException) where TKeyword : KeywordBase
    {
        return new JsonException(CreateKeywordPrefixContent<TKeyword>() + " expects valid regex string.", innerException);
    }

    [Pure]
    public static JsonException CreateKeywordHasInvalidNonNegativeIntegerJsonException(Type keywordType)
    {
        return new JsonException(CreateKeywordPrefixContent(keywordType) + " expects non-negative integer.");
    }

    private static string CreateKeywordPrefixContent<TKeyword>() where TKeyword : KeywordBase
    {
        return $"Keyword:{KeywordBase.GetKeywordName<TKeyword>()}";
    }

    private static string CreateKeywordPrefixContent(Type keywordType)
    {
        return $"Keyword:{KeywordBase.GetKeywordName(keywordType)}";
    }
}