using LateApexEarlySpeed.Json.Schema.Generator.TypeAbstraction;
using LateApexEarlySpeed.Json.Schema.JSchema;
using LateApexEarlySpeed.Json.Schema.Keywords;

namespace LateApexEarlySpeed.Json.Schema.Generator.SchemaGenerators;

internal interface ISchemaGenerator
{
    BodyJsonSchema Generate(IType typeToConvert, IEnumerable<KeywordBase> keywordsFromProperty, JsonSchemaGeneratorOptions options);
}