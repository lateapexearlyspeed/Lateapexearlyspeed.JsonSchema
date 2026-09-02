using LateApexEarlySpeed.Json.Schema.Common;
using LateApexEarlySpeed.Json.Schema.JInstance;
using LateApexEarlySpeed.Json.Schema.Keywords.Annotations;
using LateApexEarlySpeed.Json.Schema.Keywords.interfaces;
using LateApexEarlySpeed.Json.Schema.Keywords.JsonConverters;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace LateApexEarlySpeed.Json.Schema.Keywords;

[Keyword("format")]
[JsonConverter(typeof(FormatKeywordJsonConverter))]
public class FormatKeyword : ValidationKeywordBase, IAnnotationKeyword
{
    /// <summary>
    /// The original format value from the JSON schema. It may be invalid (or unsupported) format value and can be used to serialize back to JSON schema.
    /// </summary>
    public string Format { get; }

    private readonly FormatValidator? _formatValidator;

    public FormatKeyword(string format, FormatValidator? formatValidator)
    {
        Format = format;
        _formatValidator = formatValidator;
    }

    protected internal override ValidationResult ValidateCore(JsonInstanceElement instance, JsonSchemaOptions options)
    {
        if (!options.ValidateFormat || _formatValidator is null || instance.ValueKind != JsonValueKind.String)
        {
            return ValidationResult.ValidResult;
        }

        return _formatValidator.Validate(instance.GetString()!)
            ? ValidationResult.ValidResult
            : ValidationResult.SingleErrorFailedResult(new ValidationError(ResultCode.InvalidFormat, options.GenerateErrorMessages ? ErrorMessage(Format) : string.Empty, options.ValidationPathStack, Name, instance.Location));
    }

    public static string ErrorMessage(string format)
    {
        return $"Invalid string value for format: '{format}'";
    }

    public bool ShouldAnnotate(JsonInstanceElement instance)
    {
        return instance.ValueKind == JsonValueKind.String;
    }

    public Annotation Annotate(JsonInstanceElement instance, JsonSchemaOptions options)
    {
        return AnnotationKeywordHelper.Annotate(Name, JsonSerializer.SerializeToElement(Format), instance, options);
    }
}