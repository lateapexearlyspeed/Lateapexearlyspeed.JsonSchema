namespace LateApexEarlySpeed.Json.Schema.Common;

/// <summary>
/// Type <see cref="ValidationResult"/> is designed as immutable object,
/// so that the commonly used property <see cref="ValidResult"/> can be designed as a shared (singleton) instance to reduce allocation.
/// </summary>
public class ValidationResult
{
    public bool IsValid { get; }
    internal readonly ImmutableValidationErrorCollection ValidationErrorsList;

    internal ValidationResult(bool isValid, ImmutableValidationErrorCollection validationErrorsList)
    {
        IsValid = isValid;
        ValidationErrorsList = validationErrorsList;
    }

    /// <summary>
    /// Singleton instance of 'pure' successful validation result, means there is no failure items in <see cref="ValidationErrors"/> property
    /// </summary>
    public static ValidationResult ValidResult { get; } = new(true, ImmutableValidationErrorCollection.Empty);

    public static ValidationResult SingleErrorFailedResult(ValidationError singleError)
    {
        return new ValidationResult(false, new ImmutableValidationErrorCollection(singleError));
    }

    public IEnumerable<ValidationError> ValidationErrors => ValidationErrorsList.Enumerate();
}

internal class ImmutableValidationErrorCollection
{
    private readonly ValidationError? _curValidationError;
    private readonly ImmutableValidationErrorCollection[]? _validationErrorChildren;

    public ImmutableValidationErrorCollection(ValidationError curValidationError)
    {
        _curValidationError = curValidationError;
    }

    private ImmutableValidationErrorCollection(ValidationError? curValidationError, ImmutableValidationErrorCollection[]? validationErrorChildren)
    {
        _curValidationError = curValidationError;
        _validationErrorChildren = validationErrorChildren;
    }

    public static ImmutableValidationErrorCollection Empty { get; } = new(null, null);

    internal ref struct Builder
    {
        private ValidationError? _curValidationError;
        private LinkedList<ImmutableValidationErrorCollection>? _validationErrorChildren;

        public void SetCurrent(ValidationError curValidationError)
        {
            _curValidationError = curValidationError;
        }

        public void AddChildCollection(ImmutableValidationErrorCollection collection)
        {
            if (ReferenceEquals(collection, Empty))
            {
                return;
            }

            _validationErrorChildren ??= new LinkedList<ImmutableValidationErrorCollection>();

            _validationErrorChildren.AddLast(collection);
        }

        public ImmutableValidationErrorCollection ToImmutable()
        {
            if (_curValidationError is null && _validationErrorChildren is null)
            {
                return Empty;
            }

            return new ImmutableValidationErrorCollection(_curValidationError, _validationErrorChildren?.ToArray());
        }
    }

    public IEnumerable<ValidationError> Enumerate()
    {
        if (_curValidationError is not null)
        {
            yield return _curValidationError;
        }

        if (_validationErrorChildren is null)
        {
            yield break;
        }

        foreach (ImmutableValidationErrorCollection collection in _validationErrorChildren)
        {
            foreach (ValidationError validationError in collection.Enumerate())
            {
                yield return validationError;
            }
        }
    }
}

/// <summary>
/// This type is designed as immutable object
/// </summary>
public class ValidationError
{
    public static string ErrorMessageForFailedInSubSchema => "Failed in sub-schema";

    internal ValidationError(ResultCode failedCode, string errorMessage, ValidationPathStack? validationPathStack, string? keyword, ImmutableJsonPointer instanceLocation)
        : this(failedCode,
            keyword,
            errorMessage,
            instanceLocation,
            validationPathStack?.RelativeKeywordLocationStack.ToJsonPointer(),
            validationPathStack?.ReferencedSchemaLocationStack.Peek().resource.BaseUri,
            validationPathStack?.ReferencedSchemaLocationStack.Peek().subSchemaRefFullUri
        )
    { }

    internal ValidationError(ResultCode resultCode, string? keyword, string errorMessage, ImmutableJsonPointer instanceLocation, ImmutableJsonPointer? relativeKeywordLocation, Uri? schemaResourceBaseUri, Uri? subSchemaRefFullUri)
    {
        ResultCode = resultCode;
        Keyword = keyword;
        ErrorMessage = errorMessage;
        InstanceLocation = instanceLocation;
        RelativeKeywordLocation = relativeKeywordLocation;
        SchemaResourceBaseUri = schemaResourceBaseUri;
        SubSchemaRefFullUri = subSchemaRefFullUri;
    }

    public ImmutableJsonPointer InstanceLocation { get; init; }
    public ImmutableJsonPointer? RelativeKeywordLocation { get; init; }
    public Uri? SchemaResourceBaseUri { get; init; }
    public Uri? SubSchemaRefFullUri { get; init; }
    public string? Keyword { get; init; }
    public string ErrorMessage { get; init; }
    public ResultCode ResultCode { get; init; }
}

public enum ResultCode
{
    FailedToMultiple,
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
    InvalidFormat,
    NotBeforeSpecifiedTimePoint,
    NotAfterSpecifiedTimePoint,
    FailedForCustomValidation,
    FailedToDeserialize,
    FailedInSubSchema
}