using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace LateApexEarlySpeed.Nullability.Generic.UnitTests
{
    public class NullabilityFieldTests
    {
        [Fact]
        public void NotNullValueTypeFieldTests()
        {
            NullabilityType nullabilityType = NullabilityType.GetType(typeof(ValueTypeField));

            Assert.Equal(NullabilityState.Unknown, nullabilityType.NullabilityState);
            Assert.Equal(typeof(ValueTypeField), nullabilityType.Type);

            NullabilityFieldInfo? fieldInfo = nullabilityType.GetField(nameof(ValueTypeField.Field));

            Assert.NotNull(fieldInfo);
            Assert.Equal(typeof(TestStruct), fieldInfo.FieldType);
            Assert.Equal(NullabilityState.NotNull, fieldInfo.NullabilityState);

            NullabilityType fieldType = fieldInfo.NullabilityFieldType;
            Assert.Equal(NullabilityState.NotNull, fieldType.NullabilityState);
            Assert.Equal(typeof(TestStruct), fieldType.Type);

            fieldInfo = fieldType.GetField(nameof(TestStruct.Field));

            Assert.NotNull(fieldInfo);
            Assert.Equal(typeof(int), fieldInfo.FieldType);
            Assert.Equal(NullabilityState.NotNull, fieldInfo.NullabilityState);

            fieldType = fieldInfo.NullabilityFieldType;
            Assert.Equal(NullabilityState.NotNull, fieldType.NullabilityState);
            Assert.Equal(typeof(int), fieldType.Type);
        }

        [Fact]
        public void NullableValueTypeFieldTests()
        {
            NullabilityType nullabilityType = NullabilityType.GetType(typeof(NullableValueTypeField));

            Assert.Equal(NullabilityState.Unknown, nullabilityType.NullabilityState);
            Assert.Equal(typeof(NullableValueTypeField), nullabilityType.Type);

            // GenericStruct<string>?
            NullabilityFieldInfo? fieldInfo = nullabilityType.GetField(nameof(NullableValueTypeField.Field1));

            Assert.NotNull(fieldInfo);
            Assert.Equal(typeof(GenericStruct<string>?), fieldInfo.FieldType);
            Assert.Equal(NullabilityState.Nullable, fieldInfo.NullabilityState);

            NullabilityType fieldType = fieldInfo.NullabilityFieldType;
            Assert.Equal(NullabilityState.Nullable, fieldType.NullabilityState);
            Assert.Equal(typeof(GenericStruct<string>?), fieldType.Type);

            // GenericStruct<string>
            NullabilityPropertyInfo? propertyInfo = fieldType.GetProperty(nameof(Nullable<GenericStruct<string>>.Value));

            Assert.NotNull(propertyInfo);
            Assert.Equal(typeof(GenericStruct<string>), propertyInfo.PropertyType);
            Assert.Equal(NullabilityState.NotNull, propertyInfo.NullabilityReadState);

            NullabilityType underlyingType = propertyInfo.NullabilityPropertyType;
            Assert.Equal(NullabilityState.NotNull, underlyingType.NullabilityState);

            // string
            fieldInfo = underlyingType.GetField(nameof(GenericStruct<int>.Value))!;

            Assert.Equal(NullabilityState.NotNull, fieldInfo.NullabilityState);

            // GenericStruct<string?>?
            fieldInfo = nullabilityType.GetField(nameof(NullableValueTypeField.Field2));

            Assert.NotNull(fieldInfo);
            Assert.Equal(typeof(GenericStruct<string?>?), fieldInfo.FieldType);
            Assert.Equal(NullabilityState.Nullable, fieldInfo.NullabilityState);

            fieldType = fieldInfo.NullabilityFieldType;
            Assert.Equal(NullabilityState.Nullable, fieldType.NullabilityState);
            Assert.Equal(typeof(GenericStruct<string?>?), fieldType.Type);

            // GenericStruct<string?>
            propertyInfo = fieldType.GetProperty(nameof(Nullable<GenericStruct<string?>>.Value));

            Assert.NotNull(propertyInfo);
            Assert.Equal(typeof(GenericStruct<string?>), propertyInfo.PropertyType);
            Assert.Equal(NullabilityState.NotNull, propertyInfo.NullabilityReadState);

            underlyingType = propertyInfo.NullabilityPropertyType;
            Assert.Equal(NullabilityState.NotNull, underlyingType.NullabilityState);

            // string?
            fieldInfo = underlyingType.GetField(nameof(GenericStruct<int>.Value))!;

            Assert.Equal(NullabilityState.Nullable, fieldInfo.NullabilityState);
        }

        class ValueTypeField
        {
            public TestStruct Field;
        }

        struct TestStruct
        {
            public int Field;
        }

        class NullableValueTypeField
        {
            public GenericStruct<string>? Field1;
            public GenericStruct<string?>? Field2;
        }

        struct GenericStruct<T>
        {
            public T Value;
        }

        [Fact]
        public void ReferenceFieldTypeTests()
        {
            NullabilityType rootType = NullabilityType.GetType(typeof(TestClass));

            // GenericClass<string>
            NullabilityFieldInfo? fieldInfo = rootType.GetField(nameof(TestClass.Field1));
            AssertField(fieldInfo, NullabilityState.NotNull);

            // GenericClass<string> -> GenericClass2<string>
            fieldInfo = fieldInfo.NullabilityFieldType.GetField(nameof(GenericClass<string>.Field));
            AssertField(fieldInfo, NullabilityState.NotNull);

            // GenericClass<string> -> GenericClass2<string> -> string
            fieldInfo = fieldInfo.NullabilityFieldType.GetField(nameof(GenericClass2<string>.Field));
            AssertField(fieldInfo, NullabilityState.NotNull);

            // GenericClass<string>?
            fieldInfo = rootType.GetField(nameof(TestClass.Field2));
            AssertField(fieldInfo, NullabilityState.Nullable);

            // GenericClass<string>? -> GenericClass2<string>
            fieldInfo = fieldInfo.NullabilityFieldType.GetField(nameof(GenericClass<string>.Field));
            AssertField(fieldInfo, NullabilityState.NotNull);

            // GenericClass<string>? -> GenericClass2<string> -> string
            fieldInfo = fieldInfo.NullabilityFieldType.GetField(nameof(GenericClass2<string>.Field));
            AssertField(fieldInfo, NullabilityState.NotNull);

            // GenericClass<string?>
            fieldInfo = rootType.GetField(nameof(TestClass.Field3));
            AssertField(fieldInfo, NullabilityState.NotNull);

            // GenericClass<string?> -> GenericClass2<string?>
            fieldInfo = fieldInfo.NullabilityFieldType.GetField(nameof(GenericClass<string>.Field));
            AssertField(fieldInfo, NullabilityState.NotNull);

            // GenericClass<string?> -> GenericClass2<string?> -> string?
            fieldInfo = fieldInfo.NullabilityFieldType.GetField(nameof(GenericClass2<string>.Field));
            AssertField(fieldInfo, NullabilityState.Nullable);

            // GenericClass<string?>?
            fieldInfo = rootType.GetField(nameof(TestClass.Field4));
            AssertField(fieldInfo, NullabilityState.Nullable);

            // GenericClass<string?>? -> GenericClass2<string?>
            fieldInfo = fieldInfo.NullabilityFieldType.GetField(nameof(GenericClass<string>.Field));
            AssertField(fieldInfo, NullabilityState.NotNull);

            // GenericClass<string?>? -> GenericClass2<string?> -> string?
            fieldInfo = fieldInfo.NullabilityFieldType.GetField(nameof(GenericClass2<string>.Field));
            AssertField(fieldInfo, NullabilityState.Nullable);
        }

        [Fact]
        public void GenericRootTypeTests()
        {
            // GenericClass<string>
            NullabilityType rootType = NullabilityType.GetType(typeof(GenericClass<string>), NullabilityState.NotNull);

            // GenericClass<string> -> GenericClass2<string>
            NullabilityFieldInfo? fieldInfo = rootType.GetField(nameof(GenericClass<string>.Field));
            AssertField(fieldInfo, NullabilityState.NotNull);

            // GenericClass<string> -> GenericClass2<string> -> string
            fieldInfo = fieldInfo.NullabilityFieldType.GetField(nameof(GenericClass2<string>.Field));
            AssertField(fieldInfo, NullabilityState.NotNull);

            // GenericClass<string?>
            rootType = NullabilityType.GetType(typeof(GenericClass<string?>), NullabilityState.Nullable);

            // GenericClass<string?> -> GenericClass2<string?>
            fieldInfo = rootType.GetField(nameof(GenericClass<string?>.Field));
            AssertField(fieldInfo, NullabilityState.NotNull);

            // GenericClass<string?> -> GenericClass2<string?> -> string?
            fieldInfo = fieldInfo.NullabilityFieldType.GetField(nameof(GenericClass2<string?>.Field));
            AssertField(fieldInfo, NullabilityState.Nullable);
        }

        [Fact]
        public void NestedGenericRootTypeTests()
        {
            // GenericClass2<GenericClass2<string>?>
            NullabilityType rootType = NullabilityType.GetType(typeof(GenericClass2<GenericClass2<string>?>), new[]{ new NullabilityElement(NullabilityState.Nullable, new[] { new NullabilityElement(NullabilityState.NotNull) }) });

            // GenericClass2<GenericClass2<string>?> -> GenericClass2<string>?
            NullabilityFieldInfo? fieldInfo = rootType.GetField(nameof(GenericClass2<string>.Field));
            AssertField(fieldInfo, NullabilityState.Nullable);

            // GenericClass2<GenericClass2<string>?> -> GenericClass2<string>? -> string
            fieldInfo = fieldInfo.NullabilityFieldType.GetField(nameof(GenericClass2<string>.Field));
            AssertField(fieldInfo, NullabilityState.NotNull);

            // GenericClass2<GenericClass2<string?>>
            rootType = NullabilityType.GetType(typeof(GenericClass2<GenericClass2<string?>>), new[]{ new NullabilityElement(NullabilityState.NotNull, new[] { new NullabilityElement(NullabilityState.Nullable) }) });

            // GenericClass2<GenericClass2<string?>> -> GenericClass2<string?>
            fieldInfo = rootType.GetField(nameof(GenericClass2<string?>.Field));
            AssertField(fieldInfo, NullabilityState.NotNull);

            // GenericClass2<GenericClass2<string?>> -> GenericClass2<string?> -> string?
            fieldInfo = fieldInfo.NullabilityFieldType.GetField(nameof(GenericClass2<string>.Field));
            AssertField(fieldInfo, NullabilityState.Nullable);
        }

        private static void AssertField([NotNull] NullabilityFieldInfo? fieldInfo, NullabilityState expectedState)
        {
            Assert.NotNull(fieldInfo);
            Assert.Equal(expectedState, fieldInfo.NullabilityState);
        }

        [Fact]
        public void GetSameField_ReturnSameFieldInstanceAndTypeInstance()
        {
            NullabilityType rootType = NullabilityType.GetType(typeof(TestClass));
            NullabilityFieldInfo fieldInfo1 = rootType.GetField(nameof(TestClass.Field1))!;
            NullabilityFieldInfo fieldInfo2 = rootType.GetField(nameof(TestClass.Field1))!;
            NullabilityFieldInfo fieldInfo3 = rootType.GetField(nameof(TestClass.Field1), BindingFlags.Public | BindingFlags.Instance)!;
            NullabilityFieldInfo fieldInfo4 = rootType.GetFields().First(p => p.Name == nameof(TestClass.Field1));
            NullabilityFieldInfo fieldInfo5 = rootType.GetFields(BindingFlags.Public | BindingFlags.Instance).First(p => p.Name == nameof(TestClass.Field1));

            Assert.All(new[] { fieldInfo1, fieldInfo2, fieldInfo3, fieldInfo4, fieldInfo5 }, field => Assert.Same(field, fieldInfo1));

            Assert.Same(fieldInfo1.NullabilityFieldType, fieldInfo1.NullabilityFieldType);
        }

        class TestClass
        {
            public GenericClass<string> Field1;
            public GenericClass<string>? Field2;
            public GenericClass<string?> Field3;
            public GenericClass<string?>? Field4;
        }

        class GenericClass<T>
        {
            public GenericClass2<T> Field;
        }

        class GenericClass2<T>
        {
            public T Field;
        }
    }
}