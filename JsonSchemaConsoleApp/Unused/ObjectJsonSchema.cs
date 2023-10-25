using System.Text.Json;
using System.Text.RegularExpressions;

namespace JsonSchemaConsoleApp;

internal class ObjectJsonSchema : BodyJsonSchema
{
    private readonly JsonElement _schema;
    private readonly JsonSchemaOptions _options;

    public ObjectJsonSchema(JsonElement schema, JsonSchemaOptions options)
    {
        _schema = schema;
        _options = options;
    }

    public const string TypeName = "object";

    private const string PropertyKeyword = "properties";
    private const string PatternPropertiesKeyword = "patternProperties";
    private const string AdditionalPropertiesKeyword = "additionalProperties";
    private const string RequiredKeyword = "required";
    private const string PropertyNamesKeyword = "propertyNames";
    private const string MinPropertiesKeyword = "minProperties";
    private const string MaxPropertiesKeyword = "maxProperties";

    // public bool Validate(JsonElement jsonInstance)
    // {
    //     if (jsonInstance.ValueKind != JsonValueKind.Object)
    //     {
    //         return false;
    //     }
    //
    //     Dictionary<string, JsonElement>? additionalInstanceProperties = null;
    //     if (_schema.TryGetKeyword(PropertyKeyword, out JsonElement schemaProperties))
    //     {
    //         foreach (JsonProperty instanceProperty in jsonInstance.EnumerateObject())
    //         {
    //             if (schemaProperties.TryGetProperty(instanceProperty.Name, out JsonElement schemaProperty))
    //             {
    //                 JsonSchema propertySchema = Create(schemaProperty, _options);
    //                 if (!propertySchema.Validate(instanceProperty.Value))
    //                 {
    //                     return false;
    //                 }
    //             }
    //             else
    //             {
    //                 additionalInstanceProperties ??= new();
    //                 additionalInstanceProperties.Add(instanceProperty.Name, instanceProperty.Value);
    //             }
    //         }
    //     }
    //
    //     if (_schema.TryGetKeyword(PatternPropertiesKeyword, out JsonElement patternProperties))
    //     {
    //         foreach (JsonProperty instanceProperty in jsonInstance.EnumerateObject())
    //         {
    //             bool canFindPropFromSchema = false;
    //             foreach (JsonProperty patternProperty in patternProperties.EnumerateObject())
    //             {
    //                 if (Regex.IsMatch(instanceProperty.Name, patternProperty.Name))
    //                 {
    //                     if (!Create(patternProperty.Value, _options).Validate(instanceProperty.Value))
    //                     {
    //                         return false;
    //                     }
    //
    //                     canFindPropFromSchema = true;
    //                     break;
    //                 }
    //             }
    //
    //             if (canFindPropFromSchema && additionalInstanceProperties is not null)
    //             {
    //                 additionalInstanceProperties.Remove(instanceProperty.Name);
    //             }
    //         }
    //     }
    //
    //     if (_schema.TryGetProperty(AdditionalPropertiesKeyword, out JsonElement additionalProperties))
    //     {
    //         if (additionalInstanceProperties is not null && additionalInstanceProperties.Count != 0)
    //         {
    //             if (additionalProperties.ValueKind == JsonValueKind.False)
    //             {
    //                 return false;
    //             }
    //
    //             if (additionalProperties.ValueKind == JsonValueKind.Object)
    //             {
    //                 JsonSchema additionalPropertiesSchema = Create(additionalProperties, _options);
    //                 foreach (KeyValuePair<string, JsonElement> property in additionalInstanceProperties)
    //                 {
    //                     if (!additionalPropertiesSchema.Validate(property.Value))
    //                     {
    //                         return false;
    //                     }
    //                 }
    //             }
    //         }
    //     }
    //
    //     if (_schema.TryGetKeyword(RequiredKeyword, out string[]? requiredProps))
    //     {
    //         foreach (string requiredProp in requiredProps)
    //         {
    //             if (!jsonInstance.TryGetProperty(requiredProp, out _))
    //             {
    //                 return false;
    //             }
    //         }
    //     }
    //
    //     if (_schema.TryGetKeyword(PropertyNamesKeyword, out JsonElement propertyNames))
    //     {
    //         var propertyNameSchema = new StringJsonSchema(propertyNames, _options.ShouldValidateFormat);
    //         foreach (JsonProperty property in jsonInstance.EnumerateObject())
    //         {
    //             if (!propertyNameSchema.Validate(property.Name))
    //             {
    //                 return false;
    //             }
    //         }
    //     }
    //
    //     int propCount = jsonInstance.EnumerateObject().Count();
    //
    //     if (_schema.TryGetKeyword(MinPropertiesKeyword, out uint minProp))
    //     {
    //         if (propCount < minProp)
    //         {
    //             return false;
    //         }
    //     }
    //
    //     if (_schema.TryGetKeyword(MaxPropertiesKeyword, out uint maxProp))
    //     {
    //         if (propCount > maxProp)
    //         {
    //             return false;
    //         }
    //     }
    //
    //     return true;
    // }
}