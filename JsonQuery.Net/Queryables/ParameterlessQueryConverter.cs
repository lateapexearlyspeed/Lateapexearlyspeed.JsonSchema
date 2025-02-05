﻿using System.Text.Json;
using System.Text.Json.Serialization;

namespace JsonQuery.Net.Queryables;

public class ParameterlessQueryConverter<TQuery> : JsonConverter<TQuery> where TQuery : IJsonQueryable, new()
{
    public override TQuery Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        reader.Read();
        reader.Read();

        return new TQuery();
    }

    public override void Write(Utf8JsonWriter writer, TQuery value, JsonSerializerOptions options)
    {
        writer.WriteStartArray();
        writer.WriteStringValue(value.GetKeyword());
        writer.WriteEndArray();
    }
}

public class ParameterlessQueryParserConverter<TQuery> : JsonQueryConverter<TQuery> where TQuery : IJsonQueryable, new()
{
    public override TQuery Read(ref JsonQueryReader reader)
    {
        reader.Read();
        reader.Read();

        return new TQuery();
    }
}