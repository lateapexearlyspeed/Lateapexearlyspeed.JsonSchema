using LateApexEarlySpeed.Json.Schema.Keywords;
using System.Text.Json;
using System.Text.Json.Nodes;
using Xunit;

namespace LateApexEarlySpeed.Json.Schema.UnitTests;

public class JsonValidatorTestFixture
{
    private readonly HashSet<string> _ignoredRemoteFiles = new HashSet<string> { "locationIndependentIdentifierPre2019.json", "locationIndependentIdentifierDraft4.json" };
    private readonly HashSet<string> _ignoredRemoteFolders = new HashSet<string> { "draft6", "draft7" };

    public JsonValidatorTestFixture()
    {
        ExternalSchemaDocuments = PrepareRefRemoteDocuments();

        FormatRegistry.AddFormatType<TrueToFalseFormatValidator>();
        
        // Test to add duplicated format names
        FormatRegistry.SetFormatType<TrueToTrueFormatValidator>();
        Assert.Throws<ArgumentException>(FormatRegistry.AddFormatType<TrueToFalseFormatValidator>);
        Assert.Throws<ArgumentException>(FormatRegistry.AddFormatType<DateTimeFormatValidator>);
    }

    public IEnumerable<string> ExternalSchemaDocuments { get; }

    public Uri[] HttpBasedDocumentUris { get; } = new[]
    {
        new Uri("https://json-schema.org/draft/2020-12/schema"),
        new Uri("https://json-schema.org/draft/2020-12/meta/core"),
        new Uri("https://json-schema.org/draft/2020-12/meta/applicator"),
        new Uri("https://json-schema.org/draft/2020-12/meta/unevaluated"),
        new Uri("https://json-schema.org/draft/2020-12/meta/validation"),
        new Uri("https://json-schema.org/draft/2020-12/meta/meta-data"),
        new Uri("https://json-schema.org/draft/2020-12/meta/format-annotation"),
        new Uri("https://json-schema.org/draft/2020-12/meta/content"),
    };

    private IEnumerable<string> PrepareRefRemoteDocuments()
    {
        string path = Path.Combine("JSON-Schema-Test-Suite", "remotes");

        List<JsonNode> schemaDocuments = ExtractAllSchemaDocumentsFrom(path, new Uri("http://localhost:1234"));

        return schemaDocuments.Select(doc => doc.ToJsonString());
    }

    private List<JsonNode> ExtractAllSchemaDocumentsFrom(string path, Uri uri)
    {
        var schemaDocs = new List<JsonNode>();
        var curDirectory = new DirectoryInfo(path);

        foreach (FileInfo fileInfo in curDirectory.EnumerateFiles())
        {
            if (_ignoredRemoteFiles.Contains(fileInfo.Name))
            {
                continue;
            }

            var id = new Uri(uri, fileInfo.Name);
            using (FileStream fs = fileInfo.OpenRead())
            {
                JsonNode jsonNode = JsonNode.Parse(fs)!;
                jsonNode.AsObject()["$id"] = JsonSerializer.SerializeToNode(id);
                schemaDocs.Add(jsonNode);
            }
        }

        foreach (DirectoryInfo subDirectory in curDirectory.EnumerateDirectories())
        {
            if (_ignoredRemoteFolders.Contains(subDirectory.Name))
            {
                continue;
            }

            schemaDocs.AddRange(ExtractAllSchemaDocumentsFrom(Path.Combine(path, subDirectory.Name), new Uri(uri, subDirectory.Name + "/")));
        }

        return schemaDocs;
    }
}