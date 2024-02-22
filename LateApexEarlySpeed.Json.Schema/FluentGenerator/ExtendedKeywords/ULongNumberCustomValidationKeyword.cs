using LateApexEarlySpeed.Json.Schema.JInstance;
using LateApexEarlySpeed.Json.Schema.Keywords;

namespace LateApexEarlySpeed.Json.Schema.FluentGenerator.ExtendedKeywords;

[Keyword("ext-custom-ULongNumberValidation")]
public class ULongNumberCustomValidationKeyword : NumberCustomValidationKeyword<ulong>
{
    public ULongNumberCustomValidationKeyword(Func<ulong, bool> validator, Func<ulong, string> errorMessageFunc) : base(validator, errorMessageFunc)
    {
    }

    protected override bool TryGetNumber(JsonInstanceElement instance, out ulong value)
    {
        return instance.TryGetUInt64(out value);
    }
}