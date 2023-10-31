using System.Text.Json;
using System.Text.Json.Serialization;
using JsonSchemaConsoleApp.JsonConverters;

namespace JsonSchemaConsoleApp.Keywords;

[Keyword("required")]
[JsonConverter(typeof(RequiredKeywordJsonConverter))]
internal class RequiredKeyword : KeywordBase
{
    private readonly string[] _requiredProperties;

    public RequiredKeyword(string[] requiredProperties)
    {
        _requiredProperties = requiredProperties;
    }

    protected internal override ValidationResult ValidateCore(JsonElement instance, JsonSchemaOptions options)
    {
        if (instance.ValueKind != JsonValueKind.Object)
        {
            return ValidationResult.ValidResult;
        }

        if (_requiredProperties.Length == 0)
        {
            return ValidationResult.ValidResult;
        }

        HashSet<string> instanceProperties = instance.EnumerateObject().Select(prop => prop.Name).ToHashSet();
        foreach (string requiredProperty in _requiredProperties)
        {
            if (!instanceProperties.Contains(requiredProperty))
            {
                return ValidationResult.CreateFailedResult(ResultCode.NotFoundRequiredProperty, options.ValidationPathStack);
            }
        }

        return ValidationResult.ValidResult;
    }
}