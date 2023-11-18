using System.Text.Json;
using LateApexEarlySpeed.Json.Schema.Common;

namespace LateApexEarlySpeed.Json.Schema.JInstance;

public readonly struct JsonInstanceProperty
{
    private readonly JsonProperty _jsonProperty;
    private readonly ImmutableJsonPointer _propertyLocation;

    public JsonInstanceProperty(JsonProperty jsonProperty, ImmutableJsonPointer propertyLocation)
    {
        _jsonProperty = jsonProperty;
        _propertyLocation = propertyLocation;
    }

    public string Name => _jsonProperty.Name;

    public JsonInstanceElement Value => new(_jsonProperty.Value, _propertyLocation);
}