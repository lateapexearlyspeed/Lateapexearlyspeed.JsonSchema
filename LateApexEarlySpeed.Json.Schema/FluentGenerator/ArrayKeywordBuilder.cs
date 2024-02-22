using System.Text.Json;
using LateApexEarlySpeed.Json.Schema.FluentGenerator.ExtendedKeywords;
using LateApexEarlySpeed.Json.Schema.JInstance;
using LateApexEarlySpeed.Json.Schema.Keywords;

namespace LateApexEarlySpeed.Json.Schema.FluentGenerator;

public class ArrayKeywordBuilder : KeywordBuilder
{
    private Action<JsonSchemaBuilder>? _itemsSchemaBuilderConfiguration;
    private Action<JsonSchemaBuilder>? _containsSchemaBuilderConfiguration;
    private Action<JsonSchemaBuilder>? _notContainsSchemaBuilderConfiguration;

    public ArrayKeywordBuilder() : base(new TypeKeyword(InstanceType.Array))
    {
    }

    public ArrayKeywordBuilder SerializationEquivalent(object?[] collection)
    {
        Keywords.Add(new ConstKeyword(JsonInstanceSerializer.SerializeToElement(collection)));

        return this;
    }

    public ArrayKeywordBuilder SerializationEquivalent<TItem>(TItem[] collection)
    {
        Keywords.Add(new ConstKeyword(JsonInstanceSerializer.SerializeToElement(collection)));

        return this;
    }

    public ArrayKeywordBuilder HasItems(Action<JsonSchemaBuilder> configureBuilder)
    {
        _itemsSchemaBuilderConfiguration = configureBuilder;

        return this;
    }

    public ArrayKeywordBuilder HasLength(uint length)
    {
        Keywords.AddRange(new KeywordBase[]
        {
            new MinItemsKeyword { BenchmarkValue = length },
            new MaxItemsKeyword {BenchmarkValue = length}
        });

        return this;
    }

    public ArrayKeywordBuilder HasMaxLength(uint max)
    {
        Keywords.Add(new MaxItemsKeyword{BenchmarkValue = max});

        return this;
    }

    public ArrayKeywordBuilder HasMinLength(uint min)
    {
        Keywords.Add(new MinItemsKeyword { BenchmarkValue = min });

        return this;
    }

    public ArrayKeywordBuilder HasUniqueItems()
    {
        Keywords.Add(new UniqueItemsKeyword(true));

        return this;
    }

    public ArrayKeywordBuilder HasCustomValidation<TElement>(Func<TElement[]?, bool> validator, Func<TElement[]?, string> errorMessageFunc)
    {
        Keywords.Add(new CustomValidationKeyword<TElement[]>(validator, errorMessageFunc));

        return this;
    }

    public ArrayKeywordBuilder HasCustomValidation(Func<JsonElement, bool> validator, Func<JsonElement, string> errorMessageFunc)
    {
        Keywords.Add(new JsonElementBasedObjectCustomValidationKeyword(validator, errorMessageFunc));

        return this;
    }

    public ArrayKeywordBuilder Contains(Action<JsonSchemaBuilder> configureBuilder)
    {
        _containsSchemaBuilderConfiguration = configureBuilder;

        return this;
    }

    public ArrayKeywordBuilder NotContains(Action<JsonSchemaBuilder> configureBuilder)
    {
        _notContainsSchemaBuilderConfiguration = configureBuilder;

        return this;
    }

    public ArrayKeywordBuilder Equivalent(string jsonText)
    {
        JsonInstanceElement jsonInstanceElement = JsonInstanceSerializer.Deserialize(jsonText);

        Keywords.Add(new ConstKeyword(jsonInstanceElement));

        return this;
    }

    public override List<KeywordBase> Build()
    {
        if (_itemsSchemaBuilderConfiguration is not null)
        {
            var jsonSchemaBuilder = new JsonSchemaBuilder();
            _itemsSchemaBuilderConfiguration(jsonSchemaBuilder);

            Keywords.Add(new ItemsKeyword {Schema = jsonSchemaBuilder.Build() });
        }

        if (_containsSchemaBuilderConfiguration is not null)
        {
            var jsonSchemaBuilder = new JsonSchemaBuilder();
            _containsSchemaBuilderConfiguration(jsonSchemaBuilder);

            Keywords.Add(new ContainsKeyword(jsonSchemaBuilder.Build()));
        }

        if (_notContainsSchemaBuilderConfiguration is not null)
        {
            var jsonSchemaBuilder = new JsonSchemaBuilder();
            _notContainsSchemaBuilderConfiguration(jsonSchemaBuilder);

            Keywords.Add(new NotContainsKeyword(jsonSchemaBuilder.Build()));
        }

        return Keywords.ToList();
    }
}