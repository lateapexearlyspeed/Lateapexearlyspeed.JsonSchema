using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using LateApexEarlySpeed.Json.Schema.Common;
using LateApexEarlySpeed.Json.Schema.FluentGenerator.ExtendedKeywords;
using LateApexEarlySpeed.Json.Schema.Generator;
using LateApexEarlySpeed.Json.Schema.JSchema;
using LateApexEarlySpeed.Json.Schema.Keywords;
using Xunit;
using ValidationResult = LateApexEarlySpeed.Json.Schema.Common.ValidationResult;

// ReSharper disable ClassNeverInstantiated.Local
// ReSharper disable InconsistentNaming

namespace LateApexEarlySpeed.Json.Schema.UnitTests;

public class JsonSchemaGeneratorTest
{
    [Fact]
    public void GenerateJsonValidator_LoopReference_Throw()
    {
        Assert.Throws<InvalidOperationException>(() => JsonSchemaGenerator.GenerateJsonValidator<LoopReferenceClass>());
    }

    class LoopReferenceClass
    {
        public InnerLoopReference? Prop { get; set; }
    }

    class InnerLoopReference
    {
        public LoopReferenceClass? Prop { get; set; }
    }

    [Theory]
    [MemberData(nameof(TestData))]
    public void GenerateJsonValidator_Validate(Type type, string jsonInstance, ValidationResult expectedValidationResult, JsonSchemaGeneratorOptions? options)
    {
        JsonValidator jsonValidator = JsonSchemaGenerator.GenerateJsonValidator(type, options);
        ValidationResult validationResult = jsonValidator.Validate(jsonInstance);

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
        
        // Collection
        samples = samples.Concat(CreateSamplesForCollection(typeof(Array)));
        samples = samples.Concat(CreateSamplesForCollection(typeof(List<>)));
        samples = samples.Concat(CreateSamplesForCollection(typeof(Queue<>)));
        samples = samples.Concat(CreateSamplesForCollection(typeof(Stack<>)));
        samples = samples.Concat(CreateSamplesForCollection(typeof(HashSet<>)));
        samples = samples.Concat(CreateSamplesForCollection(typeof(IReadOnlyList<>)));
        samples = samples.Concat(CreateSamplesForCollection(typeof(ReadOnlyCollection<>)));
        samples = samples.Concat(CreateSamplesForCollection(typeof(ImmutableArray<>)));
        samples = samples.Concat(CreateSamplesForCollection(typeof(ImmutableHashSet<>)));
        samples = samples.Concat(CreateSamplesForCollection(typeof(ImmutableList<>)));
        
        // Object
        samples = samples.Concat(CreateSamplesForCustomClass());
        
        // Struct
        samples = samples.Concat(CreateSamplesForCustomStruct());
        
        // Dictionary<string,>
        samples = samples.Concat(CreateSamplesForStringDictionary());
        
        // Enum
        samples = samples.Concat(CreateSamplesForEnum());
        
        // Guid
        samples = samples.Concat(CreateSamplesForGuid());
        
        // Uri
        samples = samples.Concat(CreateSamplesForUri());
        
        // DateTimeOffset
        samples = samples.Concat(CreateSamplesForDateTimeOffset());
        
        // DateTime
        samples = samples.Concat(CreateSamplesForDateTime());
        
        // Arbitrary json types
        samples = samples.Concat(CreateSamplesForArbitraryJsonTypes<JsonElement>());
        samples = samples.Concat(CreateSamplesForArbitraryJsonTypes<JsonDocument>());
        samples = samples.Concat(CreateSamplesForArbitraryJsonTypes<JsonNode>());
        samples = samples.Concat(CreateSamplesForArbitraryJsonTypes<JsonValue>());
        
        // JsonArray
        samples = samples.Concat(CreateSamplesForArbitraryJsonArray());
        
        // JsonObject
        samples = samples.Concat(CreateSamplesForArbitraryJsonObject());
        
        // Nullable value type
        samples = samples.Concat(CreateSamplesForNullableValueType());
        
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
        samples = samples.Concat(CreateSamplesForJsonIgnoreAttribute());
        samples = samples.Concat(CreateSamplesForRequiredAttribute());
        samples = samples.Concat(CreateSamplesForUniqueItemsAttribute());
        samples = samples.Concat(CreateSamplesForPropertyNameAttribute());
        samples = samples.Concat(CreateSamplesForNotNullAttribute());
        samples = samples.Concat(CreateSamplesForNullabilityAnnotation());
        samples = samples.Concat(CreateSamplesForEnumWithJsonStringEnumConverter());
        samples = samples.Concat(CreateSamplesForEnumPropertyWithJsonStringEnumConverter());
        
        return samples;
    }

    private static IEnumerable<TestSample> CreateSamplesForNullableValueType()
    {
        yield return TestSample.Create<int?>("1", ValidationResult.ValidResult);
        yield return TestSample.Create<int?>("null", ValidationResult.ValidResult);
        yield return TestSample.Create<int?>("\"a\"", new ValidationResult(ResultCode.InvalidTokenKind, "type", 
            GetInvalidTokenErrorMessage(InstanceType.String.ToString(), InstanceType.Integer),
            ImmutableJsonPointer.Empty, 
            ImmutableJsonPointer.Create("/anyOf/1/type"), 
            GetSchemaResourceBaseUri<int?>(),
            GetSchemaResourceBaseUri<int?>()
            ));

        yield return TestSample.Create<CustomStruct?>("""
            {
              "PropName": {
                "InnerProp": "abc"
              }
            }
            """, ValidationResult.ValidResult);

        yield return TestSample.Create<CustomStruct?>("""
            {
              "PropName": {
                "InnerProp": 1
              }
            }
            """, new ValidationResult(ResultCode.InvalidTokenKind, "type", GetInvalidTokenErrorMessage(InstanceType.Number.ToString(), InstanceType.String, InstanceType.Null),
            ImmutableJsonPointer.Create("/PropName/InnerProp")!,
            ImmutableJsonPointer.Create("/anyOf/1/$ref/properties/PropName/$ref/properties/InnerProp/type"), 
            GetSchemaResourceBaseUri<CustomStruct?>(),
            GetSubSchemaRefFullUriForDefs<CustomStruct?, InnerCustomStruct>()
            ));
    }

    private static IEnumerable<TestSample> CreateSamplesForDateTimeOffset()
    {
        yield return TestSample.Create<DateTimeOffset>("\"1990-02-25T15:59:59-08:00\"", ValidationResult.ValidResult);
        yield return TestSample.Create<DateTimeOffset>("\"1990-02-25T15:59:59.1-08:00\"", ValidationResult.ValidResult);
        yield return TestSample.Create<DateTimeOffset>("\"1990-02-25T15:59:59.1\"", ValidationResult.ValidResult);

        yield return TestSample.Create<DateTimeOffset>("\"abc1990-02-25T15:59:59.1\"", new ValidationResult(ResultCode.InvalidFormat, "ext-DateTimeOffsetFormat", DateTimeOffsetFormatExtensionKeyword.ErrorMessage(),
            ImmutableJsonPointer.Empty, 
            ImmutableJsonPointer.Create("/ext-DateTimeOffsetFormat"), 
            GetSchemaResourceBaseUri<DateTimeOffset>(),
            GetSchemaResourceBaseUri<DateTimeOffset>()));
    }

    private static IEnumerable<TestSample> CreateSamplesForDateTime()
    {
        yield return TestSample.Create<DateTime>("\"1990-02-25T15:59:59\"", ValidationResult.ValidResult);
        yield return TestSample.Create<DateTime>("\"1990-02-25T15:59:59.1\"", ValidationResult.ValidResult);
        yield return TestSample.Create<DateTime>("\"1990-02-25T15:59:59.1-08:00\"", ValidationResult.ValidResult);

        yield return TestSample.Create<DateTime>("\"abc1990-02-25T15:59:59.1-08:00\"", new ValidationResult(ResultCode.InvalidFormat, "ext-DateTimeFormat", DateTimeFormatExtensionKeyword.ErrorMessage(),
            ImmutableJsonPointer.Empty,
            ImmutableJsonPointer.Create("/ext-DateTimeFormat"),
            GetSchemaResourceBaseUri<DateTime>(),
            GetSchemaResourceBaseUri<DateTime>()));
    }

    private static IEnumerable<TestSample> CreateSamplesForArbitraryJsonArray()
    {
        yield return TestSample.Create<JsonArray>("[1, 2, 3]", ValidationResult.ValidResult);
        yield return TestSample.Create<JsonArray>("null", new ValidationResult(ResultCode.InvalidTokenKind, "type", GetInvalidTokenErrorMessage(InstanceType.Null.ToString(), InstanceType.Array),
            ImmutableJsonPointer.Empty, 
            ImmutableJsonPointer.Create("/type"), 
            GetSchemaResourceBaseUri<JsonArray>(),
            GetSchemaResourceBaseUri<JsonArray>()));
    }

    private static IEnumerable<TestSample> CreateSamplesForArbitraryJsonObject()
    {
        yield return TestSample.Create<JsonObject>("{}", ValidationResult.ValidResult);
        yield return TestSample.Create<JsonObject>("""
            {
              "Prop": 123
            }
            """, ValidationResult.ValidResult);

        yield return TestSample.Create<JsonObject>("null", new ValidationResult(ResultCode.InvalidTokenKind, "type", GetInvalidTokenErrorMessage(InstanceType.Null.ToString(), InstanceType.Object),
            ImmutableJsonPointer.Empty,
            ImmutableJsonPointer.Create("/type"),
            GetSchemaResourceBaseUri<JsonObject>(),
            GetSchemaResourceBaseUri<JsonObject>()));
    }

    private static IEnumerable<TestSample> CreateSamplesForArbitraryJsonTypes<T>()
    {
        yield return TestSample.Create<T>("{}", ValidationResult.ValidResult);
        yield return TestSample.Create<T>("[]", ValidationResult.ValidResult);
        yield return TestSample.Create<T>("null", ValidationResult.ValidResult);
        yield return TestSample.Create<T>("1", ValidationResult.ValidResult);
        yield return TestSample.Create<T>("\"a\"", ValidationResult.ValidResult);
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
            GetSchemaResourceBaseUri<T>(),
            GetSchemaResourceBaseUri<T>()));
        yield return TestSample.Create<T>("\"abc\"", new ValidationResult(
            ResultCode.InvalidTokenKind,
            "type",
            GetInvalidTokenErrorMessage(InstanceType.String.ToString(), InstanceType.Integer),
            ImmutableJsonPointer.Empty,
            ImmutableJsonPointer.Create("/type")!,
            GetSchemaResourceBaseUri<T>(),
            GetSchemaResourceBaseUri<T>()));
        yield return TestSample.Create<T>("1.5", new ValidationResult(
            ResultCode.InvalidTokenKind,
            "type",
            GetInvalidTokenErrorMessage(InstanceType.Number.ToString(), InstanceType.Integer),
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
            GetInvalidTokenErrorMessage(InstanceType.String.ToString(), InstanceType.Integer),
            ImmutableJsonPointer.Empty,
            ImmutableJsonPointer.Create("/type")!,
            GetSchemaResourceBaseUri<T>(),
            GetSchemaResourceBaseUri<T>()));
        yield return TestSample.Create<T>("1.5", new ValidationResult(
            ResultCode.InvalidTokenKind,
            "type",
            GetInvalidTokenErrorMessage(InstanceType.Number.ToString(), InstanceType.Integer),
            ImmutableJsonPointer.Empty,
            ImmutableJsonPointer.Create("/type")!,
            GetSchemaResourceBaseUri<T>(),
            GetSchemaResourceBaseUri<T>()));

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
            GetInvalidTokenErrorMessage(InstanceType.String.ToString(), InstanceType.Number),
            ImmutableJsonPointer.Empty,
            ImmutableJsonPointer.Create("/type")!,
            GetSchemaResourceBaseUri<T>(),
            GetSchemaResourceBaseUri<T>()));

        if (typeof(T) == typeof(float))
        {
            double max = float.MaxValue;
            double instanceData = max * 2;

            yield return TestSample.Create<T>(instanceData.ToString(CultureInfo.InvariantCulture), new ValidationResult(ResultCode.NumberOutOfRange, "maximum", MaximumKeyword.ErrorMessage(instanceData, max),
                ImmutableJsonPointer.Empty, 
                ImmutableJsonPointer.Create("/maximum"), 
                GetSchemaResourceBaseUri<T>(),
                GetSchemaResourceBaseUri<T>()
                ));

            double min = float.MinValue;
            instanceData = min * 2;

            yield return TestSample.Create<T>(instanceData.ToString(CultureInfo.InvariantCulture), new ValidationResult(ResultCode.NumberOutOfRange, "minimum", MinimumKeyword.ErrorMessage(instanceData, min),
                ImmutableJsonPointer.Empty,
                ImmutableJsonPointer.Create("/minimum"),
                GetSchemaResourceBaseUri<T>(),
                GetSchemaResourceBaseUri<T>()
            ));
        }
        else if (typeof(T) == typeof(decimal))
        {
            double instanceData = (double)decimal.MaxValue * 2;

            yield return TestSample.Create<T>(instanceData.ToString(CultureInfo.InvariantCulture), new ValidationResult(ResultCode.NumberOutOfRange, "maximum",
                MaximumKeyword.ErrorMessage(instanceData, decimal.MaxValue),
                ImmutableJsonPointer.Empty,
                ImmutableJsonPointer.Create("/maximum"),
                GetSchemaResourceBaseUri<T>(),
                GetSchemaResourceBaseUri<T>()
            ));

            instanceData = (double)decimal.MinValue * 2;
            
            yield return TestSample.Create<T>(instanceData.ToString(CultureInfo.InvariantCulture), new ValidationResult(ResultCode.NumberOutOfRange, "minimum",
                MinimumKeyword.ErrorMessage(instanceData, decimal.MinValue),
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
            GetInvalidTokenErrorMessage(InstanceType.String.ToString(), InstanceType.Boolean),
            ImmutableJsonPointer.Empty,
            ImmutableJsonPointer.Create("/type")!,
            GetSchemaResourceBaseUri<bool>(),
            GetSchemaResourceBaseUri<bool>()));
    }

    private static IEnumerable<TestSample> CreateSamplesForString()
    {
        yield return TestSample.Create<string>("\"abc\"", ValidationResult.ValidResult);
        yield return TestSample.Create<string>("null", ValidationResult.ValidResult);
        yield return TestSample.Create<string>("100", new ValidationResult(
            ResultCode.InvalidTokenKind,
            "type",
            GetInvalidTokenErrorMessage(InstanceType.Number.ToString(), InstanceType.String, InstanceType.Null),
            ImmutableJsonPointer.Empty,
            ImmutableJsonPointer.Create("/type")!,
            GetSchemaResourceBaseUri<string>(),
            GetSchemaResourceBaseUri<string>()));
    }

    private static IEnumerable<TestSample> CreateSamplesForCollection(Type openGenericCollectionOrArrayType)
    {
        Type intCollection;
        Type objectCollection;
        Type arrayItemObjectCollection;

        if (openGenericCollectionOrArrayType == typeof(Array))
        {
            intCollection = typeof(int).MakeArrayType();
            objectCollection = typeof(object).MakeArrayType();
            arrayItemObjectCollection = typeof(ArrayItemObject).MakeArrayType();
        }
        else
        {
            Assert.NotNull(openGenericCollectionOrArrayType.GetInterface("IEnumerable`1"));
            Assert.True(openGenericCollectionOrArrayType.IsGenericTypeDefinition);

            intCollection = openGenericCollectionOrArrayType.MakeGenericType(typeof(int));
            objectCollection = openGenericCollectionOrArrayType.MakeGenericType(typeof(object));
            arrayItemObjectCollection = openGenericCollectionOrArrayType.MakeGenericType(typeof(ArrayItemObject));
        }

        yield return TestSample.Create(intCollection, "[]", ValidationResult.ValidResult);
        yield return TestSample.Create(intCollection, "[1, 2]", ValidationResult.ValidResult);
        yield return TestSample.Create(intCollection, "null", ValidationResult.ValidResult);
        yield return TestSample.Create(objectCollection, """
[
  {
    "Prop": 1.5
  }
]
""", ValidationResult.ValidResult);

        yield return TestSample.Create(arrayItemObjectCollection, """
[
  {
    "Prop": 1.5
  }
]
""", ValidationResult.ValidResult);

        yield return TestSample.Create(intCollection, "null", ValidationResult.ValidResult);
        
        yield return TestSample.Create(intCollection, "1", new ValidationResult(ResultCode.InvalidTokenKind, "type", GetInvalidTokenErrorMessage(InstanceType.Number.ToString(), InstanceType.Array, InstanceType.Null),
            ImmutableJsonPointer.Empty, 
            ImmutableJsonPointer.Create("/type"), 
            GetSchemaResourceBaseUri(intCollection),
            GetSchemaResourceBaseUri(intCollection)));

        yield return TestSample.Create(arrayItemObjectCollection, """
[
  {
    "Prop": "abc"
  }
]
""", new ValidationResult(ResultCode.InvalidTokenKind, "type", 
            GetInvalidTokenErrorMessage(InstanceType.String.ToString(), InstanceType.Number),
            ImmutableJsonPointer.Create("/0/Prop")!,
            ImmutableJsonPointer.Create("/items/$ref/properties/Prop/type"),
            GetSchemaResourceBaseUri(arrayItemObjectCollection),
            GetSubSchemaRefFullUriForDefs(arrayItemObjectCollection, typeof(ArrayItemObject))));
    }

    private class ArrayItemObject
    {
        public float Prop { get; set; }
    }

    private static IEnumerable<TestSample> CreateSamplesForCustomClass()
    {
        yield return TestSample.Create<CustomClass>("""
        {
            "PropName": {
                "InnerProp": "abc"
            }
        }
        """, ValidationResult.ValidResult);

        yield return TestSample.Create<CustomClass>("null", ValidationResult.ValidResult);
        
        yield return TestSample.Create<CustomClass>("""
            {
              "PropName": null
            }
            """, ValidationResult.ValidResult);

        yield return TestSample.Create<CustomClass>("""
{
  "PropName": "invalid"
}
""", new ValidationResult(ResultCode.InvalidTokenKind, "type", GetInvalidTokenErrorMessage(InstanceType.String.ToString(), InstanceType.Object, InstanceType.Null),
            ImmutableJsonPointer.Create("/PropName")!,
            ImmutableJsonPointer.Create("/properties/PropName/$ref/type"),
            GetSchemaResourceBaseUri<CustomClass>(),
            GetSubSchemaRefFullUriForDefs<CustomClass, InnerCustomClass>()));

        yield return TestSample.Create<CustomClass>("""
            {
              "NewFieldName": 2
            }
            """, ValidationResult.ValidResult);

        yield return TestSample.Create<CustomClass>("""
            {
              "NewFieldName": 1
            }
            """, new ValidationResult(ResultCode.NumberOutOfRange, "minimum", MinimumKeyword.ErrorMessage(1, 2),
            ImmutableJsonPointer.Create("/NewFieldName")!,
            ImmutableJsonPointer.Create("/properties/NewFieldName/allOf/0/minimum"),
            GetSchemaResourceBaseUri<CustomClass>(),
            GetSchemaResourceBaseUri<CustomClass>()));

        yield return TestSample.Create<CustomClass>("""
        {
            "PropName": {
                "InnerProp": "abc",
                "InnerProp2": {
                  "InnerInnerProp": 1
                }
            }
        }
        """, ValidationResult.ValidResult);

        yield return TestSample.Create<CustomClass>("""
        {
            "PropName": {
                "InnerProp": "abc",
                "InnerProp2": {
                  "InnerInnerProp": "aaa"
                }
            }
        }
        """, new ValidationResult(ResultCode.InvalidTokenKind, "type", GetInvalidTokenErrorMessage(InstanceType.String.ToString(), InstanceType.Integer),
            ImmutableJsonPointer.Create("/PropName/InnerProp2/InnerInnerProp")!, 
            ImmutableJsonPointer.Create("/properties/PropName/$ref/properties/InnerProp2/$ref/properties/InnerInnerProp/type"),
            GetSchemaResourceBaseUri<CustomClass>(),
            GetSubSchemaRefFullUriForDefs<CustomClass, InnerCustomClass2>()
        ));
    }

    private class CustomClass
    {
        public InnerCustomClass? PropName { get; set; }

        [JsonPropertyName("NewFieldName")]
        [Minimum(2)]
        public int FieldName;
    }

    private class InnerCustomClass
    {
        public string? InnerProp { get; set; }
        public InnerCustomClass2? InnerProp2 { get; set; }
    }

    private class InnerCustomClass2
    {
        public int InnerInnerProp { get; set; }
    }

    private static IEnumerable<TestSample> CreateSamplesForCustomStruct()
    {
        yield return TestSample.Create<CustomStruct>("""
        {
            "PropName": {
                "InnerProp": "abc"
            }
        }
        """, ValidationResult.ValidResult);

        yield return TestSample.Create<CustomStruct>("""
        {
            "PropName": {
                "InnerProp": 1
            }
        }
        """, new ValidationResult(ResultCode.InvalidTokenKind, "type", GetInvalidTokenErrorMessage(InstanceType.Number.ToString(), InstanceType.String, InstanceType.Null),
            ImmutableJsonPointer.Create("/PropName/InnerProp")!,
            ImmutableJsonPointer.Create("/properties/PropName/$ref/properties/InnerProp/type"),
            GetSchemaResourceBaseUri<CustomStruct>(),
            GetSubSchemaRefFullUriForDefs<CustomStruct, InnerCustomStruct>()
            ));

        yield return TestSample.Create<CustomStruct>("""
            {
              "PropName": null
            }
            """, new ValidationResult(ResultCode.InvalidTokenKind, "type", GetInvalidTokenErrorMessage(InstanceType.Null.ToString(), InstanceType.Object),
            ImmutableJsonPointer.Create("/PropName")!,
            ImmutableJsonPointer.Create("/properties/PropName/$ref/type"),
            GetSchemaResourceBaseUri<CustomStruct>(),
            GetSubSchemaRefFullUriForDefs<CustomStruct, InnerCustomStruct>()
            ));

        yield return TestSample.Create<CustomStruct>("""
        {
          "PropName": "invalid"
        }
        """, new ValidationResult(ResultCode.InvalidTokenKind, "type", GetInvalidTokenErrorMessage(InstanceType.String.ToString(), InstanceType.Object),
            ImmutableJsonPointer.Create("/PropName")!,
            ImmutableJsonPointer.Create("/properties/PropName/$ref/type"),
            GetSchemaResourceBaseUri<CustomStruct>(),
            GetSubSchemaRefFullUriForDefs<CustomStruct, InnerCustomStruct>()));
    }

    private struct CustomStruct
    {
        public InnerCustomStruct PropName { get; set; }
    }

    private struct InnerCustomStruct
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
              "Prop": null
            }
            """, ValidationResult.ValidResult);

        yield return TestSample.Create<StringDictionaryTestClass>("""
            {
              "Prop": {
                "P1": 123
              }
            }
            """, new ValidationResult(ResultCode.InvalidTokenKind, "type", 
            GetInvalidTokenErrorMessage(InstanceType.Number.ToString(), InstanceType.Object, InstanceType.Null),
            ImmutableJsonPointer.Create("/Prop/P1")!,
            ImmutableJsonPointer.Create("/properties/Prop/additionalProperties/$ref/type"),
            GetSchemaResourceBaseUri<StringDictionaryTestClass>(),
            GetSubSchemaRefFullUriForDefs<StringDictionaryTestClass, InnerCustomClass>()
        ));
    }

    class StringDictionaryTestClass
    {
        public Dictionary<string, InnerCustomClass> Prop { get; set; } = null!;
    }

    private static IEnumerable<TestSample> CreateSamplesForEnum()
    {
        yield return TestSample.Create<TestEnum>("\"A\"", ValidationResult.ValidResult);
        yield return TestSample.Create<TestEnum>("\"B\"", ValidationResult.ValidResult);
        yield return TestSample.Create<TestEnum>("\"C\"", ValidationResult.ValidResult);
        yield return TestSample.Create<TestEnum>("0", ValidationResult.ValidResult);
        yield return TestSample.Create<TestEnum>("1", ValidationResult.ValidResult);
        yield return TestSample.Create<TestEnum>("2", ValidationResult.ValidResult);

        yield return TestSample.Create<TestEnum>("\"D\"", new ValidationResult(ResultCode.NotFoundInAllowedList, "enum", EnumKeyword.ErrorMessage("D"),
            ImmutableJsonPointer.Empty, 
            ImmutableJsonPointer.Create("/enum"),
            GetSchemaResourceBaseUri<TestEnum>(),
            GetSchemaResourceBaseUri<TestEnum>()));

        yield return TestSample.Create<TestEnum>("3", new ValidationResult(ResultCode.NotFoundInAllowedList, "enum",
            EnumKeyword.ErrorMessage("3"),
            ImmutableJsonPointer.Empty, 
            ImmutableJsonPointer.Create("/enum"),
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

    private static IEnumerable<TestSample> CreateSamplesForEnumWithJsonStringEnumConverter()
    {
        yield return TestSample.Create<TestEnumWithJsonStringEnumConverter>("\"A\"", ValidationResult.ValidResult);
        yield return TestSample.Create<TestEnumWithJsonStringEnumConverter>("\"B\"", ValidationResult.ValidResult);
        yield return TestSample.Create<TestEnumWithJsonStringEnumConverter>("\"C\"", ValidationResult.ValidResult);

        yield return TestSample.Create<TestEnumWithJsonStringEnumConverter>("\"D\"", new ValidationResult(ResultCode.NotFoundInAllowedList, "enum", EnumKeyword.ErrorMessage("D"),
            ImmutableJsonPointer.Empty,
            ImmutableJsonPointer.Create("/enum"),
            GetSchemaResourceBaseUri<TestEnumWithJsonStringEnumConverter>(),
            GetSchemaResourceBaseUri<TestEnumWithJsonStringEnumConverter>()));

        yield return TestSample.Create<TestEnumWithJsonStringEnumConverter>("0", new ValidationResult(ResultCode.NotFoundInAllowedList, "enum",
            EnumKeyword.ErrorMessage("0"),
            ImmutableJsonPointer.Empty,
            ImmutableJsonPointer.Create("/enum"),
            GetSchemaResourceBaseUri<TestEnumWithJsonStringEnumConverter>(),
            GetSchemaResourceBaseUri<TestEnumWithJsonStringEnumConverter>()
        ));

        yield return TestSample.Create<TestEnumWithJsonStringEnumConverter>("3", new ValidationResult(ResultCode.NotFoundInAllowedList, "enum",
            EnumKeyword.ErrorMessage("3"),
            ImmutableJsonPointer.Empty,
            ImmutableJsonPointer.Create("/enum"),
            GetSchemaResourceBaseUri<TestEnumWithJsonStringEnumConverter>(),
            GetSchemaResourceBaseUri<TestEnumWithJsonStringEnumConverter>()
        ));
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    private enum TestEnumWithJsonStringEnumConverter
    {
        A,
        B,
        C
    }

    private static IEnumerable<TestSample> CreateSamplesForEnumPropertyWithJsonStringEnumConverter()
    {
        yield return TestSample.Create<EnumPropertyTestClass>("""
            {
              "TestEnum": "A"
            }
            """, ValidationResult.ValidResult);

        yield return TestSample.Create<EnumPropertyTestClass>("""
            {
              "TestEnum": "B"
            }
            """, ValidationResult.ValidResult);

        yield return TestSample.Create<EnumPropertyTestClass>("""
            {
              "TestEnum": "C"
            }
            """, ValidationResult.ValidResult);

        yield return TestSample.Create<EnumPropertyTestClass>("""
            {
              "TestEnum": "D"
            }
            """, new ValidationResult(ResultCode.NotFoundInAllowedList, "enum", EnumKeyword.ErrorMessage("D"),
            ImmutableJsonPointer.Create("/TestEnum")!,
            ImmutableJsonPointer.Create("/properties/TestEnum/allOf/0/enum"),
            GetSchemaResourceBaseUri<EnumPropertyTestClass>(),
            GetSchemaResourceBaseUri<EnumPropertyTestClass>()));

        yield return TestSample.Create<EnumPropertyTestClass>("""
            {
              "TestEnum": "0"
            }
            """, new ValidationResult(ResultCode.NotFoundInAllowedList, "enum", EnumKeyword.ErrorMessage("0"),
            ImmutableJsonPointer.Create("/TestEnum")!,
            ImmutableJsonPointer.Create("/properties/TestEnum/allOf/0/enum"),
            GetSchemaResourceBaseUri<EnumPropertyTestClass>(),
            GetSchemaResourceBaseUri<EnumPropertyTestClass>()));

        yield return TestSample.Create<EnumPropertyTestClass>("""
            {
              "TestEnum": "3"
            }
            """, new ValidationResult(ResultCode.NotFoundInAllowedList, "enum", EnumKeyword.ErrorMessage("3"),
            ImmutableJsonPointer.Create("/TestEnum")!,
            ImmutableJsonPointer.Create("/properties/TestEnum/allOf/0/enum"),
            GetSchemaResourceBaseUri<EnumPropertyTestClass>(),
            GetSchemaResourceBaseUri<EnumPropertyTestClass>()));
    }

    private class EnumPropertyTestClass
    {
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public TestEnum TestEnum { get; set; }
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
        yield return TestSample.Create<Uri>("null", ValidationResult.ValidResult);
    }

    private static Uri GetSubSchemaRefFullUriForDefs<TDocument, TSubSchema>()
    {
        return GetSubSchemaRefFullUriForDefs(typeof(TDocument), typeof(TSubSchema));
    }

    private static Uri GetSubSchemaRefFullUriForDefs(Type document, Type subSchema)
    {
        return new Uri(GetSchemaResourceBaseUri(document), "#/defs/" + subSchema.FullName);
    }

    private static IEnumerable<TestSample> CreateSamplesForJsonSchemaNamingPolicy()
    {
        yield return TestSample.Create<CustomClass>("""
            {
              "PropName": 1
            }
            """, new ValidationResult(ResultCode.InvalidTokenKind, "type", 
            GetInvalidTokenErrorMessage(InstanceType.Number.ToString(), InstanceType.Object, InstanceType.Null),
            ImmutableJsonPointer.Create("/PropName")!,
            ImmutableJsonPointer.Create("/properties/PropName/$ref/type"), 
            GetSchemaResourceBaseUri<CustomClass>(),
            GetSubSchemaRefFullUriForDefs<CustomClass, InnerCustomClass>()), new JsonSchemaGeneratorOptions());

        yield return TestSample.Create<CustomClass>("""
            {
              "propName": 1
            }
            """, new ValidationResult(ResultCode.InvalidTokenKind, "type", 
            GetInvalidTokenErrorMessage(InstanceType.Number.ToString(), InstanceType.Object, InstanceType.Null),
            ImmutableJsonPointer.Create("/propName")!,
            ImmutableJsonPointer.Create("/properties/propName/$ref/type"),
            GetSchemaResourceBaseUri<CustomClass>(),
            GetSubSchemaRefFullUriForDefs<CustomClass, InnerCustomClass>()), new JsonSchemaGeneratorOptions { PropertyNamingPolicy = JsonSchemaNamingPolicy.CamelCase });

        yield return TestSample.Create<CustomClass>("""
            {
              "prop_name": 1
            }
            """, new ValidationResult(ResultCode.InvalidTokenKind, "type", 
            GetInvalidTokenErrorMessage(InstanceType.Number.ToString(), InstanceType.Object, InstanceType.Null),
            ImmutableJsonPointer.Create("/prop_name")!,
            ImmutableJsonPointer.Create("/properties/prop_name/$ref/type"),
            GetSchemaResourceBaseUri<CustomClass>(),
            GetSubSchemaRefFullUriForDefs<CustomClass, InnerCustomClass>()), new JsonSchemaGeneratorOptions { PropertyNamingPolicy = JsonSchemaNamingPolicy.SnakeCaseLower });

        yield return TestSample.Create<CustomClass>("""
            {
              "PROP_NAME": 1
            }
            """, new ValidationResult(ResultCode.InvalidTokenKind, "type", 
            GetInvalidTokenErrorMessage(InstanceType.Number.ToString(), InstanceType.Object, InstanceType.Null),
            ImmutableJsonPointer.Create("/PROP_NAME")!,
            ImmutableJsonPointer.Create("/properties/PROP_NAME/$ref/type"),
            GetSchemaResourceBaseUri<CustomClass>(),
            GetSubSchemaRefFullUriForDefs<CustomClass, InnerCustomClass>()), new JsonSchemaGeneratorOptions { PropertyNamingPolicy = JsonSchemaNamingPolicy.SnakeCaseUpper });

        yield return TestSample.Create<CustomClass>("""
            {
              "prop-name": 1
            }
            """, new ValidationResult(ResultCode.InvalidTokenKind, "type", 
            GetInvalidTokenErrorMessage(InstanceType.Number.ToString(), InstanceType.Object, InstanceType.Null),
            ImmutableJsonPointer.Create("/prop-name")!,
            ImmutableJsonPointer.Create("/properties/prop-name/$ref/type"),
            GetSchemaResourceBaseUri<CustomClass>(),
            GetSubSchemaRefFullUriForDefs<CustomClass, InnerCustomClass>()), new JsonSchemaGeneratorOptions { PropertyNamingPolicy = JsonSchemaNamingPolicy.KebabCaseLower });

        yield return TestSample.Create<CustomClass>("""
            {
              "PROP-NAME": 1
            }
            """, new ValidationResult(ResultCode.InvalidTokenKind, "type", 
            GetInvalidTokenErrorMessage(InstanceType.Number.ToString(), InstanceType.Object, InstanceType.Null),
            ImmutableJsonPointer.Create("/PROP-NAME")!,
            ImmutableJsonPointer.Create("/properties/PROP-NAME/$ref/type"),
            GetSchemaResourceBaseUri<CustomClass>(),
            GetSubSchemaRefFullUriForDefs<CustomClass, InnerCustomClass>()), new JsonSchemaGeneratorOptions { PropertyNamingPolicy = JsonSchemaNamingPolicy.KebabCaseUpper });
    }

    private static IEnumerable<TestSample> CreateSamplesForJsonIgnoreAttribute()
    {
        yield return TestSample.Create<JsonIgnoreAttributeTestClass>("""
            {
              "Prop": null
            }
            """, ValidationResult.ValidResult);
    }

    private class JsonIgnoreAttributeTestClass
    {
        [JsonIgnore]
        public int Prop { get; set; }
    }

    private static IEnumerable<TestSample> CreateSamplesForRequiredAttribute()
    {
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
            """, new ValidationResult(ResultCode.DuplicatedArrayItems, "uniqueItems", UniqueItemsKeyword.ErrorMessage("1", 0, 2),
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

    private static IEnumerable<TestSample> CreateSamplesForNotNullAttribute()
    {
        yield return TestSample.Create<CustomObjectWithNotNullProperty>("""
            {
              "StringProp": "abc",
              "ObjectProp": {
                "InnerProp": "cba"
              }
            }
            """, ValidationResult.ValidResult);

        yield return TestSample.Create<CustomObjectWithNotNullProperty>("""
            {
              "StringProp": null,
              "ObjectProp": {
                "InnerProp": "cba"
              }
            }
            """, new ValidationResult(ResultCode.InvalidTokenKind, "type", 
            GetInvalidTokenErrorMessage(InstanceType.Null.ToString(), InstanceType.Object, InstanceType.String, InstanceType.Number, InstanceType.Boolean, InstanceType.Array),
            ImmutableJsonPointer.Create("/StringProp")!,
            ImmutableJsonPointer.Create("/properties/StringProp/allOf/1/type"),
            GetSchemaResourceBaseUri<CustomObjectWithNotNullProperty>(),
            GetSchemaResourceBaseUri<CustomObjectWithNotNullProperty>()));

        yield return TestSample.Create<CustomObjectWithNotNullProperty>("""
            {
              "StringProp": "abc",
              "ObjectProp": null
            }
            """, new ValidationResult(ResultCode.InvalidTokenKind, "type",
            GetInvalidTokenErrorMessage(InstanceType.Null.ToString(), InstanceType.Object, InstanceType.String, InstanceType.Number, InstanceType.Boolean, InstanceType.Array),
            ImmutableJsonPointer.Create("/ObjectProp")!,
            ImmutableJsonPointer.Create("/properties/ObjectProp/allOf/1/type"),
            GetSchemaResourceBaseUri<CustomObjectWithNotNullProperty>(),
            GetSchemaResourceBaseUri<CustomObjectWithNotNullProperty>()));
    }

    private class CustomObjectWithNotNullProperty
    {
        [NotNull]
        public string? StringProp { get; set; }

        [NotNull]
        public InnerCustomClass? ObjectProp { get; set; }
    }

    private static IEnumerable<TestSample> CreateSamplesForNullabilityAnnotation()
    {
        // successful cases
        var options = new JsonSchemaGeneratorOptions{ NullabilityTypeInfo = { ReferenceTypeNullabilityPolicy = NullabilityPolicy.BasedOnNullabilityAnnotation }};

        yield return TestSample.Create<CustomObjectForNullabilityAnnotationTest>("""
            {
              "IntegerProp1": null,
              "IntegerProp2": 1,
              "StringProp1": null,
              "StringProp2": "abc",
              "ObjectProp1": null,
              "ObjectProp2": {
                "IntegerProp1": null,
                "IntegerProp2": 1,
                "StringProp1": null,
                "StringProp2": "abc",
                "GenericProp1": null,
                "GenericProp2": [
                  {
                    "IntegerProp1": null,
                    "IntegerProp2": 1,
                    "StringProp1": null,
                    "StringProp2": "abc"
                  }
                ]
              }
            }
            """, ValidationResult.ValidResult, options);
        
        yield return TestSample.Create<CustomObjectForNullabilityAnnotationTest>("""
            {
              "IntegerProp1": 1,
              "IntegerProp2": 1,
              "StringProp1": "abc",
              "StringProp2": "abc",
              "ObjectProp1": {},
              "ObjectProp2": {
                "IntegerProp1": 1,
                "IntegerProp2": 1,
                "StringProp1": "abc",
                "StringProp2": "abc",
                "GenericProp1": [],
                "GenericProp2": [
                  {
                    "IntegerProp1": 1,
                    "IntegerProp2": 1,
                    "StringProp1": "abc",
                    "StringProp2": "abc"
                  }
                ]
              }
            }
            """, ValidationResult.ValidResult, options);
        
        yield return TestSample.Create<CustomObjectForNullabilityAnnotationTest>("""
            {
              "IntegerProp1": 1,
              "IntegerProp2": 1,
              "StringProp1": "abc",
              "StringProp2": "abc",
              "ObjectProp1": {
                "IntegerProp1": 1,
                "IntegerProp2": 1,
                "StringProp1": "abc",
                "StringProp2": "abc",
                "GenericProp1": null,
                "GenericProp2": null
              },
              "ObjectProp2": {}
            }
            """, ValidationResult.ValidResult, options);
        
        // failed cases
        yield return TestSample.Create<CustomObjectForNullabilityAnnotationTest>("""
            {
              "IntegerProp2": null
            }
            """, new ValidationResult(
            ResultCode.InvalidTokenKind,
            "type",
            GetInvalidTokenErrorMessage(InstanceType.Null.ToString(), InstanceType.Integer),
            ImmutableJsonPointer.Create("/IntegerProp2")!,
            ImmutableJsonPointer.Create("/properties/IntegerProp2/type")!,
            GetSchemaResourceBaseUri<CustomObjectForNullabilityAnnotationTest>(),
            GetSchemaResourceBaseUri<CustomObjectForNullabilityAnnotationTest>()), options);
        
        yield return TestSample.Create<CustomObjectForNullabilityAnnotationTest>("""
            {
              "StringProp2": null
            }
            """, new ValidationResult(
            ResultCode.InvalidTokenKind,
            "type",
            GetInvalidTokenErrorMessage(InstanceType.Null.ToString(), InstanceType.Object, InstanceType.String, InstanceType.Number, InstanceType.Boolean, InstanceType.Array),
            ImmutableJsonPointer.Create("/StringProp2")!,
            ImmutableJsonPointer.Create("/properties/StringProp2/allOf/1/type")!,
            GetSchemaResourceBaseUri<CustomObjectForNullabilityAnnotationTest>(),
            GetSchemaResourceBaseUri<CustomObjectForNullabilityAnnotationTest>()), options);
        
        yield return TestSample.Create<CustomObjectForNullabilityAnnotationTest>("""
            {
              "ObjectProp1": {
                "IntegerProp2": null
              }
            }
            """, new ValidationResult(
            ResultCode.InvalidTokenKind,
            "type",
            GetInvalidTokenErrorMessage(InstanceType.Null.ToString(), InstanceType.Integer),
            ImmutableJsonPointer.Create("/ObjectProp1/IntegerProp2")!,
            ImmutableJsonPointer.Create("/properties/ObjectProp1/properties/IntegerProp2/type")!,
            GetSchemaResourceBaseUri<CustomObjectForNullabilityAnnotationTest>(),
            GetSchemaResourceBaseUri<CustomObjectForNullabilityAnnotationTest>()), options);
        
        yield return TestSample.Create<CustomObjectForNullabilityAnnotationTest>("""
            {
              "ObjectProp1": {
                "StringProp2": null
              }
            }
            """, new ValidationResult(
            ResultCode.InvalidTokenKind,
            "type",
            GetInvalidTokenErrorMessage(InstanceType.Null.ToString(), InstanceType.Object, InstanceType.String, InstanceType.Number, InstanceType.Boolean, InstanceType.Array),
            ImmutableJsonPointer.Create("/ObjectProp1/StringProp2")!,
            ImmutableJsonPointer.Create("/properties/ObjectProp1/properties/StringProp2/allOf/1/type")!,
            GetSchemaResourceBaseUri<CustomObjectForNullabilityAnnotationTest>(),
            GetSchemaResourceBaseUri<CustomObjectForNullabilityAnnotationTest>()), options);
        
        yield return TestSample.Create<CustomObjectForNullabilityAnnotationTest>("""
            {
              "ObjectProp1": {
                "GenericProp1": [
                  { "StringProp2": null }
                ]
              }
            }
            """, new ValidationResult(
            ResultCode.InvalidTokenKind,
            "type",
            GetInvalidTokenErrorMessage(InstanceType.Null.ToString(), InstanceType.Object, InstanceType.String, InstanceType.Number, InstanceType.Boolean, InstanceType.Array),
            ImmutableJsonPointer.Create("/ObjectProp1/GenericProp1/0/StringProp2")!,
            ImmutableJsonPointer.Create("/properties/ObjectProp1/properties/GenericProp1/items/$ref/properties/StringProp2/allOf/1/type")!,
            GetSchemaResourceBaseUri<CustomObjectForNullabilityAnnotationTest>(),
            GetSubSchemaRefFullUriForDefs<CustomObjectForNullabilityAnnotationTest, CustomObjectForNullabilityAnnotationTest.GenericArgumentClass>()), options);
        
        yield return TestSample.Create<CustomObjectForNullabilityAnnotationTest>("""
            {
              "ObjectProp2": null
            }
            """, new ValidationResult(
            ResultCode.InvalidTokenKind,
            "type",
            GetInvalidTokenErrorMessage(InstanceType.Null.ToString(), InstanceType.Object, InstanceType.String, InstanceType.Number, InstanceType.Boolean, InstanceType.Array),
            ImmutableJsonPointer.Create("/ObjectProp2")!,
            ImmutableJsonPointer.Create("/properties/ObjectProp2/allOf/1/type")!,
            GetSchemaResourceBaseUri<CustomObjectForNullabilityAnnotationTest>(),
            GetSchemaResourceBaseUri<CustomObjectForNullabilityAnnotationTest>()), options);

        yield return TestSample.Create<CustomObjectForNullabilityAnnotationTest>("""
            {
              "ObjectProp2": {
                "GenericProp2": null
              }
            }
            """, new ValidationResult(
            ResultCode.InvalidTokenKind,
            "type",
            GetInvalidTokenErrorMessage(InstanceType.Null.ToString(), InstanceType.Object, InstanceType.String, InstanceType.Number, InstanceType.Boolean, InstanceType.Array),
            ImmutableJsonPointer.Create("/ObjectProp2/GenericProp2")!,
            ImmutableJsonPointer.Create("/properties/ObjectProp2/allOf/0/properties/GenericProp2/allOf/1/type")!,
            GetSchemaResourceBaseUri<CustomObjectForNullabilityAnnotationTest>(),
            GetSchemaResourceBaseUri<CustomObjectForNullabilityAnnotationTest>()), options);
    }

    private class CustomObjectForNullabilityAnnotationTest
    {
        public int? IntegerProp1 { get; set; }
        public int IntegerProp2 { get; set; }

        public string? StringProp1 { get; set; }
        public string StringProp2 { get; set; } = null!;

        public InnerClass<List<GenericArgumentClass>?>? ObjectProp1 { get; set; }
        public InnerClass<List<GenericArgumentClass>> ObjectProp2 { get; set; } = null!;

        public class InnerClass<T>
        {
            public int? IntegerProp1 { get; set; }
            public int IntegerProp2 { get; set; }

            public string? StringProp1 { get; set; }
            public string StringProp2 { get; set; } = null!;

            public T? GenericProp1 { get; set; }
            public T GenericProp2 { get; set; } = default!;
        }

        public class GenericArgumentClass
        {
            public int? IntegerProp1 { get; set; }
            public int IntegerProp2 { get; set; }

            public string? StringProp1 { get; set; }
            public string StringProp2 { get; set; } = null!;
        }
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
            """, new ValidationResult(ResultCode.InvalidTokenKind, "type", 
            GetInvalidTokenErrorMessage(InstanceType.Null.ToString(), InstanceType.Integer),
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
        => GetSchemaResourceBaseUri(typeof(T));

    private static Uri GetSchemaResourceBaseUri(Type type)
        => new(BodyJsonSchemaDocument.DefaultDocumentBaseUri, "[Main]-" + type.FullName);

    private static string GetInvalidTokenErrorMessage(string actualType, params InstanceType[] expectedTypes) 
        => $"Expect type(s): '{string.Join('|', expectedTypes)}' but actual is '{actualType}'";

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
            ImmutableJsonPointer.Create("/Prop")!, ImmutableJsonPointer.Create("/properties/Prop/allOf/0/maximum"),
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
            ImmutableJsonPointer.Create("/Prop")!, ImmutableJsonPointer.Create("/properties/Prop/allOf/0/minimum"),
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
        [Generator.MinLength(1)]
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
        [Generator.MaxLength(3)]
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
            new ValidationResult(ResultCode.NotFoundInAllowedList, "enum", EnumKeyword.ErrorMessage("1"),
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
            new ValidationResult(ResultCode.NotFoundInAllowedList, "enum", EnumKeyword.ErrorMessage("c"),
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
                GetSchemaResourceBaseUri<EmailAttributeTestClass>(),
                GetSchemaResourceBaseUri<EmailAttributeTestClass>()));
    }

    private class EmailAttributeTestClass
    {
        [Email]
        public string? Email;
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
""", new ValidationResult(ResultCode.FailedToMultiple, "multipleOf", DoubleMultipleOfChecker.ErrorMessage(4.3, 1.5),
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
""", new ValidationResult(ResultCode.NotFoundInAllowedList, "enum", 
            EnumKeyword.ErrorMessage("3"),
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

        public static TestSample Create(Type type, string jsonInstance, ValidationResult expectedValidationResult, JsonSchemaGeneratorOptions? options = null)
        {
            return new TestSample(type, jsonInstance, expectedValidationResult, options);
        }
    }
}




