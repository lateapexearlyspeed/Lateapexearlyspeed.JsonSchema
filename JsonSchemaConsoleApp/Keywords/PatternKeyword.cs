using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using JsonSchemaConsoleApp.JsonConverters;

namespace JsonSchemaConsoleApp.Keywords;

[Keyword("pattern")]
[JsonConverter(typeof(PatternKeywordJsonConverter))]
internal class PatternKeyword : KeywordBase
{
    private readonly Regex _pattern;

    public PatternKeyword(string pattern)
    {
        _pattern = new Regex(pattern, RegexOptions.Compiled, TimeSpan.FromMilliseconds(200));
    }

    protected internal override ValidationResult ValidateCore(JsonElement instance, JsonSchemaOptions options)
    {
        if (instance.ValueKind != JsonValueKind.String)
        {
            return ValidationResult.ValidResult;
        }

        return _pattern.IsMatch(instance.GetString()!)
            ? ValidationResult.ValidResult
            : ValidationResult.CreateFailedResult(ResultCode.RegexNotMatch, options.ValidationPathStack);
    }
}