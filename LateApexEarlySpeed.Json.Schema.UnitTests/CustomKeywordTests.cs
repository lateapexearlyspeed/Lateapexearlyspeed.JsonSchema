using LateApexEarlySpeed.Json.Schema.Common;
using LateApexEarlySpeed.Json.Schema.JInstance;
using LateApexEarlySpeed.Json.Schema.Keywords;
using System.Text.Json;
using System.Text.Json.Serialization;
using Xunit;

namespace LateApexEarlySpeed.Json.Schema.UnitTests;

public class CustomKeywordTests
{
    [Theory]
    [InlineData(2, true)]
    [InlineData(1, false)]
    public void Validate_WithGlobalCustomKeyword(int value, bool expectedResult)
    {
        string schema = """{"custom-keyword-2": true}""";
        string instance = value.ToString();

        ValidationKeywordRegistry globalRegistry = ValidationKeywordRegistry.CreateDefaultRegistry();
        globalRegistry.AddKeyword<TwoIsValidKeyword>();

        var options = new JsonValidatorOptions { GlobalKeywordRegistry = globalRegistry };
        Assert.Equal(expectedResult, new JsonValidator(schema, options).Validate(instance).IsValid);
    }

    [Theory]
    [InlineData(2, true)]
    [InlineData(1, false)]
    public void Validate_WithOptionLevelCustomKeyword(int value, bool expectedResult)
    {
        string schema = """{"custom-keyword-2": true}""";
        string instance = value.ToString();

        var options = new JsonValidatorOptions();
        options.KeywordRegistry.AddKeyword<TwoIsValidKeyword>();
        Assert.Equal(expectedResult, new JsonValidator(schema, options).Validate(instance).IsValid);
    }

    [Theory]
    [InlineData(1, 2, true)]
    [InlineData(1, 1, false)]
    [InlineData(2, 2, false)]
    [InlineData(2, 1, false)]
    public void Validate_DifferentKeywordName_ForGlobalAndOptionLevelRegistry(int propAValue, int propBValue, bool expectedResult)
    {
        string schema = """
                        {
                          "properties": {
                            "a": { "custom-keyword-1": true },
                            "b": { "custom-keyword-2": true }
                          }
                        }
                        """;
        string instance = $$"""
                            { 
                              "a": {{propAValue}},
                              "b": {{propBValue}}
                            }
                            """;

        ValidationKeywordRegistry globalRegistry = ValidationKeywordRegistry.CreateDefaultRegistry();
        globalRegistry.AddKeyword<OneIsValidKeyword>();

        var options = new JsonValidatorOptions { GlobalKeywordRegistry = globalRegistry };
        options.KeywordRegistry.AddKeyword<TwoIsValidKeyword>();
        Assert.Equal(expectedResult, new JsonValidator(schema, options).Validate(instance).IsValid);
    }

    [Theory]
    [InlineData(1, 1, true)]
    [InlineData(2, 2, false)]
    [InlineData(1, 2, false)]
    [InlineData(2, 1, false)]
    public void Validate_SameKeywordName_OptionLevelShouldOverrideGlobalLevel(int propAValue, int propBValue, bool expectedResult)
    {
        string schema = """
                        {
                          "properties": {
                            "a": { 
                              "$id": "https://example.com/schema-a",
                              "$schema": "https://json-schema.org/draft/2019-09/schema",
                              "custom-keyword-2": true 
                            },
                            "b": {
                              "$id": "https://example.com/schema-b",
                              "$schema": "http://json-schema.org/draft-07/schema#",
                              "custom-keyword-2": true 
                            }
                          }
                        }
                        """;
        string instance = $$"""
                            { 
                              "a": {{propAValue}},
                              "b": {{propBValue}}
                            }
                            """;

        ValidationKeywordRegistry globalRegistry = ValidationKeywordRegistry.CreateDefaultRegistry();
        globalRegistry.AddKeyword<TwoIsValidKeyword>();

        var options = new JsonValidatorOptions { GlobalKeywordRegistry = globalRegistry };
        options.KeywordRegistry.AddKeyword<TwoIsInvalidKeyword>();
        Assert.Equal(expectedResult, new JsonValidator(schema, options).Validate(instance).IsValid);
    }

    [Theory]
    [InlineData(2, true, false)]
    [InlineData(1, false, true)]
    public void Validate_WithGlobalCustomKeyword_RegistriesAreIsolated(int value, bool expectedResultForValidRegistry, bool expectedResultForInvalidRegistry)
    {
        string schema = """{"custom-keyword-2": true}""";
        string instance = value.ToString();

        ValidationKeywordRegistry validRegistry = ValidationKeywordRegistry.CreateDefaultRegistry();
        validRegistry.AddKeyword<TwoIsValidKeyword>();

        ValidationKeywordRegistry invalidRegistry = ValidationKeywordRegistry.CreateDefaultRegistry();
        invalidRegistry.AddKeyword<TwoIsInvalidKeyword>();

        var optionsWithValidRegistry = new JsonValidatorOptions
        {
            GlobalKeywordRegistry = validRegistry
        };

        var optionsWithInvalidRegistry = new JsonValidatorOptions
        {
            GlobalKeywordRegistry = invalidRegistry
        };

        Assert.Equal(expectedResultForValidRegistry, new JsonValidator(schema, optionsWithValidRegistry).Validate(instance).IsValid);
        Assert.Equal(expectedResultForInvalidRegistry, new JsonValidator(schema, optionsWithInvalidRegistry).Validate(instance).IsValid);
    }
}

[Keyword("custom-keyword-2")]
[JsonConverter(typeof(TwoIsValidKeywordJsonConverter))]
public class TwoIsValidKeyword : ValidationKeywordBase
{
    protected internal override ValidationResult ValidateCore(JsonInstanceElement instance, JsonSchemaOptions options)
    {
        if (instance.ValueKind != JsonValueKind.Number || !instance.TryGetInt64(out long value))
        {
            return ValidationResult.ValidResult;
        }

        return value == 2 
            ? ValidationResult.ValidResult 
            : ValidationResult.SingleErrorFailedResult(new ValidationError(ResultCode.UnexpectedValue, "test", null, this.Name, instance.Location));        
    }
}

public class TwoIsValidKeywordJsonConverter : JsonConverter<TwoIsValidKeyword>
{
    public override TwoIsValidKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        reader.HandleFinalBlockAndSkip();
        return new TwoIsValidKeyword();
    }

    public override void Write(Utf8JsonWriter writer, TwoIsValidKeyword value, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }
}

[Keyword("custom-keyword-2")]
[JsonConverter(typeof(TwoIsInvalidKeywordJsonConverter))]
public class TwoIsInvalidKeyword : ValidationKeywordBase
{
    protected internal override ValidationResult ValidateCore(JsonInstanceElement instance, JsonSchemaOptions options)
    {
        if (instance.ValueKind != JsonValueKind.Number || !instance.TryGetInt64(out long value))
        {
            return ValidationResult.ValidResult;
        }

        return value != 2 
            ? ValidationResult.ValidResult 
            : ValidationResult.SingleErrorFailedResult(new ValidationError(ResultCode.UnexpectedValue, "test", null, this.Name, instance.Location));        
    }
}

public class TwoIsInvalidKeywordJsonConverter : JsonConverter<TwoIsInvalidKeyword>
{
    public override TwoIsInvalidKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        reader.HandleFinalBlockAndSkip();
        return new TwoIsInvalidKeyword();
    }

    public override void Write(Utf8JsonWriter writer, TwoIsInvalidKeyword value, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }
}

[Keyword("custom-keyword-1")]
[JsonConverter(typeof(OneIsValidKeywordJsonConverter))]
public class OneIsValidKeyword : ValidationKeywordBase
{
    protected internal override ValidationResult ValidateCore(JsonInstanceElement instance, JsonSchemaOptions options)
    {
        if (instance.ValueKind != JsonValueKind.Number || !instance.TryGetInt64(out long value))
        {
            return ValidationResult.ValidResult;
        }

        return value == 1 
            ? ValidationResult.ValidResult 
            : ValidationResult.SingleErrorFailedResult(new ValidationError(ResultCode.UnexpectedValue, "test", null, this.Name, instance.Location));
    }
}

public class OneIsValidKeywordJsonConverter : JsonConverter<OneIsValidKeyword>
{
    public override OneIsValidKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        reader.HandleFinalBlockAndSkip();
        return new OneIsValidKeyword();
    }

    public override void Write(Utf8JsonWriter writer, OneIsValidKeyword value, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }
}