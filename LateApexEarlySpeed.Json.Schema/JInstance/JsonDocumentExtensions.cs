using System.Text.Json;
using LateApexEarlySpeed.Json.Schema.Common;

namespace LateApexEarlySpeed.Json.Schema.JInstance;

internal static class JsonDocumentExtensions
{
    public static JsonInstanceElement RootInstanceElement(this JsonDocument jsonDoc)
    {
        return new JsonInstanceElement(jsonDoc.RootElement, ImmutableJsonPointer.Empty);
    }
}