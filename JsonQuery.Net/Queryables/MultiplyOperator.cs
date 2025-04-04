using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace JsonQuery.Net.Queryables;

[JsonConverter(typeof(QueryCollectionConverter))]
public class MultiplyOperator : IJsonQueryable, IMultipleSubQuery
{
    internal const string Keyword = "multiply";
    internal const string Operator = "*";

    private readonly IJsonQueryable[] _operandQueries;

    public MultiplyOperator(IJsonQueryable[] operandQueries)
    {
        if (operandQueries.Length < 2)
        {
            throw new ArgumentException($"Operands count of '{Operator}' operator should be greater than 1.", nameof(operandQueries));
        }

        _operandQueries = operandQueries;
    }

    public JsonNode Query(JsonNode? data)
    {
        IEnumerable<decimal> decimalOperands = _operandQueries.Select(operandQuery => operandQuery.Query(data).GetDecimalValue());

        return decimalOperands.Aggregate((aggregateValue, cur) => aggregateValue * cur);
    }

    public IEnumerable<IJsonQueryable> SubQueries => _operandQueries;
}