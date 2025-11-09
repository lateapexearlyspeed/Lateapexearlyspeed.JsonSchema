using System.Text;

namespace LateApexEarlySpeed.Json.Schema.Common;

/// <summary>
/// Type <see cref="ValidationResult"/> is designed as immutable object,
/// so that the commonly used property <see cref="ValidResult"/> can be designed as a shared (singleton) instance to reduce allocation.
/// </summary>
public class ValidationResult
{
    /// <summary>
    /// Gets the value to indicate flag result of validation
    /// </summary>
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

    /// <summary>
    /// Gets all failed validation nodes during validation.
    /// If <see cref="JsonSchemaOptions.OutputFormat"/> is set to <see cref="OutputFormat.FailFast"/>, this will contain only first found failed validation node.
    /// </summary>
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
            // try to reuse existing ImmutableValidationErrorCollection instance
            if (_curValidationError is null)
            {
                if (_validationErrorChildren is null)
                {
                    return Empty;
                }

                if (_validationErrorChildren.Count == 1)
                {
                    return _validationErrorChildren.First.Value;
                }
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
    internal ValidationError(ResultCode failedCode, string errorMessage, ValidationPathStack? validationPathStack, string? keyword, LinkedListBasedImmutableJsonPointer instanceLocation)
        : this(failedCode,
            keyword,
            errorMessage,
            instanceLocation,
            validationPathStack?.RelativeKeywordLocationStack.ToJsonPointer(),
            validationPathStack?.ReferencedSchemaLocationStack.Peek().resource.BaseUri,
            validationPathStack?.ReferencedSchemaLocationStack.Peek().subSchemaRefFullUri
        )
    { }

    internal ValidationError(ResultCode resultCode, string? keyword, string errorMessage, LinkedListBasedImmutableJsonPointer instanceLocation, LinkedListBasedImmutableJsonPointer? relativeKeywordLocation, Uri? schemaResourceBaseUri, Uri? subSchemaRefFullUri)
    {
        ResultCode = resultCode;
        Keyword = keyword;
        ErrorMessage = errorMessage;
        InstanceLocation = instanceLocation;
        RelativeKeywordLocation = relativeKeywordLocation;
        SchemaResourceBaseUri = schemaResourceBaseUri;
        SubSchemaRefFullUri = subSchemaRefFullUri;
    }

    /// <summary>
    /// Gets value to indicate json instance's location
    /// </summary>
    public LinkedListBasedImmutableJsonPointer InstanceLocation { get; init; }

    /// <summary>
    /// Gets value to indicate relative location of keyword
    /// </summary>
    public LinkedListBasedImmutableJsonPointer? RelativeKeywordLocation { get; init; }

    /// <summary>
    /// Gets value to indicate base uri of current json schema resource
    /// </summary>
    public Uri? SchemaResourceBaseUri { get; init; }

    /// <summary>
    /// Gets value to indicate full uri of referenced sub-schema
    /// </summary>
    public Uri? SubSchemaRefFullUri { get; init; }

    /// <summary>
    /// Gets value to indicate current keyword.
    /// Note: in some scenarios of json schema node, this value may be null
    /// </summary>
    public string? Keyword { get; init; }

    /// <summary>
    /// The error message to briefly describe failure reason
    /// </summary>
    public string ErrorMessage { get; init; }

    /// <summary>
    /// Gets value to indicate failure type
    /// </summary>
    public ResultCode ResultCode { get; init; }

    /// <summary>
    /// Creates and returns a string representation of the current <see cref="ValidationError"/>.
    /// </summary>
    /// <returns>A string representation of the current <see cref="ValidationError"/></returns>
    public override string ToString()
    {
        var sb = new StringBuilder(ErrorMessage);
        sb.AppendLine();

        sb.AppendFormat("Instance location (in json pointer format): {0}", InstanceLocation);
        sb.AppendLine();

        if (RelativeKeywordLocation is not null)
        {
            sb.AppendFormat("relative keyword location (in json pointer format): {0}", RelativeKeywordLocation);
            sb.AppendLine();
        }

        if (!string.IsNullOrEmpty(Keyword))
        {
            sb.AppendFormat("keyword: {0}", Keyword);
        }

        return sb.ToString();
    }
}

/// <summary>
/// Type to indicate failure types
/// </summary>
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
    FailedToDeserialize
}