using System.Reflection;
using JsonQuery.Net.Queryables;

namespace JsonQuery.Net;

internal static class OperatorRegistry
{
    private static readonly Dictionary<string, Type> Operators;

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
        Operators = Assembly.GetExecutingAssembly().GetTypes().Where(type => !type.IsAbstract && typeof(OperatorQuery).IsAssignableFrom(type)).ToDictionary(type => (string)type.GetField("Operator", BindingFlags.Static | BindingFlags.NonPublic)!.GetRawConstantValue());
    }

    public static Type FindOperatorType(string operatorName) => Operators[operatorName];
}