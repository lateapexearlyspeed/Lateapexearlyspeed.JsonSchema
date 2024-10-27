using System.Reflection;

namespace LateApexEarlySpeed.Nullability.Generic.UnitTests;

public class BaseClassTests
{
    [Fact]
    public void TestOverrideProperty()
    {
        NullabilityType rootType = NullabilityType.GetType(typeof(TestSubClass));
        NullabilityPropertyInfo propertyInfo = rootType.GetProperty(nameof(TestSubClass.Prop))!;

        Assert.Equal(NullabilityState.Nullable, propertyInfo.NullabilityReadState);
        Assert.Equal(NullabilityState.Unknown, propertyInfo.NullabilityWriteState);

        NullabilityType propertyType = propertyInfo.NullabilityPropertyType;
        AssertOverrideMemberType(propertyType);
    }

    [Fact]
    public void TestDirectField()
    {
        NullabilityType rootType = NullabilityType.GetType(typeof(TestSubClass));
        NullabilityFieldInfo fieldInfo = rootType.GetField(nameof(TestSubClass.Field))!;

        Assert.Equal(NullabilityState.Nullable, fieldInfo.NullabilityState);

        NullabilityType fieldType = fieldInfo.NullabilityFieldType;
        AssertOverrideMemberType(fieldType);
    }

    [Fact]
    public void TestOverrideMethodParameters()
    {
        NullabilityType rootType = NullabilityType.GetType(typeof(TestSubClass));
        NullabilityMethodInfo methodInfo = rootType.GetMethod(nameof(TestSubClass.Function))!;

        Assert.Equal(NullabilityState.Nullable, methodInfo.NullabilityReturnParameter.NullabilityState);
        NullabilityParameterInfo parameterInfo = Assert.Single(methodInfo.GetNullabilityParameters());
        Assert.Equal(NullabilityState.Nullable, parameterInfo.NullabilityState);

        AssertOverrideMemberType(methodInfo.NullabilityReturnParameter.NullabilityParameterType);
        AssertOverrideMemberType(parameterInfo.NullabilityParameterType);
    }

    private static void AssertOverrideMemberType(NullabilityType type)
    {
        Assert.Equal(NullabilityState.Nullable, type.NullabilityState);
        Assert.Equal(NullabilityState.Nullable, Assert.Single(type.GenericTypeArguments).NullabilityState);
    }

    [Fact]
    public void TestInheritedProperty()
    {
        NullabilityType rootType = NullabilityType.GetType(typeof(TestSubClass));
        NullabilityPropertyInfo propertyInfo = rootType.GetProperty(nameof(BaseClass1<int, int, int>.BaseProp1))!;

        Assert.Equal(NullabilityState.Nullable, propertyInfo.NullabilityReadState);
        Assert.Equal(NullabilityState.Unknown, propertyInfo.NullabilityWriteState);

        NullabilityType propertyType = propertyInfo.NullabilityPropertyType;
        AssertInheritedMemberType(propertyType);
    }

    [Fact]
    public void TestInheritedField()
    {
        NullabilityType rootType = NullabilityType.GetType(typeof(TestSubClass));
        NullabilityFieldInfo fieldInfo = rootType.GetField(nameof(BaseClass1<int, int, int>.BaseField1))!;

        Assert.Equal(NullabilityState.Nullable, fieldInfo.NullabilityState);

        NullabilityType fieldType = fieldInfo.NullabilityFieldType;
        AssertInheritedMemberType(fieldType);
    }

    [Fact]
    public void TestInheritedMethodParameter()
    {
        NullabilityType rootType = NullabilityType.GetType(typeof(TestSubClass));
        NullabilityMethodInfo methodInfo = rootType.GetMethod(nameof(BaseClass1<int, int, int>.BaseFunction1))!;

        Assert.Equal(NullabilityState.Nullable, methodInfo.NullabilityReturnParameter.NullabilityState);
        NullabilityParameterInfo parameterInfo = Assert.Single(methodInfo.GetNullabilityParameters());
        Assert.Equal(NullabilityState.Nullable, parameterInfo.NullabilityState);

        AssertInheritedMemberType(methodInfo.NullabilityReturnParameter.NullabilityParameterType);
        AssertInheritedMemberType(parameterInfo.NullabilityParameterType);
    }

    private static void AssertInheritedMemberType(NullabilityType type)
    {
        Assert.Equal(NullabilityState.Nullable, type.NullabilityState);
        Assert.Equal(3, type.GenericTypeArguments.Length);

        NullabilityType genericStruct = type.GenericTypeArguments[0];
        Assert.Equal(NullabilityState.NotNull, genericStruct.NullabilityState);
        Assert.Equal(NullabilityState.Nullable, type.GenericTypeArguments[1].NullabilityState);
        Assert.Equal(NullabilityState.Nullable, type.GenericTypeArguments[2].NullabilityState);

        // int?
        NullabilityType nullableValue = Assert.Single(genericStruct.GenericTypeArguments);
        Assert.Equal(NullabilityState.Nullable, nullableValue.NullabilityState);

        // int
        Assert.Equal(NullabilityState.NotNull, Assert.Single(nullableValue.GenericTypeArguments).NullabilityState);
    }

    [Fact]
    public void TestInheritedPropertyFromMostBaseClass()
    {
        NullabilityType rootType = NullabilityType.GetType(typeof(TestSubClass));
        NullabilityPropertyInfo propertyInfo = rootType.GetProperty(nameof(BaseClass2<int, int, int>.BaseProp2))!;

        // BaseClass2<GenericStruct<int>, GenericClass<string?>?, GenericClass<string?>?>
        Assert.Equal(NullabilityState.NotNull, propertyInfo.NullabilityReadState);
        Assert.Equal(NullabilityState.Unknown, propertyInfo.NullabilityWriteState);

        NullabilityType propertyType = propertyInfo.NullabilityPropertyType;
        AssertInheritedMemberTypeFromMostBaseClass1(propertyType);

        // GenericStruct<int>[]?
        propertyInfo = rootType.GetProperty(nameof(BaseClass2<int, int, int>.BaseProp3))!;
        Assert.Equal(NullabilityState.Nullable, propertyInfo.NullabilityReadState);
        Assert.Equal(NullabilityState.Unknown, propertyInfo.NullabilityWriteState);

        NullabilityType type = propertyInfo.NullabilityPropertyType;
        AssertInheritedMemberTypeFromMostBaseClass2(type);
    }

    [Fact]
    public void TestInheritedFieldFromMostBaseClass()
    {
        NullabilityType rootType = NullabilityType.GetType(typeof(TestSubClass));
        NullabilityFieldInfo fieldInfo = rootType.GetField(nameof(BaseClass2<int, int, int>.BaseField2))!;

        // BaseClass2<GenericStruct<int>, GenericClass<string?>?, GenericClass<string?>?>
        Assert.Equal(NullabilityState.NotNull, fieldInfo.NullabilityState);

        NullabilityType fieldType = fieldInfo.NullabilityFieldType;
        AssertInheritedMemberTypeFromMostBaseClass1(fieldType);

        // GenericStruct<int>[]?
        fieldInfo = rootType.GetField(nameof(BaseClass2<int, int, int>.BaseField3))!;
        Assert.Equal(NullabilityState.Nullable, fieldInfo.NullabilityState);

        NullabilityType type = fieldInfo.NullabilityFieldType;
        AssertInheritedMemberTypeFromMostBaseClass2(type);
    }

    [Fact]
    public void TestInheritedMethodParameterFromMostBaseClass()
    {
        NullabilityType rootType = NullabilityType.GetType(typeof(TestSubClass));
        NullabilityMethodInfo methodInfo = rootType.GetMethod(nameof(BaseClass2<int, int, int>.BaseFunction2))!;

        // BaseClass2<GenericStruct<int>, GenericClass<string?>?, GenericClass<string?>?>
        Assert.Equal(NullabilityState.NotNull, methodInfo.NullabilityReturnParameter.NullabilityState);
        NullabilityParameterInfo parameterInfo = Assert.Single(methodInfo.GetNullabilityParameters());
        Assert.Equal(NullabilityState.NotNull, parameterInfo.NullabilityState);

        AssertInheritedMemberTypeFromMostBaseClass1(methodInfo.NullabilityReturnParameter.NullabilityParameterType);
        AssertInheritedMemberTypeFromMostBaseClass1(parameterInfo.NullabilityParameterType);

        // GenericStruct<int>[]?
        methodInfo = rootType.GetMethod(nameof(BaseClass2<int, int, int>.BaseFunction3))!;
        Assert.Equal(NullabilityState.Nullable, methodInfo.NullabilityReturnParameter.NullabilityState);
        parameterInfo = Assert.Single(methodInfo.GetNullabilityParameters());
        Assert.Equal(NullabilityState.Nullable, parameterInfo.NullabilityState);

        AssertInheritedMemberTypeFromMostBaseClass2(methodInfo.NullabilityReturnParameter.NullabilityParameterType);
        AssertInheritedMemberTypeFromMostBaseClass2(parameterInfo.NullabilityParameterType);
    }

    /// <summary>
    /// BaseClass2<GenericStruct<int>, GenericClass<string?>?, GenericClass<string?>?>
    /// </summary>
    private static void AssertInheritedMemberTypeFromMostBaseClass1(NullabilityType type)
    {
        Assert.Equal(NullabilityState.NotNull, type.NullabilityState);
        Assert.Equal(3, type.GenericTypeArguments.Length);

        // GenericStruct<int>
        NullabilityType genericStruct = type.GenericTypeArguments[0];
        Assert.Equal(NullabilityState.NotNull, genericStruct.NullabilityState);
        Assert.Equal(NullabilityState.NotNull, Assert.Single(genericStruct.GenericTypeArguments).NullabilityState);

        // GenericClass<string?>?
        NullabilityType genericClass1 = type.GenericTypeArguments[1];
        Assert.Equal(NullabilityState.Nullable, genericClass1.NullabilityState);
        Assert.Equal(NullabilityState.Nullable, Assert.Single(genericClass1.GenericTypeArguments).NullabilityState);

        // GenericClass<string?>?
        NullabilityType genericClass2 = type.GenericTypeArguments[2];
        Assert.Equal(NullabilityState.Nullable, genericClass2.NullabilityState);
        Assert.Equal(NullabilityState.Nullable, Assert.Single(genericClass2.GenericTypeArguments).NullabilityState);
    }

    /// <summary>
    /// GenericStruct<int>[]?
    /// </summary>
    private static void AssertInheritedMemberTypeFromMostBaseClass2(NullabilityType type)
    {
        Assert.Equal(NullabilityState.Nullable, type.NullabilityState);

        // GenericStruct<int> (even if the generic definition type is T0? )
        NullabilityType arrayElementType = type.GetArrayElementType()!;
        Assert.Equal(NullabilityState.NotNull, arrayElementType.NullabilityState);
        NullabilityType intType = Assert.Single(arrayElementType.GenericTypeArguments);
        Assert.Equal(NullabilityState.NotNull, intType.NullabilityState);
        Assert.Empty(intType.GenericTypeArguments);
        Assert.Null(intType.GetArrayElementType());
    }

    class TestSubClass : BaseClass1<int, string, string?>
    {
        public override GenericClass<string?>? Prop { get; }
        public override GenericClass<string?>? Function(GenericClass<string?>? arg) => throw new NotImplementedException();
        public GenericClass<string?>? Field;
    }

    class BaseClass1<T0, T1, T2> : BaseClass2<GenericStruct<T0>, GenericClass<T1?>, GenericClass<T2>?> where T0 : struct
    {
        public BaseClass1<GenericStruct<T0?>, T1?, T2>? BaseProp1 { get; }
        public BaseClass1<GenericStruct<T0?>, T1?, T2>? BaseField1;
        public BaseClass1<GenericStruct<T0?>, T1?, T2>? BaseFunction1(BaseClass1<GenericStruct<T0?>, T1?, T2>? arg) => throw new NotImplementedException();
    }

    internal class BaseClass2<T0, T1, T2> where T0 : notnull
    {
        public virtual T1? Prop { get; }
        public virtual T1? Function(T1? arg) => throw new NotImplementedException();
        
        public BaseClass2<T0, T1?, T2> BaseProp2 { get; }
        public BaseClass2<T0, T1?, T2> BaseField2;
        public BaseClass2<T0, T1?, T2> BaseFunction2(BaseClass2<T0, T1?, T2> arg) => throw new NotImplementedException();

        public T0?[]? BaseProp3 { get; }
        public T0?[]? BaseField3;
        public T0?[]? BaseFunction3(T0?[]? arg) => throw new NotImplementedException();
    }

    class GenericClass<T>
    {
        public GenericClass2<T?> Property { get; }
    }

    class GenericClass2<T>
    {
        public T Property { get; }
    }

    struct GenericStruct<T>
    {
        public T Value { get; }
    }
}