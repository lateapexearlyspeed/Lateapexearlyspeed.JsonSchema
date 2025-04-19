using LateApexEarlySpeed.Json.Schema;
using LateApexEarlySpeed.Json.Schema.Common;

namespace Json.Schema.Libraries.Benchmark;

internal class LateApexEarlySpeedValidation : IJsonSchemaValidation
{
    public JsonSchemaLibraryKinds LibraryKinds => JsonSchemaLibraryKinds.LateApexEarlySpeed;

    public bool Validate(string jsonSchema, string instance)
    {
        var jsonValidator = new JsonValidator(jsonSchema);

        return jsonValidator.Validate(instance, new JsonSchemaOptions { ValidateFormat = false }).IsValid;
    }
}