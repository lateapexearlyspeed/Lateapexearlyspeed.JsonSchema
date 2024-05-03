using LateApexEarlySpeed.Json.Schema.FluentGenerator.ExtendedKeywords.JsonConverters;
using LateApexEarlySpeed.Json.Schema.JInstance;
using LateApexEarlySpeed.Json.Schema.Keywords;
using System.Text.Json.Serialization;

namespace LateApexEarlySpeed.Json.Schema.FluentGenerator.ExtendedKeywords;

[Keyword("ext-custom-ULongNumberValidation")]
[JsonConverter(typeof(ExtendedKeywordJsonConverter))]
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