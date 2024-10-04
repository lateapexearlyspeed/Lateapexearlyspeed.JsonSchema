# LateApexEarlySpeed.Nullability.Generic

Nullability annotation info reader for .net types based on reflection and standard NullabilityInfoContext. Key improvement compared with NullabilityInfoContext is for more nullability info support of Generic type.

Starting from .net 6, there is built-in Nullability related classes to help read nullability annotation info on members of type (`NullabilityInfoContext` and so on). However, it is not possible to get annotated nullability state of members and parameters if their types are from generic type arguments:

```csharp
class Class1
{
    public GenericClass<string> Property { get; }
}

class GenericClass<T>
{
    public T Property { get; }
}

PropertyInfo property = typeof(Class1).GetProperty("Property")!.PropertyType.GetProperty("Property")!;
NullabilityInfo state = new NullabilityInfoContext().Create(property);
Assert.Equal(NullabilityState.Nullable, state.ReadState); // expected nullability state of ‘string’ property is NotNull
```

this library can get NotNull state for this 'string' property whose type is from generic type argument:

```csharp
NullabilityPropertyInfo propertyInfo = NullabilityType.GetType(typeof(Class1)).GetProperty("Property")!.NullabilityPropertyType.GetProperty("Property")!;
Assert.Equal(NullabilityState.NotNull, propertyInfo.NullabilityReadState);
```

There is no information to help infer nullability info of generic type arguments of 'root' type, so if 'root' type is generic type, this library accepts explicit nullability info of generic type arguments for 'root' type and then process all properties, fields, parameters as normal:

```csharp
NullabilityPropertyInfo propertyInfo = NullabilityType.GetType(typeof(GenericClass<string>), NullabilityState.NotNull).GetProperty("Property")!;
Assert.Equal(NullabilityState.NotNull, propertyInfo.NullabilityReadState);
```

even if with nested properties:

```csharp
class GenericClass<T>
{
    public GenericClass2<int?, T> Property { get; }
}

class GenericClass2<T1, T2>
{
    public T1 Property1 { get; }
    public T2 Property2 { get; }
    public string? Property3 { get; }
}

NullabilityPropertyInfo property = NullabilityType.GetType(typeof(GenericClass<string>), NullabilityState.NotNull).GetProperty("Property")!;
Assert.Equal(NullabilityState.NotNull, property.NullabilityReadState); // GenericClass2<int?, T>

NullabilityType type = property.NullabilityPropertyType;
Assert.Equal(NullabilityState.Nullable, type.GetProperty("Property1")!.NullabilityReadState); // int?
Assert.Equal(NullabilityState.NotNull, type.GetProperty("Property2")!.NullabilityReadState); // string
Assert.Equal(NullabilityState.Nullable, type.GetProperty("Property3")!.NullabilityReadState); // string?
```

Library also supports info of fields and parameters, take parameter as example:

```csharp
        class Class1
        {
            public GenericClass<string, string?, int, int?> Property { get; }
        }

        class GenericClass<T1, T2, T3, T4>
        {
            public GenericClass2<T1, T2>? Function(T2 p0, T3 p1, T4 p2, string p3, string? p4)
            {
                throw new NotImplementedException();
            }
        }

        class GenericClass2<T1, T2>
        {
            public T1 Property1 { get; }
            public T2 Property2 { get; }
            public string? Property3 { get; }
        }

        NullabilityMethodInfo method = NullabilityType.GetType(typeof(Class1)).GetProperty("Property")!.NullabilityPropertyType.GetMethod("Function")!;

        // GenericClass2<T1, T2>?
        NullabilityParameterInfo returnParameter = method.NullabilityReturnParameter;
        Assert.Equal(NullabilityState.Nullable, returnParameter.NullabilityState);
        NullabilityType returnType = returnParameter.NullabilityParameterType;
        Assert.Equal(NullabilityState.NotNull, returnType.GenericTypeArguments[0].NullabilityState); // string
        Assert.Equal(NullabilityState.Nullable, returnType.GenericTypeArguments[1].NullabilityState); // string?

        Assert.Equal(NullabilityState.NotNull, returnType.GetProperty("Property1")!.NullabilityReadState); // string
        Assert.Equal(NullabilityState.Nullable, returnType.GetProperty("Property2")!.NullabilityReadState); // string?

        // string? p0, int p1, int? p2, string p3, string? p4
        NullabilityParameterInfo[] parameters = method.GetNullabilityParameters();
        Assert.Equal(NullabilityState.Nullable, parameters[0].NullabilityState);
        Assert.Equal(NullabilityState.NotNull, parameters[1].NullabilityState);
        Assert.Equal(NullabilityState.Nullable, parameters[2].NullabilityState);
        Assert.Equal(NullabilityState.NotNull, parameters[3].NullabilityState);
        Assert.Equal(NullabilityState.Nullable, parameters[4].NullabilityState);
```

Calling entrypoint is static method `NullabilityType.GetType()` which has 3 overloads:

```csharp
NullabilityType GetType(Type type) // when type itself is not generic type
NullabilityType GetType(Type type, params NullabilityState[] genericTypeArgumentsNullabilities) // when type is generic type is generic type and its generic type arguments are not generic types
NullabilityType GetType(Type type, params NullabilityElement[] genericTypeArgumentsNullabilities) // when type is generic type and its generic type arguments are also generic type
```