using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using LateApexEarlySpeed.Json.Schema.JSchema;

namespace LateApexEarlySpeed.Json.Schema.Keywords;

internal class SchemaKeyword
{
    public const string Keyword = "$schema";

    private const string Draft202012IdentifierString = "https://json-schema.org/draft/2020-12/schema";
    private const string Draft201909IdentifierString = "https://json-schema.org/draft/2019-09/schema";
    private const string Draft7IdentifierString      = "http://json-schema.org/draft-07/schema#";
    private const string Draft7IdentifierString2     = "http://json-schema.org/draft-07/schema";

    private static readonly Uri Draft202012Identifier = new(Draft202012IdentifierString);
    private static readonly Uri Draft201909Identifier = new(Draft201909IdentifierString);
    private static readonly Uri Draft7Identifier = new(Draft7IdentifierString);

    private static readonly SchemaKeyword Draft202012Keyword = new(DialectKind.Draft202012);
    private static readonly SchemaKeyword Draft201909Keyword = new(DialectKind.Draft201909);
    private static readonly SchemaKeyword Draft7Keyword = new(DialectKind.Draft7);

    private SchemaKeyword(DialectKind dialect)
    {
        Dialect = dialect;
        Identifier = GetDraftIdentifier(dialect);
    }

    private SchemaKeyword(Uri metaSchemaId)
    {
        Identifier = metaSchemaId;
    }

    public static SchemaKeyword Create(ReadOnlySpan<char> schemaUri, MetaSchemaRegistry? metaSchemaRegistry)
    {
        if (TryGetStandardMetaSchema(schemaUri, out SchemaKeyword? schemaKeyword))
        {
            return schemaKeyword;
        }

        if (metaSchemaRegistry is null)
        {
            return Draft202012Keyword;
        }

        var metaSchemaId = new Uri(schemaUri.ToString());
        if (metaSchemaRegistry.GetMetaSchema(metaSchemaId) is null)
        {
            return Draft202012Keyword;
        }

        return new SchemaKeyword(metaSchemaId);
    }

    private static bool TryGetStandardMetaSchema(ReadOnlySpan<char> schemaUri, [NotNullWhen(true)] out SchemaKeyword? schemaKeyword)
    {
        // fast path
        if (schemaUri.Equals(Draft7IdentifierString, StringComparison.Ordinal))
        {
            schemaKeyword = Draft7Keyword;
            return true;
        }

        if (schemaUri.Equals(Draft202012IdentifierString, StringComparison.Ordinal))
        {
            schemaKeyword = Draft202012Keyword;
            return true;
        }

        if (schemaUri.Equals(Draft201909IdentifierString, StringComparison.Ordinal))
        {
            schemaKeyword = Draft201909Keyword;
            return true;
        }

        if (schemaUri.Equals(Draft7IdentifierString2, StringComparison.Ordinal))
        {
            schemaKeyword = Draft7Keyword;
            return true;
        }

        // slow path
        var currentSchemaUri = new Uri(schemaUri.ToString());

        return TryGetStandardMetaSchema(currentSchemaUri, out schemaKeyword);
    }

    public static bool TryGetStandardMetaSchema(Uri metaSchemaId, [NotNullWhen(true)] out SchemaKeyword? schemaKeyword)
    {
        if (metaSchemaId == Draft7Identifier)
        {
            schemaKeyword = Draft7Keyword;
            return true;
        }

        if (metaSchemaId == Draft202012Identifier)
        {
            schemaKeyword = Draft202012Keyword;
            return true;
        }

        if (metaSchemaId == Draft201909Identifier)
        {
            schemaKeyword = Draft201909Keyword;
            return true;
        }

        schemaKeyword = null;
        return false;
    }

    public static bool TryGetDialectKind(Uri metaSchemaId, MetaSchemaRegistry? metaSchemaRegistry, out DialectKind dialect)
    {
        if (TryGetStandardMetaSchema(metaSchemaId, out SchemaKeyword? schemaKeyword))
        {
            Debug.Assert(schemaKeyword.Dialect.HasValue);

            dialect = schemaKeyword.Dialect.Value;
            return true;
        }

        if (metaSchemaRegistry is null)
        {
            dialect = DialectKind.Draft202012;
            return true;
        }

        if (metaSchemaRegistry.GetMetaSchema(metaSchemaId) is null)
        {
            dialect = DialectKind.Draft202012;
            return true;
        }

        dialect = default;
        return false;
    }

    public DialectKind? Dialect { get; }
    public Uri Identifier { get; }

    public static Uri GetDraftIdentifier(DialectKind dialect)
    {
        switch (dialect)
        {
            case DialectKind.Draft7:
                return Draft7Identifier;
            case DialectKind.Draft201909:
                return Draft201909Identifier;
            default:
                Debug.Assert(dialect == DialectKind.Draft202012);
                return Draft202012Identifier;
        }
    }
}