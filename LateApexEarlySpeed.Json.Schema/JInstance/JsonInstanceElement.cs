using System.Text.Json;
using LateApexEarlySpeed.Json.Schema.Common;

namespace LateApexEarlySpeed.Json.Schema.JInstance;

public readonly struct JsonInstanceElement
{
    private readonly JsonElement _jsonElement;
    private readonly ImmutableJsonPointer _instanceLocation;

    public JsonInstanceElement(JsonElement jsonElement, ImmutableJsonPointer instanceLocation)
    {
        _jsonElement = jsonElement;
        _instanceLocation = instanceLocation;
    }

    public IEnumerable<JsonInstanceProperty> EnumerateObject()
    {
        JsonElement.ObjectEnumerator objectEnumerator = _jsonElement.EnumerateObject();
        foreach (JsonProperty jsonProperty in objectEnumerator)
        {
            yield return new JsonInstanceProperty(jsonProperty, _instanceLocation.Add(jsonProperty.Name));
        }
    }

    public IEnumerable<JsonInstanceElement> EnumerateArray()
    {
        int idx = 0;

        foreach (JsonElement item in _jsonElement.EnumerateArray())
        {
            yield return new JsonInstanceElement(item, _instanceLocation.Add(idx++));
        }
    }

    public JsonValueKind ValueKind => _jsonElement.ValueKind;

    public string? GetString() => _jsonElement.GetString();

    public double GetDouble() => _jsonElement.GetDouble();

    public uint GetUInt32() => _jsonElement.GetUInt32();

    public bool TryGetUInt32(out uint value)
    {
        return _jsonElement.TryGetUInt32(out value);
    }
}