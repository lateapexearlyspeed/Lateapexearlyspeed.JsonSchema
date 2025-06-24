using LateApexEarlySpeed.Json.Schema.Common.interfaces;
using LateApexEarlySpeed.Json.Schema.JInstance;
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

    /// <summary>
    /// Specify that current json node should not be Json Null
    /// </summary>
    public void NotJsonNull()
    {
        ThrowIfRebindKeywordBuilder();

        var keywordBuilder = new KeywordBuilder();
        keywordBuilder.Keywords.Add(new TypeKeyword(InstanceType.Object, InstanceType.Array, InstanceType.Boolean, InstanceType.Number, InstanceType.String));

        _keywordBuilder = keywordBuilder;
    }

    /// <summary>
    /// Specify that current json node should be Json True
    /// </summary>
    public void IsJsonTrue()
    {
        ThrowIfRebindKeywordBuilder();

        AssociateKeywordBuilder<TrueKeywordBuilder>();
    }

    /// <summary>
    /// Specify that current json node should be Json False
    /// </summary>
    public void IsJsonFalse()
    {
        ThrowIfRebindKeywordBuilder();

        AssociateKeywordBuilder<FalseKeywordBuilder>();
    }

    /// <summary>
    /// Specify that current json node should be Boolean type
    /// </summary>
    public void IsJsonBoolean()
    {
        ThrowIfRebindKeywordBuilder();

        AssociateKeywordBuilder<BooleanKeywordBuilder>();
    }

    /// <summary>
    /// Specify that current json node should be String type
    /// </summary>
    /// <returns></returns>
    public StringKeywordBuilder IsJsonString()
    {
        ThrowIfRebindKeywordBuilder();

        return AssociateKeywordBuilder<StringKeywordBuilder>();
    }

    /// <summary>
    /// Specify that current json node should be String type and the string content should equal to <paramref name="value"/>
    /// </summary>
    /// <param name="value">Normal string content</param>
    public StringKeywordBuilder StringEqual(string value)
    {
        ThrowIfRebindKeywordBuilder();

        return AssociateKeywordBuilder<StringKeywordBuilder>().Equal(value);
    }

    /// <summary>
    /// Specify that current json node should be json string and it should match specified <paramref name="pattern"/>
    /// </summary>
    /// <param name="pattern"></param>
    /// <returns></returns>
    public StringKeywordBuilder StringHasPattern(string pattern)
    {
        ThrowIfRebindKeywordBuilder();

        return AssociateKeywordBuilder<StringKeywordBuilder>().HasPattern(pattern);
    }

    /// <summary>
    /// Specify that current json node should be Number type
    /// </summary>
    /// <returns></returns>
    public NumberKeywordBuilder IsJsonNumber()
    {
        ThrowIfRebindKeywordBuilder();

        return AssociateKeywordBuilder<NumberKeywordBuilder>();
    }

    /// <summary>
    /// Specify that current json node should be Array type
    /// </summary>
    /// <returns></returns>
    public ArrayKeywordBuilder IsJsonArray()
    {
        ThrowIfRebindKeywordBuilder();

        return AssociateKeywordBuilder<ArrayKeywordBuilder>();
    }

    /// <summary>
    /// Specify that current json node should be Array type and all of its elements should match schema constraint of <paramref name="configureBuilder"/>
    /// </summary>
    /// <param name="configureBuilder">Configuration to specify schema metadata</param>
    public ArrayKeywordBuilder ArrayHasItems(Action<JsonSchemaBuilder> configureBuilder)
    {
        ThrowIfRebindKeywordBuilder();

        return AssociateKeywordBuilder<ArrayKeywordBuilder>().HasItems(configureBuilder);
    }

    /// <summary>
    /// Specify that current json node should be json array and it should contain any of element matching specified schema constraint from <paramref name="configureBuilder"/>
    /// </summary>
    /// <param name="configureBuilder">Schema metadata configuration for array element</param>
    /// <returns></returns>
    public ArrayKeywordBuilder ArrayContains(Action<JsonSchemaBuilder> configureBuilder)
    {
        ThrowIfRebindKeywordBuilder();

        return AssociateKeywordBuilder<ArrayKeywordBuilder>().Contains(configureBuilder);
    }

    /// <summary>
    /// Specify that current json node should be Json Null
    /// </summary>
    public void IsJsonNull()
    {
        ThrowIfRebindKeywordBuilder();

        AssociateKeywordBuilder<NullKeywordBuilder>();
    }

    /// <summary>
    /// Specify that current json node should be Object type
    /// </summary>
    /// <returns></returns>
    public ObjectKeywordBuilder IsJsonObject()
    {
        ThrowIfRebindKeywordBuilder();

        return AssociateKeywordBuilder<ObjectKeywordBuilder>();
    }

    /// <summary>
    /// Specify that current json node should be Object type and this json object should have specified <paramref name="property"/> and its property value should match schema of <paramref name="configureBuilder"/>
    /// </summary>
    /// <param name="property">Property name</param>
    /// <param name="configureBuilder">Schema for specified <paramref name="property"/>'s value</param>
    public ObjectKeywordBuilder ObjectHasProperty(string property, Action<JsonSchemaBuilder> configureBuilder)
    {
        ThrowIfRebindKeywordBuilder();

        ObjectKeywordBuilder objectKeywordBuilder = AssociateKeywordBuilder<ObjectKeywordBuilder>();
        return objectKeywordBuilder.HasProperty(property, configureBuilder);
    }

    /// <summary>
    /// Specify that current json node should be equivalent to <paramref name="jsonText"/>
    /// </summary>
    /// <param name="jsonText"></param>
    public void Equivalent(string jsonText)
    {
        ThrowIfRebindKeywordBuilder();

        var keywordBuilder = new KeywordBuilder();
        keywordBuilder.Keywords.Add(new ConstKeyword(JsonInstanceSerializer.Deserialize(jsonText)));
        _keywordBuilder = keywordBuilder;
    }

    /// <summary>
    /// Specify that current json node should match any of <paramref name="configureSchemaBuilders"/>
    /// </summary>
    /// <param name="configureSchemaBuilders">specified schema constraints</param>
    public void Or(params Action<JsonSchemaBuilder>[] configureSchemaBuilders)
    {
        ThrowIfRebindKeywordBuilder();

        _keywordBuilder = new OrKeywordBuilder(configureSchemaBuilders);
    }

    internal BodyJsonSchema Build()
    {
        return _keywordBuilder is not null 
            ? BuildBodyJsonSchema(_keywordBuilder) 
            : new BodyJsonSchema(Enumerable.Empty<KeywordBase>());
    }

    internal static BodyJsonSchema BuildBodyJsonSchema(KeywordBuilder keywordBuilder)
    {
        KeywordCollection keywordCollection = keywordBuilder.Build();

        return new BodyJsonSchema(keywordCollection.Keywords,
            keywordCollection.ArrayContainsValidator is null
                ? Enumerable.Empty<ISchemaContainerValidationNode>()
                : new ISchemaContainerValidationNode[] { keywordCollection.ArrayContainsValidator },
            null, null, null, null, null);
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

    private void ThrowIfRebindKeywordBuilder()
    {
        if (_keywordBuilder is not null)
        {
            throw CreateExceptionOfRebindKeywordBuilder();
        }
    }

    private static InvalidOperationException CreateExceptionOfRebindKeywordBuilder()
        => new($"{nameof(JsonSchemaBuilder)} instance only needs to be configured once.");
}