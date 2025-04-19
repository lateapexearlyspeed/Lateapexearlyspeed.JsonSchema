using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;

namespace Json.Schema.Libraries.Benchmark;

internal class NewtonsoftValidation : IJsonSchemaValidation
{
    public JsonSchemaLibraryKinds LibraryKinds => JsonSchemaLibraryKinds.Newtonsoft;

    public bool Validate(string jsonSchema, string instance)
    {
        JSchema schema = JSchema.Parse(jsonSchema);
        JToken jToken = JToken.Parse(instance);
        
        return jToken.IsValid(schema);
    }
}