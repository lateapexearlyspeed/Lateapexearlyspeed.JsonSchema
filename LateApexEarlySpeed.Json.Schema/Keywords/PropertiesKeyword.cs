﻿using System.Text.Json;
using System.Text.Json.Serialization;
using LateApexEarlySpeed.Json.Schema.Common;
using LateApexEarlySpeed.Json.Schema.Common.interfaces;
using LateApexEarlySpeed.Json.Schema.JInstance;
using LateApexEarlySpeed.Json.Schema.JSchema;
using LateApexEarlySpeed.Json.Schema.Keywords.JsonConverters;

namespace LateApexEarlySpeed.Json.Schema.Keywords;

[Keyword("properties")]
[JsonConverter(typeof(PropertiesKeywordJsonConverter))]
internal class PropertiesKeyword : KeywordBase, ISchemaContainerElement
{
    public IReadOnlyDictionary<string, JsonSchema> PropertiesSchemas { get; }

    public PropertiesKeyword(IDictionary<string, JsonSchema> propertiesSchemas, bool propertyNameIgnoreCase)
    {
        PropertiesSchemas = new Dictionary<string, JsonSchema>(propertiesSchemas, propertyNameIgnoreCase ? StringComparer.OrdinalIgnoreCase : null);

        foreach (var (propName, schema) in PropertiesSchemas)
        {
            schema.Name = propName;
        }
    }

    protected internal override ValidationResult ValidateCore(JsonInstanceElement instance, JsonSchemaOptions options)
    {
        if (instance.ValueKind != JsonValueKind.Object)
        {
            return ValidationResult.ValidResult;
        }

        foreach (JsonInstanceProperty instanceProperty in instance.EnumerateObject())
        {
            if (PropertiesSchemas.TryGetValue(instanceProperty.Name, out JsonSchema? schema))
            {
                ValidationResult result = schema.Validate(instanceProperty.Value, options);
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
        return PropertiesSchemas.GetValueOrDefault(name);
    }

    public IEnumerable<ISchemaContainerElement> EnumerateElements()
    {
        return PropertiesSchemas.Values;
    }

    public bool IsSchemaType => false;

    public JsonSchema GetSchema()
    {
        throw new InvalidOperationException();
    }

    public bool ContainsPropertyName(string propertyName)
    {
        return PropertiesSchemas.ContainsKey(propertyName);
    }
}