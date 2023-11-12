using System.Text.Json;
using LateApexEarlySpeed.Json.Schema.Common;
using LateApexEarlySpeed.Json.Schema.Keywords.interfaces;

namespace LateApexEarlySpeed.Json.Schema.Keywords;

internal abstract class ArrayLengthKeywordBase : KeywordBase, IBenchmarkValueKeyword
{
    public uint BenchmarkValue { get; init; }

    protected internal override ValidationResult ValidateCore(JsonElement instance, JsonSchemaOptions options)
    {
        if (instance.ValueKind != JsonValueKind.Array)
        {
            return ValidationResult.ValidResult;
        }

        int instanceLength = instance.EnumerateArray().Count();

        return IsSizeInRange(instanceLength)
            ? ValidationResult.ValidResult
            : ValidationResult.CreateFailedResult(ResultCode.ArrayLengthOutOfRange, GetErrorMessage(instanceLength), options.ValidationPathStack, Name);

    }

    protected abstract bool IsSizeInRange(int instanceArrayLength);

    protected abstract string GetErrorMessage(int instanceArrayLength);
}