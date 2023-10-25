using System.Diagnostics.Contracts;
using System.Text.Json;
using JsonSchemaConsoleApp.Keywords;

namespace JsonSchemaConsoleApp;

internal class ThrowHelper
{
    [Pure]
    public static JsonException CreateKeywordHasInvalidJsonValueKindJsonException<TKeyword>(JsonValueKind expectedJsonKind) where TKeyword : KeywordBase
    {
        return new JsonException($"Keyword:{KeywordBase.GetKeywordName<TKeyword>()} expects json kind: {expectedJsonKind}");
    }

    [Pure]
    public static JsonException CreateKeywordHasEmptyJsonArrayJsonException<TKeyword>() where TKeyword : KeywordBase
    {
        return new JsonException($"Keyword:{KeywordBase.GetKeywordName<TKeyword>()} expects non-empty json array.");
    }

    [Pure]
    public static JsonException CreateKeywordHasDuplicatedJsonArrayElementsJsonException<TKeyword>() where TKeyword : KeywordBase
    {
        return new JsonException($"Keyword:{KeywordBase.GetKeywordName<TKeyword>()} expects that elements in array must be unique.");
    }
}