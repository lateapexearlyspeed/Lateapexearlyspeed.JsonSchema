using LateApexEarlySpeed.Json.Schema.FluentGenerator.ExtendedKeywords;
using LateApexEarlySpeed.Json.Schema.JInstance;
using LateApexEarlySpeed.Json.Schema.Keywords;

namespace LateApexEarlySpeed.Json.Schema.FluentGenerator;

public class NumberKeywordBuilder : KeywordBuilder
{
    public NumberKeywordBuilder() : base(new TypeKeyword(InstanceType.Number))
    {
    }

    /// <summary>
    /// Specify that current json number should equal to <paramref name="value"/>
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public NumberKeywordBuilder Equal(double value)
    {
        return Equal<double>(value);
    }

    /// <summary>
    /// Specify that current json number should equal to <paramref name="value"/>
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public NumberKeywordBuilder Equal(decimal value)
    {
        return Equal<decimal>(value);
    }

    /// <summary>
    /// Specify that current json number should equal to <paramref name="value"/>
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public NumberKeywordBuilder Equal(long value)
    {
        return Equal<long>(value);
    }

    /// <summary>
    /// Specify that current json number should equal to <paramref name="value"/>
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public NumberKeywordBuilder Equal(ulong value)
    {
        return Equal<ulong>(value);
    }

    private NumberKeywordBuilder Equal<T>(T value) where T : unmanaged
    {
        Keywords.Add(new ConstKeyword(JsonInstanceSerializer.SerializeToElement(value)));

        return this;
    }

    /// <summary>
    /// Specify that current json number should be one of <paramref name="collection"/>
    /// </summary>
    /// <param name="collection"></param>
    /// <returns></returns>
    public NumberKeywordBuilder IsIn(double[] collection)
    {
        return IsIn<double>(collection);
    }

    /// <summary>
    /// Specify that current json number should be one of <paramref name="collection"/>
    /// </summary>
    /// <param name="collection"></param>
    /// <returns></returns>
    public NumberKeywordBuilder IsIn(long[] collection)
    {
        return IsIn<long>(collection);
    }

    private NumberKeywordBuilder IsIn<T>(IEnumerable<T> collection) where T : unmanaged
    {
        Keywords.Add(new EnumKeyword(collection.Select(item => JsonInstanceSerializer.SerializeToElement(item))));

        return this;
    }

    /// <summary>
    /// Specify that current json number should be greater than <paramref name="min"/>
    /// </summary>
    /// <param name="min"></param>
    /// <returns></returns>
    public NumberKeywordBuilder IsGreaterThan(long min)
    {
        Keywords.Add(new ExclusiveMinimumKeyword(min));

        return this;
    }

    /// <summary>
    /// Specify that current json number should be greater than <paramref name="min"/>
    /// </summary>
    /// <param name="min"></param>
    /// <returns></returns>
    public NumberKeywordBuilder IsGreaterThan(ulong min)
    {
        Keywords.Add(new ExclusiveMinimumKeyword(min));

        return this;
    }

    /// <summary>
    /// Specify that current json number should be greater than <paramref name="min"/>
    /// </summary>
    /// <param name="min"></param>
    /// <returns></returns>
    public NumberKeywordBuilder IsGreaterThan(decimal min)
    {
        Keywords.Add(new ExclusiveMinimumKeyword(min));

        return this;
    }

    /// <summary>
    /// Specify that current json number should be greater than <paramref name="min"/>
    /// </summary>
    /// <param name="min"></param>
    /// <returns></returns>
    public NumberKeywordBuilder IsGreaterThan(double min)
    {
        Keywords.Add(new ExclusiveMinimumKeyword(min));

        return this;
    }

    /// <summary>
    /// Specify that current json number should be less than <paramref name="max"/>
    /// </summary>
    /// <param name="max"></param>
    /// <returns></returns>
    public NumberKeywordBuilder IsLessThan(long max)
    {
        Keywords.Add(new ExclusiveMaximumKeyword(max));

        return this;
    }

    /// <summary>
    /// Specify that current json number should be less than <paramref name="max"/>
    /// </summary>
    /// <param name="max"></param>
    /// <returns></returns>
    public NumberKeywordBuilder IsLessThan(ulong max)
    {
        Keywords.Add(new ExclusiveMaximumKeyword(max));

        return this;
    }

    /// <summary>
    /// Specify that current json number should be less than <paramref name="max"/>
    /// </summary>
    /// <param name="max"></param>
    /// <returns></returns>
    public NumberKeywordBuilder IsLessThan(decimal max)
    {
        Keywords.Add(new ExclusiveMaximumKeyword(max));

        return this;
    }

    /// <summary>
    /// Specify that current json number should be less than <paramref name="max"/>
    /// </summary>
    /// <param name="max"></param>
    /// <returns></returns>
    public NumberKeywordBuilder IsLessThan(double max)
    {
        Keywords.Add(new ExclusiveMaximumKeyword(max));

        return this;
    }

    /// <summary>
    /// Specify that current json number should not be greater than <paramref name="max"/>
    /// </summary>
    /// <param name="max"></param>
    /// <returns></returns>
    public NumberKeywordBuilder NotGreaterThan(long max)
    {
        Keywords.Add(new MaximumKeyword(max));

        return this;
    }

    /// <summary>
    /// Specify that current json number should not be greater than <paramref name="max"/>
    /// </summary>
    /// <param name="max"></param>
    /// <returns></returns>
    public NumberKeywordBuilder NotGreaterThan(ulong max)
    {
        Keywords.Add(new MaximumKeyword(max));

        return this;
    }

    /// <summary>
    /// Specify that current json number should not be greater than <paramref name="max"/>
    /// </summary>
    /// <param name="max"></param>
    /// <returns></returns>
    public NumberKeywordBuilder NotGreaterThan(decimal max)
    {
        Keywords.Add(new MaximumKeyword(max));

        return this;
    }

    /// <summary>
    /// Specify that current json number should not be greater than <paramref name="max"/>
    /// </summary>
    /// <param name="max"></param>
    /// <returns></returns>
    public NumberKeywordBuilder NotGreaterThan(double max)
    {
        Keywords.Add(new MaximumKeyword(max));

        return this;
    }

    /// <summary>
    /// Specify that current json number should not be less than <paramref name="min"/>
    /// </summary>
    /// <param name="min"></param>
    /// <returns></returns>
    public NumberKeywordBuilder NotLessThan(long min)
    {
        Keywords.Add(new MinimumKeyword(min));

        return this;
    }

    /// <summary>
    /// Specify that current json number should not be less than <paramref name="min"/>
    /// </summary>
    /// <param name="min"></param>
    /// <returns></returns>
    public NumberKeywordBuilder NotLessThan(ulong min)
    {
        Keywords.Add(new MinimumKeyword(min));

        return this;
    }

    /// <summary>
    /// Specify that current json number should not be less than <paramref name="min"/>
    /// </summary>
    /// <param name="min"></param>
    /// <returns></returns>
    public NumberKeywordBuilder NotLessThan(decimal min)
    {
        Keywords.Add(new MinimumKeyword(min));

        return this;
    }

    /// <summary>
    /// Specify that current json number should not be less than <paramref name="min"/>
    /// </summary>
    /// <param name="min"></param>
    /// <returns></returns>
    public NumberKeywordBuilder NotLessThan(double min)
    {
        Keywords.Add(new MinimumKeyword(min));

        return this;
    }

    /// <summary>
    /// Specify that current json number should be multiple of <paramref name="multipleOf"/>
    /// </summary>
    /// <param name="multipleOf"></param>
    public NumberKeywordBuilder MultipleOf(double multipleOf)
    {
        Keywords.Add(new MultipleOfKeyword(multipleOf));

        return this;
    }

    /// <summary>
    /// Specify that current json number should be multiple of <paramref name="multipleOf"/>
    /// </summary>
    /// <param name="multipleOf"></param>
    public NumberKeywordBuilder MultipleOf(decimal multipleOf)
    {
        Keywords.Add(new MultipleOfKeyword(multipleOf));

        return this;
    }

    /// <summary>
    /// Specify that current json number should be multiple of <paramref name="multipleOf"/>
    /// </summary>
    /// <param name="multipleOf"></param>
    public NumberKeywordBuilder MultipleOf(ulong multipleOf)
    {
        Keywords.Add(new MultipleOfKeyword(multipleOf));

        return this;
    }

    /// <summary>
    /// Specify that current json number should match custom <paramref name="validator"/> and report custom error message when fail to validation
    /// </summary>
    /// <param name="validator">custom validation logic, the input is <see cref="double"/> type</param>
    /// <param name="errorMessageFunc">custom error report, the input is <see cref="double"/> type</param>
    /// <returns></returns>
    public NumberKeywordBuilder HasCustomValidation(Func<double, bool> validator, Func<double, string> errorMessageFunc)
    {
        Keywords.Add(new DoubleNumberCustomValidationKeyword(validator, errorMessageFunc));

        return this;
    }

    /// <summary>
    /// Specify that current json number should match custom <paramref name="validator"/> and report custom error message when fail to validation
    /// </summary>
    /// <param name="validator">custom validation logic, the input is <see cref="long"/> type</param>
    /// <param name="errorMessageFunc">custom error report, the input is <see cref="long"/> type</param>
    /// <returns></returns>
    public NumberKeywordBuilder HasCustomValidation(Func<long, bool> validator, Func<long, string> errorMessageFunc)
    {
        Keywords.Add(new LongNumberCustomValidationKeyword(validator, errorMessageFunc));

        return this;
    }

    /// <summary>
    /// Specify that current json number should match custom <paramref name="validator"/> and report custom error message when fail to validation
    /// </summary>
    /// <param name="validator">custom validation logic, the input is <see cref="ulong"/> type</param>
    /// <param name="errorMessageFunc">custom error report, the input is <see cref="ulong"/> type</param>
    /// <returns></returns>
    public NumberKeywordBuilder HasCustomValidation(Func<ulong, bool> validator, Func<ulong, string> errorMessageFunc)
    {
        Keywords.Add(new ULongNumberCustomValidationKeyword(validator, errorMessageFunc));

        return this;
    }
}