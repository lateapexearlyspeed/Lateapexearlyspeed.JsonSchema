using System.Diagnostics;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using LateApexEarlySpeed.Json.Schema.Common;
using LateApexEarlySpeed.Json.Schema.JInstance;
using LateApexEarlySpeed.Json.Schema.Keywords.JsonConverters;

namespace LateApexEarlySpeed.Json.Schema.Keywords;

[Obfuscation(ApplyToMembers = false)]
[Keyword("multipleOf")]
[JsonConverter(typeof(MultipleOfKeywordJsonConverter))]
internal class MultipleOfKeyword : KeywordBase
{
    private const double Tolerance = 0.00001;

    private readonly double _multipleOf;

    public MultipleOfKeyword(double multipleOf)
    {
        _multipleOf = multipleOf;
    }

    protected internal override ValidationResult ValidateCore(JsonInstanceElement instance, JsonSchemaOptions options)
    {
        Debug.Assert(_multipleOf > 0);

        if (instance.ValueKind != JsonValueKind.Number || _multipleOf <= 1e-8)
        {
            return ValidationResult.ValidResult;
        }

        double instanceValue = instance.GetDouble();
        double remainder = Math.Abs(instanceValue % _multipleOf);

        // See https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/operators/arithmetic-operators#floating-point-remainder
        // and 'Precision in Comparisons' part of https://learn.microsoft.com/en-us/dotnet/api/system.double.equals
        double actualTolerance = _multipleOf * Tolerance;
        return remainder < actualTolerance || Math.Abs(remainder - _multipleOf) < actualTolerance 
            ? ValidationResult.ValidResult 
            : ValidationResult.CreateFailedResult(ResultCode.FailedToMultiple, ErrorMessage(instanceValue, _multipleOf), options.ValidationPathStack, Name, instance.Location);
    }

    [Obfuscation]
    public static string ErrorMessage(double instanceValue, double multipleOf)
    {
        return $"Instance: '{instanceValue}' is not multiple of '{multipleOf}'";
    }
}