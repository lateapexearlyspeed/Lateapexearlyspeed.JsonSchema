using System.Diagnostics;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace JsonQuery.Net.Queryables;

[JsonConverter(typeof(QueryCollectionConverter))]
public class DivideQuery : IJsonQueryable, IMultipleSubQuery
{
    internal const string Keyword = "divide";
    internal const string Operator = "/";

    private readonly IJsonQueryable[] _operandQueries;

    public DivideQuery(IJsonQueryable[] operandQueries)
    {
        if (operandQueries.Length < 2)
        {
            throw new ArgumentException($"Operands count of '{Operator}' operator should be greater than 1.", nameof(operandQueries));
        }

        _operandQueries = operandQueries;
    }

    public JsonNode? Query(JsonNode? data)
    {
        IEnumerable<decimal> decimalOperands = _operandQueries.Select(operandQuery => operandQuery.Query(data).GetDecimalValue());

        using (IEnumerator<decimal> enumerator = decimalOperands.GetEnumerator())
        {
            bool moveNext = enumerator.MoveNext();

            Debug.Assert(moveNext);

            decimal result = enumerator.Current;

            while (enumerator.MoveNext())
            {
                if (enumerator.Current == 0)
                {
                    return null;
                }

                result /= enumerator.Current;
            }

            return result;
        }
    }

    public IEnumerable<IJsonQueryable> SubQueries => _operandQueries;
}