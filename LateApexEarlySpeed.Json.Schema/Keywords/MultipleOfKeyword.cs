using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
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
    private readonly IMultipleOfChecker _multipleOfChecker;

    public MultipleOfKeyword(double multipleOf)
    {
        if (multipleOf <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(multipleOf), multipleOf, $"Argument: '{nameof(multipleOf)}' expects positive number.");
        }

        _multipleOfChecker = new DoubleMultipleOfChecker(multipleOf);
    }

    public MultipleOfKeyword(ulong multipleOf)
    {
        if (multipleOf == 0)
        {
            throw new ArgumentOutOfRangeException(nameof(multipleOf), multipleOf, $"Argument: '{nameof(multipleOf)}' expects positive number.");
        }

        _multipleOfChecker = new ULongMultipleOfChecker(multipleOf);
    }

    public MultipleOfKeyword(decimal multipleOf)
    {
        if (multipleOf <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(multipleOf), multipleOf, $"Argument: '{nameof(multipleOf)}' expects positive number.");
        }

        _multipleOfChecker = new DecimalMultipleOfChecker(multipleOf);
    }

    public void WriteMultipleOfValue(Utf8JsonWriter writer)
    {
        _multipleOfChecker.WriteMultipleOfValue(writer);
    }

    protected internal override ValidationResult ValidateCore(JsonInstanceElement instance, JsonSchemaOptions options)
    {
        if (instance.ValueKind != JsonValueKind.Number)
        {
            return ValidationResult.ValidResult;
        }

        MultipleOfResult multipleOfResult = _multipleOfChecker.Check(instance.InternalJsonElement);
        return multipleOfResult.IsSuccess
            ? ValidationResult.ValidResult 
            : ValidationResult.SingleErrorFailedResult(new ValidationError(ResultCode.FailedToMultiple, multipleOfResult.ErrorMessage, options.ValidationPathStack, Name, instance.Location));
    }
}

internal class DecimalMultipleOfChecker : IMultipleOfChecker
{
    private const decimal DecimalTolerance = 0.000_000_1m;
    private const double DoubleTolerance = 0.000_01;

    private readonly decimal _multipleOf;

    public DecimalMultipleOfChecker(decimal multipleOf)
    {
        _multipleOf = multipleOf;
    }

    public MultipleOfResult Check(JsonElement instance)
    {
        Debug.Assert(_multipleOf > 0);

        if (instance.TryGetDecimal(out decimal decimalInstance))
        {
            decimal remainder = Math.Abs(decimalInstance % _multipleOf);

            // See https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/operators/arithmetic-operators#floating-point-remainder
            // and 'Precision in Comparisons' part of https://learn.microsoft.com/en-us/dotnet/api/system.double.equals
            decimal actualTolerance = _multipleOf * DecimalTolerance;
            return remainder < actualTolerance || Math.Abs(remainder - _multipleOf) < actualTolerance
                ? MultipleOfResult.Success()
                : MultipleOfResult.Fail(ErrorMessage(decimalInstance, _multipleOf));
        }
        else
        {
            // All numbers can be represented by 'double' type even if it is greater than double.MaxValue which will be represented to be 'infinity'
            double doubleInstance = instance.GetDouble();

            double multipleOf = (double)_multipleOf;
            double remainder = Math.Abs(doubleInstance % multipleOf);

            // See https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/operators/arithmetic-operators#floating-point-remainder
            // and 'Precision in Comparisons' part of https://learn.microsoft.com/en-us/dotnet/api/system.double.equals
            double actualTolerance = multipleOf * DoubleTolerance;
            return remainder < actualTolerance || Math.Abs(remainder - multipleOf) < actualTolerance
                ? MultipleOfResult.Success()
                : MultipleOfResult.Fail(ErrorMessage(doubleInstance, _multipleOf));
        }
    }

    public void WriteMultipleOfValue(Utf8JsonWriter writer)
    {
        writer.WriteNumberValue(_multipleOf);
    }

    public static string ErrorMessage(object instanceValue, decimal multipleOf)
    {
        return $"Instance: '{instanceValue}' is not multiple of '{multipleOf}'";
    }
}

internal class ULongMultipleOfChecker : IMultipleOfChecker
{
    private readonly ulong _multipleOf;

    public ULongMultipleOfChecker(ulong multipleOf)
    {
        _multipleOf = multipleOf;
    }

    public MultipleOfResult Check(JsonElement instance)
    {
        Debug.Assert(_multipleOf != 0);

        if (instance.TryGetInt64(out long longInstance))
        {
            if (_multipleOf > long.MaxValue)
            {
                return longInstance == 0 
                    ? MultipleOfResult.Success() 
                    : MultipleOfResult.Fail(ErrorMessage(longInstance, _multipleOf));
            }

            return longInstance % (long)_multipleOf == 0 
                ? MultipleOfResult.Success() 
                : MultipleOfResult.Fail(ErrorMessage(longInstance, _multipleOf));
        }

        if (instance.TryGetUInt64(out ulong ulongInstance))
        {
            return ulongInstance % _multipleOf == 0
                ? MultipleOfResult.Success()
                : MultipleOfResult.Fail(ErrorMessage(ulongInstance, _multipleOf));
        }

        // Now the instance should be float point number
        double doubleInstance = instance.GetDouble();
        Debug.Assert(doubleInstance % _multipleOf != 0);
        return MultipleOfResult.Fail(ErrorMessage(doubleInstance, _multipleOf));
    }

    public void WriteMultipleOfValue(Utf8JsonWriter writer)
    {
        writer.WriteNumberValue(_multipleOf);
    }

    public static string ErrorMessage(object instanceValue, ulong multipleOf)
    {
        return $"Instance: '{instanceValue}' is not multiple of '{multipleOf}'";
    }
}

internal class DoubleMultipleOfChecker : IMultipleOfChecker
{
    private const double Tolerance = 0.000_01;

    private readonly double _multipleOf;

    public DoubleMultipleOfChecker(double multipleOf)
    {
        _multipleOf = multipleOf;
    }

    public MultipleOfResult Check(JsonElement instance)
    {
        Debug.Assert(_multipleOf > 0);

        if (_multipleOf <= 1e-8)
        {
            return MultipleOfResult.Success();
        }

        double instanceValue = instance.GetDouble();
        double remainder = Math.Abs(instanceValue % _multipleOf);

        // See https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/operators/arithmetic-operators#floating-point-remainder
        // and 'Precision in Comparisons' part of https://learn.microsoft.com/en-us/dotnet/api/system.double.equals
        double actualTolerance = _multipleOf * Tolerance;
        return remainder < actualTolerance || Math.Abs(remainder - _multipleOf) < actualTolerance
            ? MultipleOfResult.Success()
            : MultipleOfResult.Fail(ErrorMessage(instanceValue, _multipleOf));
    }

    public void WriteMultipleOfValue(Utf8JsonWriter writer)
    {
        writer.WriteNumberValue(_multipleOf);
    }

    public static string ErrorMessage(double instanceValue, double multipleOf)
    {
        return $"Instance: '{instanceValue}' is not multiple of '{multipleOf}'";
    }
}

internal interface IMultipleOfChecker
{
    MultipleOfResult Check(JsonElement instance);
    void WriteMultipleOfValue(Utf8JsonWriter writer);
}

internal readonly ref struct MultipleOfResult
{
    [MemberNotNullWhen(false, nameof(ErrorMessage))]
    public bool IsSuccess { get; }

    public string? ErrorMessage { get; }

    private MultipleOfResult(bool isSuccess, string? errorMessage)
    {
        IsSuccess = isSuccess;
        ErrorMessage = errorMessage;
    }

    public static MultipleOfResult Success()
    {
        return new MultipleOfResult(true, null);
    }

    public static MultipleOfResult Fail(string errorMessage)
    {
        return new MultipleOfResult(false, errorMessage);
    }
}