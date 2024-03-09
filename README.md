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

# What is all in Lateapexearlyspeed.JsonSchema

This repository is about json validation, the wish is to help develop json related validation easily. 

The core library is 'Lateapexearlyspeed.Json.Schema' which is a simple and high performance json schema implementation for .Net, and also contains other convenient helper to develop validation code rather than write standard json schema like: fluent json validation and validator generation from your type.

Based on the core library, there are Entityframework core extensions which has solution of json column validation to protect your database in application side and a Xunit assertion extensions which has powerful json assertion capability.

##### nuget packages:

<table>
<tbody>
<tr>
<td>Lateapexearlyspeed.Json.Schema<br><a href="https://www.nuget.org/packages/LateApexEarlySpeed.Json.Schema/"><img alt="NuGet version" src="https://img.shields.io/nuget/v/LateApexEarlySpeed.Json.Schema"></img></a></td>
<td>Core library of json schema and other validation helpers - see <a href="https://github.com/lateapexearlyspeed/Lateapexearlyspeed.JsonSchema#lateapexearlyspeedjsonschema-1">doc</a></td>
</tr>
<tr>
<td>LateApexEarlySpeed.Xunit.Assertion.Json<br><a href="https://www.nuget.org/packages/LateApexEarlySpeed.Xunit.Assertion.Json"><img alt="NuGet version" src="https://img.shields.io/nuget/v/LateApexEarlySpeed.Xunit.Assertion.Json"></img></a></td>
<td>Xunit assertion extension for json - see <a href="https://github.com/lateapexearlyspeed/Lateapexearlyspeed.JsonSchema#-lateapexearlyspeedxunitassertionjson">doc</a></td>
</tr>
<tr>
<td>LateApexEarlySpeed.EntityFrameworkCore.V6.Json.Schema<br><a href="https://www.nuget.org/packages/LateApexEarlySpeed.EntityFrameworkCore.V6.Json.Schema/"><img alt="NuGet version" src="https://img.shields.io/nuget/v/LateApexEarlySpeed.EntityFrameworkCore.V6.Json.Schema"></img></a></td>
<td>Json column validation for Entityframework core v6+ - see <a href="https://github.com/lateapexearlyspeed/Lateapexearlyspeed.JsonSchema#-lateapexearlyspeedentityframeworkcorev6jsonschema--lateapexearlyspeedentityframeworkcorev3jsonschema">doc</a></td>
</tr>
<tr>
<td>LateApexEarlySpeed.EntityFrameworkCore.V3.Json.Schema<br><a href="https://www.nuget.org/packages/LateApexEarlySpeed.EntityFrameworkCore.V3.Json.Schema/"><img alt="NuGet version" src="https://img.shields.io/nuget/v/LateApexEarlySpeed.EntityFrameworkCore.V3.Json.Schema"></img></a></td>
<td>Json column validation for Entityframework core v3 - see <a href="https://github.com/lateapexearlyspeed/Lateapexearlyspeed.JsonSchema#-lateapexearlyspeedentityframeworkcorev6jsonschema--lateapexearlyspeedentityframeworkcorev3jsonschema">doc</a></td>
</tr>
</tbody>
</table>

## <a name="xunit-assertion-json"></a> LateApexEarlySpeed.Xunit.Assertion.Json

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

## <a name="entityframework-core-json"></a> LateApexEarlySpeed.EntityFrameworkCore.V6.Json.Schema & LateApexEarlySpeed.EntityFrameworkCore.V3.Json.Schema

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

This is a high performance Json schema .Net implementation library based on [Json schema](https://json-schema.org/) - draft 2020.12 (latest one by 2023.12). The json validation functionalities have passed [official json schema test-suite](https://github.com/json-schema-org/JSON-Schema-Test-Suite) for draft 2020.12 (except cases about limitation listed below)

This library also supports fluent validation and validator generation from your class code.

**High performance** - this .Net library has good performance compared with existing more popular and excellent .Net implementations in common cases by BenchmarkDotnet [result](https://github.com/lateapexearlyspeed/Lateapexearlyspeed.JsonSchema/wiki/Performance).

### Basic Usage

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

Output Information [here](https://github.com/lateapexearlyspeed/Lateapexearlyspeed.JsonSchema/wiki/Validation-Output)

#### External json schema document reference support

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

#### Custom keyword support

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

### Fluent schema builder

If not familiar with Standard Json schema, recommend using fluent configuration. The fluent configuration is designed not completely align with standard Json schema keywords interface. The standard Json schema is a powerful and flexiable language to specify json shape, but most developers may be more familiar with "stronger type" smell. For this, library's fluent configuration will (in most cases) firstly "ask" developers what json type they want, then continue "ask" subsequence validation requirements which are scoped based on known json type. 

By doing that, developers may have more friendly method invoke chains and will not be easy to make mistake because some standard json schema keywords only have functionalities on specific type. Also, because validation methods is on specific json type builder, so those validation methods can be designed to accept concret .net type rather than raw json.

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

### Validator generation from code

Besides of user-provided json schema, this library also supports to generate validator from code.

```csharp
JsonValidator validator = JsonSchemaGenerator.GenerateJsonValidator<TestClass>();

// Now use validator instance as normal
```

#### Supported .net types by now

Numeric types: byte, sbyte, short, ushort, int, uint, long, ulong, float, double, decimal.

Boolean, String, Dictionary<string,TAny>, JsonElement, JsonDocument, JsonNode, JsonValue, JsonArray, JsonObject, generic type of IEnumerable<TAny>, Enum, Guid, Uri, DateTimeOffset, DateTime, Nullable value type (generic type of Nullable<TValue>), Custom object.

#### Supported validation attributes by now

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

Example:

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

More custom usage for validator generation from code, please check [wiki](https://github.com/lateapexearlyspeed/Lateapexearlyspeed.JsonSchema/wiki/Validator-generation-from-type)

## Issue report

Welcome to raise issue and wishlist, I will try to fix if make sense, thanks !
