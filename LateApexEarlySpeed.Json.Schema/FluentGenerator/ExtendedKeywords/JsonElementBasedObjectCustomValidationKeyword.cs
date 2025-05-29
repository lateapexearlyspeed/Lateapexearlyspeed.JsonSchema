﻿using System.Text.Json;
using System.Text.Json.Serialization;
using LateApexEarlySpeed.Json.Schema.Common;
using LateApexEarlySpeed.Json.Schema.FluentGenerator.ExtendedKeywords.JsonConverters;
using LateApexEarlySpeed.Json.Schema.JInstance;
using LateApexEarlySpeed.Json.Schema.Keywords;

namespace LateApexEarlySpeed.Json.Schema.FluentGenerator.ExtendedKeywords;

[Keyword("ext-custom-JsonElementBasedValidation")]
[JsonConverter(typeof(ExtendedKeywordJsonConverter))]
internal class JsonElementBasedObjectCustomValidationKeyword : KeywordBase
{
    private readonly Func<JsonElement, bool> _validator;
    private readonly Func<JsonElement, string> _errorMessageFunc;

    public JsonElementBasedObjectCustomValidationKeyword(Func<JsonElement, bool> validator, Func<JsonElement, string> errorMessageFunc)
    {
        _validator = validator;
        _errorMessageFunc = errorMessageFunc;
    }

    protected internal override ValidationResult ValidateCore(JsonInstanceElement instance, JsonSchemaOptions options)
    {
        return _validator(instance.InternalJsonElement)
            ? ValidationResult.ValidResult
            : ValidationResult.SingleErrorFailedResult(new ValidationError(ResultCode.FailedForCustomValidation, _errorMessageFunc(instance.InternalJsonElement), options.ValidationPathStack,
                Name, instance.Location));
    }
}