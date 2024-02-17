using LateApexEarlySpeed.Json.Schema.FluentGenerator.ExtendedKeywords;
using LateApexEarlySpeed.Json.Schema.JInstance;
using LateApexEarlySpeed.Json.Schema.Keywords;

namespace LateApexEarlySpeed.Json.Schema.FluentGenerator;

public class StringKeywordBuilder : KeywordBuilder
{
    public StringKeywordBuilder() : base(new TypeKeyword(InstanceType.String))
    {
    }

    public StringKeywordBuilder Equal(string value)
    {
        Keywords.Add(new ConstKeyword(JsonInstanceSerializer.SerializeToElement(value)));

        return this;
    }

    public StringKeywordBuilder IsIn(IEnumerable<string> collection)
    {
        Keywords.Add(new EnumKeyword(collection.Select(JsonInstanceSerializer.SerializeToElement).ToList()));

        return this;
    }

    public StringKeywordBuilder HasMaxLength(uint max)
    {
        Keywords.Add(new MaxLengthKeyword{BenchmarkValue = max});

        return this;
    }

    public StringKeywordBuilder HasMinLength(uint min)
    {
        Keywords.Add(new MinLengthKeyword { BenchmarkValue = min });

        return this;
    }

    public StringKeywordBuilder HasPattern(string pattern)
    {
        Keywords.Add(new PatternKeyword(pattern));

        return this;
    }

    public StringKeywordBuilder HasCustomValidation(Func<string, bool> validator, Func<string, string> errorMessageFunc)
    {
        Keywords.Add(new StringCustomValidationKeyword(validator, errorMessageFunc));

        return this;
    }
}