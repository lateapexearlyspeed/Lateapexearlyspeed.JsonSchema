using System.Text.Json;
using System.Text.Json.Serialization;
using JsonSchemaConsoleApp.Keywords;

namespace JsonSchemaConsoleApp.JsonConverters;

internal class PropertiesSizeKeywordJsonConverter<TPropertiesSizeBoundaryKeyword> : JsonConverter<TPropertiesSizeBoundaryKeyword> 
    where TPropertiesSizeBoundaryKeyword : PropertiesSizeKeywordBase, new()
{
    public override TPropertiesSizeBoundaryKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (!reader.TryGetUInt32(out uint size))
        {
            throw ThrowHelper.CreateKeywordHasInvalidNonNegativeIntegerJsonException(typeToConvert);
        }

        return new TPropertiesSizeBoundaryKeyword { PropertiesBenchmark = size };
    }

    public override void Write(Utf8JsonWriter writer, TPropertiesSizeBoundaryKeyword value, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }
}