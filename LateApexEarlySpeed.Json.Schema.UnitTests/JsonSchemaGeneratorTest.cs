using LateApexEarlySpeed.Json.Schema.Common;
using LateApexEarlySpeed.Json.Schema.Generator;
using LateApexEarlySpeed.Json.Schema.JSchema;
using LateApexEarlySpeed.Json.Schema.Keywords;
using Xunit;

namespace LateApexEarlySpeed.Json.Schema.UnitTests;

public class JsonSchemaGeneratorTest
{
    [Theory]
    [MemberData(nameof(TestData))]
    public void GenerateJsonValidator_Validate(Type type, string jsonInstance, ValidationResult expectedValidationResult)
    {
        JsonValidator jsonValidator = JsonSchemaGenerator.GenerateJsonValidator(type);
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

    public static IEnumerable<object[]> TestData
    {
        get
        {
            IEnumerable<TestSample> testSamples = CreateTestSamples();

            return testSamples.Select(sample => new object[] { sample.Type, sample.JsonInstance, sample.ExpectedValidationResult });
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

        // boolean
        samples = samples.Concat(CreateSamplesForBoolean());

        // string
        samples = samples.Concat(CreateSamplesForString());

        return samples;
    }

    private static IEnumerable<TestSample> CreateSamplesForString()
    {
        yield return TestSample.Create<string>("\"abc\"", ValidationResult.ValidResult);
        yield return TestSample.Create<string>("null", new ValidationResult(
            ResultCode.InvalidTokenKind,
            "type",
            $"Expect type '{InstanceType.String}' but actual is '{InstanceType.Null}'",
            ImmutableJsonPointer.Empty,
            ImmutableJsonPointer.Create("/type")!,
            new Uri(BodyJsonSchemaDocument.DefaultDocumentBaseUri, typeof(string).FullName),
            new Uri(BodyJsonSchemaDocument.DefaultDocumentBaseUri, typeof(string).FullName)
        ));

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
                "Invalid string value for format:'email'", 
                ImmutableJsonPointer.Create("/Email")!, 
                ImmutableJsonPointer.Create("/properties/Email/format")!,
                new Uri(BodyJsonSchemaDocument.DefaultDocumentBaseUri, typeof(EmailAttributeTestClass).FullName),
                new Uri(BodyJsonSchemaDocument.DefaultDocumentBaseUri, typeof(EmailAttributeTestClass).FullName)));

        yield return TestSample.Create<StringEnumAttributeTestClass>(
            """
{
  "Prop": "b"
}
""",
            ValidationResult.ValidResult
            );
        yield return TestSample.Create<StringEnumAttributeTestClass>(
            """
{
  "Prop": 1
}
""",
            new ValidationResult(ResultCode.NotFoundInAllowedList, "enum", "Not found in allowed list", 
                ImmutableJsonPointer.Create("/Prop")!,
                ImmutableJsonPointer.Create("/properties/Prop/enum"),
                GetMainDocBaseUri<StringEnumAttributeTestClass>(),
                GetMainDocBaseUri<StringEnumAttributeTestClass>()
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
                GetMainDocBaseUri<StringEnumAttributeTestClass>(),
                GetMainDocBaseUri<StringEnumAttributeTestClass>())
            );

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
                "Invalid string value for format:'ipv4'",
                ImmutableJsonPointer.Create("/Prop")!,
                ImmutableJsonPointer.Create("/properties/Prop/format")!,
                GetMainDocBaseUri<IPv4AttributeTestClass>(),
                GetMainDocBaseUri<IPv4AttributeTestClass>()));

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
                "Invalid string value for format:'ipv6'",
                ImmutableJsonPointer.Create("/Prop")!,
                ImmutableJsonPointer.Create("/properties/Prop/format")!,
                GetMainDocBaseUri<IPv6AttributeTestClass>(),
                GetMainDocBaseUri<IPv6AttributeTestClass>()));
    }

    private class IPv4AttributeTestClass
    {
        [IPv4]
        public string Prop { get; set; }
    }

    private class IPv6AttributeTestClass
    {
        [IPv6]
        public string Prop { get; set; }
    }

    private static Uri GetMainDocBaseUri<T>() 
        => new(BodyJsonSchemaDocument.DefaultDocumentBaseUri, typeof(T).FullName);

    private static string GetInvalidTokenErrorMessage(InstanceType expected, InstanceType actual) 
        => $"Expect type '{expected}' but actual is '{actual}'";

    internal class EmailAttributeTestClass
    {
        [Email]
        public string? Email { get; set; }
    }

    internal class StringEnumAttributeTestClass
    {
        [StringEnum("a", "b")]
        public string? Prop { get; set; }
    }

    private static IEnumerable<TestSample> CreateSamplesForBoolean()
    {
        yield return TestSample.Create<bool>("true", ValidationResult.ValidResult);
        yield return TestSample.Create<bool>("false", ValidationResult.ValidResult);
        yield return TestSample.Create<bool>("\"true\"", new ValidationResult(
            ResultCode.InvalidTokenKind,
            "type",
            $"Expect type '{InstanceType.Boolean}' but actual is '{InstanceType.String}'",
            ImmutableJsonPointer.Empty,
            ImmutableJsonPointer.Create("/type")!,
            new Uri(BodyJsonSchemaDocument.DefaultDocumentBaseUri, typeof(bool).FullName),
            new Uri(BodyJsonSchemaDocument.DefaultDocumentBaseUri, typeof(bool).FullName)
            ));
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
            $"Expect type '{InstanceType.Integer}' but actual is '{InstanceType.String}'",
            ImmutableJsonPointer.Empty,
            ImmutableJsonPointer.Create("/type")!,
            new Uri(BodyJsonSchemaDocument.DefaultDocumentBaseUri, typeof(T).FullName),
            new Uri(BodyJsonSchemaDocument.DefaultDocumentBaseUri, typeof(T).FullName)
        ));
        yield return TestSample.Create<T>("1.5", new ValidationResult(
            ResultCode.NotBeInteger,
            "type",
            $"Expect type '{InstanceType.Integer}' but actual is float number",
            ImmutableJsonPointer.Empty,
            ImmutableJsonPointer.Create("/type")!,
            new Uri(BodyJsonSchemaDocument.DefaultDocumentBaseUri, typeof(T).FullName),
            new Uri(BodyJsonSchemaDocument.DefaultDocumentBaseUri, typeof(T).FullName)
        ));
    }

    private static IEnumerable<TestSample> CreateSamplesForSignedInteger<T>() where T : unmanaged
    {
        yield return TestSample.Create<T>("0", ValidationResult.ValidResult);
        yield return TestSample.Create<T>("1", ValidationResult.ValidResult);
        yield return TestSample.Create<T>("-1", ValidationResult.ValidResult);
        yield return TestSample.Create<T>("\"abc\"", new ValidationResult(
            ResultCode.InvalidTokenKind,
            "type",
            $"Expect type '{InstanceType.Integer}' but actual is '{InstanceType.String}'",
            ImmutableJsonPointer.Empty,
            ImmutableJsonPointer.Create("/type")!,
            new Uri(BodyJsonSchemaDocument.DefaultDocumentBaseUri, typeof(T).FullName),
            new Uri(BodyJsonSchemaDocument.DefaultDocumentBaseUri, typeof(T).FullName)
        ));
        yield return TestSample.Create<T>("1.5", new ValidationResult(
            ResultCode.NotBeInteger,
            "type",
            $"Expect type '{InstanceType.Integer}' but actual is float number",
            ImmutableJsonPointer.Empty,
            ImmutableJsonPointer.Create("/type")!,
            new Uri(BodyJsonSchemaDocument.DefaultDocumentBaseUri, typeof(T).FullName),
            new Uri(BodyJsonSchemaDocument.DefaultDocumentBaseUri, typeof(T).FullName)
        ));
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
            $"Expect type '{InstanceType.Number}' but actual is '{InstanceType.String}'",
            ImmutableJsonPointer.Empty,
            ImmutableJsonPointer.Create("/type")!,
            new Uri(BodyJsonSchemaDocument.DefaultDocumentBaseUri, typeof(T).FullName),
            new Uri(BodyJsonSchemaDocument.DefaultDocumentBaseUri, typeof(T).FullName)
        ));
    }

    private class TestSample
    {
        private TestSample(Type type, string jsonInstance, ValidationResult expectedValidationResult)
        {
            Type = type;
            JsonInstance = jsonInstance;
            ExpectedValidationResult = expectedValidationResult;
        }

        public Type Type { get; }
        public string JsonInstance { get; }
        public ValidationResult ExpectedValidationResult { get; }

        public static TestSample Create<T>(string jsonInstance, ValidationResult expectedValidationResult)
        {
            return new TestSample(typeof(T), jsonInstance, expectedValidationResult);
        }
    }
}

