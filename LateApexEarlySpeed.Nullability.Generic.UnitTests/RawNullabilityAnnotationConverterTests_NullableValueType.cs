namespace LateApexEarlySpeed.Nullability.Generic.UnitTests;

public class RawNullabilityAnnotationConverterTests_NullableValueType
{
    [Theory]
    [MemberData(nameof(TestElements1))]
    public void TestNullableValueType(NullabilityElement result, bool checkRootState)
    {
        Assert.Equal(NullabilityState.Nullable, result.State);
        Assert.False(result.HasArrayElement);
        NullabilityElement underlyingElement = Assert.Single(result.GenericTypeArguments);
        Assert.Equal(NullabilityState.NotNull, underlyingElement.State);
        Assert.Empty(underlyingElement.GenericTypeArguments);
    }

    public static IEnumerable<object[]> TestElements1 => TestHelper.GenerateNullabilityElements(typeof(TestClass2), nameof(TestClass2.Property), nameof(TestClass2.Field), nameof(TestClass2.Func));

    class TestClass2
    {
        public int? Property { get; set; }
        public int? Field;
        public int? Func(int? arg) => throw new NotImplementedException();
    }
}