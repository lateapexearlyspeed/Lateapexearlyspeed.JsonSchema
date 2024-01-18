using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using LateApexEarlySpeed.Json.Schema.Common;
using LateApexEarlySpeed.Json.Schema.JInstance;
using LateApexEarlySpeed.Json.Schema.Keywords.JsonConverters;

namespace LateApexEarlySpeed.Json.Schema.Keywords;

[Obfuscation(ApplyToMembers = false)]
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
                    return ValidationResult.CreateFailedResult(ResultCode.DuplicatedArrayItems, ErrorMessage(), options.ValidationPathStack, Name, instance.Location);
                }
            }
        }

        return ValidationResult.ValidResult;
    }

    [Obfuscation]
    public static string ErrorMessage()
    {
        return "There are duplicated array items";
    }
}