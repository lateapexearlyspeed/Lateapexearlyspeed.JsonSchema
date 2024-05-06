using System.Text.Json;
using System.Text.Json.Serialization;
using LateApexEarlySpeed.Json.Schema.Common;

namespace LateApexEarlySpeed.Json.Schema.Keywords.JsonConverters;

internal class DependentRequiredKeywordJsonConverter : JsonConverter<DependentRequiredKeyword>
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
        JsonSerializer.Serialize(writer, value.DependentProperties, options);
    }

    public override bool HandleNull => true;
}