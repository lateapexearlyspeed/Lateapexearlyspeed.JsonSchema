using System.Reflection;

namespace LateApexEarlySpeed.Nullability.Generic.UnitTests;

public class RawNullabilityAnnotationConverterTests_Array
{
    /// <summary>
    /// int[]
    /// </summary>
    [Theory]
    [MemberData(nameof(TestElements1))]
    public void TestArray1(NullabilityElement result, bool needCheckRootState)
    {
        Assert.Equal(NullabilityState.NotNull, result.State);
        Assert.True(result.HasArrayElement);
        Assert.Empty(result.GenericTypeArguments);
        Assert.Equal(NullabilityState.NotNull, result.ArrayElement.State);
        Assert.Empty(result.ArrayElement.GenericTypeArguments);
    }

    public static IEnumerable<object[]> TestElements1 => TestHelper.GenerateNullabilityElements(typeof(TestClass4), nameof(TestClass4.Property1), nameof(TestClass4.Field1), nameof(TestClass4.Func1));

    /// <summary>
    /// int[]?
    /// </summary>
    [Theory]
    [MemberData(nameof(TestElements2))]
    public void TestArray2(NullabilityElement result, bool needCheckRootState)
    {
        Assert.Equal(NullabilityState.Nullable, result.State);
        Assert.True(result.HasArrayElement);
        Assert.Empty(result.GenericTypeArguments);
        Assert.Equal(NullabilityState.NotNull, result.ArrayElement.State);
        Assert.Empty(result.ArrayElement.GenericTypeArguments);
    }

    public static IEnumerable<object[]> TestElements2 => TestHelper.GenerateNullabilityElements(typeof(TestClass4), nameof(TestClass4.Property2), nameof(TestClass4.Field2), nameof(TestClass4.Func2));

    /// <summary>
    /// int?[]?
    /// </summary>
    [Theory]
    [MemberData(nameof(TestElements3))]
    public void TestArray3(NullabilityElement result, bool needCheckRootState)
    {
        Assert.Equal(NullabilityState.Nullable, result.State);
        Assert.True(result.HasArrayElement);
        Assert.Empty(result.GenericTypeArguments);
        Assert.Equal(NullabilityState.Nullable, result.ArrayElement.State);
        Assert.Equal(NullabilityState.NotNull, Assert.Single(result.ArrayElement.GenericTypeArguments).State);
    }

    public static IEnumerable<object[]> TestElements3 => TestHelper.GenerateNullabilityElements(typeof(TestClass4), nameof(TestClass4.Property3), nameof(TestClass4.Field3), nameof(TestClass4.Func3));

    /// <summary>
    /// int?[]
    /// </summary>
    [Theory]
    [MemberData(nameof(TestElements4))]
    public void TestArray4(NullabilityElement result, bool needCheckRootState)
    {
        Assert.Equal(NullabilityState.NotNull, result.State);
        Assert.True(result.HasArrayElement);
        Assert.Empty(result.GenericTypeArguments);
        Assert.Equal(NullabilityState.Nullable, result.ArrayElement.State);
        Assert.Equal(NullabilityState.NotNull, Assert.Single(result.ArrayElement.GenericTypeArguments).State);
    }

    public static IEnumerable<object[]> TestElements4 => TestHelper.GenerateNullabilityElements(typeof(TestClass4), nameof(TestClass4.Property4), nameof(TestClass4.Field4), nameof(TestClass4.Func4));

    /// <summary>
    /// string?[]
    /// </summary>
    [Theory]
    [MemberData(nameof(TestElements5))]
    public void TestArray5(NullabilityElement result, bool needCheckRootState)
    {
        Assert.Equal(NullabilityState.NotNull, result.State);
        Assert.True(result.HasArrayElement);
        Assert.Empty(result.GenericTypeArguments);
        Assert.Equal(NullabilityState.Nullable, result.ArrayElement.State);
        Assert.Empty(result.ArrayElement.GenericTypeArguments);
    }

    public static IEnumerable<object[]> TestElements5 => TestHelper.GenerateNullabilityElements(typeof(TestClass4), nameof(TestClass4.Property5), nameof(TestClass4.Field5), nameof(TestClass4.Func5));

    /// <summary>
    /// string[]
    /// </summary>
    [Theory]
    [MemberData(nameof(TestElements6))]
    public void TestArray6(NullabilityElement result, bool needCheckRootState)
    {
        Assert.Equal(NullabilityState.NotNull, result.State);
        Assert.True(result.HasArrayElement);
        Assert.Empty(result.GenericTypeArguments);
        Assert.Equal(NullabilityState.NotNull, result.ArrayElement.State);
        Assert.Empty(result.ArrayElement.GenericTypeArguments);
    }

    public static IEnumerable<object[]> TestElements6 => TestHelper.GenerateNullabilityElements(typeof(TestClass4), nameof(TestClass4.Property6), nameof(TestClass4.Field6), nameof(TestClass4.Func6));

    /// <summary>
    /// string?[]?
    /// </summary>
    [Theory]
    [MemberData(nameof(TestElements7))]
    public void TestArray7(NullabilityElement result, bool needCheckRootState)
    {
        Assert.Equal(NullabilityState.Nullable, result.State);
        Assert.True(result.HasArrayElement);
        Assert.Empty(result.GenericTypeArguments);
        Assert.Equal(NullabilityState.Nullable, result.ArrayElement.State);
        Assert.Empty(result.ArrayElement.GenericTypeArguments);
    }

    public static IEnumerable<object[]> TestElements7 => TestHelper.GenerateNullabilityElements(typeof(TestClass4), nameof(TestClass4.Property7), nameof(TestClass4.Field7), nameof(TestClass4.Func7));

    class TestClass4
    {
        public int[] Property1 { get; set; }
        public int[] Field1;
        public int[] Func1(int[] arg) => throw new NotImplementedException();


        public int[]? Property2 { get; set; }
        public int[]? Field2;
        public int[]? Func2(int[]? arg) => throw new NotImplementedException();

        public int?[]? Property3 { get; set; }
        public int?[]? Field3;
        public int?[]? Func3(int?[]? arg) => throw new NotImplementedException();
        
        public int?[] Property4 { get; set; }
        public int?[] Field4;
        public int?[] Func4(int?[] arg) => throw new NotImplementedException();

        public string?[] Property5 { get; set; }
        public string?[] Field5;
        public string?[] Func5(string?[] arg) => throw new NotImplementedException();
        
        public string[] Property6 { get; set; }
        public string[] Field6;
        public string[] Func6(string[] arg) => throw new NotImplementedException();

        public string?[]? Property7 { get; set; }
        public string?[]? Field7;
        public string?[]? Func7(string?[]? arg) => throw new NotImplementedException();
    }
}