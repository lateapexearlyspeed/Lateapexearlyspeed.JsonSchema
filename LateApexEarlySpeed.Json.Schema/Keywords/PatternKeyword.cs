using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using LateApexEarlySpeed.Json.Schema.Common;
using LateApexEarlySpeed.Json.Schema.JInstance;
using LateApexEarlySpeed.Json.Schema.Keywords.JsonConverters;

namespace LateApexEarlySpeed.Json.Schema.Keywords;

[Keyword("pattern")]
[JsonConverter(typeof(PatternKeywordJsonConverter))]
internal class PatternKeyword : KeywordBase
{
    private readonly Regex _pattern;

    public PatternKeyword(string pattern)
    {
        _pattern = new Regex(pattern, RegexOptions.Compiled, TimeSpan.FromMilliseconds(200));
    }

    protected internal override ValidationResult ValidateCore(JsonInstanceElement instance, JsonSchemaOptions options)
    {
        if (instance.ValueKind != JsonValueKind.String)
        {
            return ValidationResult.ValidResult;
        }

        string instanceText = instance.GetString()!;
        return _pattern.IsMatch(instanceText)
            ? ValidationResult.ValidResult
            : ValidationResult.CreateFailedResult(ResultCode.RegexNotMatch, $"Regex: '{_pattern}' cannot find match in instance: '{instanceText}'", options.ValidationPathStack, Name, instance.Location);
    }
}