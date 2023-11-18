using System.Text.Json;
using System.Text.Json.Serialization;
using LateApexEarlySpeed.Json.Schema.Common;
using LateApexEarlySpeed.Json.Schema.Common.interfaces;
using LateApexEarlySpeed.Json.Schema.JInstance;
using LateApexEarlySpeed.Json.Schema.JSchema;
using LateApexEarlySpeed.Json.Schema.Keywords.interfaces;
using LateApexEarlySpeed.Json.Schema.Keywords.JsonConverters;

namespace LateApexEarlySpeed.Json.Schema.Keywords;

[Keyword("propertyNames")]
[JsonConverter(typeof(SingleSchemaJsonConverter<PropertyNamesKeyword>))]
internal class PropertyNamesKeyword : KeywordBase, ISchemaContainerElement, ISingleSubSchema
{
    public JsonSchema Schema { get; init; } = null!;

    protected internal override ValidationResult ValidateCore(JsonInstanceElement instance, JsonSchemaOptions options)
    {
        if (instance.ValueKind != JsonValueKind.Object)
        {
            return ValidationResult.ValidResult;
        }

        foreach (JsonInstanceProperty jsonProperty in instance.EnumerateObject())
        {
            ValidationResult validationResult = Schema.Validate(new JsonInstanceElement(JsonSerializer.SerializeToElement(jsonProperty.Name), ImmutableJsonPointer.Empty), options);
            if (!validationResult.IsValid)
            {
                return ValidationResult.CreateFailedResult(ResultCode.InvalidPropertyName, $"Found invalid property name: {jsonProperty.Name}", options.ValidationPathStack, Name, instance.Location);
            }
        }

        return ValidationResult.ValidResult;
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