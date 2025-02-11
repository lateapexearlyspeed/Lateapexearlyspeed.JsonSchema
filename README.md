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
![NuGet Version](https://img.shields.io/nuget/v/LateApexEarlySpeed.Xunit.V3.Assertion.Json?label=LateApexEarlySpeed.Xunit.V3.Assertion.Json)
![NuGet Version](https://img.shields.io/nuget/v/LateApexEarlySpeed.Nullability.Generic?label=LateApexEarlySpeed.Nullability.Generic)
![NuGet Version](https://img.shields.io/nuget/v/JsonQuery.Net?label=JsonQuery.Net)

# What is all in Lateapexearlyspeed.JsonSchema

This repository is about json validation, the wish is to help develop json related validation easily. 

The core library is 'Lateapexearlyspeed.Json.Schema' which is a simple and high performance json schema implementation for .Net, and also contains other convenient helper to develop validation code rather than write standard json schema like: fluent json validation and validator generation from your type.

Based on the core library, there is a Xunit assertion extensions which has powerful json assertion capability.

During developing core library, there are some basic requirement which turns to other reusable library, like: dealing with Nullability info of type (even if it is generic type).

There is also [JsonQuery](https://jsonquerylang.org/) .Net implementation library 'JsonQuery.Net'.

nuget packages:

<table>
<tbody>
<tr>
<td>Lateapexearlyspeed.Json.Schema<br><a href="https://www.nuget.org/packages/LateApexEarlySpeed.Json.Schema/"><img alt="NuGet version" src="https://img.shields.io/nuget/v/LateApexEarlySpeed.Json.Schema"></img></a></td>
<td>Core library of json schema and other validation helpers - see <a href="https://github.com/lateapexearlyspeed/Lateapexearlyspeed.JsonSchema?tab=readme-ov-file#lateapexearlyspeedjsonschema-1">doc</a></td>
</tr>
<tr>
<td>LateApexEarlySpeed.Xunit.Assertion.Json<br><a href="https://www.nuget.org/packages/LateApexEarlySpeed.Xunit.Assertion.Json"><img alt="NuGet version" src="https://img.shields.io/nuget/v/LateApexEarlySpeed.Xunit.Assertion.Json"></img></a></td>
<td>Xunit v2 assertion extension for json - see <a href="https://github.com/lateapexearlyspeed/Lateapexearlyspeed.JsonSchema?tab=readme-ov-file#lateapexearlyspeedxunitassertionjson">doc</a></td>
</tr>
<tr>
<td>LateApexEarlySpeed.Xunit.V3.Assertion.Json<br><a href="https://www.nuget.org/packages/LateApexEarlySpeed.Xunit.V3.Assertion.Json"><img alt="NuGet version" src="https://img.shields.io/nuget/v/LateApexEarlySpeed.Xunit.V3.Assertion.Json"></img></a></td>
<td>Xunit v3 assertion extension for json - see <a href="https://github.com/lateapexearlyspeed/Lateapexearlyspeed.JsonSchema?tab=readme-ov-file#lateapexearlyspeedxunitassertionjson">doc</a></td>
</tr>
<tr>
<td>LateApexEarlySpeed.Nullability.Generic<br><a href="https://www.nuget.org/packages/LateApexEarlySpeed.Nullability.Generic/"><img alt="NuGet version" src="https://img.shields.io/nuget/v/LateApexEarlySpeed.Nullability.Generic"></img></a></td>
<td>Nullability info reader including for generic type - see <a href="https://github.com/lateapexearlyspeed/Lateapexearlyspeed.JsonSchema?tab=readme-ov-file#lateapexearlyspeednullabilitygeneric">doc</a></td>
</tr>
<tr>
<td>JsonQuery.Net<br><a href="https://www.nuget.org/packages/JsonQuery.Net/"><img alt="NuGet version" src="https://img.shields.io/nuget/v/JsonQuery.Net"></img></a></td>
<td>JsonQuery .net implementation - see <a href="https://github.com/lateapexearlyspeed/Lateapexearlyspeed.JsonSchema?tab=readme-ov-file#jsonquerynet">doc</a></td>
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

[xunit v2 extension package](https://www.nuget.org/packages/LateApexEarlySpeed.Xunit.Assertion.Json)

[xunit v3 extension package](https://www.nuget.org/packages/LateApexEarlySpeed.Xunit.V3.Assertion.Json)

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

## LateApexEarlySpeed.Nullability.Generic

### Intro

Starting from .net 6, there is built-in Nullability related classes to help read nullability annotation info on members of type (`NullabilityInfoContext` and so on). However, it is not possible to get annotated nullability state of members and parameters if their types are from generic type arguments:

```csharp
class Class1
{
    public GenericClass<string> Property { get; }
}

class GenericClass<T>
{
    public T Property { get; }
}

PropertyInfo property = typeof(Class1).GetProperty("Property")!.PropertyType.GetProperty("Property")!;
NullabilityInfo state = new NullabilityInfoContext().Create(property);
Assert.Equal(NullabilityState.Nullable, state.ReadState); // expected nullability state of ‘string’ property is NotNull
```

this library can get NotNull state for this 'string' property whose type is from generic type argument:

```csharp
NullabilityPropertyInfo propertyInfo = NullabilityType.GetType(typeof(Class1)).GetProperty("Property")!.NullabilityPropertyType.GetProperty("Property")!;
Assert.Equal(NullabilityState.NotNull, propertyInfo.NullabilityReadState);
```

There is no information to help infer nullability info of generic type arguments of 'root' type, so if 'root' type is generic type, this library accepts explicit nullability info of generic type arguments for 'root' type and then process all properties, fields, parameters as normal:

```csharp
NullabilityPropertyInfo propertyInfo = NullabilityType.GetType(typeof(GenericClass<string>), NullabilityState.NotNull).GetProperty("Property")!;
Assert.Equal(NullabilityState.NotNull, propertyInfo.NullabilityReadState);
```

even if with nested properties:

```csharp
class GenericClass<T>
{
    public GenericClass2<int?, T> Property { get; }
}

class GenericClass2<T1, T2>
{
    public T1 Property1 { get; }
    public T2 Property2 { get; }
    public string? Property3 { get; }
}

NullabilityPropertyInfo property = NullabilityType.GetType(typeof(GenericClass<string>), NullabilityState.NotNull).GetProperty("Property")!;
Assert.Equal(NullabilityState.NotNull, property.NullabilityReadState); // GenericClass2<int?, T>

NullabilityType type = property.NullabilityPropertyType;
Assert.Equal(NullabilityState.Nullable, type.GetProperty("Property1")!.NullabilityReadState); // int?
Assert.Equal(NullabilityState.NotNull, type.GetProperty("Property2")!.NullabilityReadState); // string
Assert.Equal(NullabilityState.Nullable, type.GetProperty("Property3")!.NullabilityReadState); // string?
```

Library also supports info of fields and parameters, take parameter as example:

```csharp
        class Class1
        {
            public GenericClass<string, string?, int, int?> Property { get; }
        }

        class GenericClass<T1, T2, T3, T4>
        {
            public GenericClass2<T1, T2>? Function(T2 p0, T3 p1, T4 p2, string p3, string? p4)
            {
                throw new NotImplementedException();
            }
        }

        class GenericClass2<T1, T2>
        {
            public T1 Property1 { get; }
            public T2 Property2 { get; }
            public string? Property3 { get; }
        }

        NullabilityMethodInfo method = NullabilityType.GetType(typeof(Class1)).GetProperty("Property")!.NullabilityPropertyType.GetMethod("Function")!;

        // GenericClass2<T1, T2>?
        NullabilityParameterInfo returnParameter = method.NullabilityReturnParameter;
        Assert.Equal(NullabilityState.Nullable, returnParameter.NullabilityState);
        NullabilityType returnType = returnParameter.NullabilityParameterType;
        Assert.Equal(NullabilityState.NotNull, returnType.GenericTypeArguments[0].NullabilityState); // string
        Assert.Equal(NullabilityState.Nullable, returnType.GenericTypeArguments[1].NullabilityState); // string?

        Assert.Equal(NullabilityState.NotNull, returnType.GetProperty("Property1")!.NullabilityReadState); // string
        Assert.Equal(NullabilityState.Nullable, returnType.GetProperty("Property2")!.NullabilityReadState); // string?

        // string? p0, int p1, int? p2, string p3, string? p4
        NullabilityParameterInfo[] parameters = method.GetNullabilityParameters();
        Assert.Equal(NullabilityState.Nullable, parameters[0].NullabilityState);
        Assert.Equal(NullabilityState.NotNull, parameters[1].NullabilityState);
        Assert.Equal(NullabilityState.Nullable, parameters[2].NullabilityState);
        Assert.Equal(NullabilityState.NotNull, parameters[3].NullabilityState);
        Assert.Equal(NullabilityState.Nullable, parameters[4].NullabilityState);
```

Calling entrypoint is static method `NullabilityType.GetType()` which has 3 overloads:

```csharp
NullabilityType GetType(Type type) // when type itself is not generic type
NullabilityType GetType(Type type, params NullabilityState[] genericTypeArgumentsNullabilities) // when type is generic type is generic type and its generic type arguments are not generic types
NullabilityType GetType(Type type, params NullabilityElement[] genericTypeArgumentsNullabilities) // when type is generic type and its generic type arguments are also generic type
```

More API doc see [wiki](https://github.com/lateapexearlyspeed/Lateapexearlyspeed.JsonSchema/wiki/LateApexEarlySpeed.Nullability.Generic).

### Thread Safety

All types under namespace 'LateApexEarlySpeed.Nullability.Generic' are thread safe.

### Performance consideration

To reduce reflection process time, you can reuse `NullabilityType` instance for specific type. 

In that case, multiple calls on memberInfo related methods on same `NullabilityType` instance will return same memberInfo instance for each memberInfo. E.g.:

```csharp
NullabilityType type = NullabilityType.GetType(typeof(TestClass));
NullabilityPropertyInfo? propertyInfo = type.GetProperty("PropName");
// Later
NullabilityPropertyInfo? propertyInfo2 = type.GetProperty("PropName"); // propertyInfo2 is same instance as propertyInfo
```

Also retrieving nullability type on specific memberInfo will return same `NullabilityType` instance... so that all members tree for 'root' type are cached.

E.g.:
```csharp
Assert.Same(propertyInfo.NullabilityPropertyType, propertyInfo2.NullabilityPropertyType); // NullabilityPropertyType property will always return same instance
```

There are thread safe caches in nullability related types. (This is another difference with standard .net core `NullabilityInfoContext` which is not thread safe)

## JsonQuery.Net

This is [JsonQuery](https://jsonquerylang.org/) .Net implementation library, which passed official [test suite](https://github.com/jsonquerylang/jsonquery/blob/develop/test-suite/README.md). 

For syntax and advantages of JsonQuery, check [official page](https://github.com/jsonquerylang/jsonquery).

#### Compile json format query to query engine:

```csharp
IJsonQueryable queryable = JsonQueryable.Compile(jsonFormatQuery);
```

#### Parse json query statement to query engine:

```csharp
IJsonQueryable queryable = JsonQueryable.Parse(jsonQuery);
```

#### Execute query against json data by query engine:

```csharp
JsonNode? result = queryable.Query(jsonData);
```

#### Serialize query engine itself to json format

```csharp
string jsonFormatQuery = queryable.SerializeToJsonFormat();
```

#### More functionalities later.
