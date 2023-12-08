using System;

namespace LateApexEarlySpeed.Json.Schema.Keywords;

[AttributeUsage(AttributeTargets.Class)]
public sealed class KeywordAttribute : Attribute
{
    public string Name { get; }

    public KeywordAttribute(string name)
    {
        Name = name;
    }
}