using System.Text.Json.Serialization;
using LateApexEarlySpeed.Json.Schema.Common;
using LateApexEarlySpeed.Json.Schema.JInstance;
using LateApexEarlySpeed.Json.Schema.Keywords.JsonConverters;

namespace LateApexEarlySpeed.Json.Schema.Keywords;

[Keyword("const")]
[JsonConverter(typeof(ConstKeywordJsonConverter))]
internal class ConstKeyword : KeywordBase
{
    private readonly JsonInstanceElement _constValue;

    public ConstKeyword(JsonInstanceElement constValue)
    {
        _constValue = constValue;
    }

    protected internal override ValidationResult ValidateCore(JsonInstanceElement instance, JsonSchemaOptions options)
    {
        return _constValue == instance
            ? ValidationResult.ValidResult
            : ValidationResult.CreateFailedResult(ResultCode.UnexpectedValue, "Unexpected value found", options.ValidationPathStack, Name, instance.Location);
    }
}