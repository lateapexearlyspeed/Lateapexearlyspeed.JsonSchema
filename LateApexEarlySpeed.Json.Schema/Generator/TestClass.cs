using System.Text;
using LateApexEarlySpeed.Json.Schema.Keywords;

namespace LateApexEarlySpeed.Json.Schema.Generator;

public class TestClass
{
    public int A
    {
        set { throw new NotImplementedException(); }
    }

    [IntegerEnum(1, 2, 3)]
    public SubClass B { get; } = null!;

    [UniqueItems]
    [MinLength(1)]
    public SubClass[] C { get; } = null!;

    [StringEnum("1", "2")]
    public string D { get; set; } = null!;
}

public class SubClass
{
}