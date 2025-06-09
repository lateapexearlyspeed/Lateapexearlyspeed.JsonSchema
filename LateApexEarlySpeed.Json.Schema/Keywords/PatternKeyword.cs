using System.Text.Json;
using System.Text.Json.Serialization;
using LateApexEarlySpeed.Json.Schema.Common;
using LateApexEarlySpeed.Json.Schema.JInstance;
using LateApexEarlySpeed.Json.Schema.Keywords.JsonConverters;

namespace LateApexEarlySpeed.Json.Schema.Keywords;

[Keyword("pattern")]
[JsonConverter(typeof(PatternKeywordJsonConverter))]
internal class PatternKeyword : KeywordBase
{
    public PatternKeyword(string pattern)
    {
        Pattern = pattern;
    }

    public string Pattern { get; }

    protected internal override ValidationResult ValidateCore(JsonInstanceElement instance, JsonSchemaOptions options)
    {
        if (instance.ValueKind != JsonValueKind.String)
        {
            return ValidationResult.ValidResult;
        }

        string instanceText = instance.GetString()!;
        return RegexMatcher.IsMatch(Pattern, instanceText, options.RegexMatchTimeout)
            ? ValidationResult.ValidResult
            : ValidationResult.SingleErrorFailedResult(new ValidationError(ResultCode.RegexNotMatch, ErrorMessage(Pattern, instanceText), options.ValidationPathStack, Name, instance.Location));
    }

    public static string ErrorMessage(string pattern, string instanceText)
    {
        return $"Regex: '{pattern}' cannot find match in instance: '{instanceText}'";
    }
}