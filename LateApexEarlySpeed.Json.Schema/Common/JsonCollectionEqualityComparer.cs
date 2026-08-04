using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text.Json;
using LateApexEarlySpeed.Json.Schema.JInstance;

namespace LateApexEarlySpeed.Json.Schema.Common;

/// <summary>
/// Determines the JSON arrays comparison used to compare JSON arrays, such as ordered and orderless comparison.
/// </summary>
public abstract class JsonCollectionEqualityComparer
{
    /// <summary>
    /// Gets the <see cref="JsonCollectionEqualityComparer"/> for ordered JSON arrays comparison
    /// </summary>
    public static JsonCollectionEqualityComparer Equality { get; } = new OrderedJsonCollectionComparer();

    /// <summary>
    /// Gets the <see cref="JsonCollectionEqualityComparer"/> for orderless JSON arrays comparison
    /// </summary>
    public static JsonCollectionEqualityComparer Equivalence { get; } = new OrderlessJsonCollectionComparer();

    protected internal abstract EquivalentResult Equals(JsonInstanceElement jsonArray1, JsonInstanceElement jsonArray2, StringComparison stringComparison);

    internal virtual EquivalentResult Equals(JsonInstanceElement jsonArray1, JsonInstanceElement jsonArray2, StringComparison stringComparison, ComparisonDetail comparisonDetail)
    {
        return Equals(jsonArray1, jsonArray2, stringComparison);
    }
}

internal class OrderedJsonCollectionComparer : JsonCollectionEqualityComparer
{
    protected internal override EquivalentResult Equals(JsonInstanceElement jsonArray1, JsonInstanceElement jsonArray2, StringComparison stringComparison)
    {
        return Equals(jsonArray1, jsonArray2, stringComparison, ComparisonDetail.IncludeFailureDetails);
    }

    internal override EquivalentResult Equals(JsonInstanceElement jsonArray1, JsonInstanceElement jsonArray2, StringComparison stringComparison, ComparisonDetail comparisonDetail)
    {
        Debug.Assert(jsonArray1.ValueKind == JsonValueKind.Array);
        Debug.Assert(jsonArray2.ValueKind == JsonValueKind.Array);

        int arrayLength1 = jsonArray1.GetArrayLength();
        int arrayLength2 = jsonArray2.GetArrayLength();

        if (arrayLength1 != arrayLength2)
        {
            return comparisonDetail == ComparisonDetail.ResultOnly
                ? EquivalentResult.FailWithoutDetails()
                : CreateArrayLengthMismatchResult(arrayLength1, arrayLength2, jsonArray1.Location, jsonArray2.Location);
        }

        using (JsonInstanceElement.InstanceArrayEnumerable.InstanceArrayEnumerator enumerator1 = jsonArray1.EnumerateArray().GetEnumerator())
        using (JsonInstanceElement.InstanceArrayEnumerable.InstanceArrayEnumerator enumerator2 = jsonArray2.EnumerateArray().GetEnumerator())
        {
            while (enumerator1.MoveNext())
            {
                bool hasElement = enumerator2.MoveNext();
                Debug.Assert(hasElement);

                EquivalentResult equivalentResult = enumerator1.Current.Equivalent(enumerator2.Current, this, stringComparison, comparisonDetail);

                if (!equivalentResult.Result)
                {
                    return equivalentResult;
                }
            }
        }

        return EquivalentResult.Success();
    }

    private static EquivalentResult CreateArrayLengthMismatchResult(int arrayLength1, int arrayLength2, LinkedListBasedImmutableJsonPointer location1, LinkedListBasedImmutableJsonPointer location2)
    {
        return EquivalentResult.Fail(() => $"Array length not same, one is {arrayLength1} but another is {arrayLength2}", location1, location2);
    }
}

internal class OrderlessJsonCollectionComparer : JsonCollectionEqualityComparer
{
    protected internal override EquivalentResult Equals(JsonInstanceElement jsonArray1, JsonInstanceElement jsonArray2, StringComparison stringComparison)
    {
        return Equals(jsonArray1, jsonArray2, stringComparison, ComparisonDetail.IncludeFailureDetails);
    }

    internal override EquivalentResult Equals(JsonInstanceElement jsonArray1, JsonInstanceElement jsonArray2, StringComparison stringComparison, ComparisonDetail comparisonDetail)
    {
        Debug.Assert(jsonArray1.ValueKind == JsonValueKind.Array);
        Debug.Assert(jsonArray2.ValueKind == JsonValueKind.Array);

        int arrayLength1 = jsonArray1.GetArrayLength();

        JsonInstanceElement[] tmpJsonArray2 = jsonArray2.ToArray();
        int arrayLength2 = tmpJsonArray2.Length;

        if (arrayLength1 != arrayLength2)
        {
            return comparisonDetail == ComparisonDetail.ResultOnly
                ? EquivalentResult.FailWithoutDetails()
                : CreateArrayLengthMismatchResult(arrayLength1, arrayLength2, jsonArray1.Location, jsonArray2.Location);
        }

        int startIdx = 0;
        foreach (JsonInstanceElement elementOfArray1 in jsonArray1.EnumerateArray())
        {
            Debug.Assert(startIdx < tmpJsonArray2.Length); // the 'equivalentResult' will always be assigned inside 'for' block below
            Unsafe.SkipInit(out EquivalentResult equivalentResult);

            for (int i = startIdx; i < tmpJsonArray2.Length; i++)
            {
                equivalentResult = elementOfArray1.Equivalent(tmpJsonArray2[i], this, stringComparison, comparisonDetail);

                if (equivalentResult.Result)
                {
                    if (i != startIdx)
                    {
                        ref JsonInstanceElement startElementRef = ref tmpJsonArray2[startIdx];
                        ref JsonInstanceElement foundElementRef = ref tmpJsonArray2[i];
                        (startElementRef, foundElementRef) = (foundElementRef, startElementRef);
                    }

                    break;
                }
            }

            if (!equivalentResult.Result)
            {
                return equivalentResult;
            }

            startIdx++;
        }

        return EquivalentResult.Success();
    }

    private static EquivalentResult CreateArrayLengthMismatchResult(int arrayLength1, int arrayLength2, LinkedListBasedImmutableJsonPointer location1, LinkedListBasedImmutableJsonPointer location2)
    {
        return EquivalentResult.Fail(() => $"Array length not same, one is {arrayLength1} but another is {arrayLength2}", location1, location2);
    }
}