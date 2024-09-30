using System.Reflection;

namespace LateApexEarlySpeed.Nullability.Generic.UnitTests;

public class NullabilityTypeStateTests
{
    [Fact]
    public void GenericTypeArgumentsTest()
    {
        NullabilityType rootType = NullabilityType.GetType(typeof(TestClass));

        NullabilityPropertyInfo? propertyInfo = rootType.GetProperty(nameof(TestClass.Property));
        Assert.NotNull(propertyInfo);

        propertyInfo = propertyInfo.NullabilityPropertyType.GetProperty(nameof(GenericClass<int, int, int, int>.Property));
        Assert.NotNull(propertyInfo);

        NullabilityType type = propertyInfo.NullabilityPropertyType;

        // GenericClass<List<int>, List<int?>?, List<string>, List<string?>?>?
        Assert.Equal(NullabilityState.Nullable, type.NullabilityState);
        NullabilityType[] genericTypeArguments = type.GenericTypeArguments;
        Assert.Equal(4, genericTypeArguments.Length);

        // List<int>
        Assert.Equal(NullabilityState.NotNull, genericTypeArguments[0].NullabilityState);
        type = Assert.Single(genericTypeArguments[0].GenericTypeArguments);
        Assert.Equal(NullabilityState.NotNull, type.NullabilityState);

        // List<int?>?
        Assert.Equal(NullabilityState.Nullable, genericTypeArguments[1].NullabilityState);
        type = Assert.Single(genericTypeArguments[1].GenericTypeArguments);
        Assert.Equal(NullabilityState.Nullable, type.NullabilityState);

        // int
        NullabilityType integerType = Assert.Single(type.GenericTypeArguments);
        Assert.Equal(NullabilityState.NotNull, integerType.NullabilityState);

        // List<string>
        Assert.Equal(NullabilityState.NotNull, genericTypeArguments[2].NullabilityState);
        type = Assert.Single(genericTypeArguments[2].GenericTypeArguments);
        Assert.Equal(NullabilityState.NotNull, type.NullabilityState);

        // List<string?>?
        Assert.Equal(NullabilityState.Nullable, genericTypeArguments[3].NullabilityState);
        type = Assert.Single(genericTypeArguments[3].GenericTypeArguments);
        Assert.Equal(NullabilityState.Nullable, type.NullabilityState);
    }

    [Fact]
    public void GetArrayElementTypeTest()
    {
        NullabilityType rootType = NullabilityType.GetType(typeof(TestClass));

        NullabilityPropertyInfo? propertyInfo = rootType.GetProperty(nameof(TestClass.Property));

        Assert.NotNull(propertyInfo);

        rootType = propertyInfo.NullabilityPropertyType;

        // List<int>[]
        NullabilityType? type = rootType.GetProperty(nameof(GenericClass<int, int, int, int>.ArrayProperty0))!.NullabilityPropertyType;

        Assert.Equal(NullabilityState.NotNull, type.NullabilityState);
        // List<int>
        type = type.GetArrayElementType();
        Assert.NotNull(type);
        Assert.Equal(NullabilityState.NotNull, type.NullabilityState); 
        // int
        Assert.Equal(NullabilityState.NotNull, Assert.Single(type.GenericTypeArguments).NullabilityState);

        // List<int?>?[]
        type = rootType.GetProperty(nameof(GenericClass<int, int, int, int>.ArrayProperty1))!.NullabilityPropertyType;
        Assert.Equal(NullabilityState.NotNull, type.NullabilityState);
        // List<int?>?
        type = type.GetArrayElementType();
        Assert.NotNull(type);
        Assert.Equal(NullabilityState.Nullable, type.NullabilityState);
        // int?
        type = Assert.Single(type.GenericTypeArguments);
        Assert.Equal(NullabilityState.Nullable, type.NullabilityState);
        // int
        type = Assert.Single(type.GenericTypeArguments);
        Assert.Equal(NullabilityState.NotNull, type.NullabilityState);

        // List<string>[]
        type = rootType.GetProperty(nameof(GenericClass<int, int, int, int>.ArrayProperty2))!.NullabilityPropertyType;
        Assert.Equal(NullabilityState.NotNull, type.NullabilityState);
        // List<string>
        type = type.GetArrayElementType();
        Assert.NotNull(type);
        Assert.Equal(NullabilityState.NotNull, type.NullabilityState);
        // string
        Assert.Equal(NullabilityState.NotNull, Assert.Single(type.GenericTypeArguments).NullabilityState);

        // List<string?>?[]
        type = rootType.GetProperty(nameof(GenericClass<int, int, int, int>.ArrayProperty3))!.NullabilityPropertyType;
        Assert.Equal(NullabilityState.NotNull, type.NullabilityState);
        // List<string?>?
        type = type.GetArrayElementType();
        Assert.NotNull(type);
        Assert.Equal(NullabilityState.Nullable, type.NullabilityState);
        // string?
        Assert.Equal(NullabilityState.Nullable, Assert.Single(type.GenericTypeArguments).NullabilityState);

        // GenericClass<T0, T1, T2, T3>?[]
        propertyInfo = rootType.GetProperty(nameof(GenericClass<int, int, int, int>.ArrayProperty));
        Assert.NotNull(propertyInfo);

        Assert.Equal(NullabilityState.NotNull, propertyInfo.NullabilityPropertyType.NullabilityState);
        // GenericClass<T0, T1, T2, T3>?
        type = propertyInfo.NullabilityPropertyType.GetArrayElementType();
        Assert.NotNull(type);
        Assert.Equal(NullabilityState.Nullable, type.NullabilityState);

        Assert.Equal(4, type.GenericTypeArguments.Length);

        // List<int>
        Assert.Equal(NullabilityState.NotNull, type.GenericTypeArguments[0].NullabilityState);
        // int
        Assert.Equal(NullabilityState.NotNull, Assert.Single(type.GenericTypeArguments[0].GenericTypeArguments).NullabilityState);

        // List<int?>?
        Assert.Equal(NullabilityState.Nullable, type.GenericTypeArguments[1].NullabilityState);
        // int?
        Assert.Equal(NullabilityState.Nullable, Assert.Single(type.GenericTypeArguments[1].GenericTypeArguments).NullabilityState);

        // List<string>
        Assert.Equal(NullabilityState.NotNull, type.GenericTypeArguments[2].NullabilityState);
        // string
        Assert.Equal(NullabilityState.NotNull, Assert.Single(type.GenericTypeArguments[2].GenericTypeArguments).NullabilityState);

        // List<string?>?
        Assert.Equal(NullabilityState.Nullable, type.GenericTypeArguments[3].NullabilityState);
        // string?
        Assert.Equal(NullabilityState.Nullable, Assert.Single(type.GenericTypeArguments[3].GenericTypeArguments).NullabilityState);
    }

    [Fact]
    public void RootTypeStateTest()
    {
        NullabilityType type = NullabilityType.GetType(typeof(int));
        Assert.Equal(NullabilityState.NotNull, type.NullabilityState);

        type = NullabilityType.GetType(typeof(int?), NullabilityState.NotNull);
        Assert.Equal(NullabilityState.Nullable, type.NullabilityState);

        type = NullabilityType.GetType(typeof(string));
        Assert.Equal(NullabilityState.Unknown, type.NullabilityState);
    }

    class TestClass
    {
        public GenericClass<List<int>, List<int?>?, List<string>, List<string?>?> Property { get; }
    }
}

internal class GenericClass<T0, T1, T2, T3>
{
    public GenericClass<T0, T1, T2, T3>? Property { get; }

    public T0[] ArrayProperty0 { get; }
    public T1[] ArrayProperty1 { get; }
    public T2[] ArrayProperty2 { get; }
    public T3[] ArrayProperty3 { get; }
    public GenericClass<T0, T1, T2, T3>?[] ArrayProperty { get; }
}