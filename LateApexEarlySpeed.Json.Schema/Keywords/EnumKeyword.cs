﻿using System.Text.Json.Serialization;
using LateApexEarlySpeed.Json.Schema.Common;
using LateApexEarlySpeed.Json.Schema.JInstance;
using LateApexEarlySpeed.Json.Schema.Keywords.JsonConverters;

namespace LateApexEarlySpeed.Json.Schema.Keywords;

[Keyword("enum")]
[JsonConverter(typeof(EnumKeywordJsonConverter))]
internal class EnumKeyword : KeywordBase
{
    public List<JsonInstanceElement> EnumList { get; }

    public EnumKeyword(List<JsonInstanceElement> enumList)
    {
        EnumList = enumList;
    }

    protected internal override ValidationResult ValidateCore(JsonInstanceElement instance, JsonSchemaOptions options)
    {
        return EnumList.Contains(instance)
            ? ValidationResult.ValidResult
            : ValidationResult.CreateFailedResult(ResultCode.NotFoundInAllowedList, ErrorMessage(instance.ToString()), options.ValidationPathStack, Name, instance.Location);
    }

    internal static string ErrorMessage(string instanceJsonText)
    {
        return $"Instance: {instanceJsonText} not found in allowed list";
    }
}