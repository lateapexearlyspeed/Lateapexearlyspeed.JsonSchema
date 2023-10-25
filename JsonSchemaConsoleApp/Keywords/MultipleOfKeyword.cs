using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using JsonSchemaConsoleApp.JsonConverters;

namespace JsonSchemaConsoleApp.Keywords;

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

        double remainder = Math.Abs(instance.GetDouble() % MultipleOf);

        Debug.Assert(MultipleOf > 0);

        // See https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/operators/arithmetic-operators#floating-point-remainder
        // and 'Precision in Comparisons' part of https://learn.microsoft.com/en-us/dotnet/api/system.double.equals
        return remainder < Tolerance || Math.Abs(remainder - MultipleOf) < MultipleOf * Tolerance 
            ? ValidationResult.ValidResult 
            : ValidationResult.CreateFailedResult(ResultCode.FailedToMultiple, options.ValidationPathStack);
    }
}