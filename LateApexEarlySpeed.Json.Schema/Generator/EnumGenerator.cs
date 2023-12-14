using LateApexEarlySpeed.Json.Schema.JInstance;
using LateApexEarlySpeed.Json.Schema.Keywords;

namespace LateApexEarlySpeed.Json.Schema.Generator;

internal class EnumGenerator<T> where T : notnull
{
    private readonly T[] _allowedValues;

    public EnumGenerator(T[] allowedValues)
    {
        _allowedValues = allowedValues;
    }

    public KeywordBase CreateKeyword()
    {
        return new EnumKeyword(_allowedValues.Select(value => JsonInstanceSerializer.SerializeToElement(value)).ToList());
    }
}