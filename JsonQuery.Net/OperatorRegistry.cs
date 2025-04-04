using System.Reflection;
using JsonQuery.Net.Queryables;

namespace JsonQuery.Net;

internal static class OperatorRegistry
{
    private static readonly Dictionary<string, Type> OperatorNameTypeMaps;
    private static readonly Dictionary<Type, int> OperatorTypePrecedenceMaps = new();

    /// <summary>
    /// Operator precedence from highest to lowest
    /// </summary>
    private static readonly Type[][] PrecedenceOperatorsTable = new[]
    {
        new[] { typeof(PowQuery) },
        new[] { typeof(MultiplyOperator), typeof(DivideQuery), typeof(ModQuery) },
        new[] { typeof(AddOperator), typeof(SubtractQuery) },
        new[] { typeof(GtQuery), typeof(GteQuery), typeof(LtQuery), typeof(LteQuery), typeof(InQuery), typeof(NotInQuery) },
        new[] { typeof(EqQuery), typeof(NeQuery) },
        new[] { typeof(AndQuery) },
        new[] { typeof(OrQuery) }
    };

    private static readonly HashSet<Type> VarargOperatorTypes = new HashSet<Type>
    {
        typeof(AddOperator),
        typeof(SubtractQuery),
        typeof(MultiplyOperator),
        typeof(DivideQuery),
        typeof(ModQuery),

        typeof(AndQuery),
        typeof(OrQuery)
    };

    public static string[] SortedOperatorKeywords { get; } = new[]
    {
        EqQuery.Operator,
        GteQuery.Operator,
        GtQuery.Operator,
        LteQuery.Operator,
        LtQuery.Operator,
        NeQuery.Operator,
        AndQuery.Operator,
        OrQuery.Operator,
        NotInQuery.Operator,
        InQuery.Operator,
        AddOperator.Operator,
        SubtractQuery.Operator,
        MultiplyOperator.Operator,
        DivideQuery.Operator,
        ModQuery.Operator,
        PowQuery.Operator
    };

    static OperatorRegistry()
    {
        OperatorNameTypeMaps = PrecedenceOperatorsTable.SelectMany(operatorsInSamePrecedence => operatorsInSamePrecedence).ToDictionary(type => (string)type.GetField("Operator", BindingFlags.Static | BindingFlags.NonPublic)!.GetRawConstantValue());

        for (int precedence = 0; precedence < PrecedenceOperatorsTable.Length; precedence++)
        {
            Type[] operatorTypes = PrecedenceOperatorsTable[precedence];

            foreach (Type operatorType in operatorTypes)
            {
                OperatorTypePrecedenceMaps.Add(operatorType, precedence);
            }
        }
    }

    public static Type FindOperatorType(string operatorName) => OperatorNameTypeMaps[operatorName];
    
    public static int FindOperatorPrecedenceValue(string operatorName)
    {
        Type operatorType = FindOperatorType(operatorName);
        return OperatorTypePrecedenceMaps[operatorType];
    }

    public static bool IsVarargOperator(string operatorName)
    {
        Type operatorType = FindOperatorType(operatorName);
        return VarargOperatorTypes.Contains(operatorType);
    }
}