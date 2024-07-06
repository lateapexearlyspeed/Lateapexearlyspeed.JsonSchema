using System.Diagnostics;
using System.Globalization;
using System.Text.Json;
using LateApexEarlySpeed.Json.Schema.Common;

namespace LateApexEarlySpeed.Json.Schema.JInstance;

public readonly struct JsonInstanceElement : IEquatable<JsonInstanceElement>
{
    private readonly CacheHolder _cacheHolder;

    private readonly ImmutableJsonPointer _instanceLocation;

    internal JsonElement InternalJsonElement { get; }

    public JsonInstanceElement(JsonElement jsonElement, ImmutableJsonPointer instanceLocation)
    {
        InternalJsonElement = jsonElement;
        _instanceLocation = instanceLocation;
        _cacheHolder = new CacheHolder();
    }

    public IEnumerable<JsonInstanceProperty> EnumerateObject()
    {
        return _cacheHolder.ObjectProperties ??= EnumerateObjectInternal().ToArray();
    }

    private IEnumerable<JsonInstanceProperty> EnumerateObjectInternal()
    {
        JsonElement.ObjectEnumerator objectEnumerator = InternalJsonElement.EnumerateObject();
        foreach (JsonProperty jsonProperty in objectEnumerator)
        {
            yield return new JsonInstanceProperty(jsonProperty, _instanceLocation);
        }
    }

    public IEnumerable<JsonInstanceElement> EnumerateArray()
    {
        return _cacheHolder.ArrayItems ??= EnumerateArrayInternal().ToArray();
    }

    private IEnumerable<JsonInstanceElement> EnumerateArrayInternal()
    {
        int idx = 0;

        foreach (JsonElement item in InternalJsonElement.EnumerateArray())
        {
            yield return new JsonInstanceElement(item, _instanceLocation.Add(idx++));
        }
    }

    public ImmutableJsonPointer Location => _instanceLocation;

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

    public EquivalentResult Equivalent(JsonInstanceElement other) => Equivalent(other, true);

    private EquivalentResult Equivalent(JsonInstanceElement other, bool generateDetailedMessage)
    {
        if (ValueKind != other.ValueKind)
        {
            return EquivalentResult.Fail(generateDetailedMessage 
                ? $"Json kind not same, one is {ValueKind}, but another is {other.ValueKind}"
                : string.Empty, 
                _instanceLocation, other._instanceLocation);
        }

        switch (ValueKind)
        {
            case JsonValueKind.True:
            case JsonValueKind.False:
            case JsonValueKind.Null:
                return EquivalentResult.Success();

            case JsonValueKind.String:

                string curString = GetString()!;
                string otherString = other.GetString()!;

                return curString == otherString
                    ? EquivalentResult.Success()
                    : EquivalentResult.Fail(generateDetailedMessage ? StringNotSameMessageTemplate(curString, otherString) : string.Empty, 
                        _instanceLocation, other._instanceLocation);

            case JsonValueKind.Number:
                return NumberEquivalent(other, generateDetailedMessage);

            case JsonValueKind.Array:
                return SequenceEquivalent(other, generateDetailedMessage);

            case JsonValueKind.Object:
                return ObjectEquivalent(other, generateDetailedMessage);

            default:
                Debug.Fail("Should not go to this default block, because all JsonValueKinds should already be handled.");
                throw new NotImplementedException("Should not go here, because all JsonValueKinds should already be handled.");
        }
    }

    private EquivalentResult NumberEquivalent(JsonInstanceElement other, bool generateDetailedMessage)
    {
        if (InternalJsonElement.TryGetInt64(out long thisLongValue))
        {
            // To compatible with json schema const definition, when failed to convert to long type directly, we need to consider zero decimal case.
            if (other.InternalJsonElement.TryGetInt64(out long otherLongValue) || other.TryGetInt64ForJsonSchema(out otherLongValue))
            {
                return thisLongValue == otherLongValue
                    ? EquivalentResult.Success()
                    : EquivalentResult.Fail(GenerateNumberNotSameMessage(thisLongValue, otherLongValue, generateDetailedMessage), _instanceLocation, other._instanceLocation);
            }

            other.GetNumericValue(out double? doubleValue, out long? longValue, out ulong? ulongValue);

            Debug.Assert(!longValue.HasValue);

            return EquivalentResult.Fail(GenerateNumberNotSameMessage(thisLongValue, doubleValue.HasValue ? doubleValue.Value.ToString(CultureInfo.InvariantCulture) : ulongValue.GetValueOrDefault().ToString(), generateDetailedMessage), _instanceLocation, other._instanceLocation);
        }

        if (InternalJsonElement.TryGetUInt64(out ulong thisULongValue))
        {
            // To compatible with json schema const definition, when failed to convert to ulong type directly, we need to consider zero decimal case.
            if (other.InternalJsonElement.TryGetUInt64(out ulong otherULongValue) || other.TryGetUInt64ForJsonSchema(out otherULongValue))
            {
                return thisULongValue == otherULongValue
                    ? EquivalentResult.Success()
                    : EquivalentResult.Fail(GenerateNumberNotSameMessage(thisULongValue, otherULongValue, generateDetailedMessage), _instanceLocation, other._instanceLocation);
            }

            other.GetNumericValue(out double? doubleValue, out long? longValue, out ulong? ulongValue);

            Debug.Assert(!ulongValue.HasValue);

            return EquivalentResult.Fail(GenerateNumberNotSameMessage(thisULongValue, doubleValue.HasValue ? doubleValue.Value.ToString(CultureInfo.InvariantCulture) : longValue.GetValueOrDefault(), generateDetailedMessage), _instanceLocation, other._instanceLocation);
        }

        if (InternalJsonElement.TryGetDecimal(out decimal thisDecimalValue))
        {
            if (other.TryGetDecimal(out decimal otherDecimalValue))
            {
                const decimal tolerance = 0.0000000000000000000000001m;

                decimal actualTolerance = Math.Abs(thisDecimalValue) * tolerance;

                return Math.Abs(thisDecimalValue - otherDecimalValue) <= actualTolerance
                    ? EquivalentResult.Success()
                    : EquivalentResult.Fail(GenerateNumberNotSameMessage(thisDecimalValue, otherDecimalValue, generateDetailedMessage), _instanceLocation, other._instanceLocation);
            }

            double otherDoubleValue = other.GetDouble();

            return EquivalentResult.Fail(GenerateNumberNotSameMessage(thisDecimalValue, otherDoubleValue, generateDetailedMessage), _instanceLocation, other._instanceLocation);
        }

        if (InternalJsonElement.TryGetDouble(out double thisDoubleValue))
        {
            const double tolerance = 0.000001;
            
            double otherDoubleValue = other.GetDouble();

            double actualTolerance = Math.Abs(thisDoubleValue) * tolerance;

            return Math.Abs(thisDoubleValue - otherDoubleValue) <= actualTolerance
                ? EquivalentResult.Success()
                : EquivalentResult.Fail(GenerateNumberNotSameMessage(thisDoubleValue, otherDoubleValue, generateDetailedMessage), _instanceLocation, other._instanceLocation);
        }

        Debug.Fail("Should not go here, have considered all numeric types. Missed any other types ?");
        throw new NotImplementedException("Should have considered all numeric types. Missed any other types ?");

        static string GenerateNumberNotSameMessage(object thisValue, object otherValue, bool generateDetailedMessage)
        {
            return generateDetailedMessage 
                ? NumberNotSameMessageTemplate(thisValue, otherValue)
                : string.Empty;
        }
    }

    internal static string NumberNotSameMessageTemplate(object thisValue, object otherValue)
    {
        return $"Number not same, one is '{thisValue}' but another is '{otherValue}'";
    }

    internal static string StringNotSameMessageTemplate(string thisValue, string otherValue)
    {
        return $"String content not same, one is '{thisValue}', but another is '{otherValue}'";
    }

    private EquivalentResult ObjectEquivalent(JsonInstanceElement other, bool generateDetailedMessage)
    {
        Debug.Assert(ValueKind == JsonValueKind.Object);
        Debug.Assert(other.ValueKind == JsonValueKind.Object);

        Dictionary<string, JsonInstanceElement> otherProperties = other.EnumerateObject().ToDictionary(prop => prop.Name, prop => prop.Value);

        int thisPropertyCount = EnumerateObject().Count();
        if (thisPropertyCount != otherProperties.Count)
        {
            return EquivalentResult.Fail(generateDetailedMessage ? $"Property count not same, one is {thisPropertyCount} but another is {otherProperties.Count}" : string.Empty, 
                _instanceLocation, other._instanceLocation);
        }

        foreach (JsonInstanceProperty thisProperty in EnumerateObject())
        {
            if (!otherProperties.TryGetValue(thisProperty.Name, out JsonInstanceElement otherPropertyValue))
            {
                return EquivalentResult.Fail(generateDetailedMessage ? $"Properties not match, one has property:{thisProperty.Name} but another not" : string.Empty,
                    _instanceLocation, other._instanceLocation);
            }

            EquivalentResult equivalentResult = thisProperty.Value.Equivalent(otherPropertyValue, generateDetailedMessage);
            if (!equivalentResult.Result)
            {
                return equivalentResult;
            }
        }

        return EquivalentResult.Success();
    }

    private EquivalentResult SequenceEquivalent(JsonInstanceElement other, bool generateDetailedMessage)
    {
        Debug.Assert(ValueKind == JsonValueKind.Array);
        Debug.Assert(other.ValueKind == JsonValueKind.Array);

        int thisCount = EnumerateArray().Count();
        int otherCount = other.EnumerateArray().Count();

        if (thisCount != otherCount)
        {
            return EquivalentResult.Fail(generateDetailedMessage 
                ? $"Array length not same, one is {thisCount} but another is {otherCount}" : string.Empty, 
                _instanceLocation, other._instanceLocation);
        }

        using (IEnumerator<JsonInstanceElement> thisEnumerator = EnumerateArray().GetEnumerator())
        using (IEnumerator<JsonInstanceElement> otherEnumerator = other.EnumerateArray().GetEnumerator())
        {
            while (thisEnumerator.MoveNext())
            {
                bool otherMoveNext = otherEnumerator.MoveNext();
                Debug.Assert(otherMoveNext);

                EquivalentResult elementEquivalentResult = thisEnumerator.Current.Equivalent(otherEnumerator.Current, generateDetailedMessage);

                if (!elementEquivalentResult.Result)
                {
                    return elementEquivalentResult;
                }
            }
        }

        return EquivalentResult.Success();
    }

    public bool Equals(JsonInstanceElement other)
    {
        return Equivalent(other, false).Result;
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
        return new JsonInstanceElement(JsonElement.ParseValue(ref reader), ImmutableJsonPointer.Empty);
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
        public JsonInstanceProperty[]? ObjectProperties;
        public JsonInstanceElement[]? ArrayItems;
    }
}
