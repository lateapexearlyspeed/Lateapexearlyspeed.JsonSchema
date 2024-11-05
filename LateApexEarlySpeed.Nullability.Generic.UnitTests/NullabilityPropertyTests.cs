using System.Diagnostics.CodeAnalysis;

namespace LateApexEarlySpeed.Nullability.Generic.UnitTests
{
    public class NullabilityPropertyTests
    {
        [Fact]
        public void NotNullValueTypeGetOnlyPropertyTests()
        {
            NullabilityType nullabilityType = NullabilityType.GetType(typeof(ValueTypeProperty));

            Assert.Equal(NullabilityState.Unknown, nullabilityType.NullabilityState);
            Assert.Equal(typeof(ValueTypeProperty), nullabilityType.Type);

            NullabilityPropertyInfo? propertyInfo = nullabilityType.GetProperty(nameof(ValueTypeProperty.PropertyGetterOnly));
            
            Assert.NotNull(propertyInfo);
            Assert.Equal(typeof(TestStruct), propertyInfo.PropertyType);
            Assert.Equal(NullabilityState.NotNull, propertyInfo.NullabilityReadState);
            Assert.Equal(NullabilityState.Unknown, propertyInfo.NullabilityWriteState);

            NullabilityType propertyType = propertyInfo.NullabilityPropertyType;
            Assert.Equal(NullabilityState.NotNull, propertyType.NullabilityState);
            Assert.Equal(typeof(TestStruct), propertyType.Type);

            propertyInfo = propertyType.GetProperty(nameof(TestStruct.PropertyGetterOnly));

            Assert.NotNull(propertyInfo);
            Assert.Equal(typeof(int), propertyInfo.PropertyType);
            Assert.Equal(NullabilityState.NotNull, propertyInfo.NullabilityReadState);
            Assert.Equal(NullabilityState.Unknown, propertyInfo.NullabilityWriteState);

            propertyType = propertyInfo.NullabilityPropertyType;
            Assert.Equal(NullabilityState.NotNull, propertyType.NullabilityState);
            Assert.Equal(typeof(int), propertyType.Type);
        }

        [Fact]
        public void NotNullValueTypeSetOnlyPropertyTests()
        {
            NullabilityType nullabilityType = NullabilityType.GetType(typeof(ValueTypeProperty));

            Assert.Equal(NullabilityState.Unknown, nullabilityType.NullabilityState);
            Assert.Equal(typeof(ValueTypeProperty), nullabilityType.Type);

            NullabilityPropertyInfo? propertyInfo = nullabilityType.GetProperty(nameof(ValueTypeProperty.PropertySetterOnly));

            Assert.NotNull(propertyInfo);
            Assert.Equal(typeof(TestStruct), propertyInfo.PropertyType);
            Assert.Equal(NullabilityState.Unknown, propertyInfo.NullabilityReadState);
            Assert.Equal(NullabilityState.NotNull, propertyInfo.NullabilityWriteState);

            NullabilityType propertyType = propertyInfo.NullabilityPropertyType;
            Assert.Equal(NullabilityState.NotNull, propertyType.NullabilityState);
            Assert.Equal(typeof(TestStruct), propertyType.Type);

            propertyInfo = propertyType.GetProperty(nameof(TestStruct.PropertySetterOnly));

            Assert.NotNull(propertyInfo);
            Assert.Equal(typeof(int), propertyInfo.PropertyType);
            Assert.Equal(NullabilityState.Unknown, propertyInfo.NullabilityReadState);
            Assert.Equal(NullabilityState.NotNull, propertyInfo.NullabilityWriteState);

            propertyType = propertyInfo.NullabilityPropertyType;
            Assert.Equal(NullabilityState.NotNull, propertyType.NullabilityState);
            Assert.Equal(typeof(int), propertyType.Type);
        }

        [Fact]
        public void NotNullValueTypeGetSetPropertyTests()
        {
            NullabilityType nullabilityType = NullabilityType.GetType(typeof(ValueTypeProperty));

            Assert.Equal(NullabilityState.Unknown, nullabilityType.NullabilityState);
            Assert.Equal(typeof(ValueTypeProperty), nullabilityType.Type);

            NullabilityPropertyInfo? propertyInfo = nullabilityType.GetProperty(nameof(ValueTypeProperty.PropertyGetterSetter));

            Assert.NotNull(propertyInfo);
            Assert.Equal(typeof(TestStruct), propertyInfo.PropertyType);
            Assert.Equal(NullabilityState.NotNull, propertyInfo.NullabilityReadState);
            Assert.Equal(NullabilityState.NotNull, propertyInfo.NullabilityWriteState);

            NullabilityType propertyType = propertyInfo.NullabilityPropertyType;
            Assert.Equal(NullabilityState.NotNull, propertyType.NullabilityState);
            Assert.Equal(typeof(TestStruct), propertyType.Type);

            propertyInfo = propertyType.GetProperty(nameof(TestStruct.PropertyGetterSetter));

            Assert.NotNull(propertyInfo);
            Assert.Equal(typeof(int), propertyInfo.PropertyType);
            Assert.Equal(NullabilityState.NotNull, propertyInfo.NullabilityReadState);
            Assert.Equal(NullabilityState.NotNull, propertyInfo.NullabilityWriteState);

            propertyType = propertyInfo.NullabilityPropertyType;
            Assert.Equal(NullabilityState.NotNull, propertyType.NullabilityState);
            Assert.Equal(typeof(int), propertyType.Type);
        }

        [Fact]
        public void NullableValueTypeGetOnlyPropertyTests()
        {
            NullabilityType nullabilityType = NullabilityType.GetType(typeof(NullableValueTypeProperty));

            Assert.Equal(NullabilityState.Unknown, nullabilityType.NullabilityState);
            Assert.Equal(typeof(NullableValueTypeProperty), nullabilityType.Type);

            // GenericStruct<string>?
            NullabilityPropertyInfo? propertyInfo = nullabilityType.GetProperty(nameof(NullableValueTypeProperty.Property1));

            Assert.NotNull(propertyInfo);
            Assert.Equal(typeof(GenericStruct<string>?), propertyInfo.PropertyType);
            Assert.Equal(NullabilityState.Nullable, propertyInfo.NullabilityReadState);
            Assert.Equal(NullabilityState.Unknown, propertyInfo.NullabilityWriteState);

            NullabilityType propertyType = propertyInfo.NullabilityPropertyType;
            Assert.Equal(NullabilityState.Nullable, propertyType.NullabilityState);
            Assert.Equal(typeof(GenericStruct<string>?), propertyType.Type);

            // GenericStruct<string>
            propertyInfo = propertyType.GetProperty(nameof(Nullable<GenericStruct<string>>.Value));

            Assert.NotNull(propertyInfo);
            Assert.Equal(typeof(GenericStruct<string>), propertyInfo.PropertyType);
            Assert.Equal(NullabilityState.NotNull, propertyInfo.NullabilityReadState);
            Assert.Equal(NullabilityState.Unknown, propertyInfo.NullabilityWriteState);

            NullabilityType underlyingType = propertyInfo.NullabilityPropertyType;
            Assert.Equal(NullabilityState.NotNull, underlyingType.NullabilityState);

            // string
            propertyInfo = underlyingType.GetProperty(nameof(GenericStruct<int>.Value))!;

            Assert.Equal(NullabilityState.NotNull, propertyInfo.NullabilityReadState);
            Assert.Equal(NullabilityState.Unknown, propertyInfo.NullabilityWriteState);

            // GenericStruct<string?>?
            propertyInfo = nullabilityType.GetProperty(nameof(NullableValueTypeProperty.Property2));

            Assert.NotNull(propertyInfo);
            Assert.Equal(typeof(GenericStruct<string?>?), propertyInfo.PropertyType);
            Assert.Equal(NullabilityState.Nullable, propertyInfo.NullabilityReadState);
            Assert.Equal(NullabilityState.Unknown, propertyInfo.NullabilityWriteState);

            propertyType = propertyInfo.NullabilityPropertyType;
            Assert.Equal(NullabilityState.Nullable, propertyType.NullabilityState);
            Assert.Equal(typeof(GenericStruct<string?>?), propertyType.Type);

            // GenericStruct<string?>
            propertyInfo = propertyType.GetProperty(nameof(Nullable<GenericStruct<string?>>.Value));

            Assert.NotNull(propertyInfo);
            Assert.Equal(typeof(GenericStruct<string?>), propertyInfo.PropertyType);
            Assert.Equal(NullabilityState.NotNull, propertyInfo.NullabilityReadState);
            Assert.Equal(NullabilityState.Unknown, propertyInfo.NullabilityWriteState);

            underlyingType = propertyInfo.NullabilityPropertyType;
            Assert.Equal(NullabilityState.NotNull, underlyingType.NullabilityState);

            // string?
            propertyInfo = underlyingType.GetProperty(nameof(GenericStruct<int>.Value))!;

            Assert.Equal(NullabilityState.Nullable, propertyInfo.NullabilityReadState);
            Assert.Equal(NullabilityState.Unknown, propertyInfo.NullabilityWriteState);
        }

        class ValueTypeProperty
        {
            public TestStruct PropertyGetterOnly { get; }
            public TestStruct PropertySetterOnly
            {
                set => throw new NotImplementedException();
            }

            public TestStruct PropertyGetterSetter { get; set; }
        }

        struct TestStruct
        {
            public int PropertyGetterOnly { get; }
            public int PropertySetterOnly
            {
                set => throw new NotImplementedException();
            }

            public int PropertyGetterSetter { get; set; }
        }

        class NullableValueTypeProperty
        {
            public GenericStruct<string>? Property1 { get; }
            public GenericStruct<string?>? Property2 { get; }
        }

        struct GenericStruct<T>
        {
            public T Value { get; }
        }

        [Theory]
        [InlineData(typeof(TestClass))]
        [InlineData(typeof(ITestInterface))]
        public void ReferencePropertyTypeTests(Type testRootType)
        {
            NullabilityType rootType = NullabilityType.GetType(testRootType);

            // GenericClass<string>
            NullabilityPropertyInfo? propertyInfo = rootType.GetProperty(nameof(TestClass.Property1));
            AssertProperty(propertyInfo, NullabilityState.NotNull);

            // GenericClass<string> -> GenericClass2<string>
            propertyInfo = propertyInfo.NullabilityPropertyType.GetProperty(nameof(GenericClass<string>.Property));
            AssertProperty(propertyInfo, NullabilityState.NotNull);

            // GenericClass<string> -> GenericClass2<string> -> string
            propertyInfo = propertyInfo.NullabilityPropertyType.GetProperty(nameof(GenericClass2<string>.Property));
            AssertProperty(propertyInfo, NullabilityState.NotNull);

            // GenericClass<string>?
            propertyInfo = rootType.GetProperty(nameof(TestClass.Property2));
            AssertProperty(propertyInfo, NullabilityState.Nullable);

            // GenericClass<string>? -> GenericClass2<string>
            propertyInfo = propertyInfo.NullabilityPropertyType.GetProperty(nameof(GenericClass<string>.Property));
            AssertProperty(propertyInfo, NullabilityState.NotNull);

            // GenericClass<string>? -> GenericClass2<string> -> string
            propertyInfo = propertyInfo.NullabilityPropertyType.GetProperty(nameof(GenericClass2<string>.Property));
            AssertProperty(propertyInfo, NullabilityState.NotNull);

            // GenericClass<string?>
            propertyInfo = rootType.GetProperty(nameof(TestClass.Property3));
            AssertProperty(propertyInfo, NullabilityState.NotNull);

            // GenericClass<string?> -> GenericClass2<string?>
            propertyInfo = propertyInfo.NullabilityPropertyType.GetProperty(nameof(GenericClass<string>.Property));
            AssertProperty(propertyInfo, NullabilityState.NotNull);

            // GenericClass<string?> -> GenericClass2<string?> -> string?
            propertyInfo = propertyInfo.NullabilityPropertyType.GetProperty(nameof(GenericClass2<string>.Property));
            AssertProperty(propertyInfo, NullabilityState.Nullable);

            // GenericClass<string?>?
            propertyInfo = rootType.GetProperty(nameof(TestClass.Property4));
            AssertProperty(propertyInfo, NullabilityState.Nullable);

            // GenericClass<string?>? -> GenericClass2<string?>
            propertyInfo = propertyInfo.NullabilityPropertyType.GetProperty(nameof(GenericClass<string>.Property));
            AssertProperty(propertyInfo, NullabilityState.NotNull);

            // GenericClass<string?>? -> GenericClass2<string?> -> string?
            propertyInfo = propertyInfo.NullabilityPropertyType.GetProperty(nameof(GenericClass2<string>.Property));
            AssertProperty(propertyInfo, NullabilityState.Nullable);
        }

        [Fact]
        public void GenericRootTypeTests()
        {
            // GenericClass<string>
            NullabilityType rootType = NullabilityType.GetType(typeof(GenericClass<string>), NullabilityState.NotNull);

            // GenericClass<string> -> GenericClass2<string>
            NullabilityPropertyInfo? propertyInfo = rootType.GetProperty(nameof(GenericClass<string>.Property));
            AssertProperty(propertyInfo, NullabilityState.NotNull);

            // GenericClass<string> -> GenericClass2<string> -> string
            propertyInfo = propertyInfo.NullabilityPropertyType.GetProperty(nameof(GenericClass2<string>.Property));
            AssertProperty(propertyInfo, NullabilityState.NotNull);

            // GenericClass<string?>
            rootType = NullabilityType.GetType(typeof(GenericClass<string?>), NullabilityState.Nullable);

            // GenericClass<string?> -> GenericClass2<string?>
            propertyInfo = rootType.GetProperty(nameof(GenericClass<string?>.Property));
            AssertProperty(propertyInfo, NullabilityState.NotNull);

            // GenericClass<string?> -> GenericClass2<string?> -> string?
            propertyInfo = propertyInfo.NullabilityPropertyType.GetProperty(nameof(GenericClass2<string?>.Property));
            AssertProperty(propertyInfo, NullabilityState.Nullable);
        }

        [Fact]
        public void NestedGenericRootTypeTests()
        {
            // GenericClass2<GenericClass2<string>?>
            NullabilityType rootType = NullabilityType.GetType(typeof(GenericClass2<GenericClass2<string>?>), new[]{ new NullabilityElement(NullabilityState.Nullable, new[] { new NullabilityElement(NullabilityState.NotNull) }) });

            // GenericClass2<GenericClass2<string>?> -> GenericClass2<string>?
            NullabilityPropertyInfo? propertyInfo = rootType.GetProperty(nameof(GenericClass2<string>.Property));
            AssertProperty(propertyInfo, NullabilityState.Nullable);

            // GenericClass2<GenericClass2<string>?> -> GenericClass2<string>? -> string
            propertyInfo = propertyInfo.NullabilityPropertyType.GetProperty(nameof(GenericClass2<string>.Property));
            AssertProperty(propertyInfo, NullabilityState.NotNull);

            // GenericClass2<GenericClass2<string?>>
            rootType = NullabilityType.GetType(typeof(GenericClass2<GenericClass2<string?>>), new[]{ new NullabilityElement(NullabilityState.NotNull, new[] { new NullabilityElement(NullabilityState.Nullable) }) });

            // GenericClass2<GenericClass2<string?>> -> GenericClass2<string?>
            propertyInfo = rootType.GetProperty(nameof(GenericClass2<string?>.Property));
            AssertProperty(propertyInfo, NullabilityState.NotNull);

            // GenericClass2<GenericClass2<string?>> -> GenericClass2<string?> -> string?
            propertyInfo = propertyInfo.NullabilityPropertyType.GetProperty(nameof(GenericClass2<string>.Property));
            AssertProperty(propertyInfo, NullabilityState.Nullable);
        }

        private static void AssertProperty([NotNull] NullabilityPropertyInfo? propertyInfo, NullabilityState expectedReadState)
        {
            Assert.NotNull(propertyInfo);
            Assert.Equal(expectedReadState, propertyInfo.NullabilityReadState);
            Assert.Equal(NullabilityState.Unknown, propertyInfo.NullabilityWriteState);
        }

        class TestClass
        {
            public GenericClass<string> Property1 { get; }
            public GenericClass<string>? Property2 { get; }
            public GenericClass<string?> Property3 { get; }
            public GenericClass<string?>? Property4 { get; }
        }

        interface ITestInterface
        {
            GenericClass<string> Property1 { get; }
            GenericClass<string>? Property2 { get; }
            GenericClass<string?> Property3 { get; }
            GenericClass<string?>? Property4 { get; }
        }

        class GenericClass<T>
        {
            public GenericClass2<T> Property { get; }
        }

        class GenericClass2<T>
        {
            public T Property { get; }
        }
    }
}