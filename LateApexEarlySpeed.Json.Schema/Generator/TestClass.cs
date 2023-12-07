namespace LateApexEarlySpeed.Json.Schema.Generator;

public class TestClass
{
    public int A
    {
        set { throw new NotImplementedException(); }
    }

    public SubClass B { get; } = null!;
    public SubClass C { get; } = null!;
}

public class SubClass
{
}