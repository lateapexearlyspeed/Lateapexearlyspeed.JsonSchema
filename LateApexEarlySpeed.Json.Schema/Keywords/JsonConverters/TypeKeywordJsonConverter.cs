using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using LateApexEarlySpeed.Json.Schema.Common;

namespace LateApexEarlySpeed.Json.Schema.Keywords.JsonConverters;

internal class TypeKeywordJsonConverter : JsonConverter<TypeKeyword>
{
    public override TypeKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        InstanceType[] instanceTypes;
        var newOptions = new JsonSerializerOptions { Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, false) } };

        if (reader.TokenType == JsonTokenType.String)
        {
            instanceTypes = new[] { JsonSerializer.Deserialize<InstanceType>(ref reader, newOptions) };
        }
        else if (reader.TokenType == JsonTokenType.StartArray)
        {
            InstanceType[]? types = JsonSerializer.Deserialize<InstanceType[]>(ref reader, newOptions);

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
        throw new NotImplementedException();
    }

    public override bool HandleNull => true;
}