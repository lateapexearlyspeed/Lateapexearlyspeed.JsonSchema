﻿using System.Text.Json;
using System.Text.Json.Serialization;
using LateApexEarlySpeed.Json.Schema.Common;
using LateApexEarlySpeed.Json.Schema.JInstance;
using LateApexEarlySpeed.Json.Schema.Keywords.JsonConverters;

namespace LateApexEarlySpeed.Json.Schema.Keywords;

[Keyword("uniqueItems")]
[JsonConverter(typeof(UniqueItemsKeywordJsonConverter))]
internal class UniqueItemsKeyword : KeywordBase
{
    private readonly bool _enabled;

    public UniqueItemsKeyword(bool enabled)
    {
        _enabled = enabled;
    }

    protected internal override ValidationResult ValidateCore(JsonInstanceElement instance, JsonSchemaOptions options)
    {
        if (!_enabled || instance.ValueKind != JsonValueKind.Array)
        {
            return ValidationResult.ValidResult;
        }

        JsonInstanceElement[] items = instance.EnumerateArray().ToArray();
        for (int i = 0; i < items.Length; i++)
        {
            JsonInstanceElement curItem = items[i];

            for (int j = i + 1; j < items.Length; j++)
            {
                if (items[j] == curItem)
                {
                    return ValidationResult.CreateFailedResult(ResultCode.DuplicatedArrayItems, ErrorMessage(curItem.ToString(), i, j), options.ValidationPathStack, Name, instance.Location);
                }
            }
        }

        return ValidationResult.ValidResult;
    }

    public static string ErrorMessage(string instanceJson, int idx1, int idx2)
    {
        return $"There are duplicated array items, index: {idx1} and {idx2}, data: {instanceJson}";
    }
}