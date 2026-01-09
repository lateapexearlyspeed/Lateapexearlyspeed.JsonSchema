using System.Diagnostics;

namespace LateApexEarlySpeed.Json.Schema.Keywords;

internal class SchemaKeyword
{
    public const string Keyword = "$schema";

    private static readonly Uri Draft202012Identifier = new("https://json-schema.org/draft/2020-12/schema");
    private static readonly Uri Draft201909Identifier = new("https://json-schema.org/draft/2019-09/schema");
    private static readonly Uri Draft7Identifier = new("http://json-schema.org/draft-07/schema#");

    private static readonly SchemaKeyword Draft202012Keyword = new(DialectKind.Draft202012);
    private static readonly SchemaKeyword Draft201909Keyword = new(DialectKind.Draft201909);
    private static readonly SchemaKeyword Draft7Keyword = new(DialectKind.Draft7);

    private SchemaKeyword(DialectKind dialect)
    {
        Dialect = dialect;
    }

    public static SchemaKeyword Create(Uri schemaUri)
    {
        if (schemaUri == Draft7Identifier)
        {
            return Draft7Keyword;
        }

        if (schemaUri == Draft201909Identifier)
        {
            return Draft201909Keyword;
        }

        return Draft202012Keyword;
    }

    public DialectKind Dialect { get; }

    public Uri DraftIdentifier
    {
        get
        {
            switch (Dialect)
            {
                case DialectKind.Draft7:
                    return Draft7Identifier;
                case DialectKind.Draft201909:
                    return Draft201909Identifier;
                default:
                    Debug.Assert(Dialect == DialectKind.Draft202012);
                    return Draft202012Identifier;
            }
        }
    }
}