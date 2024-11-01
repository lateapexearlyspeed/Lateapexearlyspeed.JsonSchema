namespace LateApexEarlySpeed.Nullability.Generic.UnitTests;

public class RawNullabilityAnnotationConverterTests_GenericTypeParameter
{
    [Theory]
    [MemberData(nameof(TestElements1))]
    public void TestGenericTypeParameter(NullabilityElement result, bool checkRootState)
    {
        if (checkRootState)
        {
            Assert.Equal(NullabilityState.NotNull, result.State);
        }
        
        Assert.False(result.HasArrayElement);
        Assert.Empty(result.GenericTypeArguments);
    }

    public static IEnumerable<object[]> TestElements1 => TestHelper.GenerateNullabilityElements(typeof(GenericClass2<,>), nameof(GenericClass2<int, int>.Property1), nameof(GenericClass2<int, int>.Field1), nameof(GenericClass2<int, int>.Func1));

    [Theory]
    [MemberData(nameof(TestElements2))]
    public void TestGenericTypeParameter2(NullabilityElement result, bool checkRootState)
    {
        Assert.Equal(NullabilityState.Nullable, result.State);
        Assert.False(result.HasArrayElement);
        Assert.Equal(NullabilityState.NotNull, Assert.Single(result.GenericTypeArguments).State);
    }

    public static IEnumerable<object[]> TestElements2 => TestHelper.GenerateNullabilityElements(typeof(GenericClass2<,>), nameof(GenericClass2<int, int>.Property2), nameof(GenericClass2<int, int>.Field2), nameof(GenericClass2<int, int>.Func2));

    [Theory]
    [MemberData(nameof(TestElements3))]
    public void TestGenericTypeParameter3(NullabilityElement result, bool checkRootState)
    {
        if (checkRootState)
        {
            Assert.Equal(NullabilityState.NotNull, result.State);
        }
        
        Assert.False(result.HasArrayElement);
        Assert.Equal(2, result.GenericTypeArguments.Length);
        Assert.Equal(NullabilityState.NotNull, result.GenericTypeArguments[0].State);
        Assert.Equal(NullabilityState.NotNull, result.GenericTypeArguments[1].State);
        Assert.Equal(NullabilityState.NotNull, Assert.Single(result.GenericTypeArguments[1].GenericTypeArguments).State);
    }

    public static IEnumerable<object[]> TestElements3 => TestHelper.GenerateNullabilityElements(typeof(GenericClass2<,>), nameof(GenericClass2<int, int>.Property3), nameof(GenericClass2<int, int>.Field3), nameof(GenericClass2<int, int>.Func3), typeof(TestBaseClass1<,>));

    class TestBaseClass1<T1, T2> : GenericClass<T1, List<T2>> where T1 : struct
    {
    }

    [Theory]
    [MemberData(nameof(TestElements4))]
    public void TestGenericTypeParameter4(NullabilityElement result, bool checkRootState)
    {
        if (checkRootState)
        {
            Assert.Equal(NullabilityState.NotNull, result.State);
        }
        
        Assert.False(result.HasArrayElement);
        Assert.Equal(2, result.GenericTypeArguments.Length);
        Assert.Equal(NullabilityState.NotNull, result.GenericTypeArguments[0].State);
        Assert.Equal(NullabilityState.NotNull, result.GenericTypeArguments[1].State);
        NullabilityElement nullableValueTypeElement = Assert.Single(result.GenericTypeArguments[1].GenericTypeArguments);
        Assert.Equal(NullabilityState.Nullable, nullableValueTypeElement.State);
        Assert.Equal(NullabilityState.NotNull, Assert.Single(nullableValueTypeElement.GenericTypeArguments).State);
    }

    public static IEnumerable<object[]> TestElements4 => TestHelper.GenerateNullabilityElements(typeof(GenericClass2<,>), nameof(GenericClass2<int, int>.Property4), nameof(GenericClass2<int, int>.Field4), nameof(GenericClass2<int, int>.Func4), typeof(TestBaseClass2<,>));

    class TestBaseClass2<T1, T2> : GenericClass<int, List<T1?>> where T1 : struct
    {
    }

    [Theory]
    [MemberData(nameof(TestElements5))]
    public void TestGenericTypeParameter5(NullabilityElement result, bool checkRootState)
    {
        if (checkRootState)
        {
            Assert.Equal(NullabilityState.NotNull, result.State);
        }

        Assert.False(result.HasArrayElement);
        Assert.Equal(2, result.GenericTypeArguments.Length);
        Assert.Equal(NullabilityState.NotNull, result.GenericTypeArguments[0].State);
        Assert.Equal(NullabilityState.Nullable, result.GenericTypeArguments[1].State);
        NullabilityElement nullableValueTypeElement = Assert.Single(result.GenericTypeArguments[1].GenericTypeArguments);
        Assert.Equal(NullabilityState.Nullable, nullableValueTypeElement.State);
        Assert.Equal(NullabilityState.NotNull, Assert.Single(nullableValueTypeElement.GenericTypeArguments).State);
    }

    public static IEnumerable<object[]> TestElements5 => TestHelper.GenerateNullabilityElements(typeof(GenericClass2<,>), nameof(GenericClass2<int, int>.Property5), nameof(GenericClass2<int, int>.Field5), nameof(GenericClass2<int, int>.Func5), typeof(TestBaseClass3<,>));

    class TestBaseClass3<T1, T2> : GenericClass<int, List<T1?>?> where T1 : struct
    {
    }

    [Theory]
    [MemberData(nameof(TestElements6))]
    public void TestGenericTypeParameter6(NullabilityElement result, bool checkRootState)
    {
        Assert.Equal(NullabilityState.NotNull, result.State);
        Assert.False(result.HasArrayElement);
        Assert.Empty(result.GenericTypeArguments);
    }

    public static IEnumerable<object[]> TestElements6 => TestHelper.GenerateNullabilityElements(typeof(GenericClass2<,>), nameof(GenericClass2<int, int>.Property6), nameof(GenericClass2<int, int>.Field6), nameof(GenericClass2<int, int>.Func6));

    [Theory]
    [MemberData(nameof(TestElements7))]
    public void TestGenericTypeParameter7(NullabilityElement result, bool checkRootState)
    {
        Assert.Equal(NullabilityState.Nullable, result.State);
        Assert.False(result.HasArrayElement);
        Assert.Empty(result.GenericTypeArguments);
    }

    public static IEnumerable<object[]> TestElements7 => TestHelper.GenerateNullabilityElements(typeof(GenericClass2<,>), nameof(GenericClass2<int, int>.Property7), nameof(GenericClass2<int, int>.Field7), nameof(GenericClass2<int, int>.Func7));

    [Theory]
    [MemberData(nameof(TestElements8))]
    public void TestGenericTypeParameter8(NullabilityElement result, bool checkRootState)
    {
        if (checkRootState)
        {
            Assert.Equal(NullabilityState.NotNull, result.State);
        }

        Assert.False(result.HasArrayElement);
        Assert.Equal(2, result.GenericTypeArguments.Length);
        Assert.Equal(NullabilityState.NotNull, result.GenericTypeArguments[0].State);
        Assert.Equal(NullabilityState.NotNull, result.GenericTypeArguments[1].State);
        Assert.Equal(NullabilityState.Nullable, Assert.Single(result.GenericTypeArguments[1].GenericTypeArguments).State);
    }

    public static IEnumerable<object[]> TestElements8 => TestHelper.GenerateNullabilityElements(typeof(GenericClass2<,>), nameof(GenericClass2<int, int>.Property8), nameof(GenericClass2<int, int>.Field8), nameof(GenericClass2<int, int>.Func8), typeof(TestBaseClass4<,>));

    class TestBaseClass4<T1, T2> : GenericClass<T1, List<T2?>> where T1 : struct
    {
    }

    class GenericClass2<T1, T2> where T1 : struct
    {
        public T1 Property1 { get; set; }
        public T1 Field1;
        public T1 Func1(T1 arg) => throw new NotImplementedException();

        public T1? Property2 { get; set; }
        public T1? Field2;
        public T1? Func2(T1? arg) => throw new NotImplementedException();

        public GenericClass<T1, List<T2>> Property3 { get; set; }
        public GenericClass<T1, List<T2>> Field3;
        public GenericClass<T1, List<T2>> Func3(GenericClass<T1, List<T2>> arg) => throw new NotImplementedException();
        
        public GenericClass<int, List<T1?>> Property4 { get; set; }
        public GenericClass<int, List<T1?>> Field4;
        public GenericClass<int, List<T1?>> Func4(GenericClass<int, List<T1?>> arg) => throw new NotImplementedException();
        
        public GenericClass<int, List<T1?>?> Property5 { get; set; }
        public GenericClass<int, List<T1?>?> Field5;
        public GenericClass<int, List<T1?>?> Func5(GenericClass<int, List<T1?>?> arg) => throw new NotImplementedException();

        public T2 Property6 { get; set; }
        public T2 Field6;
        public T2 Func6(T2 arg) => throw new NotImplementedException();

        public T2? Property7 { get; set; }
        public T2? Field7;
        public T2? Func7(T2? arg) => throw new NotImplementedException();

        public GenericClass<T1, List<T2?>> Property8 { get; set; }
        public GenericClass<T1, List<T2?>> Field8;
        public GenericClass<T1, List<T2?>> Func8(GenericClass<T1, List<T2?>> arg) => throw new NotImplementedException();
    }

    class GenericClass<T1, T2>
    {
    }
}