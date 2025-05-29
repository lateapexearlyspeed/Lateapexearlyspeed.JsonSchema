using System.Text.Json;
using System.Text.Json.Serialization;
using LateApexEarlySpeed.Json.Schema.Common;
using LateApexEarlySpeed.Json.Schema.FluentGenerator.ExtendedKeywords.JsonConverters;
using LateApexEarlySpeed.Json.Schema.JInstance;
using LateApexEarlySpeed.Json.Schema.Keywords;

namespace LateApexEarlySpeed.Json.Schema.FluentGenerator.ExtendedKeywords;

[Keyword("ext-noProperties")]
[JsonConverter(typeof(ExtendedKeywordJsonConverter))]
internal class NoPropertiesKeyword : KeywordBase
{
    private readonly HashSet<string> _propertyBlackList;

    public NoPropertiesKeyword(IEnumerable<string> propertyBlackList, bool propertyNameIgnoreCase)
    {
        _propertyBlackList = new HashSet<string>(propertyBlackList, propertyNameIgnoreCase ? StringComparer.OrdinalIgnoreCase : null);
    }

    protected internal override ValidationResult ValidateCore(JsonInstanceElement instance, JsonSchemaOptions options)
    {
        if (instance.ValueKind != JsonValueKind.Object)
        {
            return ValidationResult.ValidResult;
        }

        foreach (JsonInstanceProperty property in instance.EnumerateObject())
        {
            if (_propertyBlackList.Contains(property.Name))
            {
                return ValidationResult.SingleErrorFailedResult(new ValidationError(ResultCode.InvalidPropertyName, ErrorMessage(property.Name), options.ValidationPathStack,
                    Name, instance.Location));
            }
        }

        return ValidationResult.ValidResult;
    }

    internal static string ErrorMessage(string invalidPropertyName)
    {
        return $"Found out disallowed property: {invalidPropertyName}";
    }
}