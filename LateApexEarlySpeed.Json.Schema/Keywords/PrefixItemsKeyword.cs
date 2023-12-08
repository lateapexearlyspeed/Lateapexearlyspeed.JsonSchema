using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using LateApexEarlySpeed.Json.Schema.Common;
using LateApexEarlySpeed.Json.Schema.Common.interfaces;
using LateApexEarlySpeed.Json.Schema.JInstance;
using LateApexEarlySpeed.Json.Schema.JSchema;
using LateApexEarlySpeed.Json.Schema.Keywords.interfaces;
using LateApexEarlySpeed.Json.Schema.Keywords.JsonConverters;

namespace LateApexEarlySpeed.Json.Schema.Keywords;

[Keyword("prefixItems")]
[JsonConverter(typeof(SubSchemaCollectionJsonConverter<PrefixItemsKeyword>))]
internal class PrefixItemsKeyword : KeywordBase, ISchemaContainerElement, ISubSchemaCollection
{
    public List<JsonSchema> SubSchemas { get; init; } = null!;

    protected internal override ValidationResult ValidateCore(JsonInstanceElement instance, JsonSchemaOptions options)
    {
        if (instance.ValueKind != JsonValueKind.Array)
        {
            return ValidationResult.ValidResult;
        }

        int schemaIdx = 0;
        foreach (JsonInstanceElement instanceItem in instance.EnumerateArray())
        {
            if (schemaIdx >= SubSchemas.Count)
            {
                break;
            }

            ValidationResult validationResult = SubSchemas[schemaIdx].Validate(instanceItem, options);
            if (!validationResult.IsValid)
            {
                return validationResult;
            }

            schemaIdx++;
        }

        return ValidationResult.ValidResult;
    }

    public ISchemaContainerElement? GetSubElement(string name)
    {
        return ((ISubSchemaCollection)this).GetSubElement(name);
    }

    public IEnumerable<ISchemaContainerElement> EnumerateElements()
    {
        return ((ISubSchemaCollection)this).EnumerateElements();
    }

    public bool IsSchemaType => false;

    public JsonSchema GetSchema()
    {
        throw new InvalidOperationException();
    }
}