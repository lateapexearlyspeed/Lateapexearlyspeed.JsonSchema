using System.Text.Json;
using System.Text.Json.Serialization;
using JsonSchemaConsoleApp.JsonConverters;
using JsonSchemaConsoleApp.Keywords.interfaces;

namespace JsonSchemaConsoleApp.Keywords;

[Keyword("dependentSchemas")]
[JsonConverter(typeof(DependentSchemasKeywordJsonConverter))]
internal class DependentSchemasKeyword : KeywordBase, ISchemaContainerElement
{
    public Dictionary<string, JsonSchema> DependentSchemas { get; init; } = null!;

    protected internal override ValidationResult ValidateCore(JsonElement instance, JsonSchemaOptions options)
    {
        if (instance.ValueKind != JsonValueKind.Object)
        {
            return ValidationResult.ValidResult;
        }

        foreach (JsonProperty instanceProperty in instance.EnumerateObject())
        {
            if (DependentSchemas.TryGetValue(instanceProperty.Name, out JsonSchema? subSchema))
            {
                ValidationResult result = subSchema.Validate(instance, options);
                if (!result.IsValid)
                {
                    return result;
                }
            }
        }

        return ValidationResult.ValidResult;
    }

    public ISchemaContainerElement? GetSubElement(string name)
    {
        return DependentSchemas.GetValueOrDefault(name);
    }

    public IEnumerable<ISchemaContainerElement> EnumerateElements()
    {
        return DependentSchemas.Values;
    }

    public bool IsSchemaType => false;

    public JsonSchema GetSchema()
    {
        throw new InvalidOperationException();
    }
}