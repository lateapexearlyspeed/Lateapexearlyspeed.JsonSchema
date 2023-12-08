using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using LateApexEarlySpeed.Json.Schema.Common;
using LateApexEarlySpeed.Json.Schema.JSchema;

namespace LateApexEarlySpeed.Json.Schema.Keywords.JsonConverters;

internal class PatternPropertiesKeywordJsonConverter : JsonConverter<PatternPropertiesKeyword>
{
    public override PatternPropertiesKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw ThrowHelper.CreateKeywordHasInvalidJsonValueKindJsonException<PatternPropertiesKeyword>(JsonValueKind.Object);
        }

        Dictionary<string, JsonSchema> patternSchemas = JsonSerializer.Deserialize<Dictionary<string, JsonSchema>>(ref reader, options)!;
        foreach (KeyValuePair<string, JsonSchema> patternSchema in patternSchemas)
        {
            patternSchema.Value.Name = patternSchema.Key;
        }

        try
        {
            return new PatternPropertiesKeyword(patternSchemas);
        }
        catch (Exception e)
        {
            throw ThrowHelper.CreateKeywordHasInvalidRegexJsonException<PatternPropertiesKeyword>(e);
        }
    }

    public override void Write(Utf8JsonWriter writer, PatternPropertiesKeyword value, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }

    public override bool HandleNull => true;
}