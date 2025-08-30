using System.Text.Json;
using LateApexEarlySpeed.Json.Schema.Common;

namespace LateApexEarlySpeed.Json.Schema.JInstance;

public readonly struct JsonInstanceProperty
{
    internal JsonInstanceProperty(JsonProperty jsonProperty, ImmutableJsonPointer parentLocation)
    {
        Name = jsonProperty.Name;
        Value = new JsonInstanceElement(jsonProperty.Value, parentLocation.Add(Name));
    }

    public string Name { get; }

    public JsonInstanceElement Value { get; }
}