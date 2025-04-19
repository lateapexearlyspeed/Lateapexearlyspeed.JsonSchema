namespace Json.Schema.Libraries.Benchmark;

[Flags]
public enum TestValidationResult
{
    Positive = 0x1,
    Negative = 0x2,
    All = Positive | Negative
}