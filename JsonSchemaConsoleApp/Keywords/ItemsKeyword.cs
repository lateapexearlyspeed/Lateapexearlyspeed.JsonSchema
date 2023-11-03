using System.Text.Json;
using System.Text.Json.Serialization;
using JsonSchemaConsoleApp.JsonConverters;
using JsonSchemaConsoleApp.Keywords.interfaces;

namespace JsonSchemaConsoleApp.Keywords;

[Keyword("items")]
[JsonConverter(typeof(SingleSchemaJsonConverter<ItemsKeyword>))]
internal class ItemsKeyword : KeywordBase, ISchemaContainerElement, ISingleSubSchema
{
    public JsonSchema Schema { get; init; } = null!;

    public PrefixItemsKeyword? PrefixItemsKeyword { get; set; }

    protected internal override ValidationResult ValidateCore(JsonElement instance, JsonSchemaOptions options)
    {
        if (instance.ValueKind != JsonValueKind.Array)
        {
            return ValidationResult.ValidResult;
        }

        int idx = 0;
        foreach (JsonElement instanceItem in instance.EnumerateArray())
        {
            if (PrefixItemsKeyword is not null && idx < PrefixItemsKeyword.SubSchemas.Count)
            {
                idx++;
                continue;
            }

            ValidationResult validationResult = Schema.Validate(instanceItem, options);
            if (!validationResult.IsValid)
            {
                return validationResult;
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