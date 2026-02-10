using LateApexEarlySpeed.Json.Schema.Common;
using LateApexEarlySpeed.Json.Schema.Common.interfaces;
using LateApexEarlySpeed.Json.Schema.JSchema;
using LateApexEarlySpeed.Json.Schema.Keywords;
using System.Reflection;

namespace LateApexEarlySpeed.Json.Schema.Generator.SchemaGenerators;

internal static class SchemaGenerationHelper
{
    public static BodyJsonSchema GenerateSchemaForUnsignedInteger(IEnumerable<KeywordBase> keywordsFromProperty, ulong max)
    {
        return new BodyJsonSchema(keywordsFromProperty.Append(new TypeKeyword(InstanceType.Integer)).Append(new MinimumKeyword(0)).Append(new MaximumKeyword(max)));
    }

    public static BodyJsonSchema GenerateSchemaForSignedInteger(IEnumerable<KeywordBase> keywordsFromProperty, long min, long max)
    {
        return new BodyJsonSchema(keywordsFromProperty.Append(new TypeKeyword(InstanceType.Integer)).Append(new MinimumKeyword(min)).Append(new MaximumKeyword(max)));
    }

    public static BodyJsonSchema GenerateSchemaForDouble(IEnumerable<KeywordBase> keywordsFromProperty, double min, double max)
    {
        return new BodyJsonSchema(keywordsFromProperty.Append(new TypeKeyword(InstanceType.Number)).Append(new MinimumKeyword(min)).Append(new MaximumKeyword(max)));
    }

    public static BodyJsonSchema GenerateSchemaForDecimal(IEnumerable<KeywordBase> keywordsFromProperty)
    {
        return new BodyJsonSchema(keywordsFromProperty.Append(new TypeKeyword(InstanceType.Number)).Append(new MinimumKeyword(decimal.MinValue)).Append(new MaximumKeyword(decimal.MaxValue)));
    }

    public static BodyJsonSchema GenerateSchemaReference(Type type, IEnumerable<KeywordBase> keywordsFromProperty, Uri baseUri)
    {
        return new BodyJsonSchema(keywordsFromProperty, Enumerable.Empty<ISchemaContainerValidationNode>(), new [] { new SchemaReferenceKeyword(CreateRefUri(type, baseUri)) }, null, null, null, null);
    }

    private static Uri CreateRefUri(Type type, Uri baseUri)
    {
        var relativeRefUri = new Uri("#" + new ArrayBasedImmutableJsonPointer(
            new[] { DefsKeyword.Keyword, TypeSchemaDefinitions.GetDefName(type) }), UriKind.Relative);

        return new Uri(baseUri, relativeRefUri);
    }

    public static BodyJsonSchema GenerateSchemaForJsonType(InstanceType instanceType, IEnumerable<KeywordBase> keywordsFromProperty)
    {
        return new BodyJsonSchema(keywordsFromProperty.Append(new TypeKeyword(instanceType)));
    }

    /// <summary>
    /// Extract attributes from header of <see cref="Type"/> itself
    /// </summary>
    public static IEnumerable<KeywordBase> GenerateKeywordsFromType(Type type)
    {
        IEnumerable<IKeywordGenerator> keywordGeneratorOnType = type.GetCustomAttributes().OfType<IKeywordGenerator>();
        return keywordGeneratorOnType.Select(keywordGenerator => keywordGenerator.CreateKeyword(type));
    }
}