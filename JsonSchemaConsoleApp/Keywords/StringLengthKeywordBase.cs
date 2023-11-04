using System.Text.Json;
using JsonSchemaConsoleApp.Keywords.interfaces;

namespace JsonSchemaConsoleApp.Keywords;

internal abstract class StringLengthKeywordBase : KeywordBase, IBenchmarkValueKeyword
{
    public uint BenchmarkValue { get; init; }

    protected internal override ValidationResult ValidateCore(JsonElement instance, JsonSchemaOptions options)
    {
        if (instance.ValueKind != JsonValueKind.String)
        {
            return ValidationResult.ValidResult;
        }

        return IsStringLengthInRange(instance.GetString()!.Length)
            ? ValidationResult.ValidResult
            : ValidationResult.CreateFailedResult(ResultCode.StringLengthOutOfRange, options.ValidationPathStack);
    }

    protected abstract bool IsStringLengthInRange(int instanceStringLength);
}