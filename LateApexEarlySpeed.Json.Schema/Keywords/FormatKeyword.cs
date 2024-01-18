using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using LateApexEarlySpeed.Json.Schema.Common;
using LateApexEarlySpeed.Json.Schema.JInstance;
using LateApexEarlySpeed.Json.Schema.Keywords.JsonConverters;

namespace LateApexEarlySpeed.Json.Schema.Keywords;

[Obfuscation(ApplyToMembers = false)]
[Keyword("format")]
[JsonConverter(typeof(FormatKeywordJsonConverter))]
public class FormatKeyword : KeywordBase
{
    private readonly string _format;
    private readonly FormatValidator? _formatValidator;

    public FormatKeyword(string format)
    {
        _format = format;
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
            : ValidationResult.CreateFailedResult(ResultCode.InvalidFormat, ErrorMessage(_format), options.ValidationPathStack, Name, instance.Location);
    }

    [Obfuscation]
    public static string ErrorMessage(string format)
    {
        return $"Invalid string value for format:'{format}'";
    }
}