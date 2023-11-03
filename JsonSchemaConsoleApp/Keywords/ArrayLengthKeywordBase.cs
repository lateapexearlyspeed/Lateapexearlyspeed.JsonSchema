using System.Text.Json;
using JsonSchemaConsoleApp.Keywords.interfaces;

namespace JsonSchemaConsoleApp.Keywords;

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
            : ValidationResult.CreateFailedResult(ResultCode.ArrayLengthOutOfRange, options.ValidationPathStack);

    }

    protected abstract bool IsSizeInRange(int instanceArrayLength);
}