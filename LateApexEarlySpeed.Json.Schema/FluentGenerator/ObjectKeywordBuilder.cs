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

    /// <summary>
    /// Specify that current json object should be equivalent to serialized result of <paramref name="obj"/>
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public ObjectKeywordBuilder SerializationEquivalent(object obj)
    {
        Keywords.Add(new ConstKeyword(JsonInstanceSerializer.SerializeToElement(obj)));

        return this;
    }

    /// <summary>
    /// Specify that current json object should have specified <paramref name="property"/> and its property value should match schema of <paramref name="configureBuilder"/>
    /// </summary>
    /// <param name="property">Property name</param>
    /// <param name="configureBuilder">Schema for specified <paramref name="property"/>'s value</param>
    /// <returns></returns>
    public ObjectKeywordBuilder HasProperty(string property, Action<JsonSchemaBuilder> configureBuilder)
    {
        _propertiesBuilderConfigurations.Add(property, configureBuilder);
        _requiredProperties.Add(property);

        return this;
    }

    /// <summary>
    /// Specify that current json object should have specified <paramref name="property"/>
    /// </summary>
    /// <param name="property">Property name</param>
    /// <returns></returns>
    public ObjectKeywordBuilder HasProperty(string property)
    {
        _requiredProperties.Add(property);

        return this;
    }

    /// <summary>
    /// Specify that current json object should match custom <paramref name="validator"/> and report custom error message when fail to validation
    /// </summary>
    /// <typeparam name="T">Expected type which should be able to be deserialized from current json object</typeparam>
    /// <param name="validator">custom validation logic, the input type is expected type which can be deserialized from current json object</param>
    /// <param name="errorMessageFunc">custom error report, the input type is expected type which can be deserialized from current json object</param>
    /// <returns></returns>
    public ObjectKeywordBuilder HasCustomValidation<T>(Func<T?, bool> validator, Func<T?, string> errorMessageFunc)
    {
        Keywords.Add(new CustomValidationKeyword<T>(validator, errorMessageFunc));

        return this;
    }

    /// <summary>
    /// Specify that current json object should match custom <paramref name="validator"/> and report custom error message when fail to validation
    /// </summary>
    /// <param name="type">Expected type which should be able to be deserialized from current json object</param>
    /// <param name="validator">custom validation logic, the runtime type of input instance is <paramref name="type"/></param>
    /// <param name="errorMessageFunc">custom error report, the runtime type of input instance is <paramref name="type"/></param>
    /// <returns></returns>
    public ObjectKeywordBuilder HasCustomValidation(Type type, Func<object, bool> validator, Func<object, string> errorMessageFunc)
    {
        Keywords.Add(new ObjectCustomValidationKeyword(type, validator, errorMessageFunc));

        return this;
    }

    /// <summary>
    /// Specify that current json object should match custom <paramref name="validator"/> and report custom error message when fail to validation
    /// </summary>
    /// <param name="validator">custom validation logic, input instance is raw <see cref="JsonElement"/> of current json object</param>
    /// <param name="errorMessageFunc">custom error report, input instance is raw <see cref="JsonElement"/> of current json object</param>
    /// <returns></returns>
    public ObjectKeywordBuilder HasCustomValidation(Func<JsonElement, bool> validator, Func<JsonElement, string> errorMessageFunc)
    {
        Keywords.Add(new JsonElementBasedObjectCustomValidationKeyword(validator, errorMessageFunc));

        return this;
    }

    /// <summary>
    /// Specify that current json object should be equivalent to <paramref name="jsonText"/>
    /// </summary>
    /// <param name="jsonText"></param>
    /// <returns></returns>
    public ObjectKeywordBuilder Equivalent(string jsonText)
    {
        JsonInstanceElement jsonInstanceElement = JsonInstanceSerializer.Deserialize(jsonText);

        Keywords.Add(new ConstKeyword(jsonInstanceElement));

        return this;
    }

    /// <summary>
    /// Specify that current json object should not have <paramref name="property"/>
    /// </summary>
    /// <param name="property">Property name</param>
    /// <returns></returns>
    public ObjectKeywordBuilder HasNoProperty(string property)
    {
        _propertyBlackList.Add(property);

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
}