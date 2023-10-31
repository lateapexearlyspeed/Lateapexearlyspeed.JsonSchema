using System.Text.Json;

namespace JsonSchemaConsoleApp.Keywords;

internal abstract class PropertiesSizeKeywordBase : KeywordBase
{
    public uint PropertiesBenchmark { get; init; }

    protected internal override ValidationResult ValidateCore(JsonElement instance, JsonSchemaOptions options)
    {
        if (instance.ValueKind != JsonValueKind.Object)
        {
            return ValidationResult.ValidResult;
        }

        int instanceProperties = instance.EnumerateObject().Count();

        return IsSizeInRange(instanceProperties)
            ? ValidationResult.ValidResult
            : ValidationResult.CreateFailedResult(ResultCode.PropertiesOutOfRange, options.ValidationPathStack);

    }

    protected abstract bool IsSizeInRange(int instanceProperties);
}