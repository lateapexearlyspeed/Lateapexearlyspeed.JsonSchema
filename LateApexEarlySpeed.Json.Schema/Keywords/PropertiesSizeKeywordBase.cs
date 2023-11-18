using System.Text.Json;
using LateApexEarlySpeed.Json.Schema.Common;
using LateApexEarlySpeed.Json.Schema.JInstance;
using LateApexEarlySpeed.Json.Schema.Keywords.interfaces;

namespace LateApexEarlySpeed.Json.Schema.Keywords;

internal abstract class PropertiesSizeKeywordBase : KeywordBase, IBenchmarkValueKeyword
{
    public uint BenchmarkValue { get; init; }

    protected internal override ValidationResult ValidateCore(JsonInstanceElement instance, JsonSchemaOptions options)
    {
        if (instance.ValueKind != JsonValueKind.Object)
        {
            return ValidationResult.ValidResult;
        }

        int instanceProperties = instance.EnumerateObject().Count();

        return IsSizeInRange(instanceProperties)
            ? ValidationResult.ValidResult
            : ValidationResult.CreateFailedResult(ResultCode.PropertiesOutOfRange, GetErrorMessage(instanceProperties), options.ValidationPathStack, Name, instance.Location);

    }

    protected abstract bool IsSizeInRange(int instanceProperties);

    protected abstract string GetErrorMessage(int instanceProperties);
}