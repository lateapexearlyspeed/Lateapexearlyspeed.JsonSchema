using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using LateApexEarlySpeed.Json.Schema.Common;
using LateApexEarlySpeed.Json.Schema.JInstance;
using LateApexEarlySpeed.Json.Schema.Keywords.JsonConverters;

namespace LateApexEarlySpeed.Json.Schema.Keywords;

[Obfuscation(ApplyToMembers = false)]
[Keyword("required")]
[JsonConverter(typeof(RequiredKeywordJsonConverter))]
internal class RequiredKeyword : KeywordBase
{
    private readonly string[] _requiredProperties;

    public RequiredKeyword(string[] requiredProperties)
    {
        _requiredProperties = requiredProperties;
    }

    protected internal override ValidationResult ValidateCore(JsonInstanceElement instance, JsonSchemaOptions options)
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
                return ValidationResult.CreateFailedResult(ResultCode.NotFoundRequiredProperty, ErrorMessage(requiredProperty), options.ValidationPathStack, Name, instance.Location);
            }
        }

        return ValidationResult.ValidResult;
    }

    [Obfuscation]
    public static string ErrorMessage(string missedPropertyName)
    {
        return $"Instance not contain required property '{missedPropertyName}'";
    }
}