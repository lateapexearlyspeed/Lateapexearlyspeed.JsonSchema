using System.Text.Json;
using System.Text.Json.Serialization;
using LateApexEarlySpeed.Json.Schema.Common;
using LateApexEarlySpeed.Json.Schema.JSchema;

namespace LateApexEarlySpeed.Json.Schema.Keywords.JsonConverters;

internal class DependentSchemasKeywordJsonConverter : JsonConverter<DependentSchemasKeyword>
{
    public override DependentSchemasKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        Dictionary<string, JsonSchema>? dependentSchemas = JsonSerializer.Deserialize<Dictionary<string, JsonSchema>>(ref reader);
        if (dependentSchemas is null)
        {
            throw ThrowHelper.CreateKeywordHasInvalidJsonValueKindJsonException<DependentSchemasKeyword>(JsonValueKind.Object);
        }

        foreach (KeyValuePair<string, JsonSchema> dependentSchema in dependentSchemas)
        {
            dependentSchema.Value.Name = dependentSchema.Key;
        }
        return new DependentSchemasKeyword { DependentSchemas = dependentSchemas };
    }

    public override void Write(Utf8JsonWriter writer, DependentSchemasKeyword value, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }
}