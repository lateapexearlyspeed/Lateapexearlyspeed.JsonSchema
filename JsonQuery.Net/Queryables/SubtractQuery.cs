using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace JsonQuery.Net.Queryables;

[JsonConverter(typeof(QueryCollectionConverter))]
public class SubtractQuery : IJsonQueryable, IMultipleSubQuery
{
    internal const string Keyword = "subtract";
    internal const string Operator = "-";

    private readonly IJsonQueryable[] _operandQueries;

    public SubtractQuery(IJsonQueryable[] operandQueries)
    {
        if (operandQueries.Length < 2)
        {
            throw new ArgumentException($"Operands count of '{Operator}' operator should be greater than 1.", nameof(operandQueries));
        }

        _operandQueries = operandQueries;
    }

    public JsonNode Query(JsonNode? data)
    {
        decimal[] decimalOperands = _operandQueries.Select(operandQuery => operandQuery.Query(data).GetDecimalValue()).ToArray();

        decimal firstOperand = decimalOperands[0];

        IEnumerable<decimal> subtractOperands = decimalOperands.Skip(1);

        return subtractOperands.Aggregate(firstOperand, (aggregateValue, cur) => aggregateValue - cur);
    }

    public IEnumerable<IJsonQueryable> SubQueries => _operandQueries;
}