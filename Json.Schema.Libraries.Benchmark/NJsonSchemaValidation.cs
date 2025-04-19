using NJsonSchema.Validation;

namespace Json.Schema.Libraries.Benchmark;

internal class NJsonSchemaValidation : IJsonSchemaValidation
{
    public JsonSchemaLibraryKinds LibraryKinds => JsonSchemaLibraryKinds.NJsonSchema;

    public bool Validate(string jsonSchema, string instance)
    {
        NJsonSchema.JsonSchema schema = NJsonSchema.JsonSchema.FromJsonAsync(jsonSchema).Result;

        ICollection<ValidationError> validationErrors = schema.Validate(instance);

        return validationErrors.Count == 0;
    }
}