using System.Text.Json.Nodes;

namespace LateApexEarlySpeed.Json.Schema.UnitTests;

public class JsonValidatorTestFixture
{
    private readonly HashSet<string> _ignoredRemoteFiles = new HashSet<string> { "locationIndependentIdentifierDraft4.json" };
    private readonly HashSet<string> _ignoredRemoteFolders = new HashSet<string> { "draft6" };
    
    private readonly Dictionary<Uri, string> _externalSchemaDocuments;
    private readonly string[] _customMetaSchemaDocuments;

    public JsonValidatorTestFixture()
    {
        PrepareRefRemoteDocuments(out _externalSchemaDocuments, out _customMetaSchemaDocuments);
    }

    public Dictionary<Uri, string> ExternalSchemaDocuments => _externalSchemaDocuments;
    public string[] CustomMetaSchemaDocuments => _customMetaSchemaDocuments;

    public Uri[] HttpBasedDocumentUris { get; } = new[]
    {
        // 2020
        new Uri("https://json-schema.org/draft/2020-12/schema"),
        new Uri("https://json-schema.org/draft/2020-12/meta/core"),
        new Uri("https://json-schema.org/draft/2020-12/meta/applicator"),
        new Uri("https://json-schema.org/draft/2020-12/meta/unevaluated"),
        new Uri("https://json-schema.org/draft/2020-12/meta/validation"),
        new Uri("https://json-schema.org/draft/2020-12/meta/meta-data"),
        new Uri("https://json-schema.org/draft/2020-12/meta/format-annotation"),
        new Uri("https://json-schema.org/draft/2020-12/meta/content"),

        // 2019
        new Uri("https://json-schema.org/draft/2019-09/schema"),
        new Uri("https://json-schema.org/draft/2019-09/meta/core"),
        new Uri("https://json-schema.org/draft/2019-09/meta/applicator"),
        new Uri("https://json-schema.org/draft/2019-09/meta/validation"),
        new Uri("https://json-schema.org/draft/2019-09/meta/meta-data"),
        new Uri("https://json-schema.org/draft/2019-09/meta/format"),
        new Uri("https://json-schema.org/draft/2019-09/meta/content"),

        // draft7
        new Uri("http://json-schema.org/draft-07/schema")
    };

    private void PrepareRefRemoteDocuments(out Dictionary<Uri, string> externalSchemaDocuments, out string[] customMetaSchemaDocuments)
    {
        string path = Path.Combine("JSON-Schema-Test-Suite", "remotes");

        Dictionary<Uri, JsonNode> schemaDocuments = ExtractAllSchemaDocumentsFrom(path, new Uri("http://localhost:1234"));

        externalSchemaDocuments = schemaDocuments.ToDictionary(doc => doc.Key, doc => doc.Value.ToJsonString());
        customMetaSchemaDocuments = schemaDocuments.Select(doc => doc.Value).Where(doc => doc["$id"] is not null).Select(doc => doc.ToJsonString()).ToArray();
    }

    private Dictionary<Uri, JsonNode> ExtractAllSchemaDocumentsFrom(string path, Uri uri)
    {
        var schemaDocs = new Dictionary<Uri, JsonNode>();
        var curDirectory = new DirectoryInfo(path);

        foreach (FileInfo fileInfo in curDirectory.EnumerateFiles())
        {
            if (_ignoredRemoteFiles.Contains(fileInfo.Name))
            {
                continue;
            }

            var retrievalUri = new Uri(uri, fileInfo.Name);
            using (FileStream fs = fileInfo.OpenRead())
            {
                JsonNode jsonNode = JsonNode.Parse(fs)!;
                schemaDocs.Add(retrievalUri, jsonNode);
            }
        }

        foreach (DirectoryInfo subDirectory in curDirectory.EnumerateDirectories())
        {
            if (_ignoredRemoteFolders.Contains(subDirectory.Name))
            {
                continue;
            }

            foreach ((Uri retrievalUri, JsonNode schema) in ExtractAllSchemaDocumentsFrom(Path.Combine(path, subDirectory.Name), new Uri(uri, subDirectory.Name + "/")))
            {
                schemaDocs.Add(retrievalUri, schema);
            }
        }

        return schemaDocs;
    }
}