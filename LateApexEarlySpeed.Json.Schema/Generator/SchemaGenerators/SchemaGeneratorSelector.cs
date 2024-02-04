namespace LateApexEarlySpeed.Json.Schema.Generator.SchemaGenerators;

internal static class SchemaGeneratorSelector
{
    private static readonly ISchemaGenerationCandidate[] GeneratorPriorityList = new ISchemaGenerationCandidate[]
    {
        // Signed integer
        new Int32SchemaGenerationCandidate(),
        new Int64SchemaGenerationCandidate(),
        new Int16SchemaGenerationCandidate(),
        new SByteSchemaGenerationCandidate(),

        // Unsigned integer
        new UInt32SchemaGenerationCandidate(),
        new UInt64SchemaGenerationCandidate(),
        new UInt16SchemaGenerationCandidate(),
        new ByteSchemaGenerationCandidate(),

        // Floating-point numeric types
        new FloatSchemaGenerationCandidate(),
        new DoubleSchemaGenerationCandidate(),
        new DecimalSchemaGenerationCandidate(),

        new BooleanSchemaGenerationCandidate(),
        new StringSchemaGenerationCandidate(),

        // Dictionary<string, TValue>
        new StringDictionarySchemaGenerationCandidate(),

        // Arbitrary json types
        new ArbitraryJsonTypesSchemaGenerationCandidate(),

        new JsonArraySchemaGenerationCandidate(),
        new JsonObjectSchemaGenerationCandidate(),

        // Collection
        new CollectionSchemaGenerationCandidate(),

        new EnumSchemaGenerationCandidate(),
        new GuidSchemaGenerationCandidate(),
        new UriSchemaGenerationCandidate(),
        new DateTimeOffsetSchemaGenerationCandidate(),
        new DateTimeSchemaGenerationCandidate(),

        // Nullable value type
        new NullableValueTypeSchemaGenerationCandidate()
    };

    private static readonly CustomObjectSchemaGenerator FinalSchemaGenerator = new();

    public static ISchemaGenerator Select(Type typeToConvert)
    {
        ISchemaGenerator? selectedGenerator = GeneratorPriorityList.FirstOrDefault(generator => generator.CanGenerate(typeToConvert));

        return selectedGenerator ?? FinalSchemaGenerator;
    }
}