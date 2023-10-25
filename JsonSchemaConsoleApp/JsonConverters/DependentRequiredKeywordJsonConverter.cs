using System.Text.Json;
using System.Text.Json.Serialization;
using JsonSchemaConsoleApp.Keywords;

namespace JsonSchemaConsoleApp.JsonConverters;

public class DependentRequiredKeywordJsonConverter : JsonConverter<DependentRequiredKeyword>
{
    public override DependentRequiredKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        Dictionary<string, string[]>? dependentProperties = JsonSerializer.Deserialize<Dictionary<string, string[]>>(ref reader);
        if (dependentProperties is null)
        {
            throw ThrowHelper.CreateKeywordHasInvalidJsonValueKindJsonException<DependentRequiredKeyword>(JsonValueKind.Object);
        }

        foreach (KeyValuePair<string, string[]> dependentProperty in dependentProperties)
        {
            if (dependentProperty.Value.Length != new HashSet<string>(dependentProperty.Value).Count)
            {
                throw ThrowHelper.CreateKeywordHasDuplicatedJsonArrayElementsJsonException<DependentRequiredKeyword>();
            }
        }

        return new DependentRequiredKeyword { DependentProperties = dependentProperties };
    }

    public override void Write(Utf8JsonWriter writer, DependentRequiredKeyword value, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }
}