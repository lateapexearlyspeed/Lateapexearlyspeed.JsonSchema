using System.Text.Json;
using System.Text.Json.Serialization;
using LateApexEarlySpeed.Json.Schema.Common;

namespace LateApexEarlySpeed.Json.Schema.Keywords.JsonConverters;

internal class RequiredKeywordJsonConverter : JsonConverter<RequiredKeyword>
{
    public override RequiredKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        string[]? requiredProperties = JsonSerializer.Deserialize<string[]>(ref reader);
        if (requiredProperties is null)
        {
            throw ThrowHelper.CreateKeywordHasInvalidJsonValueKindJsonException<RequiredKeyword>(JsonValueKind.Array);
        }

        if (requiredProperties.Length != new HashSet<string>(requiredProperties).Count)
        {
            throw ThrowHelper.CreateKeywordHasDuplicatedJsonArrayElementsJsonException<RequiredKeyword>();
        }

        return new RequiredKeyword(requiredProperties);
    }

    public override void Write(Utf8JsonWriter writer, RequiredKeyword value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value.RequiredProperties, options);
    }

    public override bool HandleNull => true;
}