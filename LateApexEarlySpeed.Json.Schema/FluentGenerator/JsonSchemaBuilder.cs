using LateApexEarlySpeed.Json.Schema.JSchema;
using LateApexEarlySpeed.Json.Schema.Keywords;

namespace LateApexEarlySpeed.Json.Schema.FluentGenerator;

public class JsonSchemaBuilder
{
    private KeywordBuilder? _keywordBuilder;

    private void IsGuid()
    {
        _keywordBuilder = new GuidKeywordBuilder();
    }

    private DateTimeKeywordBuilder IsDateTime(string[]? formats = null)
    {
        var dateTimeKeywordBuilder = new DateTimeKeywordBuilder(formats);
        _keywordBuilder = dateTimeKeywordBuilder;

        return dateTimeKeywordBuilder;
    }

    private DateTimeOffsetKeywordBuilder IsDateTimeOffset(string[]? formats = null)
    {
        var dateTimeOffsetKeywordBuilder = new DateTimeOffsetKeywordBuilder(formats);
        _keywordBuilder = dateTimeOffsetKeywordBuilder;

        return dateTimeOffsetKeywordBuilder;
    }

    private static InvalidOperationException CreateExceptionOfRebindKeywordBuilder() 
        => new($"{nameof(JsonSchemaBuilder)} instance only needs to be configured once.");

    /// <summary>
    /// Specify that current json node should not be Json Null
    /// </summary>
    public void NotJsonNull()
    {
        if (_keywordBuilder is not null)
        {
            throw CreateExceptionOfRebindKeywordBuilder();
        }

        var keywordBuilder = new KeywordBuilder();
        keywordBuilder.Keywords.Add(new TypeKeyword(InstanceType.Object, InstanceType.Array, InstanceType.Boolean, InstanceType.Number, InstanceType.String));

        _keywordBuilder = keywordBuilder;
    }

    /// <summary>
    /// Specify that current json node should be Json True
    /// </summary>
    public void IsJsonTrue()
    {
        if (_keywordBuilder is not null)
        {
            throw CreateExceptionOfRebindKeywordBuilder();
        }

        AssociateKeywordBuilder<TrueKeywordBuilder>();
    }

    /// <summary>
    /// Specify that current json node should be Json False
    /// </summary>
    public void IsJsonFalse()
    {
        if (_keywordBuilder is not null)
        {
            throw CreateExceptionOfRebindKeywordBuilder();
        }

        AssociateKeywordBuilder<FalseKeywordBuilder>();
    }

    /// <summary>
    /// Specify that current json node should be Boolean type
    /// </summary>
    public void IsJsonBoolean()
    {
        if (_keywordBuilder is not null)
        {
            throw CreateExceptionOfRebindKeywordBuilder();
        }

        AssociateKeywordBuilder<BooleanKeywordBuilder>();
    }

    /// <summary>
    /// Specify that current json node should be String type
    /// </summary>
    /// <returns></returns>
    public StringKeywordBuilder IsJsonString()
    {
        if (_keywordBuilder is not null)
        {
            throw CreateExceptionOfRebindKeywordBuilder();
        }

        return AssociateKeywordBuilder<StringKeywordBuilder>();
    }

    /// <summary>
    /// Specify that current json node should be Number type
    /// </summary>
    /// <returns></returns>
    public NumberKeywordBuilder IsJsonNumber()
    {
        if (_keywordBuilder is not null)
        {
            throw CreateExceptionOfRebindKeywordBuilder();
        }

        return AssociateKeywordBuilder<NumberKeywordBuilder>();
    }

    /// <summary>
    /// Specify that current json node should be Array type
    /// </summary>
    /// <returns></returns>
    public ArrayKeywordBuilder IsJsonArray()
    {
        if (_keywordBuilder is not null)
        {
            throw CreateExceptionOfRebindKeywordBuilder();
        }

        return AssociateKeywordBuilder<ArrayKeywordBuilder>();
    }

    /// <summary>
    /// Specify that current json node should be Json Null
    /// </summary>
    /// <returns></returns>
    public NullKeywordBuilder IsJsonNull()
    {
        if (_keywordBuilder is not null)
        {
            throw CreateExceptionOfRebindKeywordBuilder();
        }

        return AssociateKeywordBuilder<NullKeywordBuilder>();
    }

    /// <summary>
    /// Specify that current json node should be Object type
    /// </summary>
    /// <returns></returns>
    public ObjectKeywordBuilder IsJsonObject()
    {
        if (_keywordBuilder is not null)
        {
            throw CreateExceptionOfRebindKeywordBuilder();
        }

        return AssociateKeywordBuilder<ObjectKeywordBuilder>();
    }

    /// <summary>
    /// Specify that current json node should match any of <paramref name="configureSchemaBuilders"/>
    /// </summary>
    /// <param name="configureSchemaBuilders">specified schema constraints</param>
    public void Or(params Action<JsonSchemaBuilder>[] configureSchemaBuilders)
    {
        if (_keywordBuilder is not null)
        {
            throw CreateExceptionOfRebindKeywordBuilder();
        }

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