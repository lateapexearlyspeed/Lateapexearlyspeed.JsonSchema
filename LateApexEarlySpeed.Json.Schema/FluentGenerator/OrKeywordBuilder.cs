using LateApexEarlySpeed.Json.Schema.JSchema;
using LateApexEarlySpeed.Json.Schema.Keywords;

namespace LateApexEarlySpeed.Json.Schema.FluentGenerator;

public class OrKeywordBuilder : KeywordBuilder
{
    private readonly Action<JsonSchemaBuilder>[] _configureSchemaBuilders;

    public OrKeywordBuilder(Action<JsonSchemaBuilder>[] configureSchemaBuilders)
    {
        _configureSchemaBuilders = configureSchemaBuilders;
    }

    internal override KeywordCollection Build()
    {
        IEnumerable<BodyJsonSchema> bodyJsonSchema = _configureSchemaBuilders.Select(configure =>
        {
            var jsonSchemaBuilder = new JsonSchemaBuilder();
            configure(jsonSchemaBuilder);
            return jsonSchemaBuilder.Build();
        });

        Keywords.Add(new AnyOfKeyword(bodyJsonSchema));

        return new KeywordCollection(Keywords);
    }
}