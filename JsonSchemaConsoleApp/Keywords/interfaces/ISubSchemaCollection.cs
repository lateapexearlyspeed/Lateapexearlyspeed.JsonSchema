namespace JsonSchemaConsoleApp.Keywords.interfaces;

internal interface ISubSchemaCollection
{
    List<JsonSchema> SubSchemas { get; init; }
}