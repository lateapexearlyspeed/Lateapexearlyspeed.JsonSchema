using LateApexEarlySpeed.Json.Schema.Common;
using LateApexEarlySpeed.Json.Schema.JInstance;
using LateApexEarlySpeed.Json.Schema.JSchema;
using LateApexEarlySpeed.Json.Schema.Keywords.interfaces;
using System.Diagnostics;
using System.Text.Json.Serialization;
using LateApexEarlySpeed.Json.Schema.Keywords.JsonConverters;

namespace LateApexEarlySpeed.Json.Schema.Keywords;

[Keyword(Keyword)]
[JsonConverter(typeof(SchemaRecursiveReferenceKeywordJsonConverter))]
internal class SchemaRecursiveReferenceKeyword : KeywordBase, IReferenceKeyword
{
    public const string Keyword = "$recursiveRef";

    public const string Value = "#";

    private readonly SchemaReferenceKeyword _staticSchemaReferenceKeyword = new(new Uri(Value, UriKind.Relative));

    public Uri ParentResourceBaseUri
    {
        set => _staticSchemaReferenceKeyword.ParentResourceBaseUri = value;
    }

    protected internal override ValidationResult ValidateCore(JsonInstanceElement instance, JsonSchemaOptions options)
    {
        JsonSchemaResource referencedSchemaResource = GetReferencedSchemaResource(options);

        if (!options.SchemaRecursionRecorder.TryPushRecord(referencedSchemaResource, instance.Location))
        {
            throw new InvalidOperationException($"Infinite recursion loop detected. Instance path: {instance.Location}");
        }

        options.ValidationPathStack.PushSchemaResource(referencedSchemaResource);
        
        Debug.Assert(referencedSchemaResource.BaseUri is not null);
        options.ValidationPathStack.PushReferencedSchema(referencedSchemaResource, referencedSchemaResource.BaseUri);

        ValidationResult validationResult = referencedSchemaResource.ValidateCore(instance, options);

        options.ValidationPathStack.PopReferencedSchema();
        options.ValidationPathStack.PopSchemaResource();
        options.SchemaRecursionRecorder.PopRecord();

        return validationResult;
    }

    private JsonSchemaResource GetReferencedSchemaResource(JsonSchemaOptions options)
    {
        JsonSchemaResource? staticReferencedSchemaResource = _staticSchemaReferenceKeyword.GetReferencedSchemaResource(options);
        
        Debug.Assert(staticReferencedSchemaResource is not null);

        if (!staticReferencedSchemaResource.RecursiveAnchorEnabled)
        {
            // Fall back to apply static reference way
            return staticReferencedSchemaResource;
        }

        // firstly, try to find specified recursive anchor in recursive schema resources path.
        // If cannot find out, then use the innermost subschema resource which enables specified recursive anchor.
        return options.ValidationPathStack.SchemaResourceStack.LastOrDefault(resource => resource.RecursiveAnchorEnabled) 
               ?? staticReferencedSchemaResource;
    }
}