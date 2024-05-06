using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using LateApexEarlySpeed.Json.Schema.Common;

namespace LateApexEarlySpeed.Json.Schema.Keywords.JsonConverters;

internal class TypeKeywordJsonConverter : JsonConverter<TypeKeyword>
{
    private static readonly JsonSerializerOptions InstanceTypeSerializerOptions = new() { Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, false) } };

    public override TypeKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        InstanceType[] instanceTypes;

        if (reader.TokenType == JsonTokenType.String)
        {
            instanceTypes = new[] { JsonSerializer.Deserialize<InstanceType>(ref reader, InstanceTypeSerializerOptions) };
        }
        else if (reader.TokenType == JsonTokenType.StartArray)
        {
            InstanceType[]? types = JsonSerializer.Deserialize<InstanceType[]>(ref reader, InstanceTypeSerializerOptions);

            Debug.Assert(types is not null);
            instanceTypes = types;
        }
        else
        {
            throw ThrowHelper.CreateKeywordHasInvalidJsonValueKindJsonException<TypeKeyword>(JsonValueKind.String, JsonValueKind.Array);
        }

        return new TypeKeyword(instanceTypes);
    }

    public override void Write(Utf8JsonWriter writer, TypeKeyword value, JsonSerializerOptions options)
    {
        if (value.InstanceTypes.Length == 1)
        {
            JsonSerializer.Serialize(writer, value.InstanceTypes[0], InstanceTypeSerializerOptions);
        }
        else
        {
            JsonSerializer.Serialize(writer, value.InstanceTypes, InstanceTypeSerializerOptions);
        }
    }

    public override bool HandleNull => true;
}