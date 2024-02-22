using LateApexEarlySpeed.Json.Schema.FluentGenerator.ExtendedKeywords;
using LateApexEarlySpeed.Json.Schema.JInstance;
using LateApexEarlySpeed.Json.Schema.Keywords;

namespace LateApexEarlySpeed.Json.Schema.FluentGenerator;

public class NumberKeywordBuilder : KeywordBuilder
{
    public NumberKeywordBuilder() : base(new TypeKeyword(InstanceType.Number))
    {
    }

    public NumberKeywordBuilder Equal(double value)
    {
        return Equal<double>(value);
    }

    public NumberKeywordBuilder Equal(long value)
    {
        return Equal<long>(value);
    }

    public NumberKeywordBuilder Equal(ulong value)
    {
        return Equal<ulong>(value);
    }

    private NumberKeywordBuilder Equal<T>(T value) where T : unmanaged
    {
        Keywords.Add(new ConstKeyword(JsonInstanceSerializer.SerializeToElement(value)));

        return this;
    }

    public NumberKeywordBuilder IsIn(double[] collection)
    {
        return IsIn<double>(collection);
    }

    public NumberKeywordBuilder IsIn(long[] collection)
    {
        return IsIn<long>(collection);
    }

    private NumberKeywordBuilder IsIn<T>(IEnumerable<T> collection) where T : unmanaged
    {
        Keywords.Add(new EnumKeyword(collection.Select(item => JsonInstanceSerializer.SerializeToElement(item)).ToList()));

        return this;
    }

    public NumberKeywordBuilder IsGreaterThan(double min)
    {
        Keywords.Add(new ExclusiveMinimumKeyword(min));

        return this;
    }

    public NumberKeywordBuilder IsLessThan(double max)
    {
        Keywords.Add(new ExclusiveMaximumKeyword(max));

        return this;
    }

    public NumberKeywordBuilder IsGreaterThan(long min)
    {
        Keywords.Add(new ExclusiveMinimumKeyword(min));

        return this;
    }

    public NumberKeywordBuilder IsLessThan(long max)
    {
        Keywords.Add(new ExclusiveMaximumKeyword(max));

        return this;
    }

    public NumberKeywordBuilder NotGreaterThan(double max)
    {
        Keywords.Add(new MaximumKeyword(max));

        return this;
    }

    public NumberKeywordBuilder NotLessThan(double min)
    {
        Keywords.Add(new MinimumKeyword(min));

        return this;
    }

    public NumberKeywordBuilder NotGreaterThan(long max)
    {
        Keywords.Add(new MaximumKeyword(max));

        return this;
    }

    public NumberKeywordBuilder NotLessThan(long min)
    {
        Keywords.Add(new MinimumKeyword(min));

        return this;
    }

    public NumberKeywordBuilder MultipleOf(double multipleOf)
    {
        Keywords.Add(new MultipleOfKeyword(multipleOf));

        return this;
    }

    public NumberKeywordBuilder HasCustomValidation(Func<double, bool> validator, Func<double, string> errorMessageFunc)
    {
        Keywords.Add(new DoubleNumberCustomValidationKeyword(validator, errorMessageFunc));

        return this;
    }

    public NumberKeywordBuilder HasCustomValidation(Func<long, bool> validator, Func<long, string> errorMessageFunc)
    {
        Keywords.Add(new LongNumberCustomValidationKeyword(validator, errorMessageFunc));

        return this;
    }

    public NumberKeywordBuilder HasCustomValidation(Func<ulong, bool> validator, Func<ulong, string> errorMessageFunc)
    {
        Keywords.Add(new ULongNumberCustomValidationKeyword(validator, errorMessageFunc));

        return this;
    }
}