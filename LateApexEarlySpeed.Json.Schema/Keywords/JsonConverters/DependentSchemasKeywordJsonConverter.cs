using System.Text.Json;
using System.Text.Json.Serialization;
using LateApexEarlySpeed.Json.Schema.Common;
using LateApexEarlySpeed.Json.Schema.JSchema;

namespace LateApexEarlySpeed.Json.Schema.Keywords.JsonConverters;

internal class DependentSchemasKeywordJsonConverter : JsonConverter<DependentSchemasKeyword>
{
    public override DependentSchemasKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        Dictionary<string, JsonSchema>? dependentSchemas = JsonSerializer.Deserialize<Dictionary<string, JsonSchema>>(ref reader, options);
        if (dependentSchemas is null)
        {
            throw ThrowHelper.CreateKeywordHasInvalidJsonValueKindJsonException<DependentSchemasKeyword>(JsonValueKind.Object);
        }

        return new DependentSchemasKeyword(dependentSchemas, options.GetJsonValidatorOptions().PropertyNameCaseInsensitive);
    }

    public override void Write(Utf8JsonWriter writer, DependentSchemasKeyword value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value.DependentSchemas, options);
    }

    public override bool HandleNull => true;
}