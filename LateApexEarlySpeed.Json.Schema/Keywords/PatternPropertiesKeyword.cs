using System.Text.Json;
using System.Text.Json.Serialization;
using LateApexEarlySpeed.Json.Schema.Common;
using LateApexEarlySpeed.Json.Schema.Common.interfaces;
using LateApexEarlySpeed.Json.Schema.JInstance;
using LateApexEarlySpeed.Json.Schema.JSchema;
using LateApexEarlySpeed.Json.Schema.Keywords.JsonConverters;

namespace LateApexEarlySpeed.Json.Schema.Keywords;

[Keyword("patternProperties")]
[JsonConverter(typeof(PatternPropertiesKeywordJsonConverter))]
internal class PatternPropertiesKeyword : KeywordBase, ISchemaContainerElement
{
    private readonly Dictionary<string, (LazyCompiledRegex regex, JsonSchema schema)> _patternSchemas;

    public PatternPropertiesKeyword(Dictionary<string, JsonSchema> patternSchemas)
    {
        _patternSchemas = patternSchemas.ToDictionary(
            kv => kv.Key, 
            kv => (new LazyCompiledRegex(kv.Key), kv.Value));
    }

    public Dictionary<string, JsonSchema> PatternSchemas => _patternSchemas.ToDictionary(kv => kv.Key, kv => kv.Value.schema);

    protected internal override ValidationResult ValidateCore(JsonInstanceElement instance, JsonSchemaOptions options)
    {
        if (instance.ValueKind != JsonValueKind.Object)
        {
            return ValidationResult.ValidResult;
        }

        foreach (JsonInstanceProperty jsonProperty in instance.EnumerateObject())
        {
            string propertyName = jsonProperty.Name;
            JsonInstanceElement propertyValue = jsonProperty.Value;

            foreach ((LazyCompiledRegex regex, JsonSchema schema) patternSchema in _patternSchemas.Values)
            {
                if (patternSchema.regex.IsMatch(propertyName))
                {
                    ValidationResult validationResult = patternSchema.schema.Validate(propertyValue, options);
                    if (!validationResult.IsValid)
                    {
                        return validationResult;
                    }
                }
            }
        }

        return ValidationResult.ValidResult;
    }

    public ISchemaContainerElement? GetSubElement(string name)
    {
        return _patternSchemas.TryGetValue(name, out (LazyCompiledRegex regex, JsonSchema schema) regexAndSchema) 
            ? regexAndSchema.schema 
            : null;
    }

    public IEnumerable<ISchemaContainerElement> EnumerateElements()
    {
        return _patternSchemas.Values.Select(regexAndSchema => regexAndSchema.schema);
    }

    public bool IsSchemaType => false;

    public JsonSchema GetSchema()
    {
        throw new InvalidOperationException();
    }

    public bool ContainsMatchedPattern(string propertyName)
    {
        return _patternSchemas.Values.Any(regexAndSchema => regexAndSchema.regex.IsMatch(propertyName));
    }
}