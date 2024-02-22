using System.Diagnostics;
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
        EquivalentResult equivalentResult = _constValue.Equivalent(instance);

        if (equivalentResult.Result)
        {
            return ValidationResult.ValidResult;
        }

        Debug.Assert(equivalentResult.DetailedMessage is not null);
        Debug.Assert(equivalentResult.OtherLocation is not null);
        return ValidationResult.CreateFailedResult(ResultCode.UnexpectedValue, equivalentResult.DetailedMessage, options.ValidationPathStack, Name, equivalentResult.OtherLocation);
    }
}