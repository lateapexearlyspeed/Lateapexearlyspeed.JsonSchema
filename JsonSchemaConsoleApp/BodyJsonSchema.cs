using System.Text.Json;
using System.Text.Json.Serialization;
using JsonSchemaConsoleApp.JsonConverters;
using JsonSchemaConsoleApp.Keywords;
using JsonSchemaConsoleApp.Keywords.interfaces;

namespace JsonSchemaConsoleApp;

internal class BodyJsonSchema : JsonSchema
{
    private readonly List<ValidationNode> _keywords;

    private readonly ConditionalValidator _conditionalValidator;

    public SchemaReference? SchemaReference { get; }
    
    public SchemaDynamicReference? SchemaDynamicReference { get; }

    public string? Anchor { get; }

    public string? DynamicAnchor { get; }

    public BodyJsonSchema(List<ValidationNode> keywords, ConditionalValidator conditionalValidator, SchemaReference? schemaReference, SchemaDynamicReference? schemaDynamicReference, string? anchor, string? dynamicAnchor)
    {
        _keywords = keywords;
        _conditionalValidator = conditionalValidator;
        SchemaReference = schemaReference;
        SchemaDynamicReference = schemaDynamicReference;
        Anchor = anchor;
        DynamicAnchor = dynamicAnchor;
    }

    protected internal override ValidationResult ValidateCore(JsonElement instance, JsonSchemaOptions options)
    {
        if (SchemaReference is not null)
        {
            ValidationResult result = SchemaReference.Validate(instance, options);
            if (!result.IsValid)
            {
                return result;
            }
        }

        if (SchemaDynamicReference is not null)
        {
            ValidationResult result = SchemaDynamicReference.Validate(instance, options);
            if (!result.IsValid)
            {
                return result;
            }
        }

        foreach (ValidationNode keyword in _keywords)
        {
            ValidationResult result = keyword.Validate(instance, options);
            if (!result.IsValid)
            {
                return result;
            }
        }

        return _conditionalValidator.Validate(instance, options);
    }

    public override ISchemaContainerElement? GetSubElement(string name)
    {
        foreach (ValidationNode keyword in _keywords)
        {
            if (keyword.Name == name && keyword is ISchemaContainerElement schemaContainerElement)
            {
                return schemaContainerElement;
            }
        }

        JsonSchema? element = _conditionalValidator.GetSubElement(name);
        if (element is not null)
        {
            return element;
        }

        return null;
    }

    public override IEnumerable<ISchemaContainerElement> EnumerateElements()
    {
        foreach (ValidationNode validationKeyword in _keywords)
        {
            if (validationKeyword is ISchemaContainerElement element)
            {
                yield return element;
            }
        }

        if (_conditionalValidator.PredictEvaluator is not null)
        {
            yield return _conditionalValidator.PredictEvaluator;
        }

        if (_conditionalValidator.PositiveValidator is not null)
        {
            yield return _conditionalValidator.PositiveValidator;
        }

        if (_conditionalValidator.NegativeValidator is not null)
        {
            yield return _conditionalValidator.NegativeValidator;
        }
    }
}