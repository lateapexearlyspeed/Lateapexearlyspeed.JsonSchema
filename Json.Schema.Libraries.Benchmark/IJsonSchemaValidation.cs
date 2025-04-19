namespace Json.Schema.Libraries.Benchmark;

public interface IJsonSchemaValidation
{
    JsonSchemaLibraryKinds LibraryKinds { get; }
    bool Validate(string jsonSchema, string instance);
}

public enum JsonSchemaLibraryKinds
{
    LateApexEarlySpeed,
    JsonSchemaDotNet,
    NJsonSchema,
    Newtonsoft
}