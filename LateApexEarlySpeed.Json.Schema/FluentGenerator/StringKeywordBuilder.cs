using LateApexEarlySpeed.Json.Schema.FluentGenerator.ExtendedKeywords;
using LateApexEarlySpeed.Json.Schema.JInstance;
using LateApexEarlySpeed.Json.Schema.JSchema;
using LateApexEarlySpeed.Json.Schema.Keywords;

namespace LateApexEarlySpeed.Json.Schema.FluentGenerator;

public class StringKeywordBuilder : KeywordBuilder
{
    public StringKeywordBuilder() : base(new TypeKeyword(InstanceType.String))
    {
    }

    /// <summary>
    /// Specify that current json string content should equal to <paramref name="value"/>
    /// </summary>
    /// <param name="value">Normal string content</param>
    /// <returns></returns>
    public StringKeywordBuilder Equal(string value)
    {
        Keywords.Add(new ConstKeyword(JsonInstanceSerializer.SerializeToElement(value)));

        return this;
    }

    /// <summary>
    /// Specify that current json string should be one of <paramref name="collection"/>
    /// </summary>
    /// <param name="collection">available string contents</param>
    /// <returns></returns>
    public StringKeywordBuilder IsIn(IEnumerable<string> collection)
    {
        Keywords.Add(new EnumKeyword(collection.Select(JsonInstanceSerializer.SerializeToElement).ToList()));

        return this;
    }

    /// <summary>
    /// Specify that current json string should have max length of <paramref name="max"/>
    /// </summary>
    /// <param name="max"></param>
    /// <returns></returns>
    public StringKeywordBuilder HasMaxLength(uint max)
    {
        Keywords.Add(new MaxLengthKeyword{BenchmarkValue = max});

        return this;
    }

    /// <summary>
    /// Specify that current json string should have min length of <paramref name="min"/>
    /// </summary>
    /// <param name="min"></param>
    /// <returns></returns>
    public StringKeywordBuilder HasMinLength(uint min)
    {
        Keywords.Add(new MinLengthKeyword { BenchmarkValue = min });

        return this;
    }

    /// <summary>
    /// Specify that current json string should match specified <paramref name="pattern"/>
    /// </summary>
    /// <param name="pattern"></param>
    /// <returns></returns>
    public StringKeywordBuilder HasPattern(string pattern)
    {
        Keywords.Add(new PatternKeyword(pattern));

        return this;
    }

    /// <summary>
    /// Specify that current json string should not match specified <paramref name="pattern"/>
    /// </summary>
    /// <param name="pattern">The regex pattern that current json string should not match.</param>
    /// <returns></returns>
    public StringKeywordBuilder NotMatch(string pattern)
    {
        Keywords.Add(new NotKeyword{Schema = new BodyJsonSchema(new List<KeywordBase>{new PatternKeyword(pattern)})});

        return this;
    }

    /// <summary>
    /// Specify that current json string should validate specified <paramref name="validator"/> and generate error info by <paramref name="errorMessageFunc"/> if failed to validate
    /// </summary>
    /// <param name="validator">custom validation logic, argument is current string content</param>
    /// <param name="errorMessageFunc">custom validation error report, argument is current string content</param>
    /// <returns></returns>
    public StringKeywordBuilder HasCustomValidation(Func<string, bool> validator, Func<string, string> errorMessageFunc)
    {
        Keywords.Add(new StringCustomValidationKeyword(validator, errorMessageFunc));

        return this;
    }

    /// <summary>
    /// Specify that current json string should start with specified <paramref name="startString"/>
    /// </summary>
    /// <param name="startString">The string expected to be at the start of the string</param>
    /// <returns></returns>
    public StringKeywordBuilder StartsWith(string startString)
    {
        return StartsWith(startString, StringComparison.CurrentCulture);
    }

    /// <summary>
    /// Specify that current json string should start with specified <paramref name="startString"/>
    /// </summary>
    /// <param name="startString">The string expected to be at the start of the string</param>
    /// <param name="comparisonType">One of the enumeration values that determines how strings are compared</param>
    /// <returns></returns>
    public StringKeywordBuilder StartsWith(string startString, StringComparison comparisonType)
    {
        return HasCustomValidation(instance => instance.StartsWith(startString, comparisonType), instance => StartsWithErrorMessage(instance, startString));
    }

    internal static string StartsWithErrorMessage(string instanceText, string expectedStartString)
    {
        return $"Instance: '{instanceText}' not start with '{expectedStartString}'";
    }

    /// <summary>
    /// Specify that current json string should end with specified <paramref name="endString"/>
    /// </summary>
    /// <param name="endString">The string expected to be at the end of the string</param>
    /// <returns></returns>
    public StringKeywordBuilder EndsWith(string endString)
    {
        return EndsWith(endString, StringComparison.CurrentCulture);
    }

    /// <summary>
    /// Specify that current json string should end with specified <paramref name="endString"/>
    /// </summary>
    /// <param name="endString">The string expected to be at the end of the string</param>
    /// <param name="comparisonType">One of the enumeration values that determines how strings are compared</param>
    /// <returns></returns>
    public StringKeywordBuilder EndsWith(string endString, StringComparison comparisonType)
    {
        return HasCustomValidation(instance => instance.EndsWith(endString, comparisonType), instance => EndsWithErrorMessage(instance, endString));
    }

    internal static string EndsWithErrorMessage(string instanceText, string expectedEndString)
    {
        return $"Instance: '{instanceText}' not end with '{expectedEndString}'";
    }
}