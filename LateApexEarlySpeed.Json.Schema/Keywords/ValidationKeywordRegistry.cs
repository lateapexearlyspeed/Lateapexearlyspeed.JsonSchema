using System.Collections;
using LateApexEarlySpeed.Json.Schema.Keywords.Draft7;
using System.Diagnostics.CodeAnalysis;
using LateApexEarlySpeed.Json.Schema.Misc;

namespace LateApexEarlySpeed.Json.Schema.Keywords;

internal class StringKeyedDictionary<TValue> where TValue : notnull
{
    private const int BucketCount = 100;

    private readonly KeyValuePair<string, TValue>[]?[] _buckets = new KeyValuePair<string, TValue>[BucketCount][];

    public bool TryGetValue(ReadOnlySpan<char> key, [NotNullWhen(true)] out TValue? value)
    {
        int hashCode = Marvin.GetHashCode(key);

        int bucketIdx = (hashCode & 0x7F_FF_FF_FF) % BucketCount;

        KeyValuePair<string, TValue>[]? bucket = _buckets[bucketIdx];

        if (bucket is not null)
        {
            foreach (KeyValuePair<string, TValue> kv in bucket)
            {
                if (key.Equals(kv.Key, StringComparison.Ordinal))
                {
                    value = kv.Value;
                    return true;
                }
            }
        }

        value = default;
        return false;
    }

    public TValue this[ReadOnlySpan<char> key]
    {
        set
        {
            int hashCode = Marvin.GetHashCode(key);

            int bucketIdx = (hashCode & 0x7F_FF_FF_FF) % BucketCount;

            KeyValuePair<string, TValue>[]? previousBucket = _buckets[bucketIdx];

            var newEntry = new KeyValuePair<string, TValue>(key.ToString(), value);

            if (previousBucket is null)
            {
                _buckets[bucketIdx] = new[] { newEntry };

                return;
            }

            for (var i = 0; i < previousBucket.Length; i++)
            {
                if (key.Equals(previousBucket[i].Key, StringComparison.Ordinal))
                {
                    previousBucket[i] = newEntry;

                    return;
                }
            }

            // need to add new entry into existing bucket now
            KeyValuePair<string, TValue>[] newBucket = new KeyValuePair<string, TValue>[previousBucket.Length + 1];
            previousBucket.CopyTo(newBucket, 0);
            newBucket[newBucket.Length - 1] = newEntry;

            _buckets[bucketIdx] = newBucket;
        }
    }

    public TValue? GetValueOrDefault(ReadOnlySpan<char> key)
    {
        return TryGetValue(key, out TValue? value) ? value : default;
    }
}

/// <remarks>
/// The only reason to implement interface <see cref="IEnumerable{T}"/> is to be able to provide collection initializer syntax.
/// </remarks>
internal class StringKeyedHashSet : IEnumerable<string>
{
    private const int BucketCount = 100;

    private readonly string[]?[] _buckets = new string[BucketCount][];

    public bool Contains(ReadOnlySpan<char> item)
    {
        int hashCode = Marvin.GetHashCode(item);

        int bucketIdx = (hashCode & 0x7F_FF_FF_FF) % BucketCount;

        string[]? bucket = _buckets[bucketIdx];

        if (bucket is not null)
        {
            foreach (string key in bucket)
            {
                if (item.Equals(key, StringComparison.Ordinal))
                {
                    return true;
                }
            }
        }

        return false;
    }

    public void Add(ReadOnlySpan<char> item)
    {
        int hashCode = Marvin.GetHashCode(item);

        int bucketIdx = (hashCode & 0x7F_FF_FF_FF) % BucketCount;

        string[]? previousBucket = _buckets[bucketIdx];

        if (previousBucket is null)
        {
            _buckets[bucketIdx] = new[] { item.ToString() };

            return;
        }

        // Check if item already exists
        foreach (string bucketItem in previousBucket)
        {
            if (item.Equals(bucketItem, StringComparison.Ordinal))
            {
                return; // Item already exists, no need to add
            }
        }

        // Add new item into existing bucket
        string[] newBucket = new string[previousBucket.Length + 1];
        previousBucket.CopyTo(newBucket, 0);
        newBucket[newBucket.Length - 1] = item.ToString();

        _buckets[bucketIdx] = newBucket;
    }

    public IEnumerator<string> GetEnumerator()
    {
        throw new NotSupportedException();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}

public static class ValidationKeywordRegistry
{
    private static readonly StringKeyedDictionary<IDialectKeywordRegistry> KeywordsDictionary = new();

    private static readonly StringKeyedHashSet IgnoredKeywordNames = new() { "$comment", "$vocabulary", "contentEncoding", "contentMediaType", "contentSchema", "default", "deprecated", "description", "examples", "readOnly", "title", "writeOnly" };

    static ValidationKeywordRegistry()
    {
        var builtInKeywordTypes = new[] 
            { 
                typeof(AdditionalItemsKeyword),
                typeof(AdditionalPropertiesKeyword),
                typeof(AllOfKeyword),
                typeof(AnyOfKeyword),
                typeof(ConstKeyword),
                typeof(DependenciesKeyword),
                typeof(DependentRequiredKeyword),
                typeof(DependentSchemasKeyword),
                typeof(EnumKeyword),
                typeof(ExclusiveMaximumKeyword),
                typeof(ExclusiveMinimumKeyword),
                typeof(FormatKeyword),
                typeof(ItemsDraft7Keyword),
                typeof(ItemsKeyword),
                typeof(MaximumKeyword),
                typeof(MaxItemsKeyword),
                typeof(MaxLengthKeyword),
                typeof(MaxPropertiesKeyword),
                typeof(MinimumKeyword),
                typeof(MinItemsKeyword),
                typeof(MinLengthKeyword),
                typeof(MinPropertiesKeyword),
                typeof(MultipleOfKeyword),
                typeof(NotKeyword),
                typeof(OneOfKeyword),
                typeof(PatternKeyword),
                typeof(PatternPropertiesKeyword),
                typeof(PrefixItemsKeyword),
                typeof(PropertiesKeyword),
                typeof(PropertyNamesKeyword),
                typeof(RequiredKeyword),
                typeof(TypeKeyword),
                typeof(UniqueItemsKeyword)
            };

        foreach (Type builtInKeywordType in builtInKeywordTypes)
        {
            AddKeyword(builtInKeywordType);
        }
    }

    /// <summary>
    /// Add new keyword type <typeparamref name="TKeyword"/> to <see cref="ValidationKeywordRegistry"/>
    /// </summary>
    /// <typeparam name="TKeyword">New keyword type to be added</typeparam>
    /// <exception cref="ArgumentException">A keyword type with the same keyword name and dialect already exists in the <see cref="ValidationKeywordRegistry"/></exception>
    public static void AddKeyword<TKeyword>() where TKeyword : KeywordBase
    {
        AddKeyword(typeof(TKeyword));
    }

    private static void AddKeyword(Type keywordType)
    {
        ReadOnlySpan<char> keywordName = KeywordBase.GetKeywordName(keywordType);
        DialectKind[] dialects = KeywordBase.GetKeywordDialects(keywordType);

        DialectKeywordRegistry dialectKeywordRegistry;
        if (KeywordsDictionary.TryGetValue(keywordName, out IDialectKeywordRegistry? dialectKeywords))
        {
            if (dialectKeywords is GlobalDialectKeywordRegistry)
            {
                throw new ArgumentException($"A keyword type with the same keyword name and dialect has already been added. Keyword type: {keywordType}", nameof(keywordType));
            }

            dialectKeywordRegistry = (DialectKeywordRegistry)dialectKeywords;
        }
        else
        {
            if (dialects.Length == SupportedDialectsCount) // current keyword supports all dialects
            {
                KeywordsDictionary[keywordName] = new GlobalDialectKeywordRegistry(keywordType);
                return;
            }

            dialectKeywordRegistry = new DialectKeywordRegistry();
            KeywordsDictionary[keywordName] = dialectKeywordRegistry;
        }

        foreach (DialectKind dialect in dialects)
        {
            dialectKeywordRegistry.Add(dialect, keywordType);
        }
    }

    /// <summary>
    /// Set new keyword type <typeparamref name="TKeyword"/> to <see cref="ValidationKeywordRegistry"/>.
    /// If specified keyword name and dialect does not exist, it is added; otherwise it is updated with new keyword type.
    /// </summary>
    /// <typeparam name="TKeyword">New keyword type to be set</typeparam>
    public static void SetKeyword<TKeyword>() where TKeyword : KeywordBase
    {
        Type keywordType = typeof(TKeyword);
        
        ReadOnlySpan<char> keywordName = KeywordBase.GetKeywordName(keywordType);
        DialectKind[] dialects = KeywordBase.GetKeywordDialects(keywordType);

        if (dialects.Length == SupportedDialectsCount) // current keyword supports all dialects
        {
            KeywordsDictionary[keywordName] = new GlobalDialectKeywordRegistry(keywordType);
            return;
        }

        DialectKeywordRegistry dialectKeywordRegistry;
        if (KeywordsDictionary.TryGetValue(keywordName, out IDialectKeywordRegistry? dialectKeywords))
        {
            if (dialectKeywords is GlobalDialectKeywordRegistry globalDialectKeywordRegistry)
            {
                dialectKeywordRegistry = globalDialectKeywordRegistry.ToDialectKeywordRegistry();
                KeywordsDictionary[keywordName] = dialectKeywordRegistry;
            }
            else
            {
                dialectKeywordRegistry = (DialectKeywordRegistry)dialectKeywords;
            }
        }
        else
        {
            dialectKeywordRegistry = new DialectKeywordRegistry();
            KeywordsDictionary[keywordName] = dialectKeywordRegistry;
        }

        foreach (DialectKind dialect in dialects)
        {
            dialectKeywordRegistry[dialect] = keywordType;
        }
    }

    internal static int SupportedDialectsCount => Enum.GetNames(typeof(DialectKind)).Length;

    public static Type? GetKeyword(ReadOnlySpan<char> keywordName, DialectKind dialectKind)
    {
        return KeywordsDictionary.GetValueOrDefault(keywordName)?[dialectKind];
    }

    internal static bool IsIgnoredKeyword(ReadOnlySpan<char> keywordName)
    {
        return IgnoredKeywordNames.Contains(keywordName);
    }

    private interface IDialectKeywordRegistry
    {
        Type? this[DialectKind dialect] { get; }
    }

    private class DialectKeywordRegistry : IDialectKeywordRegistry
    {
        private readonly Type?[] _dialectKeywords = new Type?[SupportedDialectsCount];

        [DisallowNull]
        public Type? this[DialectKind dialect]
        {
            get => _dialectKeywords[(int)dialect];
            set => _dialectKeywords[(int)dialect] = value;
        }

        public void Add(DialectKind dialect, Type type)
        {
            if (_dialectKeywords[(int)dialect] is not null)
            {
                throw new ArgumentException($"A keyword type with the same dialect has already been added. Dialect: {dialect}, keyword type: {type}", nameof(dialect));
            }

            _dialectKeywords[(int)dialect] = type;
        }
    }

    private class GlobalDialectKeywordRegistry : IDialectKeywordRegistry
    {
        private readonly Type _keywordType;

        public GlobalDialectKeywordRegistry(Type keywordType)
        {
            _keywordType = keywordType;
        }

        public Type this[DialectKind dialect] => _keywordType;

        public DialectKeywordRegistry ToDialectKeywordRegistry()
        {
            var dialectKeywordRegistry = new DialectKeywordRegistry();

            foreach (DialectKind dialect in typeof(DialectKind).GetEnumValues())
            {
                dialectKeywordRegistry[dialect] = _keywordType;
            }

            return dialectKeywordRegistry;
        }
    }
}

/// <summary>
/// Do not change the order and underlying number of the enum members unless you know what you should change correspondingly!
/// Important dialect related calculations depend on it.
/// </summary>
public enum DialectKind
{
    Draft202012,
    Draft201909,
    Draft7
}