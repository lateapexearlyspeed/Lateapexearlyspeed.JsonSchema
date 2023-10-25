using System.Text.Json;
using System.Text.Json.Serialization;
using JsonSchemaConsoleApp.JsonConverters;
using JsonSchemaConsoleApp.Keywords;
using JsonSchemaConsoleApp.Keywords.interfaces;

namespace JsonSchemaConsoleApp;

internal class BodyJsonSchemaDocument : JsonSchemaResource, IJsonSchemaDocument
{
    private static readonly Uri DefaultDocumentBaseUri = new("http://lateapexearlyspeed");

    internal BodyJsonSchemaDocument(List<ValidationNode> keywords, ConditionalValidator conditionalValidator, SchemaReference? schemaReference, SchemaDynamicReference? schemaDynamicReference, string? anchor, string? dynamicAnchor, Uri? id = null, DefsKeyword? defsKeyword = null) 
        : base(GetBaseUri(id), keywords, conditionalValidator, schemaReference, schemaDynamicReference, anchor, dynamicAnchor, defsKeyword)
    {
    }

    // all schema resources inside current document (including current document itself)
    public SchemaResourceRegistry LocalSchemaResourceRegistry { get; } = new();

    public SchemaResourceRegistry? GlobalSchemaResourceRegistry { get; set; }

    private static Uri GetBaseUri(Uri? id)
    {
        return id is null || !id.IsAbsoluteUri 
            ? DefaultDocumentBaseUri 
            : id;
    }

    public void MakeAllIdentifierAndReferenceBeFullUri()
    {
        // here input of 'parentResourceBaseUri' is a arbitrary uri because first schema is schema document which already has real absolute uri
        MakeAllIdentifierAndReferenceBeFullUri(this, DefaultDocumentBaseUri);
    }

    private void MakeAllIdentifierAndReferenceBeFullUri(ISchemaContainerElement containerElement, Uri parentResourceBaseUri)
    {
        Uri currentResourceBaseUri = parentResourceBaseUri;

        if (containerElement is BodyJsonSchema schema)
        {
            if (containerElement is JsonSchemaResource schemaResource)
            {
                schemaResource.ParentResourceBaseUri = parentResourceBaseUri;
                currentResourceBaseUri = schemaResource.BaseUri;
                LocalSchemaResourceRegistry.AddSchemaResource(schemaResource.BaseUri, schemaResource);
            }

            if (schema.SchemaReference is not null)
            {
                schema.SchemaReference.ParentResourceBaseUri = currentResourceBaseUri;
            }

            if (schema.SchemaDynamicReference is not null)
            {
                schema.SchemaDynamicReference.ParentResourceBaseUri = currentResourceBaseUri;
            }
        }

        foreach (ISchemaContainerElement child in containerElement.EnumerateElements())
        {
            MakeAllIdentifierAndReferenceBeFullUri(child, currentResourceBaseUri);
        }
    }

    public ValidationResult Validate(JsonElement instance)
    {
        return Validate(instance, new JsonSchemaOptions
        {
            SchemaResourceRegistry = GlobalSchemaResourceRegistry,
            SchemaRecursionRecorder = new SchemaRecursionRecorder(),
            ValidationPathStack = new ValidationPathStack()
        });
    }

    protected internal override ValidationResult ValidateCore(JsonElement instance, JsonSchemaOptions options)
    {
        options.ValidationPathStack.PushReferencedSchema(this, BaseUri);
        ValidationResult validationResult = base.ValidateCore(instance, options);
        options.ValidationPathStack.PopReferencedSchema();

        return validationResult;
    }
}