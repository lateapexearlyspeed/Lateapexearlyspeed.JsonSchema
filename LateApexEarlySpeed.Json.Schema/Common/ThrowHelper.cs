using System.Diagnostics.Contracts;
using System.Text.Json;
using LateApexEarlySpeed.Json.Schema.Keywords;

namespace LateApexEarlySpeed.Json.Schema.Common;

internal class ThrowHelper
{
    [Pure]
    public static JsonException CreateKeywordHasInvalidJsonValueKindJsonException<TKeyword>(JsonValueKind expectedJsonKind) where TKeyword : ValidationKeywordBase
    {
        return CreateKeywordHasInvalidJsonValueKindJsonException(KeywordHelper.GetKeywordName(typeof(TKeyword)), expectedJsonKind);
    }

    [Pure]
    public static JsonException CreateKeywordHasInvalidJsonValueKindJsonException(string keywordName, JsonValueKind expectedJsonKind)
    {
        return new JsonException(CreateKeywordPrefixContent(keywordName) + $" expects json kind: {expectedJsonKind}");
    }

    [Pure]
    public static JsonException CreateKeywordHasInvalidJsonValueKindJsonException<TKeyword>(params JsonValueKind[] expectedJsonKinds) where TKeyword : ValidationKeywordBase
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
    public static JsonException CreateJsonSchemaHasDuplicatedKeywordsJsonException(string duplicatedKeywordName)
    {
        return new JsonException($"Json schema contains more than one keyword with name: '{duplicatedKeywordName}'");
    }

    [Pure]
    public static JsonException CreateJsonSchemaContainsSchemaKeywordJsonException()
    {
        return new JsonException($"Json schema (non-Json resource) contains '{SchemaKeyword.Keyword}'");
    }

    [Pure]
    public static JsonException CreateKeywordHasEmptyJsonArrayJsonException<TKeyword>() where TKeyword : ValidationKeywordBase
    {
        return new JsonException(CreateKeywordPrefixContent<TKeyword>() + " expects non-empty json array.");
    }

    [Pure]
    public static JsonException CreateKeywordHasDuplicatedJsonArrayElementsJsonException<TKeyword>() where TKeyword : ValidationKeywordBase
    {
        return new JsonException(CreateKeywordPrefixContent<TKeyword>() + " expects that elements in array must be unique.");
    }

    [Pure]
    public static JsonException CreateKeywordHasInvalidRegexJsonException<TKeyword>(Exception innerException) where TKeyword : ValidationKeywordBase
    {
        return new JsonException(CreateKeywordPrefixContent<TKeyword>() + " expects valid regex string.", innerException);
    }

    [Pure]
    public static JsonException CreateKeywordHasInvalidUriJsonException<TKeyword>(Exception innerException) where TKeyword : ValidationKeywordBase
    {
        return new JsonException(CreateKeywordPrefixContent<TKeyword>() + " expects valid Uri.", innerException);
    }

    [Pure]
    public static JsonException CreateKeywordHasInvalidNonNegativeIntegerJsonException(Type keywordType)
    {
        return new JsonException(CreateKeywordPrefixContent(keywordType) + " expects non-negative integer.");
    }

    [Pure]
    public static JsonException CreateKeywordHasInvalidPositiveNumberJsonException<TKeyword>() where TKeyword : ValidationKeywordBase
    {
        return new JsonException(CreateKeywordPrefixContent<TKeyword>() + " expects positive number.");
    }

    [Pure]
    public static NotSupportedException CreateExtendedKeywordCannotSerializeToStandardJsonSchemaException<TKeyword>() where TKeyword : ValidationKeywordBase
    {
        return new NotSupportedException(CreateKeywordPrefixContent<TKeyword>() + " is extended keyword type so cannot be serialized to standard json schema.");
    }

    [Pure]
    public static JsonException CreateKeywordHasInvalidNonNegativeIntegerJsonException(string keywordName)
    {
        return new JsonException(CreateKeywordPrefixContent(keywordName) + " expects non-negative integer.");
    }

    private static string CreateKeywordPrefixContent<TKeyword>() where TKeyword : ValidationKeywordBase
    {
        return CreateKeywordPrefixContent(KeywordHelper.GetKeywordName(typeof(TKeyword)));
    }

    private static string CreateKeywordPrefixContent(Type keywordType)
    {
        return CreateKeywordPrefixContent(KeywordHelper.GetKeywordName(keywordType));
    }

    private static string CreateKeywordPrefixContent(string keywordName)
    {
        return "Keyword:" + keywordName;
    }
}