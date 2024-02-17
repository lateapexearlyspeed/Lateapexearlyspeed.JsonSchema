using LateApexEarlySpeed.Json.Schema.JSchema;
using LateApexEarlySpeed.Json.Schema.Keywords;

namespace LateApexEarlySpeed.Json.Schema.FluentGenerator;

public class JsonSchemaBuilder
{
    private KeywordBuilder? _keywordBuilder;

    // public static void Test()
    // {
    //     JsonSchemaBuilder builder = new JsonSchemaBuilder();
    //
    //     builder.IsJsonString().Equal("abc").IsIn(new string[] { "a" }).HasMaxLength(10).HasMinLength(1).HasPattern("*a*").HasCustomValidation(s => s.StartsWith('1'), s => s);
    //     builder.IsJsonNumber().Equal(1).IsIn(new double[] { 1, 2, 3 }).IsGreaterThan(1).IsLessThan(10).NotGreaterThan(11).NotLessThan(0).MultipleOf(3)
    //         .HasCustomValidation(d => Math.Abs(d - 1.5) < 0.001, d => "");
    //     builder.IsJsonObject().Equivalent("{}").SerializationEquivalent(new{A = "a", B = "b"}).HasNoProperty("aaa").HasProperty("abc").HasProperty("a", b => b.IsJsonString()).HasProperty("b", b => b.IsJsonNumber())
    //         .HasCustomValidation<TestClass>(tc => true, tc => "").HasCustomValidation(typeof(TestClass), tc => true, tc => "").HasCustomValidation(element => true, element => "");
    //     builder.IsJsonArray().Equivalent("[]").NotContains(b => b.IsJsonString()).Contains(b => b.IsJsonString()).SerializationEquivalent(new object[]{}).HasItems(b => b.IsJsonString()).HasLength(8).HasMaxLength(10).HasMinLength(1).HasUniqueItems().HasCustomValidation<TestClass>(array => true, array => "").HasCustomValidation(element => true, element => "");
    //     builder.IsJsonNull();
    //     builder.IsNotJsonNull();
    //     builder.IsJsonBoolean();
    //     builder.IsJsonTrue();
    //     builder.IsJsonFalse();
    //     builder.IsDateTimeOffset().Equal(DateTimeOffset.UtcNow).Before(DateTimeOffset.UtcNow).After(DateTimeOffset.UtcNow).HasCustomValidation(dt => true, dt => "");
    //     builder.IsDateTime().Equal(DateTime.UtcNow).Before(DateTime.UtcNow).After(DateTime.UtcNow).HasCustomValidation(dt => true, dt => "");
    //     builder.IsGuid();
    //
    //     JsonValidator jsonValidator = builder.BuildValidator();
    // }

    public void IsGuid()
    {
        _keywordBuilder = new GuidKeywordBuilder();
    }

    public DateTimeKeywordBuilder IsDateTime(string[]? formats = null)
    {
        var dateTimeKeywordBuilder = new DateTimeKeywordBuilder(formats);
        _keywordBuilder = dateTimeKeywordBuilder;

        return dateTimeKeywordBuilder;
    }

    public DateTimeOffsetKeywordBuilder IsDateTimeOffset(string[]? formats = null)
    {
        var dateTimeOffsetKeywordBuilder = new DateTimeOffsetKeywordBuilder(formats);
        _keywordBuilder = dateTimeOffsetKeywordBuilder;

        return dateTimeOffsetKeywordBuilder;
    }

    public void IsNotJsonNull()
    {
        var keywordBuilder = new KeywordBuilder();
        keywordBuilder.Keywords.Add(new TypeKeyword(InstanceType.Object, InstanceType.Array, InstanceType.Boolean, InstanceType.Number, InstanceType.String));

        _keywordBuilder = keywordBuilder;
    }

    public void IsJsonTrue()
    {
        AssociateKeywordBuilder<TrueKeywordBuilder>();
    }

    public void IsJsonFalse()
    {
        AssociateKeywordBuilder<FalseKeywordBuilder>();
    }

    public void IsJsonBoolean()
    {
        AssociateKeywordBuilder<BooleanKeywordBuilder>();
    }

    public StringKeywordBuilder IsJsonString()
    {
        return AssociateKeywordBuilder<StringKeywordBuilder>();
    }

    public NumberKeywordBuilder IsJsonNumber()
    {
        return AssociateKeywordBuilder<NumberKeywordBuilder>();
    }

    public ArrayKeywordBuilder IsJsonArray()
    {
        return AssociateKeywordBuilder<ArrayKeywordBuilder>();
    }

    public NullKeywordBuilder IsJsonNull()
    {
        return AssociateKeywordBuilder<NullKeywordBuilder>();
    }

    public ObjectKeywordBuilder IsJsonObject()
    {
        return AssociateKeywordBuilder<ObjectKeywordBuilder>();
    }

    public void Or(params Action<JsonSchemaBuilder>[] configureSchemaBuilders)
    {
        _keywordBuilder = new OrKeywordBuilder(configureSchemaBuilders);
    }

    internal BodyJsonSchema Build()
    {
        if (_keywordBuilder is not null)
        {
            List<KeywordBase> keywords = _keywordBuilder.Build();

            return new BodyJsonSchema(keywords);
        }

        return new BodyJsonSchema(new List<KeywordBase>(0));
    }

    public JsonValidator BuildValidator()
    {
        BodyJsonSchema bodyJsonSchema = Build();

        BodyJsonSchemaDocument schemaDocument = bodyJsonSchema.TransformToSchemaDocument(BodyJsonSchemaDocument.DefaultDocumentBaseUri);

        return new JsonValidator(schemaDocument);
    }

    private TKeywordBuilder AssociateKeywordBuilder<TKeywordBuilder>() where TKeywordBuilder : KeywordBuilder, new()
    {
        var keywordBuilder = new TKeywordBuilder();
        _keywordBuilder = keywordBuilder;

        return keywordBuilder;
    }
}