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

    internal override List<KeywordBase> Build()
    {
        List<JsonSchema> bodyJsonSchema = _configureSchemaBuilders.Select(configure =>
        {
            var jsonSchemaBuilder = new JsonSchemaBuilder();
            configure(jsonSchemaBuilder);
            return jsonSchemaBuilder.Build();
        }).ToList<JsonSchema>();

        Keywords.Add(new AnyOfKeyword(bodyJsonSchema));

        return Keywords.ToList();
    }
}