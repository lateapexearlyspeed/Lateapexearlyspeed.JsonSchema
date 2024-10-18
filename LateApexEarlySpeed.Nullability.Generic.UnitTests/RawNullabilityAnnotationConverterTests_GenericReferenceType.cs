using System.Reflection;

namespace LateApexEarlySpeed.Nullability.Generic.UnitTests;

public class RawNullabilityAnnotationConverterTests_GenericReferenceType
{
    [Theory]
    [MemberData(nameof(TestElements1))]
    public void TestGenericReferenceType(NullabilityElement result, bool checkRootState)
    {
        if (checkRootState)
        {
            Assert.Equal(NullabilityState.NotNull, result.State);
        }

        Assert.False(result.HasArrayElement);
        Assert.Equal(2, result.GenericTypeArguments.Length);

        Assert.Equal(NullabilityState.NotNull, result.GenericTypeArguments[0].State);
        Assert.Equal(NullabilityState.NotNull, result.GenericTypeArguments[1].State);
    }

    public static IEnumerable<object[]> TestElements1 => TestHelper.GenerateNullabilityElements(typeof(TestClass5), nameof(TestClass5.Property4), nameof(TestClass5.Field4), nameof(TestClass5.Func4), typeof(TestBaseClass1));

    class TestBaseClass1 : GenericClass<int, string>
    {
    }

    [Theory]
    [MemberData(nameof(TestElements2))]
    public void TestGenericReferenceType2(NullabilityElement result, bool checkRootState)
    {
        if (checkRootState)
        {
            Assert.Equal(NullabilityState.NotNull, result.State);
        }

        Assert.False(result.HasArrayElement);
        Assert.Equal(2, result.GenericTypeArguments.Length);

        Assert.Equal(NullabilityState.Nullable, result.GenericTypeArguments[0].State);
        Assert.Equal(NullabilityState.NotNull, result.GenericTypeArguments[0].GenericTypeArguments[0].State);
        Assert.Equal(NullabilityState.Nullable, result.GenericTypeArguments[1].State);
    }

    public static IEnumerable<object[]> TestElements2 => TestHelper.GenerateNullabilityElements(typeof(TestClass5), nameof(TestClass5.Property5), nameof(TestClass5.Field5), nameof(TestClass5.Func5), typeof(TestBaseClass2));

    class TestBaseClass2 : GenericClass<int?, string?>
    {
    }

    [Theory]
    [MemberData(nameof(TestElements3))]
    public void TestGenericReferenceType3(NullabilityElement result, bool checkRootState)
    {
        if (checkRootState)
        {
            Assert.Equal(NullabilityState.Nullable, result.State);
        }
        
        Assert.False(result.HasArrayElement);

        Assert.Equal(2, result.GenericTypeArguments.Length);
        Assert.Equal(NullabilityState.NotNull, result.GenericTypeArguments[0].State);
        Assert.Equal(NullabilityState.Nullable, result.GenericTypeArguments[1].State);
    }

    public static IEnumerable<object[]> TestElements3 => TestHelper.GenerateNullabilityElements(typeof(TestClass5), nameof(TestClass5.Property6), nameof(TestClass5.Field6), nameof(TestClass5.Func6), typeof(TestBaseClass3));

    class TestBaseClass3 : GenericClass<int, string?>
    {
    }

    class TestClass5
    {
        public GenericClass<int, string> Property4 { get; set; } = null!;
        public GenericClass<int, string> Field4;
        public GenericClass<int, string> Func4(GenericClass<int, string> arg) => throw new NotImplementedException();

        public GenericClass<int?, string?> Property5 { get; set; } = null!;
        public GenericClass<int?, string?> Field5;
        public GenericClass<int?, string?> Func5(GenericClass<int?, string?> arg) => throw new NotImplementedException();

        public GenericClass<int, string?>? Property6 { get; set; }
        public GenericClass<int, string?>? Field6;
        public GenericClass<int, string?>? Func6(GenericClass<int, string?>? arg) => throw new NotImplementedException();
    }

    class GenericClass<T1, T2>
    {
    }
}