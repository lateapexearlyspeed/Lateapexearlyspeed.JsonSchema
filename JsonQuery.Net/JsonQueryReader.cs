using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace JsonQuery.Net;

public ref struct JsonQueryReader
{
    private static readonly HashSet<char> Separators = new() { ' ', ',', '=', '(', ')', '[', ']', '{', '}', ':', '=', '>', '<', '!', '+', '-', '*', '/', '^', '%' };

    private readonly Stack<ReaderStackPosition> _positionStack;
    private readonly ReadOnlySpan<char> _jsonQuery;

    /// <summary>
    /// although external concept of position of <see cref="JsonQueryReader"/> is the current token, <see cref="_index"/> points to next char to be read.
    /// </summary>
    private int _index;
    
    private object[]? _propertyPath;
    private string? _stringLiteral;
    private string? _propertyName;
    private string? _functionName;
    private string? _operatorValue;
    private decimal _decimalValue;

    public JsonQueryReader(ReadOnlySpan<char> jsonQuery) : this()
    {
        _jsonQuery = jsonQuery;
        _positionStack = new Stack<ReaderStackPosition>();
    }

    private JsonQueryReader(JsonQueryReader reader)
    {
        this = reader;
        _positionStack = new Stack<ReaderStackPosition>(reader._positionStack.Reverse());
    }

    public JsonQueryTokenType TokenType { get; private set; }
    public bool IsNull { get; private set; }

    public bool Read()
    {
        if (!SkipWhiteSpace())
        {
            IsNull = true;
            return false;
        }

        if (_jsonQuery[_index] == ',')
        {
            if (_positionStack.TryPeek(out ReaderStackPosition position1) && position1 == ReaderStackPosition.InObjectKeyValuePair)
            {
                _positionStack.Pop();
            }

            _index++;
            SkipWhiteSpace();
        }
        else if (_jsonQuery[_index] == ':')
        {
            _index++;
            SkipWhiteSpace();
        }

        if (_jsonQuery[_index] == '.')
        {
            ReadPropertyPath();
            return true;
        }

        if (_jsonQuery[_index] == '"')
        {
            ReadStringLiteral();
            return true;
        }

        if (_positionStack.TryPeek(out ReaderStackPosition position) && position == ReaderStackPosition.InObject)
        {
            Debug.Assert(_jsonQuery[_index] != '"');

            ReadUnquotedPropertyName();
            return true;
        }

        if (_jsonQuery[_index] == '(')
        {
            if (TokenType != JsonQueryTokenType.FunctionName)
            {
                _positionStack.Push(ReaderStackPosition.InParenthesis);
            }

            TokenType = JsonQueryTokenType.StartParenthesis;
            _index++;

            return true;
        }

        if (_jsonQuery[_index] == ')')
        {
            TokenType = JsonQueryTokenType.EndParenthesis;
            _index++;
            _positionStack.Pop();

            return true;
        }

        if (_jsonQuery[_index] == '[')
        {
            TokenType = JsonQueryTokenType.StartBracket;
            _index++;
            _positionStack.Push(ReaderStackPosition.InArray);

            return true;
        }

        if (_jsonQuery[_index] == ']')
        {
            TokenType = JsonQueryTokenType.EndBracket;
            _index++;
            _positionStack.Pop();

            return true;
        }

        if (_jsonQuery[_index] == '{')
        {
            TokenType = JsonQueryTokenType.StartBrace;
            _index++;
            _positionStack.Push(ReaderStackPosition.InObject);

            return true;
        }

        if (_jsonQuery[_index] == '}')
        {
            TokenType = JsonQueryTokenType.EndBrace;
            _index++;

            if (_positionStack.Peek() == ReaderStackPosition.InObjectKeyValuePair)
            {
                _positionStack.Pop();
            }
            
            _positionStack.Pop();

            return true;
        }

        if (_jsonQuery[_index] == '|')
        {
            TokenType = JsonQueryTokenType.Pipe;
            _index++;
            return true;
        }

        _operatorValue = FindOperator();
        if (_operatorValue is not null)
        {
            TokenType = JsonQueryTokenType.Operator;
            _index += _operatorValue.Length;

            return true;
        }

        int idx = _index;

        while (!HitSeparators(idx))
        {
            idx++;
        }

        ReadOnlySpan<char> tokenSegment = _jsonQuery.Slice(_index, idx - _index);

        if (!OutOfJsonQuery(idx) && _jsonQuery[idx] == '(')
        {
            _functionName = tokenSegment.ToString();
            TokenType = JsonQueryTokenType.FunctionName;
            _index = idx;
            _positionStack.Push(ReaderStackPosition.InFunction);

            return true;
        }

        if (tokenSegment.Equals("true", StringComparison.Ordinal))
        {
            TokenType = JsonQueryTokenType.True;
            _index = idx;

            return true;
        }

        if (tokenSegment.Equals("false", StringComparison.Ordinal))
        {
            TokenType = JsonQueryTokenType.False;
            _index = idx;

            return true;
        }

        if (tokenSegment.Equals("null", StringComparison.Ordinal))
        {
            TokenType = JsonQueryTokenType.Null;
            _index = idx;

            return true;
        }

        _decimalValue = decimal.Parse(tokenSegment);
        TokenType = JsonQueryTokenType.Number;
        _index = idx;

        return true;
    }

    private string? FindOperator()
    {
        ReadOnlySpan<char> queryPart = _jsonQuery.Slice(_index);

        foreach (string operatorKeyword in OperatorRegistry.SortedOperatorKeywords)
        {
            if (queryPart.StartsWith(operatorKeyword))
            {
                return operatorKeyword;
            }
        }

        return null;
    }

    private void ReadUnquotedPropertyName()
    {
        int idx = _index;
        while (!HitSeparators(idx))
        {
            idx++;
        }

        _propertyName = _jsonQuery.Slice(_index, idx - _index).ToString();
        _index = idx;
        TokenType = JsonQueryTokenType.PropertyName;
        _positionStack.Push(ReaderStackPosition.InObjectKeyValuePair);
    }

    private void ReadStringLiteral()
    {
        Debug.Assert(_jsonQuery[_index] == '"');

        ReadOnlySpan<char> stringPart = _jsonQuery.Slice(_index + 1);
        int length = stringPart.IndexOf('"');

        if (length == -1)
        {
            throw new Exception();
        }

        string stringValue = stringPart.Slice(0, length).ToString();
        if (_positionStack.TryPeek(out ReaderStackPosition position) && position == ReaderStackPosition.InObject)
        {
            _propertyName = stringValue;
            TokenType = JsonQueryTokenType.PropertyName;
            _positionStack.Push(ReaderStackPosition.InObjectKeyValuePair);
        }
        else
        {
            _stringLiteral = stringValue;
            TokenType = JsonQueryTokenType.String;
        }
        
        _index += length + 2;
    }

    /// <returns>false when <see cref="_index"/> is out of <see cref="_jsonQuery"/>; otherwise true</returns>
    private bool SkipWhiteSpace()
    {
        while (!OutOfJsonQuery(_index))
        {
            if (char.IsWhiteSpace(_jsonQuery[_index]))
            {
                _index++;
            }
            else
            {
                return true;
            }
        }

        return false;
    }

    private void ReadPropertyPath()
    {
        Debug.Assert(_jsonQuery[_index] == '.');

        var propertiesPath = new List<object>();
        int startIdx = _index + 1;
        int idx = startIdx;
        while (true)
        {
            if (HitSeparators(idx) || _jsonQuery[idx] == '.')
            {
                ReadOnlySpan<char> onePropertySpan = _jsonQuery.Slice(startIdx, idx - startIdx);
                if (int.TryParse(onePropertySpan, out int arrayIdx))
                {
                    propertiesPath.Add(arrayIdx);
                }
                else
                {
                    propertiesPath.Add(onePropertySpan.ToString());
                }

                startIdx = idx + 1;
            }

            if (HitSeparators(idx))
            {
                break;
            }

            idx++;
        }

        TokenType = JsonQueryTokenType.PropertyPath;
        _propertyPath = propertiesPath.ToArray();
        _index = idx;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private readonly bool OutOfJsonQuery(int index)
    {
        return index >= _jsonQuery.Length;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private readonly bool HitSeparators(int index)
    {
        if (OutOfJsonQuery(index))
        {
            return true;
        }

        char c = _jsonQuery[index];
        return Separators.Contains(c) || char.IsWhiteSpace(c);
    }

    public JsonQueryReader Next()
    {
        JsonQueryReader copiedReader = new JsonQueryReader(this);
        copiedReader.Read();

        return copiedReader;
    }

    public string GetFunctionName()
    {
        if (TokenType != JsonQueryTokenType.FunctionName)
        {
            throw new InvalidOperationException($"Current token type is {TokenType}");
        }

        Debug.Assert(_functionName is not null);
        return _functionName;
    }

    public object[] GetPropertyPath()
    {
        if (TokenType != JsonQueryTokenType.PropertyPath)
        {
            throw new InvalidOperationException($"Current token type is {TokenType}");
        }

        Debug.Assert(_propertyPath is not null);
        return _propertyPath;
    }

    public string GetPropertyName()
    {
        if (TokenType != JsonQueryTokenType.PropertyName)
        {
            throw new InvalidOperationException($"Current token type is {TokenType}");
        }

        Debug.Assert(_propertyName is not null);
        return _propertyName;
    }

    public string GetString()
    {
        if (TokenType != JsonQueryTokenType.String)
        {
            throw new InvalidOperationException($"Current token type is {TokenType}");
        }

        Debug.Assert(_stringLiteral is not null);
        return _stringLiteral;
    }

    public decimal GetDecimal()
    {
        if (TokenType != JsonQueryTokenType.Number)
        {
            throw new InvalidOperationException($"Current token type is {TokenType}");
        }

        return _decimalValue;
    }

    public string GetOperator()
    {
        if (TokenType != JsonQueryTokenType.Operator)
        {
            throw new InvalidOperationException($"Current token type is {TokenType}");
        }

        Debug.Assert(_operatorValue is not null);
        return _operatorValue;
    }
}

public enum JsonQueryTokenType
{
    FunctionName,
    PropertyPath,
    PropertyName,
    StartParenthesis,
    EndParenthesis,
    Pipe,
    Operator,
    String,
    Number,
    Null,
    StartBrace,
    EndBrace,
    EndBracket,
    StartBracket,
    True,
    False
}

internal enum ReaderStackPosition
{
    InFunction,
    InParenthesis,
    InArray,
    InObject,
    InObjectKeyValuePair
}