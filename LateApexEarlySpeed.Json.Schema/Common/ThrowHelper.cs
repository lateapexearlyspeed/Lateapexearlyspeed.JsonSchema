using System;
using System.Diagnostics.Contracts;
using System.Text.Json;
using LateApexEarlySpeed.Json.Schema.Keywords;

namespace LateApexEarlySpeed.Json.Schema.Common;

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
        return new JsonException(CreateKeywordPrefixContent<TKeyword>() + $" expects json kinds: \"{string.Join(",", expectedJsonKinds)}\"");
    }

    [Pure]
    public static JsonException CreateKeywordHasInvalidJsonValueKindJsonException(Type keywordType, params JsonValueKind[] expectedJsonKinds)
    {
        return new JsonException(CreateKeywordPrefixContent(keywordType) + $" expects json kinds: \"{string.Join(",", expectedJsonKinds)}\"");
    }

    [Pure]
    public static JsonException CreateJsonSchemaHasInvalidJsonValueKindJsonException(params JsonValueKind[] expectedJsonKinds)
    {
        return new JsonException($"Json schema expects json kinds: \"{string.Join(",", expectedJsonKinds)}\"");
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
    public static JsonException CreateKeywordHasInvalidUriJsonException<TKeyword>(Exception innerException) where TKeyword : KeywordBase
    {
        return new JsonException(CreateKeywordPrefixContent<TKeyword>() + " expects valid Uri.", innerException);
    }

    [Pure]
    public static JsonException CreateKeywordHasInvalidNonNegativeIntegerJsonException(Type keywordType)
    {
        return new JsonException(CreateKeywordPrefixContent(keywordType) + " expects non-negative integer.");
    }

    [Pure]
    public static JsonException CreateKeywordHasInvalidNonNegativeIntegerJsonException(string keywordName)
    {
        return new JsonException(CreateKeywordPrefixContent(keywordName) + " expects non-negative integer.");
    }

    private static string CreateKeywordPrefixContent<TKeyword>() where TKeyword : KeywordBase
    {
        return CreateKeywordPrefixContent(KeywordBase.GetKeywordName<TKeyword>());
    }

    private static string CreateKeywordPrefixContent(Type keywordType)
    {
        return CreateKeywordPrefixContent(KeywordBase.GetKeywordName(keywordType));
    }

    private static string CreateKeywordPrefixContent(string keywordName)
    {
        return "Keyword:" + keywordName;
    }
}