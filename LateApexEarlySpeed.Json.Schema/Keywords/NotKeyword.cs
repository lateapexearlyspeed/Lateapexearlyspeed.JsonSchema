using System.Text.Json.Serialization;
using LateApexEarlySpeed.Json.Schema.Common;
using LateApexEarlySpeed.Json.Schema.Common.interfaces;
using LateApexEarlySpeed.Json.Schema.JInstance;
using LateApexEarlySpeed.Json.Schema.JSchema;
using LateApexEarlySpeed.Json.Schema.Keywords.interfaces;
using LateApexEarlySpeed.Json.Schema.Keywords.JsonConverters;

namespace LateApexEarlySpeed.Json.Schema.Keywords;

[Keyword("not")]
[JsonConverter(typeof(SingleSchemaJsonConverter<NotKeyword>))]
internal class NotKeyword : KeywordBase, ISchemaContainerElement, ISingleSubSchema
{
    public JsonSchema Schema { get; init; } = null!;

    protected internal override ValidationResult ValidateCore(JsonInstanceElement instance, JsonSchemaOptions options)
    {
        return Schema.Validate(instance, options).IsValid 
            ? ValidationResult.SingleErrorFailedResult(new ValidationError(ResultCode.SubSchemaPassedUnexpected, ErrorMessage(instance.ToString()), options.ValidationPathStack, Name, instance.Location))
            : ValidationResult.ValidResult;
    }

    public static string ErrorMessage(string instanceText)
    {
        return $"Instance is validated by subSchema which is not allowed, instance data: '{instanceText}'";
    }

    public ISchemaContainerElement? GetSubElement(string name)
    {
        return Schema.GetSubElement(name);
    }

    public IEnumerable<ISchemaContainerElement> EnumerateElements()
    {
        yield return Schema;
    }

    public bool IsSchemaType => true;

    public JsonSchema GetSchema()
    {
        return Schema;
    }
}