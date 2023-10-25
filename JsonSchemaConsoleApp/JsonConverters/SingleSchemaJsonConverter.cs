using System.Text.Json;
using System.Text.Json.Serialization;
using JsonSchemaConsoleApp.Keywords.interfaces;

namespace JsonSchemaConsoleApp.JsonConverters;

// public class SingleSchemaJsonConverter<T> : JsonConverter<T> where T : ISingleSubSchema, new()
// {
//     public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
//     {
//         return new T { Schema = JsonSerializer.Deserialize<JsonSchema>(ref reader)! };
//     }
//
//     public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
//     {
//         throw new NotImplementedException();
//     }
// }