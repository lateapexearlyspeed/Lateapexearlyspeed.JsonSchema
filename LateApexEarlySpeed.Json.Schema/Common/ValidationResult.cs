﻿namespace LateApexEarlySpeed.Json.Schema.Common;

/// <summary>
/// Type <see cref="ValidationResult"/> is designed as immutable object,
/// so that the commonly used property <see cref="ValidResult"/> can be designed as a shared (singleton) instance to reduce allocation.
/// </summary>
public class ValidationResult
{
    private ValidationResult()
    {
    }

    public JsonPointer? RelativeKeywordLocation { get; init; }
    public Uri? SchemaResourceBaseUri { get; init; }
    public Uri? SubSchemaRefFullUri { get; init; }
    public string? Keyword { get; init; }
    public string? ErrorMessage { get; init; }
    public ResultCode ResultCode { get; init; }

    public bool IsValid => ResultCode == ResultCode.Valid;

    public static ValidationResult ValidResult { get; } = new() { ResultCode = ResultCode.Valid };
    public static ValidationResult CreateFailedResult(ResultCode failedCode, string errorMessage, ValidationPathStack? validationPathStack, string? keyword)
        => new()
        {
            ResultCode = failedCode,
            ErrorMessage = errorMessage,
            Keyword = keyword,
            RelativeKeywordLocation = validationPathStack?.RelativeKeywordLocationStack.ToJsonPointer(),
            SchemaResourceBaseUri = validationPathStack?.SchemaLocationStack.Peek().resource.BaseUri,
            SubSchemaRefFullUri = validationPathStack?.SchemaLocationStack.Peek().subSchemaRefFullUri
        };
}

public enum ResultCode
{
    Valid,
    FailedToMultiple,
    NotBeInteger,
    InvalidTokenKind,
    MoreThanOnePassedSchemaFound,
    AllSubSchemaFailed,
    SubSchemaPassedUnexpected,
    NotFoundRequiredDependentProperty,
    AlwaysFailedJsonSchema,
    RegexNotMatch,
    NumberOutOfRange,
    NotFoundRequiredProperty,
    PropertiesOutOfRange,
    NotFoundAnyValidatedArrayItem,
    ValidatedArrayItemsCountOutOfRange,
    ArrayLengthOutOfRange,
    StringLengthOutOfRange
}