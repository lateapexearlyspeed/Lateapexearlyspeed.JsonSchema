using System.Text.Json;
using System.Text.Json.Serialization;
using LateApexEarlySpeed.Json.Schema.Common;
using LateApexEarlySpeed.Json.Schema.JInstance;
using LateApexEarlySpeed.Json.Schema.Keywords.JsonConverters;

namespace LateApexEarlySpeed.Json.Schema.Keywords;

[Keyword("dependentRequired")]
[JsonConverter(typeof(DependentRequiredKeywordJsonConverter))]
internal class DependentRequiredKeyword : KeywordBase
{
    public Dictionary<string, string[]> DependentProperties { get; init; } = null!;

    protected internal override ValidationResult ValidateCore(JsonInstanceElement instance, JsonSchemaOptions options)
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
                foreach (string requiredProp in dependentProperty.Value)
                {
                    if (!instancePropertyNames.Contains(requiredProp))
                    {
                        return ValidationResult.CreateFailedResult(
                            ResultCode.NotFoundRequiredDependentProperty, 
                            $"Instance contains property: '{dependentProperty.Key}' but not contains dependent property: '{requiredProp}'", 
                            options.ValidationPathStack,
                            Name,
                            instance.Location);
                    }
                }
            }
        }

        return ValidationResult.ValidResult;
    }
}