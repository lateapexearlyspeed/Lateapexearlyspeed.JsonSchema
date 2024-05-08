using System.Text.Json;
using LateApexEarlySpeed.Json.Schema.FluentGenerator.ExtendedKeywords;
using LateApexEarlySpeed.Json.Schema.JInstance;
using LateApexEarlySpeed.Json.Schema.JSchema;
using LateApexEarlySpeed.Json.Schema.Keywords;

namespace LateApexEarlySpeed.Json.Schema.FluentGenerator;

public class ArrayKeywordBuilder : KeywordBuilder
{
    private ArrayContainsValidator? _arrayContainsValidator;
    private readonly List<JsonSchema> _schemas = new();

    public ArrayKeywordBuilder() : base(new TypeKeyword(InstanceType.Array))
    {
    }

    /// <summary>
    /// Specify that current json array should be equivalent to serialized result of <paramref name="collection"/>, the types of element of <paramref name="collection"/> can be different
    /// </summary>
    /// <param name="collection"></param>
    /// <returns></returns>
    public ArrayKeywordBuilder SerializationEquivalent(object?[] collection)
    {
        Keywords.Add(new ConstKeyword(JsonInstanceSerializer.SerializeToElement(collection)));

        return this;
    }

    /// <summary>
    /// Specify that current json array should be equivalent to serialized result of <paramref name="collection"/>
    /// </summary>
    /// <typeparam name="TItem">Element type of <paramref name="collection"/></typeparam>
    /// <param name="collection"></param>
    /// <returns></returns>
    public ArrayKeywordBuilder SerializationEquivalent<TItem>(TItem[] collection)
    {
        Keywords.Add(new ConstKeyword(JsonInstanceSerializer.SerializeToElement(collection)));

        return this;
    }

    /// <summary>
    /// Specify that all elements of current json array should match schema constraint of <paramref name="configureBuilder"/>
    /// </summary>
    /// <param name="configureBuilder">Configuration to specify schema metadata</param>
    /// <returns></returns>
    public ArrayKeywordBuilder HasItems(Action<JsonSchemaBuilder> configureBuilder)
    {
        var jsonSchemaBuilder = new JsonSchemaBuilder();
        configureBuilder(jsonSchemaBuilder);

        Keywords.Add(new ItemsKeyword { Schema = jsonSchemaBuilder.Build() });

        return this;
    }

    /// <summary>
    /// Specify that current json array should contain exactly a given number of elements, which should match schema constraints provided by <paramref name="elementInspectors"/>
    /// </summary>
    /// <param name="elementInspectors">The element inspectors, which specify each element in turn. The total number of element inspectors must exactly match the number of elements in current json array</param>
    /// <returns></returns>
    public ArrayKeywordBuilder HasCollection(params Action<JsonSchemaBuilder>[] elementInspectors)
    {
        var schemaBuilder = new JsonSchemaBuilder();
        List<JsonSchema> subSchemas = elementInspectors.Select<Action<JsonSchemaBuilder>, JsonSchema>(inspector =>
        {
            var builder = new JsonSchemaBuilder();
            inspector(builder);

            return builder.Build();
        }).ToList();
        var prefixItemsKeyword = new PrefixItemsKeyword(subSchemas);
        schemaBuilder.IsJsonArray().HasLength((uint)elementInspectors.Length).Keywords.Add(prefixItemsKeyword);

        _schemas.Add(schemaBuilder.Build());

        return this;
    }

    /// <summary>
    /// Specify that current json array should have length of <paramref name="length"/>
    /// </summary>
    /// <param name="length"></param>
    /// <returns></returns>
    public ArrayKeywordBuilder HasLength(uint length)
    {
        Keywords.AddRange(new KeywordBase[]
        {
            new MinItemsKeyword { BenchmarkValue = length },
            new MaxItemsKeyword {BenchmarkValue = length}
        });

        return this;
    }

    /// <summary>
    /// Specify that current json array should have max length of <paramref name="max"/>
    /// </summary>
    /// <param name="max"></param>
    /// <returns></returns>
    public ArrayKeywordBuilder HasMaxLength(uint max)
    {
        Keywords.Add(new MaxItemsKeyword{BenchmarkValue = max});

        return this;
    }

    /// <summary>
    /// Specify that current json array should have min length of <paramref name="min"/>
    /// </summary>
    /// <param name="min"></param>
    /// <returns></returns>
    public ArrayKeywordBuilder HasMinLength(uint min)
    {
        Keywords.Add(new MinItemsKeyword { BenchmarkValue = min });

        return this;
    }

    /// <summary>
    /// Specify that current json array should have no duplicated elements, elements will be compared with 'json-equivalent' manner
    /// </summary>
    /// <returns></returns>
    public ArrayKeywordBuilder HasUniqueItems()
    {
        Keywords.Add(new UniqueItemsKeyword(true));

        return this;
    }

    /// <summary>
    /// Specify that current json array should match custom <paramref name="validator"/> and report custom error message when fail to validation
    /// </summary>
    /// <param name="validator">custom validation logic, the input type is array of <typeparamref name="TElement"/></param>
    /// <param name="errorMessageFunc">custom error report, the input type is array of <typeparamref name="TElement"/></param>
    /// <returns></returns>
    public ArrayKeywordBuilder HasCustomValidation<TElement>(Func<TElement[]?, bool> validator, Func<TElement[]?, string> errorMessageFunc)
    {
        Keywords.Add(new CustomValidationKeyword<TElement[]>(validator, errorMessageFunc));

        return this;
    }

    /// <summary>
    /// Specify that current json array should match custom <paramref name="validator"/> and report custom error message when fail to validation
    /// </summary>
    /// <param name="validator">custom validation logic, the input is raw <see cref="JsonElement"/> of current json array</param>
    /// <param name="errorMessageFunc">custom error report, the input type is raw <see cref="JsonElement"/> of current json array</param>
    /// <returns></returns>
    public ArrayKeywordBuilder HasCustomValidation(Func<JsonElement, bool> validator, Func<JsonElement, string> errorMessageFunc)
    {
        Keywords.Add(new JsonElementBasedObjectCustomValidationKeyword(validator, errorMessageFunc));

        return this;
    }

    /// <summary>
    /// Specify that current json array should contain any of element matching specified schema constraint from <paramref name="configureBuilder"/>
    /// </summary>
    /// <param name="configureBuilder">Schema metadata configuration for array element</param>
    /// <returns></returns>
    public ArrayKeywordBuilder Contains(Action<JsonSchemaBuilder> configureBuilder)
    {
        var jsonSchemaBuilder = new JsonSchemaBuilder();
        configureBuilder(jsonSchemaBuilder);

        _arrayContainsValidator = new ArrayContainsValidator(jsonSchemaBuilder.Build(), null, null);

        return this;
    }

    /// <summary>
    /// Specify that current json array should not contain any of element matching specified schema constraint from <paramref name="configureBuilder"/>
    /// </summary>
    /// <param name="configureBuilder">Schema metadata configuration for array element</param>
    /// <returns></returns>
    public ArrayKeywordBuilder NotContains(Action<JsonSchemaBuilder> configureBuilder)
    {
        var jsonSchemaBuilder = new JsonSchemaBuilder();
        configureBuilder(jsonSchemaBuilder);

        Keywords.Add(new NotContainsKeyword(jsonSchemaBuilder.Build()));

        return this;
    }

    /// <summary>
    /// Specify that current json array should be equivalent to <paramref name="jsonText"/>
    /// </summary>
    /// <param name="jsonText"></param>
    /// <returns></returns>
    public ArrayKeywordBuilder Equivalent(string jsonText)
    {
        JsonInstanceElement jsonInstanceElement = JsonInstanceSerializer.Deserialize(jsonText);

        Keywords.Add(new ConstKeyword(jsonInstanceElement));

        return this;
    }

    internal override KeywordCollection Build()
    {
        if (_schemas.Count != 0)
        {
            Keywords.Add(new AllOfKeyword(_schemas));
        }

        return new KeywordCollection(Keywords.ToList(), _arrayContainsValidator);
    }
}