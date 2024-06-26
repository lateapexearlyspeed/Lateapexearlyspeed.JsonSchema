﻿using LateApexEarlySpeed.Json.Schema.FluentGenerator.ExtendedKeywords.JsonConverters;
using LateApexEarlySpeed.Json.Schema.JInstance;
using LateApexEarlySpeed.Json.Schema.Keywords;
using System.Text.Json.Serialization;

namespace LateApexEarlySpeed.Json.Schema.FluentGenerator.ExtendedKeywords;

[Keyword("ext-custom-LongNumberValidation")]
[JsonConverter(typeof(ExtendedKeywordJsonConverter))]
public class LongNumberCustomValidationKeyword : NumberCustomValidationKeyword<long>
{
    public LongNumberCustomValidationKeyword(Func<long, bool> validator, Func<long, string> errorMessageFunc) : base(validator, errorMessageFunc)
    {
    }

    protected override bool TryGetNumber(JsonInstanceElement instance, out long value)
    {
        return instance.TryGetInt64(out value);
    }
}