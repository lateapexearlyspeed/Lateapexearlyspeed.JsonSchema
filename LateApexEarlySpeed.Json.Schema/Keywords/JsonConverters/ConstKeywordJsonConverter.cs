﻿using System.Text.Json;
using System.Text.Json.Serialization;
using LateApexEarlySpeed.Json.Schema.JInstance;

namespace LateApexEarlySpeed.Json.Schema.Keywords.JsonConverters;

internal class ConstKeywordJsonConverter : JsonConverter<ConstKeyword>
{
    public override ConstKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return new ConstKeyword(JsonInstanceElement.ParseValue(ref reader));
    }

    public override void Write(Utf8JsonWriter writer, ConstKeyword value, JsonSerializerOptions options)
    {
        value.ConstValue.WriteTo(writer);
    }

    public override bool HandleNull => true;
}