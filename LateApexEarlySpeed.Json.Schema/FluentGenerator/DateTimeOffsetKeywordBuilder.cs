using LateApexEarlySpeed.Json.Schema.FluentGenerator.ExtendedKeywords;
using LateApexEarlySpeed.Json.Schema.Keywords;

namespace LateApexEarlySpeed.Json.Schema.FluentGenerator;

public class DateTimeOffsetKeywordBuilder : KeywordBuilder
{
    private readonly string[]? _formats;

    public DateTimeOffsetKeywordBuilder(string[]? formats)
    {
        _formats = formats;

        Keywords.Add(new TypeKeyword(InstanceType.String));
        Keywords.Add(new DateTimeOffsetFormatExtensionKeyword(formats));
    }

    public DateTimeOffsetKeywordBuilder Before(DateTimeOffset timePoint)
    {
        Keywords.Add(new BeforeDateTimeOffsetExtensionKeyword(_formats, timePoint));

        return this;
    }

    public DateTimeOffsetKeywordBuilder After(DateTimeOffset timePoint)
    {
        Keywords.Add(new AfterDateTimeOffsetExtensionKeyword(_formats, timePoint));

        return this;
    }

    public DateTimeOffsetKeywordBuilder Equal(DateTimeOffset timePoint)
    {
        Keywords.Add(new EqualsDateTimeOffsetExtensionKeyword(_formats, timePoint));

        return this;
    }

    public DateTimeOffsetKeywordBuilder HasCustomValidation(Func<DateTimeOffset, bool> validator, Func<DateTimeOffset, string> errorMessageFunc)
    {
        Keywords.Add(new DateTimeOffsetCustomValidationKeyword(_formats, validator, errorMessageFunc));

        return this;
    }
}