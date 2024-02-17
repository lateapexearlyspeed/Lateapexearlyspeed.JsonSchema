using LateApexEarlySpeed.Json.Schema.FluentGenerator.ExtendedKeywords;
using LateApexEarlySpeed.Json.Schema.Keywords;

namespace LateApexEarlySpeed.Json.Schema.FluentGenerator;

public class DateTimeKeywordBuilder : KeywordBuilder
{
    private readonly string[]? _formats;

    public DateTimeKeywordBuilder(string[]? formats)
    {
        _formats = formats;

        Keywords.Add(new TypeKeyword(InstanceType.String));
        Keywords.Add(new DateTimeFormatExtensionKeyword(formats));
    }

    public DateTimeKeywordBuilder Equal(DateTime value)
    {
        Keywords.Add(new EqualsDateTimeExtensionKeyword(_formats, value));

        return this;
    }


    public DateTimeKeywordBuilder Before(DateTime before)
    {
        Keywords.Add(new BeforeDateTimeExtensionKeyword(_formats, before));

        return this;
    }

    public DateTimeKeywordBuilder After(DateTime after)
    {
        Keywords.Add(new AfterDateTimeExtensionKeyword(_formats, after));

        return this;
    }

    public DateTimeKeywordBuilder HasCustomValidation(Func<DateTime, bool> validator, Func<DateTime, string> errorMessageFunc)
    {
        Keywords.Add(new DateTimeCustomValidationKeyword(_formats, validator, errorMessageFunc));

        return this;
    }
}