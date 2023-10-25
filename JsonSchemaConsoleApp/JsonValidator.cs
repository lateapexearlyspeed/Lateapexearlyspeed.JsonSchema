using System.Text.Json;
using JsonSchemaConsoleApp.Keywords;

namespace JsonSchemaConsoleApp;

internal class JsonValidator
{
    private readonly IJsonSchemaDocument _schemaDoc;

    private readonly SchemaResourceRegistry _globalSchemaResourceRegistry = new();

    public JsonValidator(string jsonSchema)
    {
        _schemaDoc = JsonSchemaDocument.Create(jsonSchema, _globalSchemaResourceRegistry);
    }

    public void AddExternalDocument(string externalJsonSchema)
    {
        JsonSchemaDocument.Create(externalJsonSchema, _globalSchemaResourceRegistry);
    }

    public ValidationResult Validate(string jsonInstance)
    {
        using (JsonDocument instance = JsonDocument.Parse(jsonInstance))
        {
            return _schemaDoc.Validate(instance.RootElement);
        }
                
    }
}