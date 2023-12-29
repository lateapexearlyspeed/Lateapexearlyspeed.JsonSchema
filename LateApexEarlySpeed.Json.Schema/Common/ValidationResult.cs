namespace LateApexEarlySpeed.Json.Schema.Common;

/// <summary>
/// Type <see cref="ValidationResult"/> is designed as immutable object,
/// so that the commonly used property <see cref="ValidResult"/> can be designed as a shared (singleton) instance to reduce allocation.
/// </summary>
public class ValidationResult
{
    private ValidationResult()
    {
    }

    internal ValidationResult(ResultCode resultCode, string? keyword, string errorMessage, ImmutableJsonPointer instanceLocation, ImmutableJsonPointer? relativeKeywordLocation, Uri? schemaResourceBaseUri, Uri? subSchemaRefFullUri)
    {
        ResultCode = resultCode;
        Keyword = keyword;
        ErrorMessage = errorMessage;
        InstanceLocation = instanceLocation;
        RelativeKeywordLocation = relativeKeywordLocation;
        SchemaResourceBaseUri = schemaResourceBaseUri;
        SubSchemaRefFullUri = subSchemaRefFullUri;
    }

    public ImmutableJsonPointer? InstanceLocation { get; init; }
    public ImmutableJsonPointer? RelativeKeywordLocation { get; init; }
    public Uri? SchemaResourceBaseUri { get; init; }
    public Uri? SubSchemaRefFullUri { get; init; }
    public string? Keyword { get; init; }
    public string? ErrorMessage { get; init; }
    public ResultCode ResultCode { get; init; }

    public bool IsValid => ResultCode == ResultCode.Valid;

    public static ValidationResult ValidResult { get; } = new() { ResultCode = ResultCode.Valid };
    public static ValidationResult CreateFailedResult(ResultCode failedCode, string errorMessage, ValidationPathStack? validationPathStack, string? keyword, ImmutableJsonPointer instanceLocation)
        => new(failedCode, 
            keyword, 
            errorMessage, 
            instanceLocation, 
            validationPathStack?.RelativeKeywordLocationStack.ToJsonPointer(),
            validationPathStack?.ReferencedSchemaLocationStack.Peek().resource.BaseUri,
            validationPathStack?.ReferencedSchemaLocationStack.Peek().subSchemaRefFullUri
            );
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
    StringLengthOutOfRange,
    InvalidPropertyName,
    DuplicatedArrayItems,
    NotFoundInAllowedList,
    UnexpectedValue,
    InvalidFormat
}