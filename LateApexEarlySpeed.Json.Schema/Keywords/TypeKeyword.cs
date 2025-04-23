using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using LateApexEarlySpeed.Json.Schema.Common;
using LateApexEarlySpeed.Json.Schema.JInstance;
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

    public InstanceType[] InstanceTypes { get; }

    public TypeKeyword(params InstanceType[] instanceTypes)
    {
        InstanceTypes = instanceTypes;
    }

    protected internal override ValidationResult ValidateCore(JsonInstanceElement instance, JsonSchemaOptions options)
    {
        Debug.Assert(InstanceTypes.Length != 0);

        foreach (InstanceType instanceType in InstanceTypes)
        {
            if (IsValidAgainstType(instance, instanceType))
            {
                return ValidationResult.ValidResult;
            }
        }

        return ValidationResult.CreateFailedResult(ResultCode.InvalidTokenKind, GetErrorMessage(instance.ValueKind), options.ValidationPathStack, Name, instance.Location);
    }

    private string GetErrorMessage(JsonValueKind actualKind)
    {
        return $"Expect type(s): '{string.Join('|', InstanceTypes.Select(EnumHelper<InstanceType>.GetCachedStringName))}' but actual is '{EnumHelper<JsonValueKind>.GetCachedStringName(actualKind)}'";
    }

    private bool IsValidAgainstType(JsonInstanceElement instance, InstanceType expectedInstanceType)
    {
        switch (expectedInstanceType)
        {
            case InstanceType.Integer:
                return instance.IsIntegerTypeForJsonSchema();

            case InstanceType.Boolean:
                return instance.ValueKind == JsonValueKind.True || instance.ValueKind == JsonValueKind.False;

            default:
                return IsValidJsonKind(instance, expectedInstanceType);
        }
    }

    private static bool IsValidJsonKind(JsonInstanceElement instance, InstanceType expectedInstanceType)
    {
        return instance.ValueKind == InstanceTypeJsonKindMap[expectedInstanceType];
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