Provide fluent json assertion for Xunit:

Assert equivalent:
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

Or more advanced custom fine-grained assertions:
```csharp
JsonAssertion.Meet(b =>
                        b.IsJsonObject()
                            .HasProperty("p1")
                            .HasProperty("p2", b => b.IsJsonNumber().Equal(5)),
                """
                {
                  "p1": null, // any value
                  "p2": 4.9
                }
                """);
```

Available assertion methods for json:

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

There are HasCustomValidation() overloads which can be used to create more custom validation logic unit.
