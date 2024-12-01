using LateApexEarlySpeed.Json.Schema.JSchema;
using LateApexEarlySpeed.Json.Schema.Keywords;
using System.Diagnostics;
using System.Reflection;
using LateApexEarlySpeed.Json.Schema.Generator.TypeAbstraction;

namespace LateApexEarlySpeed.Json.Schema.Generator.SchemaGenerators;

internal class CollectionSchemaGenerationCandidate : ISchemaGenerationCandidate
{
    private readonly HashSet<Type> _supportedGenericInterfaceDefinitions = new()
    {
        typeof(IEnumerable<>),
        typeof(ICollection<>),
        typeof(IReadOnlyCollection<>),
        typeof(IList<>),
        typeof(IReadOnlyList<>)
    };

    public bool CanGenerate(Type typeToConvert)
    {
        if (typeToConvert.GetInterface("IEnumerable`1") is null)
        {
            return false;
        }

        if (!typeToConvert.IsInterface)
        {
            return true;
        }

        return typeToConvert.IsGenericType && _supportedGenericInterfaceDefinitions.Contains(typeToConvert.GetGenericTypeDefinition());
    }

    public BodyJsonSchema Generate(IType typeToConvert, IEnumerable<KeywordBase> keywordsFromProperty, JsonSchemaGeneratorOptions options)
    {
        List<KeywordBase> keywords = new List<KeywordBase> { new TypeKeyword(InstanceType.Array, InstanceType.Null) };
        keywords.AddRange(keywordsFromProperty);

        IType elementType = GetElementType(typeToConvert);
        JsonSchema elementSchema = JsonSchemaGenerator.GenerateSchema(elementType, Enumerable.Empty<KeywordBase>(), options);

        JsonSchema itemsSchema;
        if (elementSchema is JsonSchemaResource elementSchemaResource)
        {
            options.SchemaDefinitions.AddSchemaDefinition(elementType.Type, elementSchemaResource);

            itemsSchema = SchemaGenerationHelper.GenerateSchemaReference(elementType.Type, Enumerable.Empty<KeywordBase>(), options.MainDocumentBaseUri!);
        }
        else
        {
            itemsSchema = elementSchema;
        }

        var itemsKeyword = new ItemsKeyword { Schema = itemsSchema };
        keywords.Add(itemsKeyword);

        return new BodyJsonSchema(keywords);
    }

    private static IType GetElementType(IType typeToConvert)
    {
        Type type = typeToConvert.Type;

        if (type.IsArray)
        {
            return typeToConvert.GetArrayElementType();
        }

        if (type.IsInterface)
        {
            Debug.Assert(typeToConvert.GenericTypeArguments.Length == 1);
            return typeToConvert.GenericTypeArguments[0];
        }

        Type? enumerableInterface = type.GetInterface("IEnumerable`1");
        Debug.Assert(enumerableInterface is not null);

        InterfaceMapping interfaceMapping = type.GetInterfaceMap(enumerableInterface);
        int getEnumeratorIdx = Array.IndexOf(interfaceMapping.InterfaceMethods, enumerableInterface.GetMethod(nameof(IEnumerable<int>.GetEnumerator)));
        Debug.Assert(getEnumeratorIdx != -1);
        MethodInfo getEnumeratorMethodInfo = interfaceMapping.TargetMethods[getEnumeratorIdx];

        IMethodInfo getEnumerator = typeToConvert.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).Single(method => method.MethodInfo == getEnumeratorMethodInfo);
        IType enumeratorType = getEnumerator.ReturnParameter.ParameterType;
        return enumeratorType.GenericTypeArguments[0];
    }
}