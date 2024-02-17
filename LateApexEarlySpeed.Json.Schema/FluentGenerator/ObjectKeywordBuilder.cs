using System.Text.Json;
using LateApexEarlySpeed.Json.Schema.FluentGenerator.ExtendedKeywords;
using LateApexEarlySpeed.Json.Schema.JInstance;
using LateApexEarlySpeed.Json.Schema.JSchema;
using LateApexEarlySpeed.Json.Schema.Keywords;

namespace LateApexEarlySpeed.Json.Schema.FluentGenerator;

public class ObjectKeywordBuilder : KeywordBuilder
{
    private readonly Dictionary<string, Action<JsonSchemaBuilder>> _propertiesBuilderConfigurations = new();
    private readonly List<string> _requiredProperties = new();
    private readonly List<string> _propertyBlackList = new();

    public ObjectKeywordBuilder() : base(new TypeKeyword(InstanceType.Object))
    {
    }

    public ObjectKeywordBuilder SerializationEquivalent(object obj)
    {
        Keywords.Add(new ConstKeyword(JsonInstanceSerializer.SerializeToElement(obj)));

        return this;
    }

    public ObjectKeywordBuilder HasProperty(string property, Action<JsonSchemaBuilder> configureBuilder)
    {
        _propertiesBuilderConfigurations.Add(property, configureBuilder);
        _requiredProperties.Add(property);

        return this;
    }

    public ObjectKeywordBuilder HasProperty(string property)
    {
        _requiredProperties.Add(property);

        return this;
    }

    public override List<KeywordBase> Build()
    {
        var propertiesKeyword = new PropertiesKeyword(_propertiesBuilderConfigurations
            .ToDictionary<KeyValuePair<string, Action<JsonSchemaBuilder>>, string, JsonSchema>(kv => kv.Key, kv =>
            {
                var jsonSchemaBuilder = new JsonSchemaBuilder();
                kv.Value(jsonSchemaBuilder);

                return jsonSchemaBuilder.Build();
            }));

        var requiredKeyword = new RequiredKeyword(_requiredProperties.ToArray());
        var noPropertiesKeyword = new NoPropertiesKeyword(_propertyBlackList.ToHashSet());

        Keywords.Add(propertiesKeyword);
        Keywords.Add(requiredKeyword);
        Keywords.Add(noPropertiesKeyword);

        return Keywords.ToList();
    }

    public ObjectKeywordBuilder HasCustomValidation<T>(Func<T?, bool> validator, Func<T?, string> errorMessageFunc)
    {
        Keywords.Add(new CustomValidationKeyword<T>(validator, errorMessageFunc));

        return this;
    }

    public ObjectKeywordBuilder HasCustomValidation(Type type, Func<object, bool> validator, Func<object, string> errorMessageFunc)
    {
        Keywords.Add(new ObjectCustomValidationKeyword(type, validator, errorMessageFunc));

        return this;
    }

    public ObjectKeywordBuilder HasCustomValidation(Func<JsonElement, bool> validator, Func<JsonElement, string> errorMessageFunc)
    {
        Keywords.Add(new JsonElementBasedObjectCustomValidationKeyword(validator, errorMessageFunc));

        return this;
    }

    public ObjectKeywordBuilder Equivalent(string jsonText)
    {
        JsonInstanceElement jsonInstanceElement = JsonInstanceSerializer.Deserialize(jsonText);

        Keywords.Add(new ConstKeyword(jsonInstanceElement));

        return this;
    }

    public ObjectKeywordBuilder HasNoProperty(string property)
    {
        _propertyBlackList.Add(property);

        return this;
    }
}