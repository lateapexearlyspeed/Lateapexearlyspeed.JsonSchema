using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using LateApexEarlySpeed.Json.Schema.Common;
using LateApexEarlySpeed.Json.Schema.Keywords.JsonConverters;

namespace LateApexEarlySpeed.Json.Schema.Keywords;

[Keyword("type")]
[JsonConverter(typeof(TypeKeywordJsonConverter))]
internal class TypeKeyword : KeywordBase
{
    private static readonly Dictionary<InstanceType, JsonValueKind> InstanceTypeJsonKindMap = new()
    {
        { InstanceType.Object, JsonValueKind.Object },
        { InstanceType.Array, JsonValueKind.Array },
        { InstanceType.Null, JsonValueKind.Null },
        { InstanceType.Number, JsonValueKind.Number },
        { InstanceType.String, JsonValueKind.String }
    };

    public InstanceType[] InstanceTypes { get; init; } = null!;

    protected internal override ValidationResult ValidateCore(JsonElement instance, JsonSchemaOptions options)
    {
        Debug.Assert(InstanceTypes.Length != 0);
        Unsafe.SkipInit(out ValidationResult validationResult);

        foreach (InstanceType instanceType in InstanceTypes)
        {
            validationResult = ValidateAgainstType(instance, instanceType, options);
            if (validationResult.IsValid)
            {
                return ValidationResult.ValidResult;
            }
        }

        return validationResult;
    }

    private ValidationResult ValidateAgainstType(JsonElement instance, InstanceType instanceType, JsonSchemaOptions options)
    {
        switch (instanceType)
        {
            case InstanceType.Integer:
                if (instance.ValueKind != JsonValueKind.Number)
                {
                    return ValidationResult.CreateFailedResult(ResultCode.InvalidTokenKind, options.ValidationPathStack);
                }

                string rawText = instance.GetRawText();
                int dotIdx = rawText.IndexOf('.');
                if (dotIdx != -1)
                {
                    ReadOnlySpan<char> fraction = rawText.AsSpan(dotIdx + 1);
                    foreach (char c in fraction)
                    {
                        if (c != '0')
                        {
                            return ValidationResult.CreateFailedResult(ResultCode.NotBeInteger, options.ValidationPathStack);
                        }
                    }
                }

                break;
            case InstanceType.Boolean:
                if (instance.ValueKind != JsonValueKind.True && instance.ValueKind != JsonValueKind.False)
                {
                    return ValidationResult.CreateFailedResult(ResultCode.InvalidTokenKind, options.ValidationPathStack);
                }
                break;
            default:
                return ValidateJsonKind(instance, InstanceTypeJsonKindMap[instanceType], options.ValidationPathStack);
        }

        return ValidationResult.ValidResult;
    }

    private ValidationResult ValidateJsonKind(JsonElement instance, JsonValueKind expectedJsonKind, ValidationPathStack validationPathStack)
    {
        return instance.ValueKind == expectedJsonKind 
            ? ValidationResult.ValidResult 
            : ValidationResult.CreateFailedResult(ResultCode.InvalidTokenKind, validationPathStack);
    }
}

internal enum InstanceType
{
    Null,
    Boolean,
    Object,
    Array,
    Number,
    String,
    Integer
}