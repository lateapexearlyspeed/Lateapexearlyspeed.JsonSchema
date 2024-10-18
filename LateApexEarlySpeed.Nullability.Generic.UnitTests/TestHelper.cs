using LateApexEarlySpeed.Nullability.Generic.RawNullabilityAnnotation;

namespace LateApexEarlySpeed.Nullability.Generic.UnitTests;

internal class TestHelper
{
    public static IEnumerable<object[]> GenerateNullabilityElements(Type type, string propertyName, string fieldName, string methodName, Type? testBaseClass = null)
    {
        yield return new object[] { RawNullabilityAnnotationConverter.ReadPropertyGetter(type.GetProperty(propertyName)!), true };
        yield return new object[] { RawNullabilityAnnotationConverter.ReadPropertySetter(type.GetProperty(propertyName)!), true };
        yield return new object[] { RawNullabilityAnnotationConverter.ReadField(type.GetField(fieldName)!), true };
        yield return new object[] { RawNullabilityAnnotationConverter.ReadParameter(type.GetMethod(methodName)!.GetParameters()[0]), true };
        yield return new object[] { RawNullabilityAnnotationConverter.ReadParameter(type.GetMethod(methodName)!.ReturnParameter), true };
        
        if (testBaseClass is not null)
        {
            yield return new object[] { RawNullabilityAnnotationConverter.ReadBaseClass(testBaseClass), false };
        }
    }
}