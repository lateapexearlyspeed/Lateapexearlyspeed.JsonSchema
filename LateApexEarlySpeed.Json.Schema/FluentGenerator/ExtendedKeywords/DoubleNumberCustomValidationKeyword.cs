using LateApexEarlySpeed.Json.Schema.JInstance;
using LateApexEarlySpeed.Json.Schema.Keywords;

namespace LateApexEarlySpeed.Json.Schema.FluentGenerator.ExtendedKeywords;

[Keyword("ext-custom-DoubleNumberValidation")]
public class DoubleNumberCustomValidationKeyword : NumberCustomValidationKeyword<double>
{
    public DoubleNumberCustomValidationKeyword(Func<double, bool> validator, Func<double, string> errorMessageFunc) : base(validator, errorMessageFunc)
    {
    }

    protected override bool TryGetNumber(JsonInstanceElement instance, out double value)
    {
        return instance.TryGetDouble(out value);
    }
}