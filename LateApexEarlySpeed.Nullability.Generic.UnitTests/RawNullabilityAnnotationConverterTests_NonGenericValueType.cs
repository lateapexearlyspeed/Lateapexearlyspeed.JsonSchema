namespace LateApexEarlySpeed.Nullability.Generic.UnitTests;

public class RawNullabilityAnnotationConverterTests_NonGenericValueType
{
    [Theory]
    [MemberData(nameof(TestElement))]
    public void TestNonGenericValueType(NullabilityElement result, bool needCheckRootState)
    {
        Assert.Equal(NullabilityState.NotNull, result.State);
        Assert.False(result.HasArrayElement);
        Assert.Empty(result.GenericTypeArguments);
    }

    public static IEnumerable<object[]> TestElement => TestHelper.GenerateNullabilityElements(typeof(TestClass3), nameof(TestClass3.Property), nameof(TestClass3.Field), nameof(TestClass3.Func));

    class TestClass3
    {
        public int Property { get; set; }
        public int Field;
        public int Func(int arg) => throw new NotImplementedException();
    }
}