using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using LateApexEarlySpeed.Json.Schema.Common;
using LateApexEarlySpeed.Json.Schema.Keywords.JsonConverters;

namespace LateApexEarlySpeed.Json.Schema.Keywords;

[Keyword("multipleOf")]
[JsonConverter(typeof(MultipleOfKeywordJsonConverter))]
internal class MultipleOfKeyword : KeywordBase
{
    private const double Tolerance = 0.00001;

    public double MultipleOf { get; init; }

    protected internal override ValidationResult ValidateCore(JsonElement instance, JsonSchemaOptions options)
    {
        if (instance.ValueKind != JsonValueKind.Number)
        {
            return ValidationResult.ValidResult;
        }

        double instanceValue = instance.GetDouble();
        double remainder = Math.Abs(instanceValue % MultipleOf);

        Debug.Assert(MultipleOf > 0);

        // See https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/operators/arithmetic-operators#floating-point-remainder
        // and 'Precision in Comparisons' part of https://learn.microsoft.com/en-us/dotnet/api/system.double.equals
        double actualTolerance = MultipleOf * Tolerance;
        return remainder < actualTolerance || Math.Abs(remainder - MultipleOf) < actualTolerance 
            ? ValidationResult.ValidResult 
            : ValidationResult.CreateFailedResult(ResultCode.FailedToMultiple, $"Instance: '{instanceValue}' is not multiple of '{MultipleOf}'", options.ValidationPathStack, Name);
    }
}