using System.Text.Json;
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
            ? ValidationResult.CreateFailedResult(ResultCode.SubSchemaPassedUnexpected, "Instance is validated by subSchema which is not allowed", options.ValidationPathStack, Name, instance.Location)
            : ValidationResult.ValidResult;
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