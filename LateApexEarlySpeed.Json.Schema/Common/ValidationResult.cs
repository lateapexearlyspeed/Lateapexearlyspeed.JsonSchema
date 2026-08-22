using System.Collections;
using System.Diagnostics;
using System.Text;
using LateApexEarlySpeed.Json.Schema.Keywords.Annotations;

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
    internal readonly ImmutableDoubleEndedLinkedList<Annotation> AnnotationList;

    internal ValidationResult(bool isValid, ImmutableValidationErrorCollection validationErrorsList, ImmutableDoubleEndedLinkedList<Annotation> annotationList)
    {
        IsValid = isValid;
        ValidationErrorsList = validationErrorsList;
        AnnotationList = annotationList;
    }

    /// <summary>
    /// Singleton instance of 'pure' successful validation result, means there is no failure items in <see cref="ValidationErrors"/> property and annotation items in <see cref="Annotations"/> property
    /// </summary>
    public static ValidationResult ValidResult { get; } = new(true, ImmutableValidationErrorCollection.Empty, ImmutableDoubleEndedLinkedList<Annotation>.Empty);

    public static ValidationResult SingleErrorFailedResult(ValidationError singleError)
    {
        return new ValidationResult(false, new ImmutableValidationErrorCollection(singleError), ImmutableDoubleEndedLinkedList<Annotation>.Empty);
    }

    /// <summary>
    /// Gets all failed validation nodes during validation.
    /// If <see cref="JsonSchemaOptions.OutputFormat"/> is set to <see cref="OutputFormat.FailFast"/>, this will contain only first found failed validation node.
    /// </summary>
    public IEnumerable<ValidationError> ValidationErrors => ValidationErrorsList.Enumerate();

    public IEnumerable<Annotation?> Annotations => AnnotationList;
}

internal class ImmutableDoubleEndedLinkedList<T> : IEnumerable<T>
{
    public static ImmutableDoubleEndedLinkedList<T> Empty { get; } = new();

    private readonly LinkedNode? _header;
    private readonly LinkedNode? _tailer;

    private ImmutableDoubleEndedLinkedList()
    {
    }

    private ImmutableDoubleEndedLinkedList(LinkedNode header, LinkedNode tailer)
    {
        _header = header;
        _tailer = tailer;
    }

    public bool IsEmpty => _header is null;

    public IEnumerator<T> GetEnumerator()
    {
        return new Enumerator(this);
    }

    internal class Enumerator : IEnumerator<T>
    {
        private readonly ImmutableDoubleEndedLinkedList<T> _list;
        private LinkedNode? _curNode;
        private bool _hasEnd;

        public Enumerator(ImmutableDoubleEndedLinkedList<T> list)
        {
            _list = list;
        }

        public bool MoveNext()
        {
            if (_list.IsEmpty)
            {
                return false;
            }

            if (_hasEnd)
            {
                return false;
            }

            _curNode = _curNode is null ? _list._header : _curNode.Next;

            if (ReferenceEquals(_curNode, _list._tailer))
            {
                _hasEnd = true;
            }

            return true;
        }

        public void Reset()
        {
            _hasEnd = false;
            _curNode = null;
        }

        public T Current => _curNode is null ? default! : _curNode.Value;

        object? IEnumerator.Current => Current;

        public void Dispose()
        {
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    internal ref struct Builder
    {
        private LinkedNode? _header;
        private LinkedNode? _tailer;

        public void Add(T value)
        {
            var newNode = new LinkedNode(value);
            if (_header is null)
            {
                _tailer = _header = newNode;
            }
            else
            {
                Debug.Assert(_tailer is not null);
                Debug.Assert(_tailer.Next is null); // The tailer node's Next should be null, otherwise there will be one existing linked list is being corrupted which means there is internal logic bug

                _tailer.Next = newNode;
                _tailer = newNode;
            }
        }

        public void AddRange(ImmutableDoubleEndedLinkedList<T> list)
        {
            if (list.IsEmpty)
            {
                return;
            }

            if (_header is null)
            {
                _header = list._header;
                _tailer = list._tailer;
            }
            else
            {
                Debug.Assert(_tailer is not null);
                Debug.Assert(_tailer.Next is null); // The tailer node's Next should be null, otherwise there will be one existing linked list is being corrupted which means there is internal logic bug
             
                _tailer.Next = list._header;
                _tailer = list._tailer;
            }
        }

        public ImmutableDoubleEndedLinkedList<T> ToImmutable()
        {
            if (_header is null)
            {
                Debug.Assert(_tailer is null);

                return Empty;
            }

            Debug.Assert(_tailer is not null);
            return new ImmutableDoubleEndedLinkedList<T>(_header, _tailer);
        }
    }

    private class LinkedNode
    {
        public readonly T Value;
        public LinkedNode? Next;

        public LinkedNode(T value)
        {
            Value = value;
        }
    }
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
    public ValidationError(ResultCode failedCode, string errorMessage, ValidationPathStack? validationPathStack, string? keyword, ImmutableJsonPointer instanceLocation)
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

    /// <summary>
    /// Gets value to indicate json instance's location
    /// </summary>
    public ImmutableJsonPointer InstanceLocation { get; init; }

    /// <summary>
    /// Gets value to indicate relative location of keyword
    /// </summary>
    public ImmutableJsonPointer? RelativeKeywordLocation { get; init; }

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
    /// Gets the human-readable description of the validation failure.
    /// Built-in validators return <see cref="string.Empty"/> when
    /// <see cref="JsonSchemaOptions.GenerateErrorMessages"/> is disabled.
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
        var sb = new StringBuilder();

        if (!string.IsNullOrEmpty(ErrorMessage))
        {
            sb.AppendLine(ErrorMessage);
        }

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