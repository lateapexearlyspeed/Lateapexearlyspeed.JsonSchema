﻿using System.Text.Json;
using System.Text.Json.Serialization;
using LateApexEarlySpeed.Json.Schema.Common;
using LateApexEarlySpeed.Json.Schema.FluentGenerator.ExtendedKeywords.JsonConverters;
using LateApexEarlySpeed.Json.Schema.JInstance;
using LateApexEarlySpeed.Json.Schema.Keywords;

namespace LateApexEarlySpeed.Json.Schema.FluentGenerator.ExtendedKeywords;

[Keyword("ext-custom-StringValidation")]
[JsonConverter(typeof(ExtendedKeywordJsonConverter))]
public class StringCustomValidationKeyword : KeywordBase
{
    private readonly Func<string, bool> _validator;
    private readonly Func<string, string> _errorMessageFunc;

    public StringCustomValidationKeyword(Func<string, bool> validator, Func<string, string> errorMessageFunc)
    {
        _validator = validator;
        _errorMessageFunc = errorMessageFunc;
    }

    protected internal override ValidationResult ValidateCore(JsonInstanceElement instance, JsonSchemaOptions options)
    {
        if (instance.ValueKind != JsonValueKind.String)
        {
            return ValidationResult.ValidResult;
        }

        string instanceData = instance.GetString()!;
        return _validator(instanceData)
            ? ValidationResult.ValidResult
            : ValidationResult.SingleErrorFailedResult(new ValidationError(ResultCode.FailedForCustomValidation, _errorMessageFunc(instanceData), options.ValidationPathStack,
                Name, instance.Location));
    }
}