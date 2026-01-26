using System.Text.Json;
using System.Text.Json.Serialization;

namespace LateApexEarlySpeed.Json.Schema.Keywords.JsonConverters;

internal class SchemaRecursiveReferenceKeywordJsonConverter : JsonConverter<SchemaRecursiveReferenceKeyword>
{
    public override SchemaRecursiveReferenceKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        reader.Skip();
        return new SchemaRecursiveReferenceKeyword();
    }

    public override void Write(Utf8JsonWriter writer, SchemaRecursiveReferenceKeyword value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(SchemaRecursiveReferenceKeyword.Value);
    }

    public override bool HandleNull => true;
}