using System.Text.Json;
using System.Text.Json.Serialization;
using LateApexEarlySpeed.Json.Schema.Common;
using LateApexEarlySpeed.Json.Schema.Keywords.JsonConverters;

namespace LateApexEarlySpeed.Json.Schema.Keywords;

[Keyword("minItems")]
[JsonConverter(typeof(BenchmarkValueKeywordJsonConverter<MinItemsKeyword>))]
internal class MinItemsKeyword : ArrayLengthKeywordBase
{
    protected internal override ValidationResult ValidateCore(JsonElement instance, JsonSchemaOptions options)
    {
        if (instance.ValueKind != JsonValueKind.Array)
        {
            return ValidationResult.ValidResult;
        }

        int instanceLength = instance.EnumerateArray().Count();

        return BenchmarkValue <= instanceLength
            ? ValidationResult.ValidResult
            : ValidationResult.CreateFailedResult(ResultCode.ArrayLengthOutOfRange, $"Array length: {instanceLength} is less than '{BenchmarkValue}'", options.ValidationPathStack, Name);

    }

    protected override bool IsSizeInRange(int instanceArrayLength) 
        => BenchmarkValue <= instanceArrayLength;

    protected override string GetErrorMessage(int instanceArrayLength) 
        => $"Array length: {instanceArrayLength} is less than '{BenchmarkValue}'";
}