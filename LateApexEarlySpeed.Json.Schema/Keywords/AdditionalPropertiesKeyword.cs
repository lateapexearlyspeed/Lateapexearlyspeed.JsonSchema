using System.Text.Json;
using System.Text.Json.Serialization;
using LateApexEarlySpeed.Json.Schema.Common;
using LateApexEarlySpeed.Json.Schema.Common.interfaces;
using LateApexEarlySpeed.Json.Schema.JSchema;
using LateApexEarlySpeed.Json.Schema.Keywords.interfaces;
using LateApexEarlySpeed.Json.Schema.Keywords.JsonConverters;

namespace LateApexEarlySpeed.Json.Schema.Keywords;

[Keyword("additionalProperties")]
[JsonConverter(typeof(SingleSchemaJsonConverter<AdditionalPropertiesKeyword>))]
internal class AdditionalPropertiesKeyword : KeywordBase, ISchemaContainerElement, ISingleSubSchema
{
    public JsonSchema Schema { get; init; } = null!;

    public PropertiesKeyword? PropertiesKeyword { get; set; }

    public PatternPropertiesKeyword? PatternPropertiesKeyword { get; set; }

    protected internal override ValidationResult ValidateCore(JsonElement instance, JsonSchemaOptions options)
    {
        if (instance.ValueKind != JsonValueKind.Object)
        {
            return ValidationResult.ValidResult;
        }

        foreach (JsonProperty jsonProperty in instance.EnumerateObject())
        {
            string propertyName = jsonProperty.Name;

            bool containsInPropertiesKeyword = PropertiesKeyword is not null && PropertiesKeyword.ContainsPropertyName(propertyName);

            if (containsInPropertiesKeyword)
            {
                continue;
            }

            bool containsMatchedPattern = PatternPropertiesKeyword is not null && PatternPropertiesKeyword.ContainsMatchedPattern(propertyName);
            
            if (!containsMatchedPattern)
            {
                ValidationResult validationResult = Schema.Validate(jsonProperty.Value, options);
                if (!validationResult.IsValid)
                {
                    return validationResult;
                }
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