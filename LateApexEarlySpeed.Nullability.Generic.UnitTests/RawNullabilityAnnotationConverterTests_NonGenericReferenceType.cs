using System.Reflection;
using LateApexEarlySpeed.Nullability.Generic.RawNullabilityAnnotation;

namespace LateApexEarlySpeed.Nullability.Generic.UnitTests;

public class RawNullabilityAnnotationConverterTests_NonGenericReferenceType
{
    /// <summary>
    /// string
    /// </summary>
    [Theory]
    [MemberData(nameof(TestElements1))]
    public void Test1(NullabilityElement nullabilityElement, bool needCheckRootState)
    {
        Assert.Equal(NullabilityState.NotNull, nullabilityElement.State);
        
        Assert.False(nullabilityElement.HasArrayElement);
        Assert.Empty(nullabilityElement.GenericTypeArguments);
    }

    public static IEnumerable<object[]> TestElements1 => TestHelper.GenerateNullabilityElements(typeof(TestClass1), nameof(TestClass1.Property1), nameof(TestClass1.Field1), nameof(TestClass1.Func));

    /// <summary>
    /// string?
    /// </summary>
    [Theory]
    [MemberData(nameof(TestElements2))]
    public void Test2(NullabilityElement nullabilityElement, bool needCheckRootState)
    {
        Assert.Equal(NullabilityState.Nullable, nullabilityElement.State);

        Assert.False(nullabilityElement.HasArrayElement);
        Assert.Empty(nullabilityElement.GenericTypeArguments);
    }

    public static IEnumerable<object[]> TestElements2 => TestHelper.GenerateNullabilityElements(typeof(TestClass1), nameof(TestClass1.Property2), nameof(TestClass1.Field2), nameof(TestClass1.Func2));

    class TestClass1
    {
        public string Property1 { get; set; } = null!;
        public string? Property2 { get; set; }
        public string Field1;
        public string? Field2;
        public string Func(string arg) => throw new NotImplementedException();
        public string? Func2(string? arg) => throw new NotImplementedException();
    }
}