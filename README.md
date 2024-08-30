<p align="center">
    <img height="60" src="https://github.com/lateapexearlyspeed/Lateapexearlyspeed.JsonSchema/blob/master/LateApexEarlySpeed.Json.Schema/nuget.icon.jpg?raw=true">
</p>
<h1 align="center">Lateapexearlyspeed.Json.Schema</h1>
<p align="center">To protect and validate your JSON</p>

<p align="center">
    <img width="100%" src="https://github.com/lateapexearlyspeed/Lateapexearlyspeed.JsonSchema/blob/master/splash-image.png?raw=true">
</p>

![GitHub Actions Workflow Status](https://img.shields.io/github/actions/workflow/status/lateapexearlyspeed/Lateapexearlyspeed.JsonSchema/cicd.yml)
![GitHub top language](https://img.shields.io/github/languages/top/lateapexearlyspeed/Lateapexearlyspeed.JsonSchema)
![NuGet Version](https://img.shields.io/nuget/v/LateApexEarlySpeed.Json.Schema?label=LateApexEarlySpeed.Json.Schema)
![NuGet Version](https://img.shields.io/nuget/v/LateApexEarlySpeed.Xunit.Assertion.Json?label=LateApexEarlySpeed.Xunit.Assertion.Json)
![NuGet Version](https://img.shields.io/nuget/v/LateApexEarlySpeed.EntityFrameworkCore.V6.Json.Schema?label=LateApexEarlySpeed.EntityFrameworkCore.V6.Json.Schema)

# What is all in Lateapexearlyspeed.JsonSchema

This repository is about json validation, the wish is to help develop json related validation easily. 

The core library is 'Lateapexearlyspeed.Json.Schema' which is a simple and high performance json schema implementation for .Net, and also contains other convenient helper to develop validation code rather than write standard json schema like: fluent json validation and validator generation from your type.

Based on the core library, there are Entityframework core extensions which has solution of json column validation to protect your database in application side and a Xunit assertion extensions which has powerful json assertion capability.

nuget packages:

<table>
<tbody>
<tr>
<td>Lateapexearlyspeed.Json.Schema<br><a href="https://www.nuget.org/packages/LateApexEarlySpeed.Json.Schema/"><img alt="NuGet version" src="https://img.shields.io/nuget/v/LateApexEarlySpeed.Json.Schema"></img></a></td>
<td>Core library of json schema and other validation helpers - see <a href="https://github.com/lateapexearlyspeed/Lateapexearlyspeed.JsonSchema?tab=readme-ov-file#lateapexearlyspeedjsonschema-1">doc</a></td>
</tr>
<tr>
<td>LateApexEarlySpeed.Xunit.Assertion.Json<br><a href="https://www.nuget.org/packages/LateApexEarlySpeed.Xunit.Assertion.Json"><img alt="NuGet version" src="https://img.shields.io/nuget/v/LateApexEarlySpeed.Xunit.Assertion.Json"></img></a></td>
<td>Xunit assertion extension for json - see <a href="https://github.com/lateapexearlyspeed/Lateapexearlyspeed.JsonSchema?tab=readme-ov-file#lateapexearlyspeedxunitassertionjson">doc</a></td>
</tr>
<tr>
<td>LateApexEarlySpeed.EntityFrameworkCore.V6.Json.Schema<br><a href="https://www.nuget.org/packages/LateApexEarlySpeed.EntityFrameworkCore.V6.Json.Schema/"><img alt="NuGet version" src="https://img.shields.io/nuget/v/LateApexEarlySpeed.EntityFrameworkCore.V6.Json.Schema"></img></a></td>
<td>Json column validation for Entityframework core v6+ - see <a href="https://github.com/lateapexearlyspeed/Lateapexearlyspeed.JsonSchema?tab=readme-ov-file#lateapexearlyspeedentityframeworkcorev6jsonschema--lateapexearlyspeedentityframeworkcorev3jsonschema">doc</a></td>
</tr>
<tr>
<td>LateApexEarlySpeed.EntityFrameworkCore.V3.Json.Schema<br><a href="https://www.nuget.org/packages/LateApexEarlySpeed.EntityFrameworkCore.V3.Json.Schema/"><img alt="NuGet version" src="https://img.shields.io/nuget/v/LateApexEarlySpeed.EntityFrameworkCore.V3.Json.Schema"></img></a></td>
<td>Json column validation for Entityframework core v3 - see <a href="https://github.com/lateapexearlyspeed/Lateapexearlyspeed.JsonSchema?tab=readme-ov-file#lateapexearlyspeedentityframeworkcorev6jsonschema--lateapexearlyspeedentityframeworkcorev3jsonschema">doc</a></td>
</tr>
</tbody>
</table>

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
```

Output Information in [wiki](https://github.com/lateapexearlyspeed/Lateapexearlyspeed.JsonSchema/wiki/Validation-Output)

External json schema document reference support, please check [wiki](https://github.com/lateapexearlyspeed/Lateapexearlyspeed.JsonSchema/wiki/External-json-schema-document-reference)

Custom keyword support, please check [wiki](https://github.com/lateapexearlyspeed/Lateapexearlyspeed.JsonSchema/wiki/Custom-keyword-support)

More schema validation options like case-insensitive property names matching, Regex cache and so on, please check [wiki](https://github.com/lateapexearlyspeed/Lateapexearlyspeed.JsonSchema/wiki/More-validation-options)

### Fluent schema builder

If not familiar with Standard Json schema, recommend using fluent configuration. The fluent configuration is designed not completely align with standard Json schema keywords interface. The standard Json schema is a powerful and flexiable language to specify json shape, but most developers may be more familiar with "stronger type" smell. For this, library's fluent configuration will (in most cases) firstly "ask" developers what json type they want, then continue "ask" subsequence validation requirements which are scoped based on known json type. 

By doing that, developers may have more friendly method invoke chains and will not be easy to make mistake because some standard json schema keywords only have functionalities on specific type. Also, because validation methods is on specific json type builder, so those validation methods can be designed to accept concret .net type rather than raw json.

```csharp
    var b = new JsonSchemaBuilder();
    b.ObjectHasProperty("A", b => b.IsJsonString().HasMinLength(5))
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

For available fluent validation methods and custom methods, please check [wiki](https://github.com/lateapexearlyspeed/Lateapexearlyspeed.JsonSchema/wiki/Fluent-schema-builder#available-fluent-validation-methods)

### Validator generation from code

Besides of user-provided json schema, this library also supports to generate validator from code.

```csharp
JsonValidator validator = JsonSchemaGenerator.GenerateJsonValidator<TestClass>();

// Now use validator instance as normal
```

More detailed info and custom usage about validator generation from code, please check [wiki](https://github.com/lateapexearlyspeed/Lateapexearlyspeed.JsonSchema/wiki/Validator-generation-from-type)

### Output standard Json schema text from JsonValidator

As we can see as above, there are multiple ways to construct JsonValidator to do validation from: standard Json schema, fluent builder & .net types.

Now this library can also output standard Json schema text from JsonValidator instance.

```csharp
string standardJsonSchema = jsonValidator.GetStandardJsonSchemaText();
```

**Note:** to support flexible fluent builder and ".net type code first" construction manners, internally this library tries best to still use standard keywords but some Build methods have to involve "extend" keywords, so when you have built a JsonValidator instance by using those Build methods,  ```GetStandardJsonSchemaText()``` on this instance will not be supported because these extend keywords cannot apply to standard Json schema and recorgnized by other application:

```csharp
var builder = new JsonSchemaBuilder();
builder.IsJsonNumber().HasCustomValidation((double _) => true, _ => "");
JsonValidator jsonValidator = builder.BuildValidator();

Assert.Throws<NotSupportedException>(() => jsonValidator.GetStandardJsonSchemaText());
```

Extend keyword involved build method list is [here](https://github.com/lateapexearlyspeed/Lateapexearlyspeed.JsonSchema/wiki/Output-standard-Json-schema-text-from-JsonValidator).

## LateApexEarlySpeed.Xunit.Assertion.Json

There were already json related test assertion libraries, most ones were asserting json's equivalent.

However, imagine what you would test is other factors besides of equivalent on whole json, remember the assertion methods in Xunit (Contains, AllOf, IsTypeOf ..) at ANY json node location.

This library can assert json node data in any node location, not only against whole json. Also, you won't write multiple assertion code lines for different json node, you just need to have one JsonAssertion.Meet(...) method then inside it, you specify what you would test, all in one place. When assertion failed, library will throw detailed assert exception which reports failed json node location. 

```csharp
JsonAssertion.Meet(b => b.ObjectHasProperty("p1", b => b.IsJsonObject())
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

check available validation methods in [wiki](https://github.com/lateapexearlyspeed/Lateapexearlyspeed.JsonSchema/wiki/Fluent-schema-builder#available-fluent-validation-methods).

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

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<Blog>()
        .Property(b => b.JsonContent).HasJsonValidation(b =>
        {
            b.ObjectHasProperty("A", b => b.IsJsonString().HasMinLength(5))
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

Available validations for json column: check [wiki](https://github.com/lateapexearlyspeed/Lateapexearlyspeed.JsonSchema/wiki/Fluent-schema-builder#available-fluent-validation-methods).

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
