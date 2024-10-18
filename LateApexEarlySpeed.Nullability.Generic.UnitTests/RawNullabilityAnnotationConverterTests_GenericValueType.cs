using System.Reflection;

namespace LateApexEarlySpeed.Nullability.Generic.UnitTests;

public class RawNullabilityAnnotationConverterTests_GenericValueType
{
    [Theory]
    [MemberData(nameof(TestElements1))]
    public void TestGenericValueType(NullabilityElement result, bool checkRootState)
    {
        Assert.Equal(NullabilityState.NotNull, result.State);
        Assert.False(result.HasArrayElement);
        Assert.Equal(2, result.GenericTypeArguments.Length);

        Assert.Equal(NullabilityState.NotNull, result.GenericTypeArguments[0].State);
        Assert.Equal(NullabilityState.NotNull, result.GenericTypeArguments[1].State);
    }

    public static IEnumerable<object[]> TestElements1 => TestHelper.GenerateNullabilityElements(typeof(TestClass5), nameof(TestClass5.Property1), nameof(TestClass5.Field1), nameof(TestClass5.Func1));

    [Theory]
    [MemberData(nameof(TestElements2))]
    public void TestGenericValueType2(NullabilityElement result, bool checkRootState)
    {
        Assert.Equal(NullabilityState.NotNull, result.State);
        Assert.False(result.HasArrayElement);
        Assert.Equal(2, result.GenericTypeArguments.Length);

        Assert.Equal(NullabilityState.Nullable, result.GenericTypeArguments[0].State);
        Assert.Equal(NullabilityState.NotNull, result.GenericTypeArguments[0].GenericTypeArguments[0].State);
        Assert.Equal(NullabilityState.Nullable, result.GenericTypeArguments[1].State);
    }

    public static IEnumerable<object[]> TestElements2 => TestHelper.GenerateNullabilityElements(typeof(TestClass5), nameof(TestClass5.Property2), nameof(TestClass5.Field2), nameof(TestClass5.Func2));

    [Theory]
    [MemberData(nameof(TestElements3))]
    public void TestGenericValueType3(NullabilityElement result, bool checkRootState)
    {
        Assert.Equal(NullabilityState.Nullable, result.State);
        Assert.False(result.HasArrayElement);

        NullabilityElement underlyingElement = Assert.Single(result.GenericTypeArguments);
        Assert.Equal(NullabilityState.NotNull, underlyingElement.State);

        Assert.Equal(NullabilityState.NotNull, underlyingElement.GenericTypeArguments[0].State);
        Assert.Equal(NullabilityState.Nullable, underlyingElement.GenericTypeArguments[1].State);
    }

    public static IEnumerable<object[]> TestElements3 => TestHelper.GenerateNullabilityElements(typeof(TestClass5), nameof(TestClass5.Property3), nameof(TestClass5.Field3), nameof(TestClass5.Func3));

    class TestClass5
    {
        public GenericStruct<int, string> Property1 { get; set; }
        public GenericStruct<int, string> Field1;
        public GenericStruct<int, string> Func1(GenericStruct<int, string> arg) => throw new NotImplementedException();

        public GenericStruct<int?, string?> Property2 { get; set; }
        public GenericStruct<int?, string?> Field2;
        public GenericStruct<int?, string?> Func2(GenericStruct<int?, string?> arg) => throw new NotImplementedException();

        public GenericStruct<int, string?>? Property3 { get; set; }
        public GenericStruct<int, string?>? Field3;
        public GenericStruct<int, string?>? Func3(GenericStruct<int, string?>? arg) => throw new NotImplementedException();
    }

    struct GenericStruct<T1, T2>
    {
    }
}