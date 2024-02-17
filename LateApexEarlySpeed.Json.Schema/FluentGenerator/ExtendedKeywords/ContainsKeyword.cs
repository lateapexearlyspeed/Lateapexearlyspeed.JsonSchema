using System.Text.Json;
using LateApexEarlySpeed.Json.Schema.Common;
using LateApexEarlySpeed.Json.Schema.JInstance;
using LateApexEarlySpeed.Json.Schema.JSchema;
using LateApexEarlySpeed.Json.Schema.Keywords;

namespace LateApexEarlySpeed.Json.Schema.FluentGenerator.ExtendedKeywords;

[Keyword("ext-contains")]
internal class ContainsKeyword : KeywordBase
{
    private readonly JsonSchema _schema;

    public ContainsKeyword(JsonSchema schema)
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
            ValidationResult validationResult = _schema.Validate(element, options);
            if (validationResult.IsValid)
            {
                return ValidationResult.ValidResult;
            }
        }

        return ValidationResult.CreateFailedResult(ResultCode.NotFoundAnyValidatedArrayItem, ErrorMessage(instance.ToString()), options.ValidationPathStack,
            Name, instance.Location);
    }

    internal string ErrorMessage(string instanceJson)
    {
        return $"Array not contain any element met validation, array instance: {instanceJson}";
    }
}