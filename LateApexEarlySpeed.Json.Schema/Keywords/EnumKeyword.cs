using System.Reflection;
using System.Text.Json.Serialization;
using LateApexEarlySpeed.Json.Schema.Common;
using LateApexEarlySpeed.Json.Schema.JInstance;
using LateApexEarlySpeed.Json.Schema.Keywords.JsonConverters;

namespace LateApexEarlySpeed.Json.Schema.Keywords;

[Obfuscation(ApplyToMembers = false)]
[Keyword("enum")]
[JsonConverter(typeof(EnumKeywordJsonConverter))]
internal class EnumKeyword : KeywordBase
{
    private readonly List<JsonInstanceElement> _enumList;


    public EnumKeyword(List<JsonInstanceElement> enumList)
    {
        _enumList = enumList;
    }

    protected internal override ValidationResult ValidateCore(JsonInstanceElement instance, JsonSchemaOptions options)
    {
        return _enumList.Contains(instance)
            ? ValidationResult.ValidResult
            : ValidationResult.CreateFailedResult(ResultCode.NotFoundInAllowedList, ErrorMessage(), options.ValidationPathStack, Name, instance.Location);
    }

    [Obfuscation]
    public static string ErrorMessage()
    {
        return "Not found in allowed list";
    }
}