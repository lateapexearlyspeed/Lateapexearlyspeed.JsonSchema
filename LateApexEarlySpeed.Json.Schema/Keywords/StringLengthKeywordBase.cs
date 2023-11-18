using System.Text.Json;
using LateApexEarlySpeed.Json.Schema.Common;
using LateApexEarlySpeed.Json.Schema.JInstance;
using LateApexEarlySpeed.Json.Schema.Keywords.interfaces;

namespace LateApexEarlySpeed.Json.Schema.Keywords;

internal abstract class StringLengthKeywordBase : KeywordBase, IBenchmarkValueKeyword
{
    public uint BenchmarkValue { get; init; }

    protected internal override ValidationResult ValidateCore(JsonInstanceElement instance, JsonSchemaOptions options)
    {
        if (instance.ValueKind != JsonValueKind.String)
        {
            return ValidationResult.ValidResult;
        }

        int instanceStringLength = instance.GetString()!.Length;
        return IsStringLengthInRange(instanceStringLength)
            ? ValidationResult.ValidResult
            : ValidationResult.CreateFailedResult(ResultCode.StringLengthOutOfRange, GetErrorMessage(instanceStringLength), options.ValidationPathStack, Name, instance.Location);
    }

    protected abstract bool IsStringLengthInRange(int instanceStringLength);

    protected abstract string GetErrorMessage(int instanceStringLength);
}