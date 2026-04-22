using LateApexEarlySpeed.Json.Schema.Common;
using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text.Json;

namespace LateApexEarlySpeed.Json.Schema.JInstance;

public readonly struct JsonInstanceElement : IEquatable<JsonInstanceElement>
{
    private readonly LinkedListBasedImmutableJsonPointer _instanceLocation;

    private readonly CacheHolder _cacheHolder;

    internal JsonElement InternalJsonElement { get; }

    public JsonInstanceElement(JsonElement jsonElement, LinkedListBasedImmutableJsonPointer instanceLocation)
    {
        InternalJsonElement = jsonElement;
        _instanceLocation = instanceLocation;
        _cacheHolder = new CacheHolder();
    }

    public InstanceObjectEnumerable EnumerateObject()
    {
        return new InstanceObjectEnumerable(this);
    }

    public readonly struct InstanceObjectEnumerable : IEnumerable<JsonInstanceProperty>
    {
        private readonly JsonInstanceElement _jsonInstanceElement;

        internal InstanceObjectEnumerable(JsonInstanceElement jsonInstanceElement)
        {
            _jsonInstanceElement = jsonInstanceElement;
        }

        public InstanceObjectEnumerator GetEnumerator()
        {
            return new InstanceObjectEnumerator(_jsonInstanceElement);
        }

        public struct InstanceObjectEnumerator : IEnumerator<JsonInstanceProperty>
        {
            private readonly LinkedListBasedImmutableJsonPointer _instanceLocation;
            private readonly CacheHolder _cacheHolder;

            private JsonElement.ObjectEnumerator _objectEnumerator;
            private List<JsonInstanceProperty>.Enumerator _cacheListEnumerator;

            /// <summary>
            /// Besides accumulate the enumerated properties, this field is also used as the flag to indicate whether cache existed or not:
            /// If this field is not null, it means cache did not exist and enumerator should enumerate from <see cref="_objectEnumerator"/>;
            /// if this field is null, it means cache already existed and enumerator should enumerate from <see cref="_cacheListEnumerator"/>.
            /// </summary>
            /// <remarks>
            /// Also see <see cref="UseCache"/>
            /// </remarks>
            private List<JsonInstanceProperty>? _properties;

            internal InstanceObjectEnumerator(JsonInstanceElement jsonInstanceElement)
            {
                _instanceLocation = jsonInstanceElement._instanceLocation;
                _cacheHolder = jsonInstanceElement._cacheHolder;

                if (_cacheHolder.Properties is null)
                {
                    _objectEnumerator = jsonInstanceElement.InternalJsonElement.EnumerateObject();
                    _properties = new();
                }
                else
                {
                    _cacheListEnumerator = _cacheHolder.Properties.GetEnumerator();
                    _properties = null;
                }
            }

            public bool MoveNext()
            {
                return UseCache() ? MoveNextByCache() : MoveNextWithoutCache();
            }

            private bool MoveNextByCache()
            {
                if (_cacheListEnumerator.MoveNext())
                {
                    Current = _cacheListEnumerator.Current;
                    return true;
                }

                return false;
            }

            private bool MoveNextWithoutCache()
            {
                Debug.Assert(_properties is not null);

                if (_objectEnumerator.MoveNext())
                {
                    Current = new JsonInstanceProperty(_objectEnumerator.Current, _instanceLocation);
                    _properties.Add(Current);

                    return true;
                }

                _cacheHolder.Properties = _properties;
                return false;
            }

            public void Reset()
            {
                if (UseCache())
                {
                    ((IEnumerator)_cacheListEnumerator).Reset();
                }
                else
                {
                    _objectEnumerator.Reset();
                    _properties = new();
                }
            }

            public JsonInstanceProperty Current { get; private set; }

            object IEnumerator.Current => Current;

            public void Dispose()
            {
                if (UseCache())
                {
                    _cacheListEnumerator.Dispose();
                }
                else
                {
                    _objectEnumerator.Dispose();
                }
            }

            private bool UseCache() => _properties is null;
        }

        IEnumerator<JsonInstanceProperty> IEnumerable<JsonInstanceProperty>.GetEnumerator()
        {
            return GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public InstanceArrayEnumerable EnumerateArray()
    {
        return new InstanceArrayEnumerable(this);
    }

    public readonly struct InstanceArrayEnumerable : IEnumerable<JsonInstanceElement>
    {
        private readonly JsonInstanceElement _jsonInstanceElement;

        internal InstanceArrayEnumerable(JsonInstanceElement jsonInstanceElement)
        {
            _jsonInstanceElement = jsonInstanceElement;
        }

        public InstanceArrayEnumerator GetEnumerator()
        {
            return new InstanceArrayEnumerator(_jsonInstanceElement);
        }

        public struct InstanceArrayEnumerator : IEnumerator<JsonInstanceElement>
        {
            private readonly LinkedListBasedImmutableJsonPointer _instanceLocation;
            private readonly CacheHolder _cacheHolder;

            private JsonElement.ArrayEnumerator _arrayEnumerator;
            private List<JsonInstanceElement>.Enumerator _cacheListEnumerator;

            /// <summary>
            /// Besides accumulate the enumerated items, this field is also used as the flag to indicate whether cache existed or not:
            /// If this field is not null, it means cache did not exist and enumerator should enumerate from <see cref="_arrayEnumerator"/>;
            /// if this field is null, it means cache already existed and enumerator should enumerate from <see cref="_cacheListEnumerator"/>.
            /// </summary>
            /// <remarks>
            /// Also see <see cref="UseCache"/>
            /// </remarks>
            private List<JsonInstanceElement>? _items;

            private int _idx;

            internal InstanceArrayEnumerator(JsonInstanceElement jsonInstanceElement)
            {
                _instanceLocation = jsonInstanceElement._instanceLocation;
                _cacheHolder = jsonInstanceElement._cacheHolder;

                if (_cacheHolder.ArrayItems is null)
                {
                    _arrayEnumerator = jsonInstanceElement.InternalJsonElement.EnumerateArray();
                    _items = new();
                    _idx = 0;
                }
                else
                {
                    _cacheListEnumerator = _cacheHolder.ArrayItems.GetEnumerator();
                    _items = null;
                }
            }

            public bool MoveNext()
            {
                return UseCache() ? MoveNextByCache() : MoveNextWithoutCache();
            }

            private bool MoveNextByCache()
            {
                if (_cacheListEnumerator.MoveNext())
                {
                    Current = _cacheListEnumerator.Current;
                    return true;
                }

                return false;
            }

            private bool MoveNextWithoutCache()
            {
                Debug.Assert(_items is not null);

                if (_arrayEnumerator.MoveNext())
                {
                    Current = new JsonInstanceElement(_arrayEnumerator.Current, _instanceLocation.Add(_idx++));
                    _items.Add(Current);

                    return true;
                }

                _cacheHolder.ArrayItems = _items;
                return false;
            }

            public void Reset()
            {
                if (UseCache())
                {
                    ((IEnumerator)_cacheListEnumerator).Reset();
                }
                else
                {
                    _arrayEnumerator.Reset();
                    _items = new();
                    _idx = 0;
                }
            }

            public JsonInstanceElement Current { get; private set; }

            object IEnumerator.Current => Current;

            public void Dispose()
            {
                if (UseCache())
                {
                    _cacheListEnumerator.Dispose();
                }
                else
                {
                    _arrayEnumerator.Dispose();
                }
            }

            private bool UseCache() => _items is null;
        }

        IEnumerator<JsonInstanceElement> IEnumerable<JsonInstanceElement>.GetEnumerator()
        {
            return GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public LinkedListBasedImmutableJsonPointer Location => _instanceLocation;

    public JsonValueKind ValueKind => InternalJsonElement.ValueKind;

    public string? GetString() => InternalJsonElement.GetString();

    public double GetDouble() => InternalJsonElement.GetDouble();

    public bool TryGetDouble(out double value) => InternalJsonElement.TryGetDouble(out value);
    
    public bool TryGetInt64(out long value) => InternalJsonElement.TryGetInt64(out value);
    
    public bool TryGetUInt64(out ulong value) => InternalJsonElement.TryGetUInt64(out value);

    public bool TryGetDecimal(out decimal value)
    {
        return InternalJsonElement.TryGetDecimal(out value);
    }

    public string GetRawText()
    {
        return InternalJsonElement.GetRawText();
    }

    public ReadOnlySpan<byte> GetRawUtf8Value()
    {
        return JsonMarshal.GetRawUtf8Value(InternalJsonElement);
    }

    /// <summary>
    /// This method will check the numeric range and convert to corresponding type. The matching order is: long -> ulong -> double.
    /// This method will ensure one parameter will be set as <see cref="Nullable{T}.HasValue"/> unless exception thrown.
    /// </summary>
    public void GetNumericValue(out double? doubleValue, out long? longValue, out ulong? ulongValue)
    {
        if (InternalJsonElement.TryGetInt64(out long tmpLong))
        {
            longValue = tmpLong;
            doubleValue = null;
            ulongValue = null;
            return;
        }

        if (InternalJsonElement.TryGetUInt64(out ulong tmpULong))
        {
            ulongValue = tmpULong;
            doubleValue = null;
            longValue = null;
            return;
        }

        doubleValue = InternalJsonElement.GetDouble();
        longValue = null;
        ulongValue = null;
    }

    /// <summary>
    /// Check if can convert to long or ulong range (consider zero decimal)
    /// </summary>
    public bool IsIntegerTypeForJsonSchema()
    {
        if (!IsIntegerOrZeroDecimalNumber())
        {
            return false;
        }

        double value = GetDouble();
        return value <= ulong.MaxValue && value >= long.MinValue;
    }

    /// <summary>
    /// Check if can convert to ulong range (consider zero decimal)
    /// </summary>
    public bool TryGetUInt64ForJsonSchema(out ulong value)
    {
        if (!IsIntegerOrZeroDecimalNumber())
        {
            value = default;
            return false;
        }

        if (TryGetUInt64(out ulong ulongValue))
        {
            value = ulongValue;
            return true;
        }

        if (TryGetDecimal(out decimal decimalValue))
        {
            if (decimalValue >= ulong.MinValue && decimalValue <= ulong.MaxValue)
            {
                value = (ulong)decimalValue;
                return true;
            }

            value = default;
            return false;
        }

        double doubleValue = GetDouble();
        if (doubleValue >= ulong.MinValue && doubleValue <= ulong.MaxValue)
        {
            value = (ulong)doubleValue;
            return true;
        }

        value = default;
        return false;
    }

    /// <summary>
    /// Check if can convert to long range (consider zero decimal)
    /// </summary>
    public bool TryGetInt64ForJsonSchema(out long value)
    {
        if (!IsIntegerOrZeroDecimalNumber())
        {
            value = default;
            return false;
        }

        if (TryGetInt64(out long longValue))
        {
            value = longValue;
            return true;
        }

        if (TryGetDecimal(out decimal decimalValue))
        {
            if (decimalValue >= long.MinValue && decimalValue <= long.MaxValue)
            {
                value = (long)decimalValue;
                return true;
            }

            value = default;
            return false;
        }

        double doubleValue = GetDouble();
        if (doubleValue >= long.MinValue && doubleValue <= long.MaxValue)
        {
            value = (long)doubleValue;
            return true;
        }

        value = default;
        return false;
    }

    /// <summary>
    /// Check if value is integer (consider zero decimal), without checking actual numeric range.
    /// </summary>
    private bool IsIntegerOrZeroDecimalNumber()
    {
        if (ValueKind != JsonValueKind.Number)
        {
            return false;
        }

        string rawText = GetRawText();
        int dotIdx = rawText.IndexOf('.');
        if (dotIdx != -1)
        {
            ReadOnlySpan<char> fraction = rawText.AsSpan(dotIdx + 1);
            foreach (char c in fraction)
            {
                if (c != '0')
                {
                    return false;
                }
            }
        }

        return true;
    }

    public EquivalentResult Equivalent(JsonInstanceElement other, JsonCollectionEqualityComparer jsonCollectionEqualityComparer)
    {
        if (ValueKind != other.ValueKind)
        {
            JsonValueKind curValueKind = ValueKind;
            return EquivalentResult.Fail(() => $"Json kind not same, one is {curValueKind}, but another is {other.ValueKind}", _instanceLocation, other._instanceLocation);
        }

        switch (ValueKind)
        {
            case JsonValueKind.True:
            case JsonValueKind.False:
            case JsonValueKind.Null:
                return EquivalentResult.Success();

            case JsonValueKind.String:

                if (GetRawUtf8Value().SequenceEqual(other.GetRawUtf8Value()))
                {
                    return EquivalentResult.Success();
                }

                string curString = GetString()!;
                string otherString = other.GetString()!;

                return EquivalentResult.Fail(() => StringNotSameMessageTemplate(curString, otherString), _instanceLocation, other._instanceLocation);

            case JsonValueKind.Number:
                return NumberEquivalent(other);

            case JsonValueKind.Array:
                return SequenceEquivalent(other, jsonCollectionEqualityComparer);

            case JsonValueKind.Object:
                return ObjectEquivalent(other, jsonCollectionEqualityComparer);

            default:
                Debug.Fail("Should not go to this default block, because all JsonValueKinds should already be handled.");
                throw new NotImplementedException("Should not go here, because all JsonValueKinds should already be handled.");
        }
    }

    private EquivalentResult NumberEquivalent(JsonInstanceElement other)
    {
        if (InternalJsonElement.TryGetInt64(out long thisLongValue))
        {
            // To compatible with json schema const definition, when failed to convert to long type directly, we need to consider zero decimal case.
            if (other.InternalJsonElement.TryGetInt64(out long otherLongValue) || other.TryGetInt64ForJsonSchema(out otherLongValue))
            {
                return thisLongValue == otherLongValue
                    ? EquivalentResult.Success()
                    : EquivalentResult.Fail(() => NumberNotSameMessageTemplate(thisLongValue, otherLongValue), _instanceLocation, other._instanceLocation);
            }

            other.GetNumericValue(out double? doubleValue, out long? longValue, out ulong? ulongValue);

            Debug.Assert(!longValue.HasValue);

            return EquivalentResult.Fail(() => NumberNotSameMessageTemplate(thisLongValue, doubleValue.HasValue ? doubleValue.Value.ToString(CultureInfo.InvariantCulture) : ulongValue.GetValueOrDefault().ToString()), _instanceLocation, other._instanceLocation);
        }

        if (InternalJsonElement.TryGetUInt64(out ulong thisULongValue))
        {
            // To compatible with json schema const definition, when failed to convert to ulong type directly, we need to consider zero decimal case.
            if (other.InternalJsonElement.TryGetUInt64(out ulong otherULongValue) || other.TryGetUInt64ForJsonSchema(out otherULongValue))
            {
                return thisULongValue == otherULongValue
                    ? EquivalentResult.Success()
                    : EquivalentResult.Fail(() => NumberNotSameMessageTemplate(thisULongValue, otherULongValue), _instanceLocation, other._instanceLocation);
            }

            other.GetNumericValue(out double? doubleValue, out long? longValue, out ulong? ulongValue);

            Debug.Assert(!ulongValue.HasValue);

            return EquivalentResult.Fail(() => NumberNotSameMessageTemplate(thisULongValue, doubleValue.HasValue ? doubleValue.Value.ToString(CultureInfo.InvariantCulture) : longValue.GetValueOrDefault()), _instanceLocation, other._instanceLocation);
        }

        if (InternalJsonElement.TryGetDecimal(out decimal thisDecimalValue))
        {
            if (other.TryGetDecimal(out decimal otherDecimalValue))
            {
                const decimal tolerance = 0.0000000000000000000000001m;

                decimal actualTolerance = Math.Abs(thisDecimalValue) * tolerance;

                return Math.Abs(thisDecimalValue - otherDecimalValue) <= actualTolerance
                    ? EquivalentResult.Success()
                    : EquivalentResult.Fail(() => NumberNotSameMessageTemplate(thisDecimalValue, otherDecimalValue), _instanceLocation, other._instanceLocation);
            }

            double otherDoubleValue = other.GetDouble();

            return EquivalentResult.Fail(() => NumberNotSameMessageTemplate(thisDecimalValue, otherDoubleValue), _instanceLocation, other._instanceLocation);
        }

        if (InternalJsonElement.TryGetDouble(out double thisDoubleValue))
        {
            const double tolerance = 0.000001;
            
            double otherDoubleValue = other.GetDouble();

            double actualTolerance = Math.Abs(thisDoubleValue) * tolerance;

            return Math.Abs(thisDoubleValue - otherDoubleValue) <= actualTolerance
                ? EquivalentResult.Success()
                : EquivalentResult.Fail(() => NumberNotSameMessageTemplate(thisDoubleValue, otherDoubleValue), _instanceLocation, other._instanceLocation);
        }

        Debug.Fail("Should not go here, have considered all numeric types. Missed any other types ?");
        throw new NotImplementedException("Should have considered all numeric types. Missed any other types ?");
    }

    internal static string NumberNotSameMessageTemplate(object thisValue, object otherValue)
    {
        return $"Number not same, one is '{thisValue}' but another is '{otherValue}'";
    }

    internal static string StringNotSameMessageTemplate(string thisValue, string otherValue)
    {
        return $"String content not same, one is '{thisValue}', but another is '{otherValue}'";
    }

    private EquivalentResult ObjectEquivalent(JsonInstanceElement other, JsonCollectionEqualityComparer jsonCollectionEqualityComparer)
    {
        Debug.Assert(ValueKind == JsonValueKind.Object);
        Debug.Assert(other.ValueKind == JsonValueKind.Object);

        Dictionary<string, JsonInstanceElement> otherProperties = other.EnumerateObject().ToDictionary(prop => prop.Name, prop => prop.Value);

        int thisPropertyCount = EnumerateObject().Count();
        if (thisPropertyCount != otherProperties.Count)
        {
            return EquivalentResult.Fail(() => $"Property count not same, one is {thisPropertyCount} but another is {otherProperties.Count}", _instanceLocation, other._instanceLocation);
        }

        foreach (JsonInstanceProperty thisProperty in EnumerateObject())
        {
            if (!otherProperties.TryGetValue(thisProperty.Name, out JsonInstanceElement otherPropertyValue))
            {
                return EquivalentResult.Fail(() => $"Properties not match, one has property:{thisProperty.Name} but another not", _instanceLocation, other._instanceLocation);
            }

            EquivalentResult equivalentResult = thisProperty.Value.Equivalent(otherPropertyValue, jsonCollectionEqualityComparer);
            if (!equivalentResult.Result)
            {
                return equivalentResult;
            }
        }

        return EquivalentResult.Success();
    }

    private EquivalentResult SequenceEquivalent(JsonInstanceElement other, JsonCollectionEqualityComparer jsonCollectionEqualityComparer)
    {
        return jsonCollectionEqualityComparer.Equals(this, other);
    }

    public bool Equals(JsonInstanceElement other)
    {
        return Equivalent(other, JsonCollectionEqualityComparer.Equality).Result;
    }

    public override bool Equals(object? obj)
    {
        return obj is JsonInstanceElement other && Equals(other);
    }
    
    public static bool operator ==(JsonInstanceElement left, JsonInstanceElement right)
    {
        return left.Equals(right);
    }
    
    public static bool operator !=(JsonInstanceElement left, JsonInstanceElement right)
    {
        return !left.Equals(right);
    }

    public override int GetHashCode()
    {
        throw new NotImplementedException();
    }

    public static JsonInstanceElement ParseValue(ref Utf8JsonReader reader)
    {
        return new JsonInstanceElement(JsonElement.ParseValue(ref reader), LinkedListBasedImmutableJsonPointer.Empty);
    }

    /// <summary>
    /// Writes the element to the specified writer as a JSON value.
    /// </summary>
    /// <param name="writer">The writer to which to write the element.</param>
    public void WriteTo(Utf8JsonWriter writer)
    {
        InternalJsonElement.WriteTo(writer);
    }

    /// <summary>
    /// Gets a string representation for the current value appropriate to the value type.
    /// </summary>
    public override string ToString()
    {
        return InternalJsonElement.ToString();
    }

    private class CacheHolder
    {
        public List<JsonInstanceProperty>? Properties { get; set; }
        public List<JsonInstanceElement>? ArrayItems { get; set; }
    }
}