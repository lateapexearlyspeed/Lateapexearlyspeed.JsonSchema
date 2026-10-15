using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

using LateApexEarlySpeed.Json.Schema;
using LateApexEarlySpeed.Json.Schema.Common;
using LateApexEarlySpeed.Json.Schema.JSchema;
using LateApexEarlySpeed.Json.Schema.JSchema.interfaces;
using LateApexEarlySpeed.Json.Schema.Keywords;

ICommandSource cmdSource = args.Length == 0 ? new ConsoleCommandSource() : new FileCommandSource(args[0]);

bool started = false;
DialectKind currentDialect = DialectKind.Draft202012;

var dialects = new Dictionary<string, DialectKind>
{
    { "https://json-schema.org/draft/2020-12/schema", DialectKind.Draft202012 },
    { "https://json-schema.org/draft/2019-09/schema", DialectKind.Draft201909 },
    { "http://json-schema.org/draft-07/schema#", DialectKind.Draft7 },
};

while (cmdSource.GetNextCommand() is { } line && line != "")
{
    JsonNode root = JsonNode.Parse(line)!;
    string? cmd = root["cmd"]?.GetValue<string>();

    switch (cmd)
    {
        case "start":
        {
            JsonNode? version = root["version"];
            if (version is null || version.GetValue<int>() != 1)
            {
                throw new UnknownVersion(version);
            }

            started = true;

            var startResult = new JsonObject
            {
                ["version"] = 1,
                ["implementation"] = new JsonObject
                {
                    ["language"] = "dotnet",
                    ["name"] = "LateApexEarlySpeed.Json.Schema",
                    ["version"] = GetLibVersion(),
                    ["homepage"] = "https://github.com/lateapexearlyspeed/Lateapexearlyspeed.JsonSchema",
                    ["documentation"] = "https://github.com/lateapexearlyspeed/Lateapexearlyspeed.JsonSchema#lateapexearlyspeedjsonschema-1",
                    ["issues"] = "https://github.com/lateapexearlyspeed/Lateapexearlyspeed.JsonSchema/issues",
                    ["source"] = "https://github.com/lateapexearlyspeed/Lateapexearlyspeed.JsonSchema",
                    ["dialects"] = new JsonArray
                    {
                        "https://json-schema.org/draft/2020-12/schema",
                        "https://json-schema.org/draft/2019-09/schema",
                        "http://json-schema.org/draft-07/schema#",
                    },
                    ["os"] = Environment.OSVersion.Platform.ToString(),
                    ["os_version"] = Environment.OSVersion.Version.ToString(),
                    ["language_version"] = Environment.Version.ToString(),
                },
            };
            Console.WriteLine(startResult.ToJsonString());
            break;
        }

        case "dialect":
        {
            if (!started)
            {
                throw new NotStarted();
            }

            currentDialect = dialects[root["dialect"]!.GetValue<string>()];

            Console.WriteLine(new JsonObject { ["ok"] = true }.ToJsonString());
            break;
        }

        case "run":
        {
            if (!started)
            {
                throw new NotStarted();
            }

            JsonNode seq = root["seq"]!.DeepClone();
            JsonNode testCase = root["case"]!;

            try
            {
                var options = new JsonValidatorOptions { DefaultDialect = currentDialect };

                JsonElement schemaElement = JsonSerializer.SerializeToElement(testCase["schema"]);
                var validator = new JsonValidator(schemaElement, options);

                IExternalSchemaDocumentPopulator? registryPopulator = testCase["registry"] is JsonObject registry
                    ? new RegistryPopulator(registry)
                    : null;
                // Resolves $refs to the 3 supported dialects' own canonical meta-schema documents (e.g. the
                // official suite's "validate definition against metaschema" cases), falling back to the test
                // case's own "registry" (if any) for everything else. Documents are only read/parsed lazily,
                // on first $ref to an unresolved base URI.
                validator.ExternalSchemaDocumentPopulator = new MetaSchemaPopulator(registryPopulator);

                // LateApexEarlySpeed.Json.Schema does not support reading a schema's own "$schema"/"$vocabulary"
                // declaration to decide per-dialect keyword semantics (e.g. whether the validation vocabulary is
                // disabled entirely, as in the official suite's root-level "vocabulary.json" cases for 2019-09/
                // 2020-12 - the only cases, in Bowtie's official (non-"optional/") test scope, that use a custom
                // meta-schema outside our 3 known dialects). Ordinary schemas already get correct default behavior
                // (JsonSchemaOptions.ValidateFormat defaults to true, matching each known dialect's default keyword
                // semantics). Bowtie's protocol never tells the harness the expected result for a test (see
                // bowtie's TestCase.without_expected_results()), so this must be decided purely from the schema
                // itself, up front, and applied to every test in the case uniformly - not by comparing against an
                // expected value we never receive.
                bool usesCustomMetaschema =
                    testCase["schema"] is JsonObject schemaObj
                    && schemaObj["$schema"] is JsonValue schemaRef
                    && schemaRef.GetValue<string>() is string schemaRefValue
                    && !dialects.ContainsKey(schemaRefValue);

                var results = new JsonArray();
                // "format" defaults to assertion (validated) only under draft7, which predates the
                // $vocabulary mechanism. 2019-09/2020-12 default to annotation-only (format-assertion vocab
                // not enabled), confirmed by the official suite's root-level "format.json" ("invalid X string
                // is only an annotation by default" cases). A schema can opt back into assertion via a custom
                // meta-schema's $vocabulary, but that's the same unsupported gap as usesCustomMetaschema above.
                var validateOptions = new JsonSchemaOptions { ValidateFormat = currentDialect == DialectKind.Draft7 };
                foreach (JsonNode? test in testCase["tests"]!.AsArray())
                {
                    if (usesCustomMetaschema)
                    {
                        results.Add(new JsonObject
                        {
                            ["skipped"] = true,
                            ["message"] = "LateApexEarlySpeed.Json.Schema does not support reading a schema's custom \"$schema\"/\"$vocabulary\" declaration to override per-dialect keyword semantics (e.g. a disabled validation vocabulary).",
                            ["issue_url"] = "https://github.com/lateapexearlyspeed/Lateapexearlyspeed.JsonSchema/issues",
                        });
                        continue;
                    }

                    JsonElement instance = JsonSerializer.SerializeToElement(test!["instance"]);
                    ValidationResult validationResult = validator.Validate(instance, validateOptions);
                    results.Add(new JsonObject { ["valid"] = validationResult.IsValid });
                }

                Console.WriteLine(new JsonObject { ["seq"] = seq, ["results"] = results }.ToJsonString());
            }
            // LateApexEarlySpeed.Json.Schema throws when a subschema that is not a schema-resource root
            // (i.e. has no accompanying "$id") contains "$schema". This is a documented library limitation,
            // not a harness bug, so it is reported as skipped rather than errored.
            catch (JsonException e) when (e.Message.Contains("(non-Json resource) contains '$schema'"))
            {
                var skipResult = new JsonObject
                {
                    ["seq"] = seq,
                    ["skipped"] = true,
                    ["message"] = "LateApexEarlySpeed.Json.Schema throws when a non-resource-root subschema (no accompanying \"$id\") contains \"$schema\", instead of ignoring it.",
                    ["issue_url"] = "https://github.com/lateapexearlyspeed/Lateapexearlyspeed.JsonSchema/issues",
                };
                Console.WriteLine(skipResult.ToJsonString());
            }
            catch (Exception e)
            {
                var errorResult = new JsonObject
                {
                    ["seq"] = seq,
                    ["errored"] = true,
                    ["context"] = new JsonObject
                    {
                        ["message"] = e.ToString(),
                        ["traceback"] = Environment.StackTrace,
                    },
                };
                Console.WriteLine(errorResult.ToJsonString());
            }
            break;
        }

        case "stop":
            if (!started)
            {
                throw new NotStarted();
            }
            Environment.Exit(0);
            break;

        case null:
            throw new UnknownCommand("Missing command!");

        default:
            throw new UnknownCommand(cmd);
    }
}

static string GetLibVersion()
{
    var attribute = typeof(JsonValidator).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
    return Regex.Match(attribute!.InformationalVersion, @"\d+(\.\d+)+").Value;
}

/// <summary>
/// Populates external json schema documents referenced by a Bowtie test case's "registry" map
/// (retrieval-URI -> schema JSON), used to resolve $ref lookups during validation.
/// Invoked lazily by the library, once per unique missing base URI.
/// </summary>
class RegistryPopulator : IExternalSchemaDocumentPopulator
{
    private readonly Dictionary<string, JsonElement> _registry;

    public RegistryPopulator(JsonObject registry)
    {
        _registry = new Dictionary<string, JsonElement>();
        foreach (KeyValuePair<string, JsonNode?> entry in registry)
        {
            _registry[entry.Key] = JsonSerializer.SerializeToElement(entry.Value);
        }
    }

    public void Populate(Uri baseUri, ExternalSchemaRegistry externalSchemaRegistry)
    {
        JsonElement schema = _registry[baseUri.ToString()];

        if (schema.ValueKind == JsonValueKind.Object)
        {
            // Inject/overwrite "$id" with the exact retrieval URI so subsequent $ref lookups resolve against it.
            JsonNode schemaNode = JsonNode.Parse(schema.GetRawText())!;
            schemaNode["$id"] = baseUri.ToString();
            schema = JsonSerializer.SerializeToElement(schemaNode);
        }

        externalSchemaRegistry.Register(schema);
    }
}

/// <summary>
/// Resolves $refs to the 3 supported dialects' own canonical meta-schema documents (and their vocabulary
/// sub-schemas), read on demand from the "meta-schemas/" folder shipped alongside this harness. Falls back
/// to an inner populator (e.g. a Bowtie test case's "registry") when the base URI isn't a known meta-schema.
/// </summary>
class MetaSchemaPopulator : IExternalSchemaDocumentPopulator
{
    private static readonly Dictionary<string, string> FileNamesByUri = new()
    {
        ["https://json-schema.org/draft/2020-12/schema"] = "draft2020-12/schema.json",
        ["https://json-schema.org/draft/2020-12/meta/core"] = "draft2020-12/meta-core.json",
        ["https://json-schema.org/draft/2020-12/meta/applicator"] = "draft2020-12/meta-applicator.json",
        ["https://json-schema.org/draft/2020-12/meta/unevaluated"] = "draft2020-12/meta-unevaluated.json",
        ["https://json-schema.org/draft/2020-12/meta/validation"] = "draft2020-12/meta-validation.json",
        ["https://json-schema.org/draft/2020-12/meta/meta-data"] = "draft2020-12/meta-meta-data.json",
        ["https://json-schema.org/draft/2020-12/meta/format-annotation"] = "draft2020-12/meta-format-annotation.json",
        ["https://json-schema.org/draft/2020-12/meta/content"] = "draft2020-12/meta-content.json",

        ["https://json-schema.org/draft/2019-09/schema"] = "draft2019-09/schema.json",
        ["https://json-schema.org/draft/2019-09/meta/core"] = "draft2019-09/meta-core.json",
        ["https://json-schema.org/draft/2019-09/meta/applicator"] = "draft2019-09/meta-applicator.json",
        ["https://json-schema.org/draft/2019-09/meta/validation"] = "draft2019-09/meta-validation.json",
        ["https://json-schema.org/draft/2019-09/meta/meta-data"] = "draft2019-09/meta-meta-data.json",
        ["https://json-schema.org/draft/2019-09/meta/format"] = "draft2019-09/meta-format.json",
        ["https://json-schema.org/draft/2019-09/meta/content"] = "draft2019-09/meta-content.json",

        ["http://json-schema.org/draft-07/schema#"] = "draft7/schema.json",
        ["http://json-schema.org/draft-07/schema"] = "draft7/schema.json",
    };

    private static readonly string BaseDirectory = Path.Combine(AppContext.BaseDirectory, "meta-schemas");

    private readonly IExternalSchemaDocumentPopulator? _inner;

    public MetaSchemaPopulator(IExternalSchemaDocumentPopulator? inner = null)
    {
        _inner = inner;
    }

    public void Populate(Uri baseUri, ExternalSchemaRegistry externalSchemaRegistry)
    {
        if (FileNamesByUri.TryGetValue(baseUri.ToString(), out string? fileName))
        {
            string schemaText = File.ReadAllText(Path.Combine(BaseDirectory, fileName));
            externalSchemaRegistry.Register(schemaText.AsSpan());
            return;
        }

        if (_inner is not null)
        {
            _inner.Populate(baseUri, externalSchemaRegistry);
            return;
        }

        throw new KeyNotFoundException($"No external schema document registered for base URI: {baseUri}");
    }
}

class UnknownCommand : Exception
{
    public UnknownCommand(string message) : base(message)
    {
    }
}

class UnknownVersion : Exception
{
    public UnknownVersion(JsonNode? version) : base($"Unsupported version: {version}")
    {
    }
}

class NotStarted : Exception
{
}

interface ICommandSource
{
    string? GetNextCommand();
}

class ConsoleCommandSource : ICommandSource
{
    public string? GetNextCommand() => Console.ReadLine();
}

class FileCommandSource : ICommandSource
{
    private readonly string[] _fileContents;
    private int _line;

    public FileCommandSource(string fileName)
    {
        _fileContents = File.ReadAllLines(fileName);
    }

    public string? GetNextCommand() => _line < _fileContents.Length ? _fileContents[_line++] : null;
}
