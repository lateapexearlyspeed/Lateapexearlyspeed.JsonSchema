using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using JsonSchemaConsoleApp.JsonConverters;
using JsonSchemaConsoleApp.Keywords.interfaces;

namespace JsonSchemaConsoleApp.Keywords;

[Keyword("type")]
[JsonConverter(typeof(TypeKeywordJsonConverter))]
internal class TypeKeyword : KeywordBase
{
    public SchemaType SchemaType { get; set; }

    protected internal override ValidationResult ValidateCore(JsonElement instance, JsonSchemaOptions options)
    {
        switch (SchemaType)
        {
            case SchemaType.Integer:
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
                            return ValidationResult.CreateFailedResult(ResultCode.NotAnInteger, options.ValidationPathStack);
                        }
                    }
                }

                break;
            case SchemaType.Object:
                if (instance.ValueKind != JsonValueKind.Object)
                {
                    return ValidationResult.CreateFailedResult(ResultCode.InvalidTokenKind, options.ValidationPathStack);
                }
                break;
        }

        return ValidationResult.ValidResult;
    }
}

internal enum SchemaType
{
    Integer,
    Object
}