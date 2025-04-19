using System.Text.Json;

namespace Json.Schema.Libraries.Benchmark;

internal static class TestSuiteReader
{
    public static TestCase[] ReadTestCasesFromJsonSchemaTestSuite(string draftVersion, string[] unsupportedKeywords, string[] unsupportedTestCases)
    {
        string[] pathFiles = Directory.GetFiles(Path.Combine("JSON-Schema-Test-Suite", "tests", draftVersion));

        IEnumerable<TestCase> result = Enumerable.Empty<TestCase>();

        foreach (string pathFile in pathFiles)
        {
            if (IsFileForUnsupportedKeyword(pathFile, unsupportedKeywords))
            {
                continue;
            }

            result = result.Concat(ReadTestCases(pathFile, unsupportedTestCases));
        }

        return result.ToArray();
    }

    private static IEnumerable<TestCase> ReadTestCases(string pathFile, string[] unsupportedTestCases)
    {
        using (FileStream fs = File.OpenRead(pathFile))
        {
            TestCase[] testCases = JsonSerializer.Deserialize<TestCase[]>(fs, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
            foreach (TestCase testCase in testCases)
            {
                if (!IsUnsupportedTestCase(testCase, unsupportedTestCases))
                {
                    yield return testCase;
                }
            }
        }
    }

    private static bool IsUnsupportedTestCase(TestCase testCase, string[] unsupportedTestCases)
    {
        return unsupportedTestCases.Contains(testCase.Description);
    }

    private static bool IsFileForUnsupportedKeyword(string pathFile, string[] unsupportedKeywords)
    {
        string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(pathFile);
        return unsupportedKeywords.Contains(fileNameWithoutExtension);
    }
}