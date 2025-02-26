using System.Diagnostics;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using JsonQuery.Net.Queryables;

namespace JsonQuery.Net;

public class DefaultJsonQueryableConverter : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert)
    {
        return typeof(IJsonQueryable).IsAssignableFrom(typeToConvert)
               && typeToConvert.GetConstructors().Length == 1
               && typeToConvert.GetCustomAttribute<JsonConverterAttribute>(false) is null;
    }

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        Type converterType = typeof(DefaultJsonQueryableConverterInner<>).MakeGenericType(typeToConvert);

        return (JsonConverter)Activator.CreateInstance(converterType);
    }

    private class DefaultJsonQueryableConverterInner<TQuery> : JsonConverter<TQuery> where TQuery : IJsonQueryable
    {
        private static readonly string KeywordName = JsonQueryableRegistry.GetKeyword(typeof(TQuery));

        public override TQuery Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartArray)
            {
                throw new JsonException($"Invalid token type '{reader.TokenType}' for query '{KeywordName}'");
            }

            ConstructorInfo[] constructors = typeToConvert.GetConstructors();

            Debug.Assert(constructors.Length == 1);

            ConstructorInfo constructor = constructors[0];

            ParameterInfo[] parameterInfos = constructor.GetParameters();

            var arguments = new object?[parameterInfos.Length];

            reader.Read();

            if (reader.TokenType != JsonTokenType.String || reader.GetString() != KeywordName)
            {
                throw new JsonException($"Invalid keyword name for query '{KeywordName}'");
            }

            reader.Read();

            for (int i = 0; i < arguments.Length; i++)
            {
                arguments[i] = JsonSerializer.Deserialize(ref reader, parameterInfos[i].ParameterType, options);

                reader.Read();
            }

            if (reader.TokenType != JsonTokenType.EndArray)
            {
                throw new JsonException($"Unexpected array argument: '{JsonSerializer.Deserialize<JsonElement>(ref reader).GetRawText()}' in query '{KeywordName}'");
            }

            return (TQuery)Activator.CreateInstance(typeof(TQuery), arguments);
        }

        public override void Write(Utf8JsonWriter writer, TQuery value, JsonSerializerOptions options)
        {
            writer.WriteStartArray();

            writer.WriteStringValue(value.GetKeyword());

            Type type = value.GetType();

            PropertyInfo[] properties = type.GetProperties(BindingFlags.GetProperty | BindingFlags.Public | BindingFlags.Instance);

            List<(int index, PropertyInfo prop)> indexedProperties = new(properties.Length);

            foreach (PropertyInfo prop in properties)
            {
                QueryParamAttribute? queryArgumentAttribute = prop.GetCustomAttribute<QueryParamAttribute>();

                if (queryArgumentAttribute is not null)
                {
                    indexedProperties.Add((queryArgumentAttribute.Index, prop));
                }
            }

            IEnumerable<object> argumentValues = indexedProperties.OrderBy(p => p.index).Select(p => p.prop.GetValue(value));

            foreach (object arg in argumentValues)
            {
                JsonSerializer.Serialize(writer, arg, arg.GetType(), options);
            }

            writer.WriteEndArray();
        }
    }
}