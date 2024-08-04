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
    public PatternPropertiesKeyword(Dictionary<string, JsonSchema> patternSchemas)
    {
        PatternSchemas = new Dictionary<string, JsonSchema>(patternSchemas);
    }

    public IReadOnlyDictionary<string, JsonSchema> PatternSchemas { get; }

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

            foreach (KeyValuePair<string, JsonSchema> patternSchema in PatternSchemas)
            {
                if (RegexMatcher.IsMatch(patternSchema.Key, propertyName, options.RegexMatchTimeout))
                {
                    ValidationResult validationResult = patternSchema.Value.Validate(propertyValue, options);
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
        return PatternSchemas.TryGetValue(name, out JsonSchema schema) 
            ? schema
            : null;
    }

    public IEnumerable<ISchemaContainerElement> EnumerateElements()
    {
        return PatternSchemas.Values;
    }

    public bool IsSchemaType => false;

    public JsonSchema GetSchema()
    {
        throw new InvalidOperationException();
    }

    public bool ContainsMatchedPattern(string propertyName, TimeSpan matchTimeout)
    {
        return PatternSchemas.Any(regexAndSchema => RegexMatcher.IsMatch(regexAndSchema.Key, propertyName, matchTimeout));
    }
}