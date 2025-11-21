using LateApexEarlySpeed.Json.Schema.Common;
using LateApexEarlySpeed.Json.Schema.Keywords;
using System.Text.Json;
using LateApexEarlySpeed.Json.Schema.JSchema;
using Xunit;

namespace LateApexEarlySpeed.Json.Schema.UnitTests;

public class ListOutputFormatTests
{
    [Fact]
    public void AdditionalPropertiesKeywordTest()
    {
        string schema = """
            {
              "additionalProperties": {
                "type": "string"
              }
            }
            """;

        string instance = """
            {
              "p1": 1,
              "p2": 2,
              "p3": "a"
            }
            """;

        ValidationResult validationResult = Validate(schema, instance);

        Assert.False(validationResult.IsValid);
        Assert.Collection(validationResult.ValidationErrors,
            error =>
            {
                Assert.Equal(GetTypeErrorMessage(InstanceType.String, JsonValueKind.Number), error.ErrorMessage);
                Assert.Equal(LinkedListBasedImmutableJsonPointer.Create("/p1"), error.InstanceLocation);
                Assert.Equal("type", error.Keyword);
                Assert.Equal(LinkedListBasedImmutableJsonPointer.Create("/additionalProperties/type"), error.RelativeKeywordLocation);
                Assert.Equal(ResultCode.InvalidTokenKind, error.ResultCode);
            },
            error =>
            {
                Assert.Equal(GetTypeErrorMessage(InstanceType.String, JsonValueKind.Number), error.ErrorMessage);
                Assert.Equal(LinkedListBasedImmutableJsonPointer.Create("/p2"), error.InstanceLocation);
                Assert.Equal("type", error.Keyword);
                Assert.Equal(LinkedListBasedImmutableJsonPointer.Create("/additionalProperties/type"), error.RelativeKeywordLocation);
                Assert.Equal(ResultCode.InvalidTokenKind, error.ResultCode);
            }
            );
    }

    [Fact]
    public void AllOfKeywordTest()
    {
        string schema = """
            {
              "allOf": 
              [
                {
                  "type": "string"
                },
                {
                  "type": "string"
                },
                {
                  "type": "integer"
                }
              ]
            }
            """;

        string instance = "1";

        ValidationResult validationResult = Validate(schema, instance);

        Assert.False(validationResult.IsValid);
        Assert.Collection(validationResult.ValidationErrors,
            error =>
            {
                Assert.Equal(GetTypeErrorMessage(InstanceType.String, JsonValueKind.Number), error.ErrorMessage);
                Assert.Equal(LinkedListBasedImmutableJsonPointer.Empty, error.InstanceLocation);
                Assert.Equal("type", error.Keyword);
                Assert.Equal(LinkedListBasedImmutableJsonPointer.Create("/allOf/0/type"), error.RelativeKeywordLocation);
                Assert.Equal(ResultCode.InvalidTokenKind, error.ResultCode);
            },
            error =>
            {
                Assert.Equal(GetTypeErrorMessage(InstanceType.String, JsonValueKind.Number), error.ErrorMessage);
                Assert.Equal(LinkedListBasedImmutableJsonPointer.Empty, error.InstanceLocation);
                Assert.Equal("type", error.Keyword);
                Assert.Equal(LinkedListBasedImmutableJsonPointer.Create("/allOf/1/type"), error.RelativeKeywordLocation);
                Assert.Equal(ResultCode.InvalidTokenKind, error.ResultCode);
            }
        );
    }

    [Fact]
    public void AnyOfKeywordTest()
    {
        string schema = """
            {
              "anyOf": 
              [
                {
                  "type": "integer"
                },
                {
                  "type": "string"
                },
                {
                  "type": "string"
                }
              ]
            }
            """;

        string instance = "1";

        ValidationResult validationResult = Validate(schema, instance);

        Assert.True(validationResult.IsValid);
        Assert.Collection(validationResult.ValidationErrors,
            error =>
            {
                Assert.Equal(GetTypeErrorMessage(InstanceType.String, JsonValueKind.Number), error.ErrorMessage);
                Assert.Equal(LinkedListBasedImmutableJsonPointer.Empty, error.InstanceLocation);
                Assert.Equal("type", error.Keyword);
                Assert.Equal(LinkedListBasedImmutableJsonPointer.Create("/anyOf/1/type"), error.RelativeKeywordLocation);
                Assert.Equal(ResultCode.InvalidTokenKind, error.ResultCode);
            },
            error =>
            {
                Assert.Equal(GetTypeErrorMessage(InstanceType.String, JsonValueKind.Number), error.ErrorMessage);
                Assert.Equal(LinkedListBasedImmutableJsonPointer.Empty, error.InstanceLocation);
                Assert.Equal("type", error.Keyword);
                Assert.Equal(LinkedListBasedImmutableJsonPointer.Create("/anyOf/2/type"), error.RelativeKeywordLocation);
                Assert.Equal(ResultCode.InvalidTokenKind, error.ResultCode);
            }
        );
    }

    [Fact]
    public void ContainsKeywordTest()
    {
        string schema = """
            {
              "contains": {
                  "type": "string"
                },
              "maxContains": 2
            }
            """;

        string instance = """
            ["a", "b", "c", 1]
            """;

        ValidationResult validationResult = Validate(schema, instance);

        Assert.False(validationResult.IsValid);
        Assert.Collection(validationResult.ValidationErrors,
            error =>
            {
                Assert.Equal(ArrayContainsValidator.GetFailedMaxContainsErrorMessage(instance, 2), error.ErrorMessage);
                Assert.Equal(LinkedListBasedImmutableJsonPointer.Empty, error.InstanceLocation);
                Assert.Equal("maxContains", error.Keyword);
                Assert.Equal(LinkedListBasedImmutableJsonPointer.Create("/maxContains"), error.RelativeKeywordLocation);
                Assert.Equal(ResultCode.ValidatedArrayItemsCountOutOfRange, error.ResultCode);
            },
            error =>
            {
                Assert.Equal(GetTypeErrorMessage(InstanceType.String, JsonValueKind.Number), error.ErrorMessage);
                Assert.Equal(LinkedListBasedImmutableJsonPointer.Create("/3"), error.InstanceLocation);
                Assert.Equal("type", error.Keyword);
                Assert.Equal(LinkedListBasedImmutableJsonPointer.Create("/contains/type"), error.RelativeKeywordLocation);
                Assert.Equal(ResultCode.InvalidTokenKind, error.ResultCode);
            }
        );
    }

    [Fact]
    public void ConditionElseKeywordTest()
    {
        string schema = """
            {
              "if": {
                  "type": "string"
                }, 
              "then": false,
              "else": true
            }
            """;

        string instance = "1";

        ValidationResult validationResult = Validate(schema, instance);

        Assert.True(validationResult.IsValid);
        Assert.Collection(validationResult.ValidationErrors,
            error =>
            {
                Assert.Equal(GetTypeErrorMessage(InstanceType.String, JsonValueKind.Number), error.ErrorMessage);
                Assert.Equal(LinkedListBasedImmutableJsonPointer.Empty, error.InstanceLocation);
                Assert.Equal("type", error.Keyword);
                Assert.Equal(LinkedListBasedImmutableJsonPointer.Create("/if/type"), error.RelativeKeywordLocation);
                Assert.Equal(ResultCode.InvalidTokenKind, error.ResultCode);
            }
        );
    }

    [Fact]
    public void ConditionThenKeywordTest()
    {
        string schema = """
            {
              "if": {
                  "type": "string"
                }, 
              "then": {
                "type": "integer"
              },
              "else": true
            }
            """;

        string instance = "\"a\"";

        ValidationResult validationResult = Validate(schema, instance);

        Assert.False(validationResult.IsValid);
        Assert.Collection(validationResult.ValidationErrors,
            error =>
            {
                Assert.Equal(GetTypeErrorMessage(InstanceType.Integer, JsonValueKind.String), error.ErrorMessage);
                Assert.Equal(LinkedListBasedImmutableJsonPointer.Empty, error.InstanceLocation);
                Assert.Equal("type", error.Keyword);
                Assert.Equal(LinkedListBasedImmutableJsonPointer.Create("/then/type"), error.RelativeKeywordLocation);
                Assert.Equal(ResultCode.InvalidTokenKind, error.ResultCode);
            }
        );
    }

    [Fact]
    public void DependentRequiredKeywordTest()
    {
        string schema = """
            {
              "dependentRequired": {
                  "p1": ["d1", "d2"],
                  "p2": ["d3", "d4"]
              }
            }
            """;

        string instance = """
            {
              "p1": 1,
              "p2": 1,
              "d1": 1,
              "d3": 1
            }
            """;

        ValidationResult validationResult = Validate(schema, instance);

        Assert.False(validationResult.IsValid);
        Assert.Collection(validationResult.ValidationErrors,
            error =>
            {
                Assert.Equal(DependentRequiredKeyword.ErrorMessage("p1", "d2"), error.ErrorMessage);
                Assert.Equal(LinkedListBasedImmutableJsonPointer.Empty, error.InstanceLocation);
                Assert.Equal("dependentRequired", error.Keyword);
                Assert.Equal(LinkedListBasedImmutableJsonPointer.Create("/dependentRequired"), error.RelativeKeywordLocation);
                Assert.Equal(ResultCode.NotFoundRequiredDependentProperty, error.ResultCode);
            },
            error =>
            {
                Assert.Equal(DependentRequiredKeyword.ErrorMessage("p2", "d4"), error.ErrorMessage);
                Assert.Equal(LinkedListBasedImmutableJsonPointer.Empty, error.InstanceLocation);
                Assert.Equal("dependentRequired", error.Keyword);
                Assert.Equal(LinkedListBasedImmutableJsonPointer.Create("/dependentRequired"), error.RelativeKeywordLocation);
                Assert.Equal(ResultCode.NotFoundRequiredDependentProperty, error.ResultCode);
            }
        );
    }

    [Fact]
    public void DependentSchemasKeywordTest()
    {
        string schema = """
            {
              "dependentSchemas": {
                "p1": true,  
                "p2": false, 
                "p3": false
              }
            }
            """;

        string instance = """
            {
              "p1": 1,
              "p2": 1,
              "p3": 1
            }
            """;

        ValidationResult validationResult = Validate(schema, instance);

        Assert.False(validationResult.IsValid);
        Assert.Collection(validationResult.ValidationErrors,
            error =>
            {
                Assert.Equal(BooleanJsonSchema.ErrorMessage(), error.ErrorMessage);
                Assert.Equal(LinkedListBasedImmutableJsonPointer.Empty, error.InstanceLocation);
                Assert.Null(error.Keyword);
                Assert.Equal(LinkedListBasedImmutableJsonPointer.Create("/dependentSchemas/p2"), error.RelativeKeywordLocation);
                Assert.Equal(ResultCode.AlwaysFailedJsonSchema, error.ResultCode);
            },
            error =>
            {
                Assert.Equal(BooleanJsonSchema.ErrorMessage(), error.ErrorMessage);
                Assert.Equal(LinkedListBasedImmutableJsonPointer.Empty, error.InstanceLocation);
                Assert.Null(error.Keyword);
                Assert.Equal(LinkedListBasedImmutableJsonPointer.Create("/dependentSchemas/p3"), error.RelativeKeywordLocation);
                Assert.Equal(ResultCode.AlwaysFailedJsonSchema, error.ResultCode);
            }
        );
    }

    [Fact]
    public void ItemsKeywordTest()
    {
        string schema = """
            {
              "items": {
                "type": "string"
              }
            }
            """;

        string instance = """
            [ 1, 2, "a" ]
            """;

        ValidationResult validationResult = Validate(schema, instance);

        Assert.False(validationResult.IsValid);
        Assert.Collection(validationResult.ValidationErrors,
            error =>
            {
                Assert.Equal(GetTypeErrorMessage(InstanceType.String, JsonValueKind.Number), error.ErrorMessage);
                Assert.Equal(LinkedListBasedImmutableJsonPointer.Create("/0"), error.InstanceLocation);
                Assert.Equal("type", error.Keyword);
                Assert.Equal(LinkedListBasedImmutableJsonPointer.Create("/items/type"), error.RelativeKeywordLocation);
                Assert.Equal(ResultCode.InvalidTokenKind, error.ResultCode);
            },
            error =>
            {
                Assert.Equal(GetTypeErrorMessage(InstanceType.String, JsonValueKind.Number), error.ErrorMessage);
                Assert.Equal(LinkedListBasedImmutableJsonPointer.Create("/1"), error.InstanceLocation);
                Assert.Equal("type", error.Keyword);
                Assert.Equal(LinkedListBasedImmutableJsonPointer.Create("/items/type"), error.RelativeKeywordLocation);
                Assert.Equal(ResultCode.InvalidTokenKind, error.ResultCode);
            }
        );
    }

    [Fact]
    public void OneOfKeywordTest()
    {
        string schema = """
            {
              "oneOf": [
                {
                  "type": "string"
                },
                true,
                true
              ]
            }
            """;

        string instance = "1";

        ValidationResult validationResult = Validate(schema, instance);

        Assert.False(validationResult.IsValid);
        Assert.Collection(validationResult.ValidationErrors,
            error =>
            {
                Assert.Equal("More than one schema validate instance", error.ErrorMessage);
                Assert.Equal(LinkedListBasedImmutableJsonPointer.Empty, error.InstanceLocation);
                Assert.Equal("oneOf", error.Keyword);
                Assert.Equal(LinkedListBasedImmutableJsonPointer.Create("/oneOf"), error.RelativeKeywordLocation);
                Assert.Equal(ResultCode.MoreThanOnePassedSchemaFound, error.ResultCode);
            },
            error =>
            {
                Assert.Equal(GetTypeErrorMessage(InstanceType.String, JsonValueKind.Number), error.ErrorMessage);
                Assert.Equal(LinkedListBasedImmutableJsonPointer.Empty, error.InstanceLocation);
                Assert.Equal("type", error.Keyword);
                Assert.Equal(LinkedListBasedImmutableJsonPointer.Create("/oneOf/0/type"), error.RelativeKeywordLocation);
                Assert.Equal(ResultCode.InvalidTokenKind, error.ResultCode);
            }
        );
    }

    [Fact]
    public void PatternPropertiesKeywordTest()
    {
        string schema = """
            {
              "patternProperties": {
                "p1": false,
                "p2": false
              }
            }
            """;

        string instance = """
            {
              "p1": 1,
              "p2": 1
            }
            """;

        ValidationResult validationResult = Validate(schema, instance);

        Assert.False(validationResult.IsValid);
        Assert.Collection(validationResult.ValidationErrors,
            error =>
            {
                Assert.Equal(BooleanJsonSchema.ErrorMessage(), error.ErrorMessage);
                Assert.Equal(LinkedListBasedImmutableJsonPointer.Create("/p1"), error.InstanceLocation);
                Assert.Null(error.Keyword);
                Assert.Equal(LinkedListBasedImmutableJsonPointer.Create("/patternProperties/p1"), error.RelativeKeywordLocation);
                Assert.Equal(ResultCode.AlwaysFailedJsonSchema, error.ResultCode);
            },
            error =>
            {
                Assert.Equal(BooleanJsonSchema.ErrorMessage(), error.ErrorMessage);
                Assert.Equal(LinkedListBasedImmutableJsonPointer.Create("/p2"), error.InstanceLocation);
                Assert.Null(error.Keyword);
                Assert.Equal(LinkedListBasedImmutableJsonPointer.Create("/patternProperties/p2"), error.RelativeKeywordLocation);
                Assert.Equal(ResultCode.AlwaysFailedJsonSchema, error.ResultCode);
            }
        );
    }

    [Fact]
    public void PrefixItemsKeywordTest()
    {
        string schema = """
            {
              "prefixItems": [false, false]
            }
            """;

        string instance = "[1, 1, 1]";

        ValidationResult validationResult = Validate(schema, instance);

        Assert.False(validationResult.IsValid);
        Assert.Collection(validationResult.ValidationErrors,
            error =>
            {
                Assert.Equal(BooleanJsonSchema.ErrorMessage(), error.ErrorMessage);
                Assert.Equal(LinkedListBasedImmutableJsonPointer.Create("/0"), error.InstanceLocation);
                Assert.Null(error.Keyword);
                Assert.Equal(LinkedListBasedImmutableJsonPointer.Create("/prefixItems/0"), error.RelativeKeywordLocation);
                Assert.Equal(ResultCode.AlwaysFailedJsonSchema, error.ResultCode);
            },
            error =>
            {
                Assert.Equal(BooleanJsonSchema.ErrorMessage(), error.ErrorMessage);
                Assert.Equal(LinkedListBasedImmutableJsonPointer.Create("/1"), error.InstanceLocation);
                Assert.Null(error.Keyword);
                Assert.Equal(LinkedListBasedImmutableJsonPointer.Create("/prefixItems/1"), error.RelativeKeywordLocation);
                Assert.Equal(ResultCode.AlwaysFailedJsonSchema, error.ResultCode);
            }
        );
    }

    [Fact]
    public void PropertiesKeywordTest()
    {
        string schema = """
            {
              "properties": {
                "p1": false,
                "p2": false
              }
            }
            """;

        string instance = """
            {
              "p1": 1,
              "p2": 1
            }
            """;

        ValidationResult validationResult = Validate(schema, instance);

        Assert.False(validationResult.IsValid);
        Assert.Collection(validationResult.ValidationErrors,
            error =>
            {
                Assert.Equal(BooleanJsonSchema.ErrorMessage(), error.ErrorMessage);
                Assert.Equal(LinkedListBasedImmutableJsonPointer.Create("/p1"), error.InstanceLocation);
                Assert.Null(error.Keyword);
                Assert.Equal(LinkedListBasedImmutableJsonPointer.Create("/properties/p1"), error.RelativeKeywordLocation);
                Assert.Equal(ResultCode.AlwaysFailedJsonSchema, error.ResultCode);
            },
            error =>
            {
                Assert.Equal(BooleanJsonSchema.ErrorMessage(), error.ErrorMessage);
                Assert.Equal(LinkedListBasedImmutableJsonPointer.Create("/p2"), error.InstanceLocation);
                Assert.Null(error.Keyword);
                Assert.Equal(LinkedListBasedImmutableJsonPointer.Create("/properties/p2"), error.RelativeKeywordLocation);
                Assert.Equal(ResultCode.AlwaysFailedJsonSchema, error.ResultCode);
            }
        );
    }

    [Fact]
    public void PropertyNamesKeywordTest()
    {
        string schema = """
            {
              "propertyNames": false
            }
            """;

        string instance = """
            {
              "p1": 1,
              "p2": 1
            }
            """;

        ValidationResult validationResult = Validate(schema, instance);

        Assert.False(validationResult.IsValid);
        Assert.Collection(validationResult.ValidationErrors,
            error =>
            {
                Assert.Equal(BooleanJsonSchema.ErrorMessage(), error.ErrorMessage);
                Assert.Equal(LinkedListBasedImmutableJsonPointer.Empty, error.InstanceLocation);
                Assert.Null(error.Keyword);
                Assert.Equal(LinkedListBasedImmutableJsonPointer.Create("/propertyNames"), error.RelativeKeywordLocation);
                Assert.Equal(ResultCode.AlwaysFailedJsonSchema, error.ResultCode);
            },
            error =>
            {
                Assert.Equal(BooleanJsonSchema.ErrorMessage(), error.ErrorMessage);
                Assert.Equal(LinkedListBasedImmutableJsonPointer.Empty, error.InstanceLocation);
                Assert.Null(error.Keyword);
                Assert.Equal(LinkedListBasedImmutableJsonPointer.Create("/propertyNames"), error.RelativeKeywordLocation);
                Assert.Equal(ResultCode.AlwaysFailedJsonSchema, error.ResultCode);
            }
        );
    }

    [Fact]
    public void RequiredKeywordTest()
    {
        string schema = """
            {
              "required": ["p1", "p2"]
            }
            """;

        string instance = "{}";

        ValidationResult validationResult = Validate(schema, instance);

        Assert.False(validationResult.IsValid);
        Assert.Collection(validationResult.ValidationErrors,
            error =>
            {
                Assert.Equal(RequiredKeyword.ErrorMessage("p1"), error.ErrorMessage);
                Assert.Equal(LinkedListBasedImmutableJsonPointer.Empty, error.InstanceLocation);
                Assert.Equal("required", error.Keyword);
                Assert.Equal(LinkedListBasedImmutableJsonPointer.Create("/required"), error.RelativeKeywordLocation);
                Assert.Equal(ResultCode.NotFoundRequiredProperty, error.ResultCode);
            },
            error =>
            {
                Assert.Equal(RequiredKeyword.ErrorMessage("p2"), error.ErrorMessage);
                Assert.Equal(LinkedListBasedImmutableJsonPointer.Empty, error.InstanceLocation);
                Assert.Equal("required", error.Keyword);
                Assert.Equal(LinkedListBasedImmutableJsonPointer.Create("/required"), error.RelativeKeywordLocation);
                Assert.Equal(ResultCode.NotFoundRequiredProperty, error.ResultCode);
            }
        );
    }

    [Fact]
    public void NotKeyword_PositiveTest()
    {
        string schema = """
            {
              "not": {
                "type": "string"
              }
            }
            """;

        string instance = "1";

        ValidationResult validationResult = Validate(schema, instance);

        Assert.True(validationResult.IsValid);
        Assert.Collection(validationResult.ValidationErrors,
            error =>
            {
                Assert.Equal(GetTypeErrorMessage(InstanceType.String, JsonValueKind.Number), error.ErrorMessage);
                Assert.Equal(LinkedListBasedImmutableJsonPointer.Empty, error.InstanceLocation);
                Assert.Equal("type", error.Keyword);
                Assert.Equal(LinkedListBasedImmutableJsonPointer.Create("/not/type"), error.RelativeKeywordLocation);
                Assert.Equal(ResultCode.InvalidTokenKind, error.ResultCode);
            }
        );
    }

    [Fact]
    public void NotKeyword_NegativeTest()
    {
        string schema = """
            {
              "not": {
                "anyOf": [true, false]
              }
            }
            """;

        string instance = "1";

        ValidationResult validationResult = Validate(schema, instance);

        Assert.False(validationResult.IsValid);
        Assert.Collection(validationResult.ValidationErrors,
            error =>
            {
                Assert.Equal(NotKeyword.ErrorMessage(instance), error.ErrorMessage);
                Assert.Equal(LinkedListBasedImmutableJsonPointer.Empty, error.InstanceLocation);
                Assert.Equal("not", error.Keyword);
                Assert.Equal(LinkedListBasedImmutableJsonPointer.Create("/not"), error.RelativeKeywordLocation);
                Assert.Equal(ResultCode.SubSchemaPassedUnexpected, error.ResultCode);
            },
            error =>
            {
                Assert.Equal(BooleanJsonSchema.ErrorMessage(), error.ErrorMessage);
                Assert.Equal(LinkedListBasedImmutableJsonPointer.Empty, error.InstanceLocation);
                Assert.Null(error.Keyword);
                Assert.Equal(LinkedListBasedImmutableJsonPointer.Create("/not/anyOf/1"), error.RelativeKeywordLocation);
                Assert.Equal(ResultCode.AlwaysFailedJsonSchema, error.ResultCode);
            }
        );
    }

    [Fact]
    public void BodyJsonSchemaTest()
    {
        string schema = """
            {
              "items": false,
              "type": "string"
            }
            """;

        string instance = "[1]";

        ValidationResult validationResult = Validate(schema, instance);

        Assert.False(validationResult.IsValid);
        Assert.Collection(validationResult.ValidationErrors,
            error =>
            {
                Assert.Equal(BooleanJsonSchema.ErrorMessage(), error.ErrorMessage);
                Assert.Equal(LinkedListBasedImmutableJsonPointer.Create("/0"), error.InstanceLocation);
                Assert.Null(error.Keyword);
                Assert.Equal(LinkedListBasedImmutableJsonPointer.Create("/items"), error.RelativeKeywordLocation);
                Assert.Equal(ResultCode.AlwaysFailedJsonSchema, error.ResultCode);
            },
            error =>
            {
                Assert.Equal(GetTypeErrorMessage(InstanceType.String, JsonValueKind.Array), error.ErrorMessage);
                Assert.Equal(LinkedListBasedImmutableJsonPointer.Empty, error.InstanceLocation);
                Assert.Equal("type", error.Keyword);
                Assert.Equal(LinkedListBasedImmutableJsonPointer.Create("/type"), error.RelativeKeywordLocation);
                Assert.Equal(ResultCode.InvalidTokenKind, error.ResultCode);
            }
        );
    }

    private static string GetTypeErrorMessage(InstanceType expectedInstanceType, JsonValueKind actualKind)
    {
        return $"Expect type(s): '{expectedInstanceType}' but actual is '{actualKind}'";
    }

    private static ValidationResult Validate(string schema, string instance)
    {
        var jsonValidator = new JsonValidator(schema);
        return jsonValidator.Validate(instance, new JsonSchemaOptions { OutputFormat = OutputFormat.List });
    }
}