using System.Text.Json.Nodes;
using System.Text.Json;
using BenchmarkDotNet.Running;

namespace Json.Schema.Libraries.Benchmark
{
    internal class Program
    {
        static void Main(string[] args)
        {
            IJsonSchemaValidation[] jsonSchemaValidations = new IJsonSchemaValidation[]
            {
                new LateApexEarlySpeedValidation(),
                new JsonSchemaDotNetValidation(),
                new NJsonSchemaValidation()
            };
            
            JsonSchemaValidationRunner jsonSchemaValidationRunner = new JsonSchemaValidationRunner(jsonSchemaValidations);
            
            Console.WriteLine("Start to run ...");
            
            while (true)
            {
                jsonSchemaValidationRunner.ReuseSchema_LateApexEarlySpeed(TestValidationResult.All);
            }
            


            // BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
            var summary = BenchmarkRunner.Run<BenchmarkTests>();

            // new StopwatchTests().Run();
        }

        private static IEnumerable<string> PrepareRefRemoteDocuments()
        {
            string path = Path.Combine("JSON-Schema-Test-Suite", "remotes");

            List<JsonNode> schemaDocuments = ExtractAllSchemaDocumentsFrom(path, new Uri("http://localhost:1234"));

            return schemaDocuments.Select(doc => doc.ToJsonString());
        }

        private static List<JsonNode> ExtractAllSchemaDocumentsFrom(string path, Uri uri)
        {
            var schemaDocs = new List<JsonNode>();
            var curDirectory = new DirectoryInfo(path);

            foreach (FileInfo fileInfo in curDirectory.EnumerateFiles())
            {
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
                schemaDocs.AddRange(ExtractAllSchemaDocumentsFrom(Path.Combine(path, subDirectory.Name), new Uri(uri, subDirectory.Name + "/")));
            }

            return schemaDocs;
        }
    }
}
