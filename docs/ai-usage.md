# AI Usage Guide

This guide is for users and AI assistants applying these three public packages:

- `LateApexEarlySpeed.Json.Schema`
- `LateApexEarlySpeed.Xunit.Assertion.Json`
- `LateApexEarlySpeed.Xunit.V3.Assertion.Json`

It deliberately excludes EF Core packages, JsonQuery.Net, benchmark projects, and repository-development agent instructions.

## Choose Your Path

| User intent | Package | Main API | Start from |
| --- | --- | --- | --- |
| Validate JSON against schema text | `LateApexEarlySpeed.Json.Schema` | `JsonValidator` | `new JsonValidator(schema).Validate(instance)` |
| Validate many instances with one schema | `LateApexEarlySpeed.Json.Schema` | `JsonValidator` | Create one validator and reuse it |
| Collect all validation errors | `LateApexEarlySpeed.Json.Schema` | `JsonSchemaOptions` | `OutputFormat = OutputFormat.List` |
| Select Draft 7, 2019-09, or 2020-12 when `$schema` is missing | `LateApexEarlySpeed.Json.Schema` | `JsonValidatorOptions` | `DefaultDialect = DialectKind.Draft7` |
| Build validation rules in C# | `LateApexEarlySpeed.Json.Schema` | `JsonSchemaBuilder` | Configure one builder, then call `BuildValidator()` |
| Generate a validator from a .NET type | `LateApexEarlySpeed.Json.Schema` | `JsonSchemaGenerator` | `JsonSchemaGenerator.GenerateJsonValidator<T>()` |
| Add custom string format validation | `LateApexEarlySpeed.Json.Schema` | `FormatRegistry` | `options.FormatRegistry.AddFormat(...)` |
| Collect schema annotations | `LateApexEarlySpeed.Json.Schema` | `ValidationResult.Annotations` | `CollectAnnotations = true` and `OutputFormat.List` |
| Assert JSON shape or values in xUnit v2 | `LateApexEarlySpeed.Xunit.Assertion.Json` | `JsonAssertion.Meet` | Use the v2 assertion namespace |
| Assert JSON shape or values in xUnit v3 | `LateApexEarlySpeed.Xunit.V3.Assertion.Json` | `JsonAssertion.Meet` | Use the v3 assertion namespace |
| Assert JSON equivalence in tests | xUnit v2 or v3 assertion package | `JsonAssertion.Equivalent` | Choose package by xUnit version |

## Core Validation Quickstart

```csharp
using LateApexEarlySpeed.Json.Schema;
using LateApexEarlySpeed.Json.Schema.Common;

string schema = """
                {
                  "type": "object",
                  "required": [ "id", "email" ],
                  "properties": {
                    "id": { "type": "integer", "minimum": 1 },
                    "email": { "type": "string", "format": "email" }
                  },
                  "additionalProperties": false
                }
                """;

var validator = new JsonValidator(schema);
ValidationResult result = validator.Validate("""{ "id": 10, "email": "user@example.com" }""");

if (!result.IsValid)
{
    foreach (ValidationError error in result.ValidationErrors)
    {
        Console.WriteLine($"{error.Keyword}: {error.ErrorMessage} at {error.InstanceLocation}");
    }
}
```

For repeated validation, keep the `JsonValidator` and call `Validate` many times. Do not create a new validator for every JSON instance unless the schema changes.

## Validation Options

Use `JsonValidatorOptions` when creating the validator and `JsonSchemaOptions` when validating one instance.

```csharp
using LateApexEarlySpeed.Json.Schema;
using LateApexEarlySpeed.Json.Schema.Common;
using LateApexEarlySpeed.Json.Schema.Keywords;

var validator = new JsonValidator(schema, new JsonValidatorOptions
{
    DefaultDialect = DialectKind.Draft7,
    PropertyNameCaseInsensitive = true
});

ValidationResult result = validator.Validate(instance, new JsonSchemaOptions
{
    OutputFormat = OutputFormat.List,
    ValidateFormat = true,
    GenerateErrorMessages = true
});
```

## Fluent Builder Quickstart

Use `JsonSchemaBuilder` when users prefer C# validation rules over raw JSON Schema text.

```csharp
using LateApexEarlySpeed.Json.Schema;
using LateApexEarlySpeed.Json.Schema.FluentGenerator;

var builder = new JsonSchemaBuilder();

builder.IsJsonObject()
    .HasProperty("customerId", b => b.IsJsonString().HasMinLength(1))
    .HasProperty("items", b => b.IsJsonArray()
        .HasMinLength(1)
        .HasItems(item => item.IsJsonObject()
            .HasProperty("sku", p => p.IsJsonString().HasPattern("^[A-Z0-9-]+$"))
            .HasProperty("quantity", p => p.IsJsonNumber().IsGreaterThan(0))));

JsonValidator validator = builder.BuildValidator();
ValidationResult result = validator.Validate("""
                                          {
                                            "customerId": "C-100",
                                            "items": [ { "sku": "ABC-1", "quantity": 2 } ]
                                          }
                                          """);
```

Configure each `JsonSchemaBuilder` instance once, then call `BuildValidator()`.

## Type Generation Quickstart

Use `JsonSchemaGenerator` when the desired JSON shape should follow a .NET type.

```csharp
using LateApexEarlySpeed.Json.Schema;
using LateApexEarlySpeed.Json.Schema.Generator;

JsonValidator validator = JsonSchemaGenerator.GenerateJsonValidator<OrderDto>(
    new JsonSchemaGeneratorOptions
    {
        PropertyNamingPolicy = JsonSchemaNamingPolicy.CamelCase
    });

ValidationResult result = validator.Validate("""
                                          {
                                            "orderId": "O-100",
                                            "quantity": 2
                                          }
                                          """);

public sealed class OrderDto
{
    public string OrderId { get; set; } = string.Empty;
    public int Quantity { get; set; }
}
```

## External Schema References

Register external schema documents before validating a schema that references them.

```csharp
var validator = new JsonValidator(orderSchema);
validator.AddExternalDocument(addressSchema);

ValidationResult result = validator.Validate(orderJson);
```

For HTTP-based external schemas, use `AddHttpDocumentAsync(Uri)` during setup, then reuse the configured validator.

## Annotation Collection

Annotation collection requires both validator-level and validation-level options:

```csharp
var validator = new JsonValidator(schema, new JsonValidatorOptions
{
    CollectAnnotations = true
});

ValidationResult result = validator.Validate(instance, new JsonSchemaOptions
{
    OutputFormat = OutputFormat.List
});
```

Annotations are collected only for valid validation results.

## Custom Formats

Register custom formats through the option-level registry:

```csharp
using LateApexEarlySpeed.Json.Schema.Keywords;

var options = new JsonValidatorOptions();
options.FormatRegistry.AddFormat("slug", () => new SlugFormatValidator());

var validator = new JsonValidator("""{ "type": "string", "format": "slug" }""", options);

public sealed class SlugFormatValidator : FormatValidator
{
    public override bool Validate(string content)
    {
        return content.All(c => char.IsLower(c) || char.IsDigit(c) || c == '-');
    }
}
```

The format validator factory is evaluated lazily, and the created validator is cached by the registry. Prefer stateless, thread-safe validators.

## xUnit v2 Assertions

Use `LateApexEarlySpeed.Xunit.Assertion.Json` with xUnit v2.

```csharp
using LateApexEarlySpeed.Xunit.Assertion.Json;

JsonAssertion.Meet(b =>
        b.IsJsonObject()
            .HasProperty("status", p => p.IsJsonString().Equal("ok"))
            .HasProperty("items", p => p.IsJsonArray().HasMinLength(1)),
    """
    {
      "status": "ok",
      "items": [ { "id": 1 } ]
    }
    """);

JsonAssertion.Equivalent("""{ "a": 1, "b": 2 }""", """{ "b": 2, "a": 1 }""");
```

## xUnit v3 Assertions

Use `LateApexEarlySpeed.Xunit.V3.Assertion.Json` with xUnit v3.

```csharp
using LateApexEarlySpeed.Xunit.V3.Assertion.Json;

JsonAssertion.Meet(b =>
        b.IsJsonObject()
            .HasProperty("status", p => p.IsJsonString().Equal("ok"))
            .HasProperty("items", p => p.IsJsonArray().HasMinLength(1)),
    """
    {
      "status": "ok",
      "items": [ { "id": 1 } ]
    }
    """);

JsonAssertion.Equivalent("""{ "a": 1, "b": 2 }""", """{ "b": 2, "a": 1 }""");
```

The v2 and v3 packages intentionally use different namespaces. Pick the package and namespace that match the user's xUnit version.

## Common Mistakes

- Do not create a new `JsonValidator` inside a hot loop when the schema is the same. Reuse one validator instance.
- If the schema has no `$schema`, set `JsonValidatorOptions.DefaultDialect` when the dialect matters.
- Use `OutputFormat.List` when code needs all errors. The default fail-fast behavior is faster but returns only the first failure.
- Enable both `CollectAnnotations = true` and `OutputFormat.List` for annotations.
- Register external schema documents before validating JSON that depends on external `$ref` values.
- Do not use the xUnit v2 assertion package in an xUnit v3 test project, or the v3 assertion package in an xUnit v2 test project.
- Remember that `unevaluatedProperties`, `unevaluatedItems`, and automatic content-encoded string decoding/validation are not currently supported.
- Do not assume `GetStandardJsonSchemaText()` works for every validator. Validators containing extended keywords can throw `NotSupportedException`.
