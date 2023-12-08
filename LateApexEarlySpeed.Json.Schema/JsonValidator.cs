using System;
using System.Text.Json;
using LateApexEarlySpeed.Json.Schema.Common;
using LateApexEarlySpeed.Json.Schema.JInstance;
using LateApexEarlySpeed.Json.Schema.JSchema;
using LateApexEarlySpeed.Json.Schema.JSchema.interfaces;

namespace LateApexEarlySpeed.Json.Schema;

public class JsonValidator
{
    private readonly IJsonSchemaDocument _mainSchemaDoc;

    private readonly SchemaResourceRegistry _globalSchemaResourceRegistry = new();

    public JsonValidator(string jsonSchema) : this(jsonSchema.AsSpan())
    {
    }

    public JsonValidator(ReadOnlySpan<char> jsonSchema)
    {
        _mainSchemaDoc = JsonSchemaDocument.CreateDocAndUpdateGlobalResourceRegistry(jsonSchema, _globalSchemaResourceRegistry);
    }

    public void AddExternalDocument(string externalJsonSchema) => AddExternalDocument(externalJsonSchema.AsSpan());

    public void AddExternalDocument(ReadOnlySpan<char> externalJsonSchema)
    {
        JsonSchemaDocument.CreateDocAndUpdateGlobalResourceRegistry(externalJsonSchema, _globalSchemaResourceRegistry);
    }

    public ValidationResult Validate(string jsonInstance)
    {
        // ReSharper disable once ConvertToUsingDeclaration
        using (JsonDocument instance = JsonDocument.Parse(jsonInstance))
        {
            return _mainSchemaDoc.Validate(instance.RootInstanceElement());
        }
    }
}