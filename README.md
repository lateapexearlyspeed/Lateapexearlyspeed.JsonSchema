<p align="center">
    <img height="60" src="https://github.com/lateapexearlyspeed/Lateapexearlyspeed.JsonSchema/blob/master/LateApexEarlySpeed.Json.Schema/nuget.icon.jpg?raw=true">
</p>
<h1 align="center">Lateapexearlyspeed.Json.Schema</h1>
<p align="center">To protect and validate your JSON</p>

<p align="center">
    <img width="100%" src="https://github.com/lateapexearlyspeed/Lateapexearlyspeed.JsonSchema/blob/master/splash-image.png?raw=true">
</p>

![GitHub Actions Workflow Status](https://img.shields.io/github/actions/workflow/status/lateapexearlyspeed/Lateapexearlyspeed.JsonSchema/cicd.yml)
![NuGet Version](https://img.shields.io/nuget/v/LateApexEarlySpeed.Json.Schema)

# This repository provides following nuget packages about json schema:

- [LateApexEarlySpeed.EntityFrameworkCore.V6.Json.Schema](https://www.nuget.org/packages/LateApexEarlySpeed.EntityFrameworkCore.V6.Json.Schema/)
- [LateApexEarlySpeed.EntityFrameworkCore.V3.Json.Schema](https://www.nuget.org/packages/LateApexEarlySpeed.EntityFrameworkCore.V3.Json.Schema/)
- [LateApexEarlySpeed.Xunit.Assertion.Json](https://www.nuget.org/packages/LateApexEarlySpeed.Xunit.Assertion.Json)
- [Lateapexearlyspeed.Json.Schema](https://www.nuget.org/packages/LateApexEarlySpeed.Json.Schema/)

## LateApexEarlySpeed.Xunit.Assertion.Json

There were already json related test assertion libraries, most ones were asserting json's equivalent.

However, imagine what you would test is other factors besides of equivalent on whole json, remember the assertion methods in Xunit (Contains, AllOf, IsTypeOf ..) at ANY json node location.

This library can assert json node data in any node location, not only against whole json. Also, you won't write multiple assertion code lines for different json node, you just need to have one JsonAssertion.Meet(...) method then inside it, you specify what you would test, all in one place. When assertion failed, library will throw detailed assert exception which reports failed json node location. 

```csharp
JsonAssertion.Meet(b =>
                        b.IsJsonObject()
                            .HasProperty("p1", b => b.IsJsonObject())
                            .HasProperty("p2", b => b.IsJsonNumber().IsLessThan(5)),
                """
                {
                  "p1": {},
                  "p2": 5.1
                }
                """);
```

```
LateApexEarlySpeed.Xunit.Assertion.Json.JsonAssertException
JsonAssertion.Meet() Failure: Instance '5.1' is equal to or greater than '5', location (in json pointer format): '/p2'
```

Also it supports json-equivalent assertion:

```csharp
JsonAssertion.Equivalent("""
                {
                  "a": 1,
                  "b": 2
                }
                """,
                """
                {
                  "b": 2,
                  "a": 1
                }
                """);
```

This assertion library depends on core functionalities of library 'Lateapexearlyspeed.Json.Schema', check available validation methods [here](#available-fluent-validation-methods).

## LateApexEarlySpeed.EntityFrameworkCore.V6.Json.Schema & LateApexEarlySpeed.EntityFrameworkCore.V3.Json.Schema

Json column in database give flexible possiblilty to store information, but it may introduce unexpected json data into db incorrectly. 

This libreary provides json level validation for EF core model's json column in client side before sending DB request, the usage is similiar with EF core's existing model property configuration. 

Use package 'LateApexEarlySpeed.EntityFrameworkCore.V3.Json.Schema' for 'Microsoft.EntityFrameworkCore' v3, and package 'LateApexEarlySpeed.EntityFrameworkCore.V6.Json.Schema' for 'Microsoft.EntityFrameworkCore' v6+.

Model:
```csharp
    public class Blog
    {
        public int BlogId { get; set; }

        [Column(TypeName = "jsonb")]
        public string JsonContent { get; set; }
    }
```

Configure json column with 2 ways:

### 1. Fluent configuration

If not familiar with Standard Json schema, recommend using fluent configuration. Most developers may be more familiar with "stronger type" smell. Library's fluent configuration will (in most cases) firstly "ask" developers what json type they want, then continue "ask" subsequence validation requirements which are scoped based on known json type. 

By doing that, developers may have more friendly method invoke chains and will not be easy to make mistake because some standard json schema keywords only have functionalities on specific type. Also, because validation methods is on specific json type builder, so those validation methods can be designed to accept concret .net type rather than raw json. 

Lateapexearlyspeed.Json.Schema package extends standard keywords for some of fluent validation requirement and provide fluent validation, this library depends on it.

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<Blog>()
        .Property(b => b.JsonContent).HasJsonValidation(b =>
        {
            b.IsJsonObject()
                .HasProperty("A", b => b.IsJsonString().HasMinLength(5))
                .HasProperty("B", b => b.IsJsonNumber().IsGreaterThan(1).IsLessThan(10))
                .HasProperty("C", b => b.IsJsonArray().HasMinLength(5).HasItems(b =>
                {
                    b.NotJsonNull();
                }))
                .HasProperty("D", b => b.Or(
                        b => b.IsJsonFalse(),
                        b => b.IsJsonNumber().Equal(0),
                        b => b.IsJsonObject().HasCustomValidation((JsonElement element) => element.GetProperty("Prop").ValueKind == JsonValueKind.True, 
                            jsonElement => $"Cannot pass my custom validation, data is {jsonElement}")
                    )
                );
        });
}
```

When saving wrong entity:

```csharp
{
  "A": "abcde",
  "B": 2,
  "C": [1, 2, 3, 4, null],
  "D": 0
}
```

EF core will throw:

```csharp
Microsoft.EntityFrameworkCore.DbUpdateException: An error occurred while updating the entries. See the inner exception for details.
 ---> LateApexEarlySpeed.EntityFrameworkCore.V3.Json.Schema.JsonValidationException: Failed to validate json property: 'JsonContent'. Failed json location (json pointer format): '/C/4', reason: Expect type(s): 'Object|Array|Boolean|Number|String' but actual is 'Null'.
   at LateApexEarlySpeed.EntityFrameworkCore.V3.Json.Schema.JsonStringValueConverter.ConvertToJson(String model, JsonValidator jsonValidator, String propertyName)
   at ...
   at Microsoft.EntityFrameworkCore.DbContext.SaveChanges(Boolean acceptAllChangesOnSuccess)
```

You can get column name, failed [location (by json pointer format)](https://datatracker.ietf.org/doc/html/rfc6901) in json body and failed reason.

You don't have to specify all properties when configure, just configure necessary stuff your data requirement focuses on. The json part in data which is not configured will not be checked.

Available validations for json column: check 'Lateapexearlyspeed.Json.Schema' [here](#available-fluent-validation-methods).

### 2. Just provide standard json schema (2020.12) when call HasJsonValidation():
```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<Blog>().Property(b => b.JsonContent).HasJsonValidation("""
        {
          "properties": {
            "cba": {
              "type": "string"
            }
          }
        }
        """);    
}
```

## Lateapexearlyspeed.Json.Schema

This is a high performance Json schema .Net implementation library based on [Json schema](https://json-schema.org/) - draft 2020.12 (latest one by 2023.12).

This library also supports validator generation from your class code, see below.

---
The json validation functionalities have passed [official json schema test-suite](https://github.com/json-schema-org/JSON-Schema-Test-Suite) for draft 2020.12 (except cases about limitation listed below)

**High performance** - this .Net library has good performance compared with existing more popular and excellent .Net implementations in common cases by BenchmarkDotnet result, but please verify in your use cases.

*Some Benchmark result (note they are compared under same use pattern - see [Performance Tips](#performance-tips) ):*

12th Gen Intel Core i7-12800H, 1 CPU, 20 logical and 14 physical cores

  [Host]     : .NET 6.0.25 (6.0.2523.51912), X64 RyuJIT AVX2

  DefaultJob : .NET 6.0.25 (6.0.2523.51912), X64 RyuJIT AVX2
  
*Valid data case:*
| Method                             | Mean        | Error     | StdDev    | Gen0    | Gen1   | Allocated |
|----------------------------------- |------------:|----------:|----------:|--------:|-------:|----------:|
| ValidateByPopularSTJBasedValidator |    29.80 us |  0.584 us |  0.573 us |  4.4556 | 0.2441 |   55.1 KB |
| ValidateByThisValidator            |    15.99 us |  0.305 us |  0.300 us |  1.9531 |      - |   24.2 KB |

*Invalid data case:*
| Method                             | Mean        | Error     | StdDev     | Median      | Gen0    | Gen1   | Allocated |
|----------------------------------- |------------:|----------:|-----------:|------------:|--------:|-------:|----------:|
| ValidateByPopularSTJBasedValidator |    65.04 us |  2.530 us |   7.341 us |    66.87 us |  4.5776 | 0.1221 |  56.42 KB |
| ValidateByThisValidator            |    15.47 us |  1.160 us |   3.421 us |    17.14 us |  1.4954 |      - |  18.45 KB |

Note: "STJ" means "System.Text.Json" which is built-in json package in .net sdk, this library is also based on it.

*Benchmark Schema:*

```
{
  "$id": "http://main",
  "type": "object",

  "additionalProperties": false,
  "patternProperties": {
    "propB*lean": {
      "type": "boolean"
    }
  },
  "dependentRequired": {
    "propNull": [ "propBoolean", "propArray" ]
  },
  "dependentSchemas": {
    "propNull": {
      "type": "object"
    }
  },
  "propertyNames": true,
  "required": [ "propNull", "propBoolean" ],
  "maxProperties": 100,
  "minProperties": 0,
  "properties": {
    "propNull": {
      "type": "null"
    },
    "propBoolean": {
      "type": "boolean",
      "allOf": [
        true,
        { "type": "boolean" }
      ]
    },
    "propArray": {
      "type": "array",
      "anyOf": [ false, true ],
      "contains": { "type": "integer" },
      "maxContains": 100,
      "minContains": 2,
      "maxItems": 100,
      "minItems": 1,
      "prefixItems": [
        { "type": "integer" }
      ],
      "items": { "type": "integer" },
      "uniqueItems": true
    },
    "propNumber": {
      "type": "number",
      "if": {
        "const": 1.5
      },
      "then": true,
      "else": true,
      "enum": [ 1.5, 0, 1 ]
    },
    "propString": {
      "type": "string",
      "maxLength": 100,
      "minLength": 0,
      "not": false,
      "pattern": "abcde"
    },
    "propInteger": {
      "$ref": "#/$defs/typeIsInteger",
      "exclusiveMaximum": 100,
      "exclusiveMinimum": 0,
      "maximum": 100,
      "minimum": 0,
      "multipleOf": 0.5,
      "oneOf": [ true, false ]
    }
  },
  "$defs": {
    "typeIsInteger": { "$ref": "http://inside#/$defs/typeIsInteger" },
    "toTestAnchor": {
      "$anchor": "test-anchor"
    },
    "toTestAnotherResourceRef": {
      "$id": "http://inside",
      "$defs": {
        "typeIsInteger": { "type": "integer" }
      }
    }
  }
}
```

*Valid benchmark data:*

```
{
  "propNull": null,
  "propBoolean": true,
  "propArray": [ 1, 2, 3, 4, 5 ],
  "propNumber": 1.5,
  "propString": "abcde",
  "propInteger": 1
}
```

*Invalid benchmark data:*

```
{
  "propNull": null,
  "propBoolean": true,
  "propArray": [ 1, 2, 3, 4, 4 ], // Two '4', duplicated
  "propNumber": 1.5,
  "propString": "abcde",
  "propInteger": 1
}
```

### Basic Usage

```
Install-Package LateApexEarlySpeed.Json.Schema
```

```csharp
string jsonSchema = File.ReadAllText("schema.json");
string instance = File.ReadAllText("instance.json");

var jsonValidator = new JsonValidator(jsonSchema);
ValidationResult validationResult = jsonValidator.Validate(instance);

if (validationResult.IsValid)
{
    Console.WriteLine("good");
}
else
{
    Console.WriteLine($"Failed keyword: {validationResult.Keyword}");
    Console.WriteLine($"ResultCode: {validationResult.ResultCode}");
    Console.WriteLine($"Error message: {validationResult.ErrorMessage}");
    Console.WriteLine($"Failed instance location: {validationResult.InstanceLocation}");
    Console.WriteLine($"Failed relative keyword location: {validationResult.RelativeKeywordLocation}");
    Console.WriteLine($"Failed schema resource base uri: {validationResult.SchemaResourceBaseUri}");
}
```

### Output Information

When validation failed, you can check detailed error information by:

- **IsValid**: As summary indicator for passed validation or failed validation.

- **ResultCode**: The specific error type when validation failed.

- **ErrorMessage**: the specific wording for human readable message

- **Keyword**: current keyword when validation failed

- **InstanceLocation**: The location of the JSON value within the instance being validated. The value is a [JSON Pointer](https://datatracker.ietf.org/doc/html/rfc6901).

- **RelativeKeywordLocation**: The relative location of the validating keyword that follows the validation path. The value is a [JSON Pointer](https://datatracker.ietf.org/doc/html/rfc6901), and it includes any by-reference applicators such as "$ref" or "$dynamicRef". Eg:
    ```
    /properties/width/$ref/minimum
    ```

- **SubSchemaRefFullUri**: The absolute, dereferenced location of the validating keyword when validation failed. The value is a full URI using the canonical URI of the relevant schema resource with a [JSON Pointer](https://datatracker.ietf.org/doc/html/rfc6901) fragment, and it doesn't include by-reference applicators such as "$ref" or "$dynamicRef" as non-terminal path components. Eg:

    ```
    https://example.com/schemas/common#/$defs/count/minimum
    ```

- **SchemaResourceBaseUri**: The absolute base URI of referenced json schema resource when validation failed. Eg:
    ```
    https://example.com/schemas/common
    ```

### <a name="performance-tips"></a> Performance Tips
Reuse instantiated JsonValidator instances (which basically represent json schema) to validate incoming json instance data if possible in your cases, to gain better performance.

### External json schema document reference support

Besides of internal sub schema resource reference (inside current json schema document) support automatically, implementation supports external schema document reference support by:

- local schema text
```csharp
var jsonValidator = new JsonValidator(jsonSchema);
string externalJsonSchema = File.ReadAllText("schema2.json");
jsonValidator.AddExternalDocument(externalJsonSchema);
ValidationResult validationResult = jsonValidator.Validate(instance);
```

- remote schema url (library will retrieve actual schema content by access network)
```csharp
var jsonValidator = new JsonValidator(jsonSchema);
await jsonValidator.AddHttpDocumentAsync(new Uri("http://this-is-json-schema-document"));
ValidationResult validationResult = jsonValidator.Validate(instance);
```

### Custom keyword support

Besides of standard keywords defined in json schema specification, library supports to create custom keyword for additional validation requirement. Eg:

```
{
  "type": "object",
  "properties": {
    "prop1": {
      "customKeyword": "Expected value"
    }
  }
}
```

```csharp
ValidationKeywordRegistry.AddKeyword<CustomKeyword>();
```

```csharp
[Keyword("customKeyword")] // It is your custom keyword name
[JsonConverter(typeof(CustomKeywordJsonConverter))] // Use 'CustomKeywordJsonConverter' to deserialize to 'CustomKeyword' instance out from json schema text
internal class CustomKeyword : KeywordBase
{
    private readonly string _customValue; // Simple example value

    public CustomKeyword(string customValue)
    {
        _customValue = customValue;
    }

    // Do your custom validation work here
    protected override ValidationResult ValidateCore(JsonInstanceElement instance, JsonSchemaOptions options)
    {
        if (instance.ValueKind != JsonValueKind.String)
        {
            return ValidationResult.ValidResult;
        }

        return instance.GetString() == _customValue
            ? ValidationResult.ValidResult
            : ValidationResult.CreateFailedResult(ResultCode.UnexpectedValue, "It is not my expected value.", options.ValidationPathStack, Name, instance.Location);
    }
}
```

```csharp
internal class CustomKeywordJsonConverter : JsonConverter<CustomKeyword>
{
    // Library will input json value of your custom keyword: "customKeyword" to this method.
    public override CustomKeyword? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // Briefly: 
        return new CustomKeyword(reader.GetString()!);
    }

    public override void Write(Utf8JsonWriter writer, CustomKeyword value, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }
}
```

### Format support

This library supports following formats currently: 
- uri
- uri-reference
- date
- time
- date-time
- email
- uuid
- hostname
- ipv4
- ipv6
- json-pointer
- regex

If require more format, implement an custom FormatValidator, and register it:

```csharp
[Format("custom_format")] // this is your custom format name in json schema
public class TestCustomFormatValidator : FormatValidator
{
    public override bool Validate(string content)
    {
        // custom format validation logic here...
    }
}

// register it globally
FormatRegistry.AddFormatType<TestCustomFormatValidator>();
```

### Other extension usage doc is to be continued .

### Limitation

- This implementation focuses on json schema validation, currently not support Annotation
- Due to lack Annotation support, it also not support following keywords: unevaluatedProperties, unevaluatedItems
- Not support content-encoded string currently

## Fluent schema builder

If not familiar with Standard Json schema, recommend using fluent configuration. The fluent configuration is designed not completely align with standard Json schema keywords interface. The standard Json schema is a powerful and flexiable language to specify json shape, but most developers may be more familiar with "stronger type" smell. For this, library's fluent configuration will (in most cases) firstly "ask" developers what json type they want, then continue "ask" subsequence validation requirements which are scoped based on known json type. 

By doing that, developers may have more friendly method invoke chains and will not be easy to make mistake because some standard json schema keywords only have functionalities on specific type. Also, because validation methods is on specific json type builder, so those validation methods can be designed to accept concret .net type rather than raw json. 

Lateapexearlyspeed.Json.Schema package extends standard keywords for some of fluent validation requirement and provide fluent validation.

```csharp
    var b = new JsonSchemaBuilder();
    b.IsJsonObject()
                .HasProperty("A", b => b.IsJsonString().HasMinLength(5))
                .HasProperty("B", b => b.IsJsonNumber().IsGreaterThan(1).IsLessThan(10))
                .HasProperty("C", b => b.IsJsonArray().HasMinLength(5).HasItems(b =>
                {
                    b.NotJsonNull();
                }))
                .HasProperty("D", b => b.Or(
                        b => b.IsJsonFalse(),
                        b => b.IsJsonNumber().Equal(0),
                        b => b.IsJsonObject().HasCustomValidation((JsonElement element) => element.GetProperty("Prop").ValueKind == JsonValueKind.True, 
                            jsonElement => $"Cannot pass my custom validation, data is {jsonElement}")
                    )
                );
    JsonValidator jsonValidator = b.BuildValidator();
    jsonValidator.Validate(...);
```

### <a name="available-fluent-validation-methods"></a> Available fluent validation methods

- NotJsonNull
- IsJsonTrue
- IsJsonFalse
- IsJsonBoolean
- IsJsonNull
- IsJsonString:
  - Equal
  - IsIn
  - HasMaxLength
  - HasMinLength
  - HasPattern
  - HasCustomValidation
- IsJsonNumber:
  - Equal
  - IsIn
  - IsGreaterThan
  - IsLessThan
  - NotGreaterThan
  - NotLessThan
  - MultipleOf
  - HasCustomValidation
- IsJsonArray:
  - SerializationEquivalent
  - HasItems
  - HasLength
  - HasMaxLength
  - HasMinLength
  - HasUniqueItems
  - HasCustomValidation
  - Contains
  - NotContains
  - Equivalent
- IsJsonObject:
  - SerializationEquivalent
  - HasProperty
  - HasCustomValidation
  - Equivalent
  - HasNoProperty
- Or

There are HasCustomValidation() overloads which can be used to create custom validation logic unit.

## Validator generation from code

Besides of user-provided json schema, this library also supports to generate validator from code.

### Basic usage

```csharp
JsonValidator validator = JsonSchemaGenerator.GenerateJsonValidator<TestClass>();

// Now use validator instance as normal
```

### Supported .net types by now

Numeric types: byte, sbyte, short, ushort, int, uint, long, ulong, float, double, decimal.

Boolean, String, Dictionary<string,TAny>, JsonElement, JsonDocument, JsonNode, JsonValue, JsonArray, JsonObject, generic type of IEnumerable<TAny>, Enum, Guid, Uri, DateTimeOffset, DateTime, Nullable value type (generic type of Nullable<TValue>), Custom object.

### Supported validation attributes by now

Besides of generating validator by type, you can also indicate more constraint by Attributes, check their constructors arguments then you can get usage:

- EmailAttribute
- ExclusiveMaximumAttribute
- ExclusiveMinimumAttribute
- MaximumAttribute
- MinimumAttribute
- MultipleOfAttribute
- StringEnumAttribute
- IntegerEnumAttribute
- IPv4Attribute
- IPv6Attribute
- LengthRangeAttribute (for both string length and array length)
- MaxLengthAttribute (for both string length and array length)
- MinLengthAttribute (for both string length and array length)
- UniqueItemsAttribute (for array)
- NumberRangeAttribute
- PatternAttribute (for string)

### Usage:

```csharp
class TestClass
{
    [Maximum(2)]
    public int Prop { get; set; }

    [LengthRange(10, 20)]
    [Pattern("*abc*")]
    public string StringProp { get; set; }
}
```

### Nullable consideration

By default, this library will treat all .net reference types as be nullable. If you would mark some reference typed properties as be not nullable, annotate those properties with `[LateApexEarlySpeed.Json.Schema.Generator.NotNullAttribute]`

### Required or ignored

By default, this library will validate value of specified properties when find out those properties from json.

If you would mark some properties as required properties in json, annotate them with `[System.Text.Json.Serialization.JsonRequiredAttribute]` or `[System.ComponentModel.DataAnnotations.RequiredAttribute]`

If you would mark some properties as explicit ignored ones (that is, ignore validation even if they appear in json), annotate them with `[System.Text.Json.Serialization.JsonIgnoreAttribute]`

This library uses .net core built-in attributes for these requirement, so that user code can have consistent experience.

### Custom property name

As what System.Text.Json supports for property name, this library supports user custom property name by attributes or options:

System.Text.Json.Serialization.JsonPropertyNameAttribute:

```csharp
class CustomNamedPropertyTestClass
    {
        [JsonPropertyName("NewPropName")]
        public int Prop { get; set; }
    }
```

JsonSchemaNamingPolicy options:
- JsonSchemaNamingPolicy.CamelCase:
First word starts with a lower case character. Successive words start with an uppercase character. TempCelsius	=> tempCelsius 

- JsonSchemaNamingPolicy.KebabCaseLower: Words are separated by hyphens. All characters are lowercase. TempCelsius	-> temp-celsius

- JsonSchemaNamingPolicy.KebabCaseUpper: Words are separated by hyphens. All characters are uppercase. TempCelsius	=> TEMP-CELSIUS

- JsonSchemaNamingPolicy.SnakeCaseLower: Words are separated by underscores. All characters are lowercase. TempCelsius	-> temp_celsius

- JsonSchemaNamingPolicy.SnakeCaseUpper: Words are separated by underscores. All characters are uppercase. TempCelsius	-> TEMP_CELSIUS

- JsonSchemaNamingPolicy.SharedDefault: default option, not change original property name 

- write your own JsonSchemaNamingPolicy:
```csharp
internal class YourNamingPolicy : JsonSchemaNamingPolicy
{
    public override string ConvertName(string name)
    {
        // convert and return new name.
    }
}
```

### Usage:
```csharp
JsonValidator validator = JsonSchemaGenerator.GenerateJsonValidator(type, new JsonSchemaGeneratorOptions { PropertyNamingPolicy = JsonSchemaNamingPolicy.CamelCase }));
```

Note: when specify both JsonPropertyNameAttribute on specific property and custom PropertyNamingPolicy in option, JsonPropertyNameAttribute will take higher priority over option for that property.

## Issue report

Welcome to raise issue and wishlist, I will try to fix if make sense, thanks !

## More doc is to be written
