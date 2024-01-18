using System.Globalization;
using System.Text.Json.Serialization;
using LateApexEarlySpeed.Json.Schema.Common;
using LateApexEarlySpeed.Json.Schema.Generator;
using LateApexEarlySpeed.Json.Schema.JSchema;
using LateApexEarlySpeed.Json.Schema.Keywords;
using Xunit;

// ReSharper disable ClassNeverInstantiated.Local
// ReSharper disable InconsistentNaming

namespace LateApexEarlySpeed.Json.Schema.UnitTests;

public class JsonSchemaGeneratorTest
{
    [Theory]
    [MemberData(nameof(TestData))]
    public void GenerateJsonValidator_Validate(Type type, string jsonInstance, ValidationResult expectedValidationResult, JsonSchemaGeneratorOptions? options)
    {
        JsonValidator jsonValidator = JsonSchemaGenerator.GenerateJsonValidator(type, options);
        ValidationResult validationResult = jsonValidator.Validate(jsonInstance, new JsonSchemaOptions{ValidateFormat = true});

        Assert.Equal(expectedValidationResult.IsValid, validationResult.IsValid);
        Assert.Equal(expectedValidationResult.ResultCode, validationResult.ResultCode);
        Assert.Equal(expectedValidationResult.Keyword, validationResult.Keyword);
        Assert.Equal(expectedValidationResult.ErrorMessage, validationResult.ErrorMessage);
        Assert.Equal(expectedValidationResult.InstanceLocation, validationResult.InstanceLocation);
        Assert.Equal(expectedValidationResult.RelativeKeywordLocation, validationResult.RelativeKeywordLocation);
        Assert.Equal(expectedValidationResult.SchemaResourceBaseUri, validationResult.SchemaResourceBaseUri);
        Assert.Equal(expectedValidationResult.SubSchemaRefFullUri, validationResult.SubSchemaRefFullUri);
    }

    public static IEnumerable<object?[]> TestData
    {
        get
        {
            IEnumerable<TestSample> testSamples = CreateTestSamples();

            return testSamples.Select(sample => new object?[] { sample.Type, sample.JsonInstance, sample.ExpectedValidationResult, sample.Options });
        }
    }

    private static IEnumerable<TestSample> CreateTestSamples()
    {
        // Unsigned integer
        var samples = CreateSamplesForUnsignedInteger<byte>();
        samples = samples.Concat(CreateSamplesForUnsignedInteger<ushort>());
        samples = samples.Concat(CreateSamplesForUnsignedInteger<uint>());
        samples = samples.Concat(CreateSamplesForUnsignedInteger<ulong>());
        
        // Signed integer
        samples = samples.Concat(CreateSamplesForSignedInteger<sbyte>());
        samples = samples.Concat(CreateSamplesForSignedInteger<short>());
        samples = samples.Concat(CreateSamplesForSignedInteger<int>());
        samples = samples.Concat(CreateSamplesForSignedInteger<long>());
        
        // float number
        samples = samples.Concat(CreateSamplesForFloatNumber<float>());
        samples = samples.Concat(CreateSamplesForFloatNumber<double>());
        samples = samples.Concat(CreateSamplesForFloatNumber<decimal>());
        
        // Boolean
        samples = samples.Concat(CreateSamplesForBoolean());
        
        // string
        samples = samples.Concat(CreateSamplesForString());
        
        // Array
        samples = samples.Concat(CreateSamplesForArray());
        
        // Object
        samples = samples.Concat(CreateSamplesForObject());
        
        // Dictionary<string,>
        samples = samples.Concat(CreateSamplesForStringDictionary());
        
        // Enum
        samples = samples.Concat(CreateSamplesForEnum());
        
        // Guid
        samples = samples.Concat(CreateSamplesForGuid());
        
        // Uri
        samples = samples.Concat(CreateSamplesForUri());
        
        samples = samples.Concat(CreateSamplesForMaximumAttribute());
        samples = samples.Concat(CreateSamplesForMinimumAttribute());
        samples = samples.Concat(CreateSamplesForExclusiveMaximumAttribute());
        samples = samples.Concat(CreateSamplesForExclusiveMinimumAttribute());
        samples = samples.Concat(CreateSamplesForNumberRangeAttribute());
        samples = samples.Concat(CreateSamplesForIntegerEnumAttribute());
        samples = samples.Concat(CreateSamplesForMultipleOfAttribute());
        samples = samples.Concat(CreateSamplesForEmailAttribute());
        samples = samples.Concat(CreateSamplesForStringEnumAttribute());
        samples = samples.Concat(CreateSamplesForIPv4Attribute());
        samples = samples.Concat(CreateSamplesForIPv6Attribute());
        samples = samples.Concat(CreateSamplesForLengthRangeAttribute());
        samples = samples.Concat(CreateSamplesForMaxLengthAttribute());
        samples = samples.Concat(CreateSamplesForMinLengthAttribute());
        samples = samples.Concat(CreateSamplesForPatternAttribute());
        samples = samples.Concat(CreateSamplesForJsonSchemaNamingPolicy());
        samples = samples.Concat(CreateSamplesForJsonSchemaIgnoreAttribute());
        samples = samples.Concat(CreateSamplesForRequiredAttribute());
        samples = samples.Concat(CreateSamplesForUniqueItemsAttribute());
        samples = samples.Concat(CreateSamplesForPropertyNameAttribute());
        
        return samples;
    }

    private static IEnumerable<TestSample> CreateSamplesForUnsignedInteger<T>() where T : unmanaged
    {
        yield return TestSample.Create<T>("0", ValidationResult.ValidResult);
        yield return TestSample.Create<T>("1", ValidationResult.ValidResult);
        yield return TestSample.Create<T>("-1", new ValidationResult(
            ResultCode.NumberOutOfRange,
            "minimum",
            "Instance '-1' is less than '0'",
            ImmutableJsonPointer.Empty,
            ImmutableJsonPointer.Create("/minimum")!,
            new Uri(BodyJsonSchemaDocument.DefaultDocumentBaseUri, typeof(T).FullName),
            new Uri(BodyJsonSchemaDocument.DefaultDocumentBaseUri, typeof(T).FullName)
        ));
        yield return TestSample.Create<T>("\"abc\"", new ValidationResult(
            ResultCode.InvalidTokenKind,
            "type",
            GetInvalidTokenErrorMessage(InstanceType.Integer, InstanceType.String.ToString()),
            ImmutableJsonPointer.Empty,
            ImmutableJsonPointer.Create("/type")!,
            new Uri(BodyJsonSchemaDocument.DefaultDocumentBaseUri, typeof(T).FullName),
            new Uri(BodyJsonSchemaDocument.DefaultDocumentBaseUri, typeof(T).FullName)
        ));
        yield return TestSample.Create<T>("1.5", new ValidationResult(
            ResultCode.NotBeInteger,
            "type",
            GetInvalidTokenErrorMessage(InstanceType.Integer, "float number"),
            ImmutableJsonPointer.Empty,
            ImmutableJsonPointer.Create("/type")!,
            GetSchemaResourceBaseUri<T>(),
            GetSchemaResourceBaseUri<T>()
        ));

        ulong max = 0;

        if (typeof(T) == typeof(byte))
        {
            max = byte.MaxValue;
        }
        else if (typeof(T) == typeof(ushort))
        {
            max = ushort.MaxValue;
        }
        else if (typeof(T) == typeof(uint))
        {
            max = uint.MaxValue;
        }
        
        var instanceData = max + 1;

        if (typeof(T) != typeof(ulong))
        {
            yield return TestSample.Create<T>(instanceData.ToString(), new ValidationResult(ResultCode.NumberOutOfRange, "maximum", MaximumKeyword.ErrorMessage(instanceData, max),
                ImmutableJsonPointer.Create("")!,
                ImmutableJsonPointer.Create("/maximum"),
                GetSchemaResourceBaseUri<T>(),
                GetSchemaResourceBaseUri<T>()));
        }
    }

    private static IEnumerable<TestSample> CreateSamplesForSignedInteger<T>() where T : unmanaged
    {
        yield return TestSample.Create<T>("0", ValidationResult.ValidResult);
        yield return TestSample.Create<T>("1", ValidationResult.ValidResult);
        yield return TestSample.Create<T>("-1", ValidationResult.ValidResult);
        yield return TestSample.Create<T>("\"abc\"", new ValidationResult(
            ResultCode.InvalidTokenKind,
            "type",
            GetInvalidTokenErrorMessage(InstanceType.Integer, InstanceType.String.ToString()),
            ImmutableJsonPointer.Empty,
            ImmutableJsonPointer.Create("/type")!,
            new Uri(BodyJsonSchemaDocument.DefaultDocumentBaseUri, typeof(T).FullName),
            new Uri(BodyJsonSchemaDocument.DefaultDocumentBaseUri, typeof(T).FullName)
        ));
        yield return TestSample.Create<T>("1.5", new ValidationResult(
            ResultCode.NotBeInteger,
            "type",
            GetInvalidTokenErrorMessage(InstanceType.Integer, "float number"),
            ImmutableJsonPointer.Empty,
            ImmutableJsonPointer.Create("/type")!,
            new Uri(BodyJsonSchemaDocument.DefaultDocumentBaseUri, typeof(T).FullName),
            new Uri(BodyJsonSchemaDocument.DefaultDocumentBaseUri, typeof(T).FullName)
        ));

        ulong max;
        long min;

        if (typeof(T) == typeof(sbyte))
        {
            max = (ulong)sbyte.MaxValue;
            min = sbyte.MinValue;
        }
        else if (typeof(T) == typeof(short))
        {
            max = (ulong)short.MaxValue;
            min = short.MinValue;
        }
        else if (typeof(T) == typeof(int))
        {
            max = int.MaxValue;
            min = int.MinValue;
        }
        else
        {
            Assert.Equal(typeof(long), typeof(T));
            max = long.MaxValue;
            min = long.MinValue;
        }
        
        ulong instanceDataForMaxTest = max + 1;

        yield return TestSample.Create<T>(instanceDataForMaxTest.ToString(), new ValidationResult(ResultCode.NumberOutOfRange, "maximum", MaximumKeyword.ErrorMessage(instanceDataForMaxTest, max),
            ImmutableJsonPointer.Empty, 
            ImmutableJsonPointer.Create("/maximum"), 
            GetSchemaResourceBaseUri<T>(),
            GetSchemaResourceBaseUri<T>()
            ));

        if (typeof(T) != typeof(long))
        {
            long instanceDataForMinTest = min - 1;
        
            yield return TestSample.Create<T>(instanceDataForMinTest.ToString(), new ValidationResult(ResultCode.NumberOutOfRange, "minimum", MinimumKeyword.ErrorMessage(instanceDataForMinTest, min),
                ImmutableJsonPointer.Empty, 
                ImmutableJsonPointer.Create("/minimum"), 
                GetSchemaResourceBaseUri<T>(),
                GetSchemaResourceBaseUri<T>()
                ));
        }
    }

    private static IEnumerable<TestSample> CreateSamplesForFloatNumber<T>() where T : unmanaged
    {
        yield return TestSample.Create<T>("0", ValidationResult.ValidResult);
        yield return TestSample.Create<T>("1", ValidationResult.ValidResult);
        yield return TestSample.Create<T>("1.5", ValidationResult.ValidResult);
        yield return TestSample.Create<T>("-1.5", ValidationResult.ValidResult);
        yield return TestSample.Create<T>("\"abc\"", new ValidationResult(
            ResultCode.InvalidTokenKind,
            "type",
            GetInvalidTokenErrorMessage(InstanceType.Number, InstanceType.String.ToString()),
            ImmutableJsonPointer.Empty,
            ImmutableJsonPointer.Create("/type")!,
            new Uri(BodyJsonSchemaDocument.DefaultDocumentBaseUri, typeof(T).FullName),
            new Uri(BodyJsonSchemaDocument.DefaultDocumentBaseUri, typeof(T).FullName)
        ));

        if (typeof(T) == typeof(float))
        {
            double max = float.MaxValue;
            double instanceData = double.MaxValue;

            yield return TestSample.Create<T>(instanceData.ToString(CultureInfo.InvariantCulture), new ValidationResult(ResultCode.NumberOutOfRange, "maximum", MaximumKeyword.ErrorMessage(instanceData, max),
                ImmutableJsonPointer.Empty, 
                ImmutableJsonPointer.Create("/maximum"), 
                GetSchemaResourceBaseUri<T>(),
                GetSchemaResourceBaseUri<T>()
                ));

            double min = float.MinValue;
            instanceData = double.MinValue;

            yield return TestSample.Create<T>(instanceData.ToString(CultureInfo.InvariantCulture), new ValidationResult(ResultCode.NumberOutOfRange, "minimum", MinimumKeyword.ErrorMessage(instanceData, min),
                ImmutableJsonPointer.Empty,
                ImmutableJsonPointer.Create("/minimum"),
                GetSchemaResourceBaseUri<T>(),
                GetSchemaResourceBaseUri<T>()
            ));
        }
    }

    private static IEnumerable<TestSample> CreateSamplesForBoolean()
    {
        yield return TestSample.Create<bool>("true", ValidationResult.ValidResult);
        yield return TestSample.Create<bool>("false", ValidationResult.ValidResult);
        yield return TestSample.Create<bool>("\"true\"", new ValidationResult(
            ResultCode.InvalidTokenKind,
            "type",
            GetInvalidTokenErrorMessage(InstanceType.Boolean, InstanceType.String.ToString()),
            ImmutableJsonPointer.Empty,
            ImmutableJsonPointer.Create("/type")!,
            new Uri(BodyJsonSchemaDocument.DefaultDocumentBaseUri, typeof(bool).FullName),
            new Uri(BodyJsonSchemaDocument.DefaultDocumentBaseUri, typeof(bool).FullName)
        ));
    }

    private static IEnumerable<TestSample> CreateSamplesForString()
    {
        yield return TestSample.Create<string>("\"abc\"", ValidationResult.ValidResult);
        yield return TestSample.Create<string>("null", new ValidationResult(
            ResultCode.InvalidTokenKind,
            "type",
            GetInvalidTokenErrorMessage(InstanceType.String, InstanceType.Null.ToString()),
            ImmutableJsonPointer.Empty,
            ImmutableJsonPointer.Create("/type")!,
            new Uri(BodyJsonSchemaDocument.DefaultDocumentBaseUri, typeof(string).FullName),
            new Uri(BodyJsonSchemaDocument.DefaultDocumentBaseUri, typeof(string).FullName)
        ));
    }

    private static IEnumerable<TestSample> CreateSamplesForArray()
    {
        yield return TestSample.Create<int[]>("[]", ValidationResult.ValidResult);
        yield return TestSample.Create<int[]>("[1, 2]", ValidationResult.ValidResult);
        yield return TestSample.Create<object[]>("""
[
  {
    "Prop": 1.5
  }
]
""", ValidationResult.ValidResult);

        yield return TestSample.Create<ArrayItemObject[]>("""
[
  {
    "Prop": 1.5
  }
]
""", ValidationResult.ValidResult);

        yield return TestSample.Create<int[]>("null", new ValidationResult(ResultCode.InvalidTokenKind, "type", GetInvalidTokenErrorMessage(InstanceType.Array, InstanceType.Null.ToString()),
            ImmutableJsonPointer.Empty,
            ImmutableJsonPointer.Create("/type"),
            GetSchemaResourceBaseUri<int[]>(),
            GetSchemaResourceBaseUri<int[]>()));

        yield return TestSample.Create<ArrayItemObject[]>("""
[
  {
    "Prop": "abc"
  }
]
""", new ValidationResult(ResultCode.InvalidTokenKind, "type", GetInvalidTokenErrorMessage(InstanceType.Number, InstanceType.String.ToString()),
            ImmutableJsonPointer.Create("/0/Prop")!,
            ImmutableJsonPointer.Create("/items/$ref/properties/Prop/type"),
            GetSchemaResourceBaseUri<ArrayItemObject[]>(),
            GetSubSchemaRefFullUriForDefs<ArrayItemObject[], ArrayItemObject>()));
    }

    private class ArrayItemObject
    {
        public float Prop { get; set; }
    }

    private static IEnumerable<TestSample> CreateSamplesForObject()
    {
        yield return TestSample.Create<CustomObject>("""
{
  "PropName": {
    "InnerProp": "abc"
  }
}
""", ValidationResult.ValidResult);

        yield return TestSample.Create<CustomObject>("""
{
  "PropName": "invalid"
}
""", new ValidationResult(ResultCode.InvalidTokenKind, "type", GetInvalidTokenErrorMessage(InstanceType.Object, InstanceType.String.ToString()),
            ImmutableJsonPointer.Create("/PropName")!,
            ImmutableJsonPointer.Create("/properties/PropName/$ref/type"),
            GetSchemaResourceBaseUri<CustomObject>(),
            GetSubSchemaRefFullUriForDefs<CustomObject, InnerCustomObject>()));
    }

    private class CustomObject
    {
        public InnerCustomObject? PropName { get; set; }
    }

    private class InnerCustomObject
    {
        public string? InnerProp { get; set; }
    }

    private static IEnumerable<TestSample> CreateSamplesForStringDictionary()
    {
        yield return TestSample.Create<StringDictionaryTestClass>("""
            {
              "Prop": {
                "P1": {
                  "InnerProp": "string"
                }
              }
            }
            """, ValidationResult.ValidResult);

        yield return TestSample.Create<StringDictionaryTestClass>("""
            {
              "Prop": {
                "P1": 123
              }
            }
            """, new ValidationResult(ResultCode.InvalidTokenKind, "type", GetInvalidTokenErrorMessage(InstanceType.Object, InstanceType.Number.ToString()),
            ImmutableJsonPointer.Create("/Prop/P1")!,
            ImmutableJsonPointer.Create("/properties/Prop/additionalProperties/$ref/type"),
            GetSchemaResourceBaseUri<StringDictionaryTestClass>(),
            GetSubSchemaRefFullUriForDefs<StringDictionaryTestClass, InnerCustomObject>()
        ));
    }

    class StringDictionaryTestClass
    {
        public Dictionary<string, InnerCustomObject> Prop { get; set; } = null!;
    }

    private static IEnumerable<TestSample> CreateSamplesForEnum()
    {
        yield return TestSample.Create<TestEnum>("\"A\"", ValidationResult.ValidResult);

        yield return TestSample.Create<TestEnum>("\"D\"", new ValidationResult(ResultCode.NotFoundInAllowedList, "enum", EnumKeyword.ErrorMessage(),
            ImmutableJsonPointer.Empty, 
            ImmutableJsonPointer.Create("/enum"),
            GetSchemaResourceBaseUri<TestEnum>(),
            GetSchemaResourceBaseUri<TestEnum>()));

        yield return TestSample.Create<TestEnum>("0", new ValidationResult(ResultCode.InvalidTokenKind, "type", GetInvalidTokenErrorMessage(InstanceType.String, InstanceType.Number.ToString()),
            ImmutableJsonPointer.Empty, 
            ImmutableJsonPointer.Create("/type"),
            GetSchemaResourceBaseUri<TestEnum>(),
            GetSchemaResourceBaseUri<TestEnum>()
            ));
    }

    private enum TestEnum
    {
        A,
        B,
        C
    }

    private static IEnumerable<TestSample> CreateSamplesForGuid()
    {
        yield return TestSample.Create<Guid>($"\"{Guid.NewGuid()}\"", ValidationResult.ValidResult);

        yield return TestSample.Create<Guid>("\"123321\"", new ValidationResult(ResultCode.InvalidFormat, "format", FormatKeyword.ErrorMessage("uuid"),
            ImmutableJsonPointer.Empty, 
            ImmutableJsonPointer.Create("/format"), 
            GetSchemaResourceBaseUri<Guid>(),
            GetSchemaResourceBaseUri<Guid>()));
    }

    private static IEnumerable<TestSample> CreateSamplesForUri()
    {
        yield return TestSample.Create<Uri>("\"http://localhost\"", ValidationResult.ValidResult);

        yield return TestSample.Create<Uri>("\"localhost\"", ValidationResult.ValidResult);
    }

    private static Uri GetSubSchemaRefFullUriForDefs<TDocument, TSubSchema>()
    {
        return new Uri(GetSchemaResourceBaseUri<TDocument>(), "#/defs/" + typeof(TSubSchema).FullName);
    }

    private static IEnumerable<TestSample> CreateSamplesForJsonSchemaNamingPolicy()
    {
        yield return TestSample.Create<CustomObject>("""
            {
              "PropName": null
            }
            """, new ValidationResult(ResultCode.InvalidTokenKind, "type", GetInvalidTokenErrorMessage(InstanceType.Object, InstanceType.Null.ToString()),
            ImmutableJsonPointer.Create("/PropName")!,
            ImmutableJsonPointer.Create("/properties/PropName/$ref/type"), 
            GetSchemaResourceBaseUri<CustomObject>(),
            GetSubSchemaRefFullUriForDefs<CustomObject, InnerCustomObject>()), new JsonSchemaGeneratorOptions{PropertyNamingPolicy = JsonSchemaNamingPolicy.SharedDefault});

        yield return TestSample.Create<CustomObject>("""
            {
              "propName": null
            }
            """, new ValidationResult(ResultCode.InvalidTokenKind, "type", GetInvalidTokenErrorMessage(InstanceType.Object, InstanceType.Null.ToString()),
            ImmutableJsonPointer.Create("/propName")!,
            ImmutableJsonPointer.Create("/properties/propName/$ref/type"),
            GetSchemaResourceBaseUri<CustomObject>(),
            GetSubSchemaRefFullUriForDefs<CustomObject, InnerCustomObject>()), new JsonSchemaGeneratorOptions { PropertyNamingPolicy = JsonSchemaNamingPolicy.CamelCase });

        yield return TestSample.Create<CustomObject>("""
            {
              "prop_name": null
            }
            """, new ValidationResult(ResultCode.InvalidTokenKind, "type", GetInvalidTokenErrorMessage(InstanceType.Object, InstanceType.Null.ToString()),
            ImmutableJsonPointer.Create("/prop_name")!,
            ImmutableJsonPointer.Create("/properties/prop_name/$ref/type"),
            GetSchemaResourceBaseUri<CustomObject>(),
            GetSubSchemaRefFullUriForDefs<CustomObject, InnerCustomObject>()), new JsonSchemaGeneratorOptions { PropertyNamingPolicy = JsonSchemaNamingPolicy.SnakeCaseLower });

        yield return TestSample.Create<CustomObject>("""
            {
              "PROP_NAME": null
            }
            """, new ValidationResult(ResultCode.InvalidTokenKind, "type", GetInvalidTokenErrorMessage(InstanceType.Object, InstanceType.Null.ToString()),
            ImmutableJsonPointer.Create("/PROP_NAME")!,
            ImmutableJsonPointer.Create("/properties/PROP_NAME/$ref/type"),
            GetSchemaResourceBaseUri<CustomObject>(),
            GetSubSchemaRefFullUriForDefs<CustomObject, InnerCustomObject>()), new JsonSchemaGeneratorOptions { PropertyNamingPolicy = JsonSchemaNamingPolicy.SnakeCaseUpper });

        yield return TestSample.Create<CustomObject>("""
            {
              "prop-name": null
            }
            """, new ValidationResult(ResultCode.InvalidTokenKind, "type", GetInvalidTokenErrorMessage(InstanceType.Object, InstanceType.Null.ToString()),
            ImmutableJsonPointer.Create("/prop-name")!,
            ImmutableJsonPointer.Create("/properties/prop-name/$ref/type"),
            GetSchemaResourceBaseUri<CustomObject>(),
            GetSubSchemaRefFullUriForDefs<CustomObject, InnerCustomObject>()), new JsonSchemaGeneratorOptions { PropertyNamingPolicy = JsonSchemaNamingPolicy.KebabCaseLower });

        yield return TestSample.Create<CustomObject>("""
            {
              "PROP-NAME": null
            }
            """, new ValidationResult(ResultCode.InvalidTokenKind, "type", GetInvalidTokenErrorMessage(InstanceType.Object, InstanceType.Null.ToString()),
            ImmutableJsonPointer.Create("/PROP-NAME")!,
            ImmutableJsonPointer.Create("/properties/PROP-NAME/$ref/type"),
            GetSchemaResourceBaseUri<CustomObject>(),
            GetSubSchemaRefFullUriForDefs<CustomObject, InnerCustomObject>()), new JsonSchemaGeneratorOptions { PropertyNamingPolicy = JsonSchemaNamingPolicy.KebabCaseUpper });
    }

    private static IEnumerable<TestSample> CreateSamplesForJsonSchemaIgnoreAttribute()
    {
        yield return TestSample.Create<JsonSchemaIgnoreAttributeTestClass>("""
            {
              "Prop": null
            }
            """, ValidationResult.ValidResult);
    }

    private class JsonSchemaIgnoreAttributeTestClass
    {
        [JsonSchemaIgnore]
        public int Prop { get; set; }
    }

    private static IEnumerable<TestSample> CreateSamplesForRequiredAttribute()
    {
        yield return TestSample.Create<RequiredAttributeTestClass>("""
            {
              "Prop": 1
            }
            """, ValidationResult.ValidResult);

        yield return TestSample.Create<RequiredAttributeTestClass>("{}", new ValidationResult(ResultCode.NotFoundRequiredProperty, "required", RequiredKeyword.ErrorMessage("Prop"), 
            ImmutableJsonPointer.Create("")!, 
            ImmutableJsonPointer.Create("/required"),
            GetSchemaResourceBaseUri<RequiredAttributeTestClass>(),
            GetSchemaResourceBaseUri<RequiredAttributeTestClass>()));

        yield return TestSample.Create<RequiredAttributeForCustomNamedPropertyTestClass>("""
            {
              "NewPropName": 1
            }
            """, ValidationResult.ValidResult);

        yield return TestSample.Create<RequiredAttributeForCustomNamedPropertyTestClass>("""
        {
          "Prop": 1
        }
        """, new ValidationResult(ResultCode.NotFoundRequiredProperty, "required", RequiredKeyword.ErrorMessage("NewPropName"),
            ImmutableJsonPointer.Create("")!,
            ImmutableJsonPointer.Create("/required"),
            GetSchemaResourceBaseUri<RequiredAttributeForCustomNamedPropertyTestClass>(),
            GetSchemaResourceBaseUri<RequiredAttributeForCustomNamedPropertyTestClass>()));
    }

    private class RequiredAttributeTestClass
    {
        [Required]
        public int Prop { get; set; }
    }

    private class RequiredAttributeForCustomNamedPropertyTestClass
    {
        [Required]
        [JsonPropertyName("NewPropName")]
        public int Prop { get; set; }
    }

    private static IEnumerable<TestSample> CreateSamplesForUniqueItemsAttribute()
    {
        yield return TestSample.Create<UniqueItemsAttributeTestClass>("""
            {
              "Prop": [1, 2, 3]
            }
            """, ValidationResult.ValidResult);

        yield return TestSample.Create<UniqueItemsAttributeTestClass>("""
            {
              "Prop": [1, 2, 1]
            }
            """, new ValidationResult(ResultCode.DuplicatedArrayItems, "uniqueItems", UniqueItemsKeyword.ErrorMessage(),
            ImmutableJsonPointer.Create("/Prop")!,
            ImmutableJsonPointer.Create("/properties/Prop/uniqueItems"), 
            GetSchemaResourceBaseUri<UniqueItemsAttributeTestClass>(),
            GetSchemaResourceBaseUri<UniqueItemsAttributeTestClass>()));
    }

    private class UniqueItemsAttributeTestClass
    {
        [UniqueItems]
        public int[]? Prop { get; set; }
    }

    private static IEnumerable<TestSample> CreateSamplesForPropertyNameAttribute()
    {
        yield return TestSample.Create<PropertyNameAttributeTestClass>("""
            {
              "Prop": null
            }
            """, ValidationResult.ValidResult);

        yield return TestSample.Create<PropertyNameAttributeTestClass>("""
            {
              "NewPropName": null
            }
            """, new ValidationResult(ResultCode.InvalidTokenKind, "type", GetInvalidTokenErrorMessage(InstanceType.Integer, InstanceType.Null.ToString()),
            ImmutableJsonPointer.Create("/NewPropName")!,
            ImmutableJsonPointer.Create("/properties/NewPropName/type"),
            GetSchemaResourceBaseUri<PropertyNameAttributeTestClass>(),
            GetSchemaResourceBaseUri<PropertyNameAttributeTestClass>()));
    }

    private class PropertyNameAttributeTestClass
    {
        [JsonPropertyName("NewPropName")]
        public int Prop { get; set; }
    }

    private static IEnumerable<TestSample> CreateSamplesForPatternAttribute()
    {
        yield return TestSample.Create<PatternAttributeTestClass>(""" 
{
  "Prop": "ab"
}
""", ValidationResult.ValidResult);

        yield return TestSample.Create<PatternAttributeTestClass>(""" 
{
  "Prop": "aa"
}
""", new ValidationResult(ResultCode.RegexNotMatch, "pattern", PatternKeyword.ErrorMessage("a*b", "aa"),
            ImmutableJsonPointer.Create("/Prop")!,
            ImmutableJsonPointer.Create("/properties/Prop/pattern"),
            GetSchemaResourceBaseUri<PatternAttributeTestClass>(),
            GetSchemaResourceBaseUri<PatternAttributeTestClass>()
        ));
    }

    private class PatternAttributeTestClass
    {
        [Pattern("a*b")]
        public string? Prop { get; set; }
    }

    private static Uri GetSchemaResourceBaseUri<T>() 
        => new(BodyJsonSchemaDocument.DefaultDocumentBaseUri, typeof(T).FullName);

    private static string GetInvalidTokenErrorMessage(InstanceType expected, string actual) 
        => $"Expect type '{expected}' but actual is '{actual}'";

    private static IEnumerable<TestSample> CreateSamplesForMaximumAttribute()
    {
        yield return TestSample.Create<MaximumAttributeTestClass>("""
{
  "Prop": 2.5
}
""", ValidationResult.ValidResult);

        yield return TestSample.Create<MaximumAttributeTestClass>("""
{
  "Prop": 1.5
}
""", ValidationResult.ValidResult);

        yield return TestSample.Create<MaximumAttributeTestClass>("""
{
  "Prop": 2.50001
}
""", new ValidationResult(ResultCode.NumberOutOfRange, "maximum", MaximumKeyword.ErrorMessage(2.50001, 2.5),
            ImmutableJsonPointer.Create("/Prop")!, ImmutableJsonPointer.Create("/properties/Prop/maximum"),
            GetSchemaResourceBaseUri<MaximumAttributeTestClass>(),
            GetSchemaResourceBaseUri<MaximumAttributeTestClass>()
        ));
    }

    internal class MaximumAttributeTestClass
    {
        [Maximum(2.5)]
        public double Prop { get; set; }
    }

    private static IEnumerable<TestSample> CreateSamplesForMinimumAttribute()
    {
        yield return TestSample.Create<MinimumAttributeTestClass>("""
{
  "Prop": 2.5
}
""", ValidationResult.ValidResult);

        yield return TestSample.Create<MinimumAttributeTestClass>("""
{
  "Prop": 3.5
}
""", ValidationResult.ValidResult);

        yield return TestSample.Create<MinimumAttributeTestClass>("""
{
  "Prop": 2.499999
}
""", new ValidationResult(ResultCode.NumberOutOfRange, "minimum", MinimumKeyword.ErrorMessage(2.499999, 2.5),
            ImmutableJsonPointer.Create("/Prop")!, ImmutableJsonPointer.Create("/properties/Prop/minimum"),
            GetSchemaResourceBaseUri<MinimumAttributeTestClass>(),
            GetSchemaResourceBaseUri<MinimumAttributeTestClass>()
        ));
    }

    private class MinimumAttributeTestClass
    {
        [Minimum(2.5)]
        public double Prop { get; set; }
    }

    private static IEnumerable<TestSample> CreateSamplesForNumberRangeAttribute()
    {
        yield return TestSample.Create<NumberRangeAttributeTestClass>("""
{
  "Prop": 1.0
}
""", ValidationResult.ValidResult);

        yield return TestSample.Create<NumberRangeAttributeTestClass>("""
{
  "Prop": -1.5
}
""", ValidationResult.ValidResult);

        yield return TestSample.Create<NumberRangeAttributeTestClass>("""
{
  "Prop": 2.5
}
""", ValidationResult.ValidResult);

        yield return TestSample.Create<NumberRangeAttributeTestClass>("""
{
  "Prop": -1.50001
}
""", new ValidationResult(ResultCode.NumberOutOfRange, "minimum", MinimumKeyword.ErrorMessage(-1.50001, -1.5), 
            ImmutableJsonPointer.Create("/Prop")!,
            ImmutableJsonPointer.Create("/properties/Prop/allOf/0/minimum"), 
            GetSchemaResourceBaseUri<NumberRangeAttributeTestClass>(),
            GetSchemaResourceBaseUri<NumberRangeAttributeTestClass>()
        ));

        yield return TestSample.Create<NumberRangeAttributeTestClass>("""
{
  "Prop": 2.50001
}
""", new ValidationResult(ResultCode.NumberOutOfRange, "maximum", MaximumKeyword.ErrorMessage(2.50001, 2.5),
            ImmutableJsonPointer.Create("/Prop")!,
            ImmutableJsonPointer.Create("/properties/Prop/allOf/1/maximum"),
            GetSchemaResourceBaseUri<NumberRangeAttributeTestClass>(),
            GetSchemaResourceBaseUri<NumberRangeAttributeTestClass>()
        ));
    }

    private class NumberRangeAttributeTestClass
    {
        [NumberRange(-1.5, 2.5)]
        public double Prop { get; set; }
    }

    private static IEnumerable<TestSample> CreateSamplesForIPv6Attribute()
    {
        yield return TestSample.Create<IPv6AttributeTestClass>(
            """
{
  "Prop": "AA22:BB11:1122:CDEF:1234:AA99:7654:7410"
}
""",
            ValidationResult.ValidResult);

        yield return TestSample.Create<IPv6AttributeTestClass>(
            """
{
  "Prop": "12345::"
}
""",
            new ValidationResult(
                ResultCode.InvalidFormat,
                "format",
                FormatKeyword.ErrorMessage("ipv6"),
                ImmutableJsonPointer.Create("/Prop")!,
                ImmutableJsonPointer.Create("/properties/Prop/format")!,
                GetSchemaResourceBaseUri<IPv6AttributeTestClass>(),
                GetSchemaResourceBaseUri<IPv6AttributeTestClass>()));
    }

    private class IPv6AttributeTestClass
    {
        [IPv6]
        public string? Prop { get; set; }
    }

    private static IEnumerable<TestSample> CreateSamplesForMinLengthAttribute()
    {
        yield return TestSample.Create<MinLengthAttributeTestClass<string>>("""
{
  "Prop": "a"
}
""", ValidationResult.ValidResult);

        yield return TestSample.Create<MinLengthAttributeTestClass<int[]>>("""
{
  "Prop": [1]
}
""", ValidationResult.ValidResult);

        yield return TestSample.Create<MinLengthAttributeTestClass<string>>("""
{
  "Prop": ""
}
""", new ValidationResult(ResultCode.StringLengthOutOfRange, "minLength", MinLengthKeyword.ErrorMessage(0, 1),
            ImmutableJsonPointer.Create("/Prop")!, ImmutableJsonPointer.Create("/properties/Prop/minLength"),
            GetSchemaResourceBaseUri<MinLengthAttributeTestClass<string>>(),
            GetSchemaResourceBaseUri<MinLengthAttributeTestClass<string>>()
        ));

        yield return TestSample.Create<MinLengthAttributeTestClass<int[]>>("""
{
  "Prop": []
}
""", new ValidationResult(ResultCode.ArrayLengthOutOfRange, "minItems", MinItemsKeyword.ErrorMessage(0, 1),
            ImmutableJsonPointer.Create("/Prop")!, ImmutableJsonPointer.Create("/properties/Prop/minItems"),
            GetSchemaResourceBaseUri<MinLengthAttributeTestClass<int[]>>(),
            GetSchemaResourceBaseUri<MinLengthAttributeTestClass<int[]>>()
        ));
    }

    private class MinLengthAttributeTestClass<T>
    {
        [MinLength(1)]
        public T? Prop { get; set; }
    }

    private static IEnumerable<TestSample> CreateSamplesForMaxLengthAttribute()
    {
        yield return TestSample.Create<MaxLengthAttributeTestClass<string>>("""
{
  "Prop": "abc"
}
""", ValidationResult.ValidResult);

        yield return TestSample.Create<MaxLengthAttributeTestClass<int[]>>("""
{
  "Prop": [1, 2, 3]
}
""", ValidationResult.ValidResult);

        yield return TestSample.Create<MaxLengthAttributeTestClass<string>>("""
{
  "Prop": "abcd"
}
""", new ValidationResult(ResultCode.StringLengthOutOfRange, "maxLength", MaxLengthKeyword.ErrorMessage(4, 3),
            ImmutableJsonPointer.Create("/Prop")!, ImmutableJsonPointer.Create("/properties/Prop/maxLength"),
            GetSchemaResourceBaseUri<MaxLengthAttributeTestClass<string>>(),
            GetSchemaResourceBaseUri<MaxLengthAttributeTestClass<string>>()
        ));

        yield return TestSample.Create<MaxLengthAttributeTestClass<int[]>>("""
{
  "Prop": [1, 2, 3, 4]
}
""", new ValidationResult(ResultCode.ArrayLengthOutOfRange, "maxItems", MaxItemsKeyword.ErrorMessage(4, 3),
            ImmutableJsonPointer.Create("/Prop")!, ImmutableJsonPointer.Create("/properties/Prop/maxItems"),
            GetSchemaResourceBaseUri<MaxLengthAttributeTestClass<int[]>>(),
            GetSchemaResourceBaseUri<MaxLengthAttributeTestClass<int[]>>()
        ));
    }

    private class MaxLengthAttributeTestClass<T>
    {
        [MaxLength(3)]
        public T? Prop { get; set; }
    }

    private static IEnumerable<TestSample> CreateSamplesForLengthRangeAttribute()
    {
        yield return TestSample.Create<LengthRangeAttributeTestClass<string>>("""
{
  "Prop": "a"
}
""", ValidationResult.ValidResult);

        yield return TestSample.Create<LengthRangeAttributeTestClass<int[]>>("""
{
  "Prop": [1]
}
""", ValidationResult.ValidResult);

        yield return TestSample.Create<LengthRangeAttributeTestClass<string>>("""
{
  "Prop": "ab"
}
""", ValidationResult.ValidResult);

        yield return TestSample.Create<LengthRangeAttributeTestClass<int[]>>("""
{
  "Prop": [1, 2]
}
""", ValidationResult.ValidResult);

        yield return TestSample.Create<LengthRangeAttributeTestClass<string>>("""
{
  "Prop": "abc"
}
""", ValidationResult.ValidResult);

        yield return TestSample.Create<LengthRangeAttributeTestClass<int[]>>("""
{
  "Prop": [1, 2, 3]
}
""", ValidationResult.ValidResult);

        yield return TestSample.Create<LengthRangeAttributeTestClass<string>>("""
{
  "Prop": ""
}
""", new ValidationResult(ResultCode.StringLengthOutOfRange, "minLength", MinLengthKeyword.ErrorMessage(0, 1),
            ImmutableJsonPointer.Create("/Prop")!, ImmutableJsonPointer.Create("/properties/Prop/allOf/0/minLength"),
            GetSchemaResourceBaseUri<LengthRangeAttributeTestClass<string>>(),
            GetSchemaResourceBaseUri<LengthRangeAttributeTestClass<string>>()
        ));

        yield return TestSample.Create<LengthRangeAttributeTestClass<int[]>>("""
{
  "Prop": []
}
""", new ValidationResult(ResultCode.ArrayLengthOutOfRange, "minItems", MinItemsKeyword.ErrorMessage(0, 1),
            ImmutableJsonPointer.Create("/Prop")!, ImmutableJsonPointer.Create("/properties/Prop/allOf/0/minItems"),
            GetSchemaResourceBaseUri<LengthRangeAttributeTestClass<int[]>>(),
            GetSchemaResourceBaseUri<LengthRangeAttributeTestClass<int[]>>()
        ));

        yield return TestSample.Create<LengthRangeAttributeTestClass<string>>("""
{
  "Prop": "abcd"
}
""", new ValidationResult(ResultCode.StringLengthOutOfRange, "maxLength", MaxLengthKeyword.ErrorMessage(4, 3),
            ImmutableJsonPointer.Create("/Prop")!, ImmutableJsonPointer.Create("/properties/Prop/allOf/1/maxLength"),
            GetSchemaResourceBaseUri<LengthRangeAttributeTestClass<string>>(),
            GetSchemaResourceBaseUri<LengthRangeAttributeTestClass<string>>()
        ));

        yield return TestSample.Create<LengthRangeAttributeTestClass<int[]>>("""
{
  "Prop": [1, 2, 3, 4]
}
""", new ValidationResult(ResultCode.ArrayLengthOutOfRange, "maxItems", MaxItemsKeyword.ErrorMessage(4, 3),
            ImmutableJsonPointer.Create("/Prop")!, ImmutableJsonPointer.Create("/properties/Prop/allOf/1/maxItems"),
            GetSchemaResourceBaseUri<LengthRangeAttributeTestClass<int[]>>(),
            GetSchemaResourceBaseUri<LengthRangeAttributeTestClass<int[]>>()
        ));
    }

    private class LengthRangeAttributeTestClass<T>
    {
        [LengthRange(1, 3)]
        public T? Prop { get; set; }
    }

    private static IEnumerable<TestSample> CreateSamplesForIPv4Attribute()
    {
        yield return TestSample.Create<IPv4AttributeTestClass>(
            """
{
  "Prop": "192.168.1.1"
}
""",
            ValidationResult.ValidResult);

        yield return TestSample.Create<IPv4AttributeTestClass>(
            """
{
  "Prop": "127.0.0.0.1"
}
""",
            new ValidationResult(
                ResultCode.InvalidFormat,
                "format",
                FormatKeyword.ErrorMessage("ipv4"),
                ImmutableJsonPointer.Create("/Prop")!,
                ImmutableJsonPointer.Create("/properties/Prop/format")!,
                GetSchemaResourceBaseUri<IPv4AttributeTestClass>(),
                GetSchemaResourceBaseUri<IPv4AttributeTestClass>()));
    }

    private class IPv4AttributeTestClass
    {
        [IPv4]
        public string? Prop { get; set; }
    }

    private static IEnumerable<TestSample> CreateSamplesForStringEnumAttribute()
    {
        yield return TestSample.Create<StringEnumAttributeTestClass>(
            """
{
  "Prop": "b"
}
""",
            ValidationResult.ValidResult);
        yield return TestSample.Create<StringEnumAttributeTestClass>(
            """
{
  "Prop": 1
}
""",
            new ValidationResult(ResultCode.NotFoundInAllowedList, "enum", "Not found in allowed list",
                ImmutableJsonPointer.Create("/Prop")!,
                ImmutableJsonPointer.Create("/properties/Prop/enum"),
                GetSchemaResourceBaseUri<StringEnumAttributeTestClass>(),
                GetSchemaResourceBaseUri<StringEnumAttributeTestClass>()
            ));
        yield return TestSample.Create<StringEnumAttributeTestClass>(
            """
{
  "Prop": "c"
}
""",
            new ValidationResult(ResultCode.NotFoundInAllowedList, "enum", "Not found in allowed list",
                ImmutableJsonPointer.Create("/Prop")!,
                ImmutableJsonPointer.Create("/properties/Prop/enum"),
                GetSchemaResourceBaseUri<StringEnumAttributeTestClass>(),
                GetSchemaResourceBaseUri<StringEnumAttributeTestClass>()));
    }

    internal class StringEnumAttributeTestClass
    {
        [StringEnum("a", "b")]
        public string? Prop { get; set; }
    }

    private static IEnumerable<TestSample> CreateSamplesForEmailAttribute()
    {
        yield return TestSample.Create<EmailAttributeTestClass>(
            """
{
  "Email": "hello@world.com"
}
""", ValidationResult.ValidResult);

        yield return TestSample.Create<EmailAttributeTestClass>(
            """
{
  "Email": "@world.com"
} 
""", new ValidationResult(
                ResultCode.InvalidFormat,
                "format",
                FormatKeyword.ErrorMessage("email"),
                ImmutableJsonPointer.Create("/Email")!,
                ImmutableJsonPointer.Create("/properties/Email/format")!,
                new Uri(BodyJsonSchemaDocument.DefaultDocumentBaseUri, typeof(EmailAttributeTestClass).FullName),
                new Uri(BodyJsonSchemaDocument.DefaultDocumentBaseUri, typeof(EmailAttributeTestClass).FullName)));
    }

    private class EmailAttributeTestClass
    {
        [Email]
        public string? Email { get; set; }
    }

    private static IEnumerable<TestSample> CreateSamplesForMultipleOfAttribute()
    {
        yield return TestSample.Create<MultipleOfAttributeTestClass>("""
{
  "Prop": 4.5
}
""", ValidationResult.ValidResult);

        yield return TestSample.Create<MultipleOfAttributeTestClass>("""
{
  "Prop": -4.5
}
""", ValidationResult.ValidResult);

        yield return TestSample.Create<MultipleOfAttributeTestClass>("""
{
  "Prop": 4.3
}
""", new ValidationResult(ResultCode.FailedToMultiple, "multipleOf", MultipleOfKeyword.ErrorMessage(4.3, 1.5),
            ImmutableJsonPointer.Create("/Prop")!, ImmutableJsonPointer.Create("/properties/Prop/multipleOf"),
            GetSchemaResourceBaseUri<MultipleOfAttributeTestClass>(),
            GetSchemaResourceBaseUri<MultipleOfAttributeTestClass>()
        ));
    }

    private class MultipleOfAttributeTestClass
    {
        [MultipleOf(1.5)]
        public double Prop { get; set; }
    }

    private static IEnumerable<TestSample> CreateSamplesForIntegerEnumAttribute()
    {
        yield return TestSample.Create<IntegerEnumAttributeTestClass>("""
{
 "Prop": 1
}
""", ValidationResult.ValidResult);

        yield return TestSample.Create<IntegerEnumAttributeTestClass>("""
{
 "Prop": 3
}
""", new ValidationResult(ResultCode.NotFoundInAllowedList, "enum", EnumKeyword.ErrorMessage(),
            ImmutableJsonPointer.Create("/Prop")!, ImmutableJsonPointer.Create("/properties/Prop/enum"),
            GetSchemaResourceBaseUri<IntegerEnumAttributeTestClass>(),
            GetSchemaResourceBaseUri<IntegerEnumAttributeTestClass>()
        ));
    }

    private class IntegerEnumAttributeTestClass
    {
        [IntegerEnum(1, 2)]
        public int Prop { get; set; }
    }

    private static IEnumerable<TestSample> CreateSamplesForExclusiveMaximumAttribute()
    {
        yield return TestSample.Create<ExclusiveMaximumAttributeTestClass>("""
{
  "Prop": 2.5
}
""", ValidationResult.ValidResult);

        yield return TestSample.Create<ExclusiveMaximumAttributeTestClass>("""
{
  "Prop": 2.9999
}
""", ValidationResult.ValidResult);

        yield return TestSample.Create<ExclusiveMaximumAttributeTestClass>("""
{
  "Prop": 3.0
}
""", new ValidationResult(ResultCode.NumberOutOfRange, "exclusiveMaximum", ExclusiveMaximumKeyword.ErrorMessage(3.0, 3),
            ImmutableJsonPointer.Create("/Prop")!, ImmutableJsonPointer.Create("/properties/Prop/exclusiveMaximum"),
            GetSchemaResourceBaseUri<ExclusiveMaximumAttributeTestClass>(),
            GetSchemaResourceBaseUri<ExclusiveMaximumAttributeTestClass>()
        ));
    }

    private class ExclusiveMaximumAttributeTestClass
    {
        [ExclusiveMaximum(3)]
        public double Prop { get; set; }
    }

    private static IEnumerable<TestSample> CreateSamplesForExclusiveMinimumAttribute()
    {
        yield return TestSample.Create<ExclusiveMinimumAttributeTestClass>("""
{
  "Prop": 3.5
}
""", ValidationResult.ValidResult);

        yield return TestSample.Create<ExclusiveMinimumAttributeTestClass>("""
{
  "Prop": 2.5001
}
""", ValidationResult.ValidResult);

        yield return TestSample.Create<ExclusiveMinimumAttributeTestClass>("""
{
  "Prop": 2.5
}
""", new ValidationResult(ResultCode.NumberOutOfRange, "exclusiveMinimum", ExclusiveMinimumKeyword.ErrorMessage(2.5, 2.5),
            ImmutableJsonPointer.Create("/Prop")!, ImmutableJsonPointer.Create("/properties/Prop/exclusiveMinimum"),
            GetSchemaResourceBaseUri<ExclusiveMinimumAttributeTestClass>(),
            GetSchemaResourceBaseUri<ExclusiveMinimumAttributeTestClass>()
        ));
    }

    private class ExclusiveMinimumAttributeTestClass
    {
        [ExclusiveMinimum(2.5)]
        public double Prop { get; set; }
    }

    private class TestSample
    {
        private TestSample(Type type, string jsonInstance, ValidationResult expectedValidationResult, JsonSchemaGeneratorOptions? options)
        {
            Type = type;
            JsonInstance = jsonInstance;
            ExpectedValidationResult = expectedValidationResult;
            Options = options;
        }

        public Type Type { get; }
        public string JsonInstance { get; }
        public JsonSchemaGeneratorOptions? Options { get; }
        public ValidationResult ExpectedValidationResult { get; }

        public static TestSample Create<T>(string jsonInstance, ValidationResult expectedValidationResult, JsonSchemaGeneratorOptions? options = null)
        {
            return new TestSample(typeof(T), jsonInstance, expectedValidationResult, options);
        }
    }
}



