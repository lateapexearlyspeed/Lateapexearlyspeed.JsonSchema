using System.Text.Json;
using System.Text.Json.Serialization;
using LateApexEarlySpeed.Json.Schema.Common;
using LateApexEarlySpeed.Json.Schema.JInstance;
using LateApexEarlySpeed.Json.Schema.Keywords.JsonConverters;

namespace LateApexEarlySpeed.Json.Schema.Keywords;

[Keyword("format")]
[JsonConverter(typeof(FormatKeywordJsonConverter))]
public class FormatKeyword : KeywordBase
{
    public string Format { get; }

    private readonly FormatValidator? _formatValidator;

    public FormatKeyword(string format)
    {
        Format = format;
        _formatValidator = FormatValidator.Create(format);
    }

    protected internal override ValidationResult ValidateCore(JsonInstanceElement instance, JsonSchemaOptions options)
    {
        if (!options.ValidateFormat || _formatValidator is null || instance.ValueKind != JsonValueKind.String)
        {
            return ValidationResult.ValidResult;
        }

        return _formatValidator.Validate(instance.GetString()!)
            ? ValidationResult.ValidResult
            : ValidationResult.SingleErrorFailedResult(new ValidationError(ResultCode.InvalidFormat, ErrorMessage(Format), options.ValidationPathStack, Name, instance.Location));
    }

    public static string ErrorMessage(string format)
    {
        return $"Invalid string value for format:'{format}'";
    }
}