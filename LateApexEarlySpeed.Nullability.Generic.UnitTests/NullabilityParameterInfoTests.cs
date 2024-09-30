using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace LateApexEarlySpeed.Nullability.Generic.UnitTests
{
    public class NullabilityParameterInfoTests
    {
        [Theory]
        [MemberData(nameof(ParameterInfoProviders))]
        public void NotNullValueTypeParameterTests(Func<NullabilityMethodInfo, NullabilityParameterInfo> parameterInfoProvider)
        {
            NullabilityType nullabilityType = NullabilityType.GetType(typeof(ValueTypeParameter));

            Assert.Equal(NullabilityState.Unknown, nullabilityType.NullabilityState);
            Assert.Equal(typeof(ValueTypeParameter), nullabilityType.Type);

            NullabilityMethodInfo? methodInfo = nullabilityType.GetMethod(nameof(ValueTypeParameter.Function));

            Assert.NotNull(methodInfo);

            NullabilityParameterInfo parameterInfo = parameterInfoProvider(methodInfo);

            Assert.Equal(typeof(TestStruct), parameterInfo.ParameterType);
            Assert.Equal(NullabilityState.NotNull, parameterInfo.NullabilityState);

            NullabilityType parameterType = parameterInfo.NullabilityParameterType;
            Assert.Equal(NullabilityState.NotNull, parameterType.NullabilityState);
            Assert.Equal(typeof(TestStruct), parameterType.Type);

            methodInfo = parameterType.GetMethod(nameof(TestStruct.Function));

            Assert.NotNull(methodInfo);

            parameterInfo = parameterInfoProvider(methodInfo);

            Assert.Equal(typeof(int), parameterInfo.ParameterType);
            Assert.Equal(NullabilityState.NotNull, parameterInfo.NullabilityState);

            parameterType = parameterInfo.NullabilityParameterType;
            Assert.Equal(NullabilityState.NotNull, parameterType.NullabilityState);
            Assert.Equal(typeof(int), parameterType.Type);
        }

        [Theory]
        [MemberData(nameof(ParameterInfoProviders))]
        public void NullableValueTypeParameterTests(Func<NullabilityMethodInfo, NullabilityParameterInfo> parameterInfoProvider)
        {
            NullabilityType nullabilityType = NullabilityType.GetType(typeof(NullableValueTypeParameter));

            Assert.Equal(NullabilityState.Unknown, nullabilityType.NullabilityState);
            Assert.Equal(typeof(NullableValueTypeParameter), nullabilityType.Type);

            // GenericStruct<string>?
            NullabilityMethodInfo? methodInfo = nullabilityType.GetMethod(nameof(NullableValueTypeParameter.Function1));

            Assert.NotNull(methodInfo);

            NullabilityParameterInfo parameterInfo = parameterInfoProvider(methodInfo);

            Assert.Equal(typeof(GenericStruct<string>?), parameterInfo.ParameterType);
            Assert.Equal(NullabilityState.Nullable, parameterInfo.NullabilityState);

            NullabilityType parameterType = parameterInfo.NullabilityParameterType;
            Assert.Equal(NullabilityState.Nullable, parameterType.NullabilityState);
            Assert.Equal(typeof(GenericStruct<string>?), parameterType.Type);

            // GenericStruct<string>
            NullabilityPropertyInfo? propertyInfo = parameterType.GetProperty(nameof(Nullable<GenericStruct<string>>.Value));

            Assert.NotNull(propertyInfo);
            Assert.Equal(typeof(GenericStruct<string>), propertyInfo.PropertyType);
            Assert.Equal(NullabilityState.NotNull, propertyInfo.NullabilityReadState);

            NullabilityType underlyingType = propertyInfo.NullabilityPropertyType;
            Assert.Equal(NullabilityState.NotNull, underlyingType.NullabilityState);

            // string
            methodInfo = underlyingType.GetMethod(nameof(GenericStruct<int>.Function))!;

            parameterInfo = parameterInfoProvider(methodInfo);
            Assert.Equal(NullabilityState.NotNull, parameterInfo.NullabilityState);

            // GenericStruct<string?>?
            methodInfo = nullabilityType.GetMethod(nameof(NullableValueTypeParameter.Function2));

            Assert.NotNull(methodInfo);

            parameterInfo = parameterInfoProvider(methodInfo);

            Assert.Equal(typeof(GenericStruct<string?>?), parameterInfo.ParameterType);
            Assert.Equal(NullabilityState.Nullable, parameterInfo.NullabilityState);

            parameterType = parameterInfo.NullabilityParameterType;
            Assert.Equal(NullabilityState.Nullable, parameterType.NullabilityState);
            Assert.Equal(typeof(GenericStruct<string?>?), parameterType.Type);

            // GenericStruct<string?>
            propertyInfo = parameterType.GetProperty(nameof(Nullable<GenericStruct<string?>>.Value));

            Assert.NotNull(propertyInfo);
            Assert.Equal(typeof(GenericStruct<string?>), propertyInfo.PropertyType);
            Assert.Equal(NullabilityState.NotNull, propertyInfo.NullabilityReadState);

            underlyingType = propertyInfo.NullabilityPropertyType;
            Assert.Equal(NullabilityState.NotNull, underlyingType.NullabilityState);

            // string?
            methodInfo = underlyingType.GetMethod(nameof(GenericStruct<int>.Function))!;

            parameterInfo = parameterInfoProvider(methodInfo);
            Assert.Equal(NullabilityState.Nullable, parameterInfo.NullabilityState);
        }

        class ValueTypeParameter
        {
            public TestStruct Function(TestStruct p0) => throw new NotImplementedException();
        }

        struct TestStruct
        {
            public int Function(int p0) => throw new NotImplementedException();
        }

        class NullableValueTypeParameter
        {
            public GenericStruct<string>? Function1(GenericStruct<string>? p0) => throw new NotImplementedException();
            public GenericStruct<string?>? Function2(GenericStruct<string?>? p0) => throw new NotImplementedException();
        }

        struct GenericStruct<T>
        {
            public T Function(T p0) => throw new NotImplementedException();
        }

        [Theory]
        [MemberData(nameof(ParameterInfoProviders))]
        public void ReferenceParameterTypeTests(Func<NullabilityMethodInfo, NullabilityParameterInfo> parameterInfoProvider)
        {
            NullabilityType rootType = NullabilityType.GetType(typeof(TestClass));

            // GenericClass<string>
            NullabilityMethodInfo? methodInfo = rootType.GetMethod(nameof(TestClass.Function1));
            NullabilityParameterInfo parameterInfo = AssertMethodAndParameter(methodInfo, parameterInfoProvider, NullabilityState.NotNull);

            // GenericClass<string> -> GenericClass2<string>
            methodInfo = parameterInfo.NullabilityParameterType.GetMethod(nameof(GenericClass<string>.Function));
            parameterInfo = AssertMethodAndParameter(methodInfo, parameterInfoProvider, NullabilityState.NotNull);

            // GenericClass<string> -> GenericClass2<string> -> string
            methodInfo = parameterInfo.NullabilityParameterType.GetMethod(nameof(GenericClass2<string>.Function));
            AssertMethodAndParameter(methodInfo, parameterInfoProvider, NullabilityState.NotNull);

            // GenericClass<string>?
            methodInfo = rootType.GetMethod(nameof(TestClass.Function2));
            parameterInfo = AssertMethodAndParameter(methodInfo, parameterInfoProvider, NullabilityState.Nullable);

            // GenericClass<string>? -> GenericClass2<string>
            methodInfo = parameterInfo.NullabilityParameterType.GetMethod(nameof(GenericClass<string>.Function));
            parameterInfo = AssertMethodAndParameter(methodInfo, parameterInfoProvider, NullabilityState.NotNull);

            // GenericClass<string>? -> GenericClass2<string> -> string
            methodInfo = parameterInfo.NullabilityParameterType.GetMethod(nameof(GenericClass2<string>.Function));
            parameterInfo = AssertMethodAndParameter(methodInfo, parameterInfoProvider, NullabilityState.NotNull);

            // GenericClass<string?>
            methodInfo = rootType.GetMethod(nameof(TestClass.Function3));
            parameterInfo = AssertMethodAndParameter(methodInfo, parameterInfoProvider, NullabilityState.NotNull);

            // GenericClass<string?> -> GenericClass2<string?>
            methodInfo = parameterInfo.NullabilityParameterType.GetMethod(nameof(GenericClass<string>.Function));
            parameterInfo = AssertMethodAndParameter(methodInfo, parameterInfoProvider, NullabilityState.NotNull);

            // GenericClass<string?> -> GenericClass2<string?> -> string?
            methodInfo = parameterInfo.NullabilityParameterType.GetMethod(nameof(GenericClass2<string>.Function));
            AssertMethodAndParameter(methodInfo, parameterInfoProvider, NullabilityState.Nullable);

            // GenericClass<string?>?
            methodInfo = rootType.GetMethod(nameof(TestClass.Function4));
            parameterInfo = AssertMethodAndParameter(methodInfo, parameterInfoProvider, NullabilityState.Nullable);

            // GenericClass<string?>? -> GenericClass2<string?>
            methodInfo = parameterInfo.NullabilityParameterType.GetMethod(nameof(GenericClass<string>.Function));
            parameterInfo = AssertMethodAndParameter(methodInfo, parameterInfoProvider, NullabilityState.NotNull);

            // GenericClass<string?>? -> GenericClass2<string?> -> string?
            methodInfo = parameterInfo.NullabilityParameterType.GetMethod(nameof(GenericClass2<string>.Function));
            AssertMethodAndParameter(methodInfo, parameterInfoProvider, NullabilityState.Nullable);
        }

        [Theory]
        [MemberData(nameof(ParameterInfoProviders))]
        public void GenericRootTypeTests(Func<NullabilityMethodInfo, NullabilityParameterInfo> parameterInfoProvider)
        {
            // GenericClass<string>
            NullabilityType rootType = NullabilityType.GetType(typeof(GenericClass<string>), NullabilityState.NotNull);

            // GenericClass<string> -> GenericClass2<string>
            NullabilityMethodInfo? methodInfo = rootType.GetMethod(nameof(GenericClass<string>.Function));
            NullabilityParameterInfo parameterInfo = AssertMethodAndParameter(methodInfo, parameterInfoProvider, NullabilityState.NotNull);

            // GenericClass<string> -> GenericClass2<string> -> string
            methodInfo = parameterInfo.NullabilityParameterType.GetMethod(nameof(GenericClass2<string>.Function));
            AssertMethodAndParameter(methodInfo, parameterInfoProvider, NullabilityState.NotNull);

            // GenericClass<string?>
            rootType = NullabilityType.GetType(typeof(GenericClass<string?>), NullabilityState.Nullable);

            // GenericClass<string?> -> GenericClass2<string?>
            methodInfo = rootType.GetMethod(nameof(GenericClass<string?>.Function));
            parameterInfo = AssertMethodAndParameter(methodInfo, parameterInfoProvider, NullabilityState.NotNull);

            // GenericClass<string?> -> GenericClass2<string?> -> string?
            methodInfo = parameterInfo.NullabilityParameterType.GetMethod(nameof(GenericClass2<string?>.Function));
            AssertMethodAndParameter(methodInfo, parameterInfoProvider, NullabilityState.Nullable);
        }

        [Theory]
        [MemberData(nameof(ParameterInfoProviders))]
        public void NestedGenericRootTypeTests(Func<NullabilityMethodInfo, NullabilityParameterInfo> parameterInfoProvider)
        {
            // GenericClass2<GenericClass2<string>?>
            NullabilityType rootType = NullabilityType.GetType(typeof(GenericClass2<GenericClass2<string>?>), new NullabilityElement(NullabilityState.Nullable, new []{new NullabilityElement(NullabilityState.NotNull)}));

            // GenericClass2<GenericClass2<string>?> -> GenericClass2<string>?
            NullabilityMethodInfo? methodInfo = rootType.GetMethod(nameof(GenericClass2<string>.Function));
            NullabilityParameterInfo parameterInfo = AssertMethodAndParameter(methodInfo, parameterInfoProvider, NullabilityState.Nullable);

            // GenericClass2<GenericClass2<string>?> -> GenericClass2<string>? -> string
            methodInfo = parameterInfo.NullabilityParameterType.GetMethod(nameof(GenericClass2<string>.Function));
            AssertMethodAndParameter(methodInfo, parameterInfoProvider, NullabilityState.NotNull);

            // GenericClass2<GenericClass2<string?>>
            rootType = NullabilityType.GetType(typeof(GenericClass2<GenericClass2<string?>>), new NullabilityElement(NullabilityState.NotNull, new[] { new NullabilityElement(NullabilityState.Nullable) }));

            // GenericClass2<GenericClass2<string?>> -> GenericClass2<string?>
            methodInfo = rootType.GetMethod(nameof(GenericClass2<string?>.Function));
            parameterInfo = AssertMethodAndParameter(methodInfo, parameterInfoProvider, NullabilityState.NotNull);

            // GenericClass2<GenericClass2<string?>> -> GenericClass2<string?> -> string?
            methodInfo = parameterInfo.NullabilityParameterType.GetMethod(nameof(GenericClass2<string>.Function));
            AssertMethodAndParameter(methodInfo, parameterInfoProvider, NullabilityState.Nullable);
        }

        private static NullabilityParameterInfo AssertMethodAndParameter([NotNull] NullabilityMethodInfo? methodInfo, Func<NullabilityMethodInfo, NullabilityParameterInfo> parameterInfoProvider, NullabilityState expectedState)
        {
            Assert.NotNull(methodInfo);

            NullabilityParameterInfo parameterInfo = parameterInfoProvider(methodInfo);
            Assert.Equal(expectedState, parameterInfo.NullabilityState);

            return parameterInfo;
        }

        public static IEnumerable<object[]> ParameterInfoProviders {
            get
            {
                yield return new object[] { (NullabilityMethodInfo m) => m.NullabilityReturnParameter };
                yield return new object[] { (NullabilityMethodInfo m) => m.GetNullabilityParameters()[0] };
            }
        }

        class TestClass
        {
            public GenericClass<string> Function1(GenericClass<string> p0) => throw new NotImplementedException();
            public GenericClass<string>? Function2(GenericClass<string>? p0) => throw new NotImplementedException();
            public GenericClass<string?> Function3(GenericClass<string?> p0) => throw new NotImplementedException();
            public GenericClass<string?>? Function4(GenericClass<string?>? p0) => throw new NotImplementedException();
        }

        class GenericClass<T>
        {
            public GenericClass2<T> Function(GenericClass2<T> p0) => throw new NotImplementedException();
        }

        class GenericClass2<T>
        {
            public T Function(T p0) => throw new NotImplementedException();
        }
    }
}