namespace LateApexEarlySpeed.Json.Schema.Generator.SchemaGenerators;

internal interface ISchemaGenerationTypeMatcher
{
    bool CanGenerate(Type typeToConvert);
}