namespace JsonSchemaConsoleApp;

public class ValidationResult
{
    private ValidationResult()
    {
    }

    public JsonPointer? RelativeKeywordLocation { get; init; }
    public Uri? SchemaResourceBaseUri { get; init; }
    public Uri? SubSchemaRefFullUri { get; init; }
    public ResultCode ResultCode { get; init; }

    public bool IsValid => ResultCode == ResultCode.Valid;

    public static ValidationResult ValidResult { get; } = new() { ResultCode = ResultCode.Valid };
    public static ValidationResult CreateFailedResult(ResultCode failedCode, ValidationPathStack? validationPathStack)
        => new()
        {
            ResultCode = failedCode,
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
    SubSchemaPassed,
    NotFoundRequiredDependentProperty,
    AlwaysFailed,
    RegexNotMatch
}