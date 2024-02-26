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

1. Fluent configuration

If not familiar with Standard Json schema, recommend using fluent configuration. The fluent configuration is designed not completely align with standard Json schema keywords interface. The standard Json schema is a powerful and flexiable language to specify json shape, but most developers may be more familiar with "stronger type" smell. For this, library's fluent configuration will (in most cases) firstly "ask" developers what json type they want, then continue "ask" subsequence validation requirements which are scoped based on known json type. 

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

Available validations for json column:

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

2. Just provide standard json schema (2020.12) when call HasJsonValidation():
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