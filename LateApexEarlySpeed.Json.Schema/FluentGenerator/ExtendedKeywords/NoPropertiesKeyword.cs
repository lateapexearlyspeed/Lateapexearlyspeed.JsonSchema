﻿using System.Text.Json;
using LateApexEarlySpeed.Json.Schema.Common;
using LateApexEarlySpeed.Json.Schema.JInstance;
using LateApexEarlySpeed.Json.Schema.Keywords;

namespace LateApexEarlySpeed.Json.Schema.FluentGenerator.ExtendedKeywords;

[Keyword("ext-noProperties")]
internal class NoPropertiesKeyword : KeywordBase
{
    private readonly HashSet<string> _propertyBlackList;

    public NoPropertiesKeyword(HashSet<string> propertyBlackList)
    {
        _propertyBlackList = propertyBlackList;
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
                return ValidationResult.CreateFailedResult(ResultCode.InvalidPropertyName, ErrorMessage(property.Name), options.ValidationPathStack,
                    Name, instance.Location);
            }
        }

        return ValidationResult.ValidResult;
    }

    internal static string ErrorMessage(string invalidPropertyName)
    {
        return $"Found out disallowed property: {invalidPropertyName}";
    }
}