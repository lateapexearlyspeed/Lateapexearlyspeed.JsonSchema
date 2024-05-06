using System.Text.Json;
using System.Text.Json.Serialization;
using LateApexEarlySpeed.Json.Schema.Common;
using LateApexEarlySpeed.Json.Schema.FluentGenerator.ExtendedKeywords.JsonConverters;
using LateApexEarlySpeed.Json.Schema.JInstance;
using LateApexEarlySpeed.Json.Schema.JSchema;
using LateApexEarlySpeed.Json.Schema.Keywords;

namespace LateApexEarlySpeed.Json.Schema.FluentGenerator.ExtendedKeywords;

[Keyword("ext-notContains")]
[JsonConverter(typeof(ExtendedKeywordJsonConverter))]
internal class NotContainsKeyword : KeywordBase
{
    private readonly JsonSchema _schema;

    public NotContainsKeyword(JsonSchema schema)
    {
        _schema = schema;
    }

    protected internal override ValidationResult ValidateCore(JsonInstanceElement instance, JsonSchemaOptions options)
    {
        if (instance.ValueKind != JsonValueKind.Array)
        {
            return ValidationResult.ValidResult;
        }

        foreach (JsonInstanceElement element in instance.EnumerateArray())
        {
            if (_schema.Validate(element, options).IsValid)
            {
                return ValidationResult.CreateFailedResult(ResultCode.SubSchemaPassedUnexpected, ErrorMessage(element.ToString()), options.ValidationPathStack,
                    Name, element.Location);
            }
        }

        return ValidationResult.ValidResult;
    }

    internal static string ErrorMessage(string instanceData)
    {
        return $"Expect array not contain validated element but found, instance: {instanceData}";
    }
}