using System.Text.Json;
using System.Text.Json.Serialization;
using JsonSchemaConsoleApp.JsonConverters;

namespace JsonSchemaConsoleApp.Keywords;

[Keyword("dependentRequired")]
[JsonConverter(typeof(DependentRequiredKeywordJsonConverter))]
internal class DependentRequiredKeyword : KeywordBase
{
    public Dictionary<string, string[]> DependentProperties { get; init; } = null!;

    protected internal override ValidationResult ValidateCore(JsonElement instance, JsonSchemaOptions options)
    {
        if (instance.ValueKind != JsonValueKind.Object)
        {
            return ValidationResult.ValidResult;
        }

        var instancePropertyNames = new HashSet<string>(instance.EnumerateObject().Select(p => p.Name));

        foreach (KeyValuePair<string, string[]> dependentProperty in DependentProperties)
        {
            if (instancePropertyNames.Contains(dependentProperty.Key))
            {
                if (dependentProperty.Value.Any(requiredProp => !instancePropertyNames.Contains(requiredProp)))
                {
                    return ValidationResult.CreateFailedResult(ResultCode.NotFoundRequiredDependentProperty, options.ValidationPathStack);
                }
            }
        }

        return ValidationResult.ValidResult;
    }
}