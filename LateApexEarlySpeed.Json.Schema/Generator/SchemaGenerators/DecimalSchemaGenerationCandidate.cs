﻿using LateApexEarlySpeed.Json.Schema.JSchema;
using LateApexEarlySpeed.Json.Schema.Keywords;

namespace LateApexEarlySpeed.Json.Schema.Generator.SchemaGenerators;

internal class DecimalSchemaGenerationCandidate : ISchemaGenerationCandidate
{
    public bool CanGenerate(Type typeToConvert)
    {
        return typeToConvert == typeof(decimal);
    }

    public BodyJsonSchema Generate(Type typeToConvert, IEnumerable<KeywordBase> keywordsFromProperty, JsonSchemaGeneratorOptions options)
    {
        return SchemaGenerationHelper.GenerateSchemaForDecimal(keywordsFromProperty);
    }
}