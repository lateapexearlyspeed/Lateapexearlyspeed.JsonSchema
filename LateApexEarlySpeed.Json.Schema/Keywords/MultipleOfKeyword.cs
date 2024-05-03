using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using LateApexEarlySpeed.Json.Schema.Common;
using LateApexEarlySpeed.Json.Schema.JInstance;
using LateApexEarlySpeed.Json.Schema.Keywords.JsonConverters;

namespace LateApexEarlySpeed.Json.Schema.Keywords;

[Keyword("multipleOf")]
[JsonConverter(typeof(MultipleOfKeywordJsonConverter))]
internal class MultipleOfKeyword : KeywordBase
{
    private const double Tolerance = 0.00001;

    public double MultipleOf { get; }

    public MultipleOfKeyword(double multipleOf)
    {
        if (multipleOf <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(multipleOf), multipleOf, $"Argument: '{nameof(multipleOf)}' expects positive number.");
        }

        MultipleOf = multipleOf;
    }

    protected internal override ValidationResult ValidateCore(JsonInstanceElement instance, JsonSchemaOptions options)
    {
        Debug.Assert(MultipleOf > 0);

        if (instance.ValueKind != JsonValueKind.Number || MultipleOf <= 1e-8)
        {
            return ValidationResult.ValidResult;
        }

        double instanceValue = instance.GetDouble();
        double remainder = Math.Abs(instanceValue % MultipleOf);

        // See https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/operators/arithmetic-operators#floating-point-remainder
        // and 'Precision in Comparisons' part of https://learn.microsoft.com/en-us/dotnet/api/system.double.equals
        double actualTolerance = MultipleOf * Tolerance;
        return remainder < actualTolerance || Math.Abs(remainder - MultipleOf) < actualTolerance 
            ? ValidationResult.ValidResult 
            : ValidationResult.CreateFailedResult(ResultCode.FailedToMultiple, ErrorMessage(instanceValue, MultipleOf), options.ValidationPathStack, Name, instance.Location);
    }

    public static string ErrorMessage(double instanceValue, double multipleOf)
    {
        return $"Instance: '{instanceValue}' is not multiple of '{multipleOf}'";
    }
}