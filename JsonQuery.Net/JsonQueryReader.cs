using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text.Json;

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

    /// <summary>
    /// external concept of position of <see cref="JsonQueryReader"/>, which is start char position of current token.
    /// </summary>
    public int Position { get; private set; }

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

    internal int PositionStackCount => _positionStack.Count;

    public bool Read()
    {
        ReaderStackPosition position;

        if (!SkipWhiteSpace())
        {
            if (TokenType == JsonQueryTokenType.None)
            {
                throw new JsonQueryParseException("Empty json query is invalid", _index);
            }

            if (TokenType == JsonQueryTokenType.Pipe)
            {
                throw new JsonQueryParseException("Pipe missing 'right hand value'", _index);
            }

            // check missing 'right hand value' firstly
            if (TokenType == JsonQueryTokenType.Operator)
            {
                throw new JsonQueryParseException("Operator missing 'right hand value'", _index);
            }

            if (_positionStack.TryPeek(out position) && position == ReaderStackPosition.MeetOperator)
            {
                _positionStack.Pop();
            }

            if (_positionStack.TryPop(out position)) // (a or (a + b
            {
                char missingToken;
                if (position == ReaderStackPosition.InArray)
                {
                    missingToken = ']';
                }
                else if (position == ReaderStackPosition.InObject || position == ReaderStackPosition.InObjectKeyValuePair)
                {
                    missingToken = '}';
                }
                else
                {
                    Debug.Assert(position == ReaderStackPosition.InFunction || position == ReaderStackPosition.InParenthesis);

                    missingToken = ')';
                }

                throw new JsonQueryParseException($"Missing '{missingToken}'", _index);
            }

            IsNull = true;
            return false;
        }

        bool meetComma = false;
        bool meetColon = false;

        if (_jsonQuery[_index] == ',')
        {
            if (_positionStack.TryPeek(out position) && position == ReaderStackPosition.MeetOperator)
            {
                if (TokenType == JsonQueryTokenType.Operator)
                {
                    throw new JsonQueryParseException("Unexpected ','", _index);
                }

                _positionStack.Pop();
            }

            bool shouldThrow = false;

            if (_positionStack.TryPeek(out position))
            {
                if (position == ReaderStackPosition.InArray)
                {
                    if (TokenType == JsonQueryTokenType.StartBracket || TokenType == JsonQueryTokenType.Pipe || TokenType == JsonQueryTokenType.Operator)
                    {
                        shouldThrow = true;
                    }
                }
                else if (position == ReaderStackPosition.InFunction)
                {
                    if (TokenType == JsonQueryTokenType.FunctionName || TokenType == JsonQueryTokenType.StartParenthesis || TokenType == JsonQueryTokenType.Pipe || TokenType == JsonQueryTokenType.Operator)
                    {
                        shouldThrow = true;
                    }
                }
                else if (position == ReaderStackPosition.InParenthesis)
                {
                    shouldThrow = true;
                }
                else if (position == ReaderStackPosition.InObject)
                {
                    shouldThrow = true;
                }
                else
                {
                    Debug.Assert(position == ReaderStackPosition.InObjectKeyValuePair);
                    
                    if (TokenType == JsonQueryTokenType.PropertyName)
                    {
                        shouldThrow = true;
                    }
                }
            }
            else
            {
                shouldThrow = true;
            }

            if (shouldThrow)
            {
                throw new JsonQueryParseException("Unexpected ','", _index);
            }

            if (_positionStack.TryPeek(out position) && position == ReaderStackPosition.InObjectKeyValuePair)
            {
                _positionStack.Pop();
            }

            meetComma = true;
            _index++;
            if (!SkipWhiteSpace())
            {
                throw new JsonQueryParseException("Missing token", _index);
            }
        }
        else if (_jsonQuery[_index] == ':')
        {
            if (TokenType != JsonQueryTokenType.PropertyName)
            {
                throw new JsonQueryParseException("Unexpected ':'", _index);
            }

            meetColon = true;
            _index++;
            if (!SkipWhiteSpace())
            {
                throw new JsonQueryParseException("Missing property value", _index);
            }
        }

        Position = _index;

        if (_jsonQuery[_index] == '.')
        {
            ValidateValueToken(meetComma, meetColon, ".");

            ReadPropertyPath();
            return true;
        }

        if (_jsonQuery[_index] == '}')
        {
            if (_positionStack.TryPeek(out position) && position == ReaderStackPosition.MeetOperator)
            {
                if (TokenType == JsonQueryTokenType.Operator)
                {
                    throw new JsonQueryParseException("Unexpected '}'", _index);
                }

                _positionStack.Pop();
            }

            if (_positionStack.TryPop(out position))
            {
                if (position == ReaderStackPosition.InObjectKeyValuePair)
                {
                    if (TokenType == JsonQueryTokenType.PropertyName)
                    {
                        throw new JsonQueryParseException("Unexpected '}'", _index);
                    }

                    position = _positionStack.Pop();
                    Debug.Assert(position == ReaderStackPosition.InObject);
                }
                else if (position != ReaderStackPosition.InObject)
                {
                    throw new JsonQueryParseException("Unexpected '}'", _index);
                }
            }
            else
            {
                throw new JsonQueryParseException("Unexpected '}'", _index);
            }

            TokenType = JsonQueryTokenType.EndBrace;
            _index++;

            return true;
        }

        // Property name check should be after processing of '}' for case: { a : b, }
        if (_positionStack.TryPeek(out position) && position == ReaderStackPosition.InObject)
        {
            ReadObjectPropertyName();
            return true;
        }

        if (_jsonQuery[_index] == '"')
        {
            ReadStringLiteral(meetComma, meetColon);
            return true;
        }

        if (_jsonQuery[_index] == '(')
        {
            if (TokenType != JsonQueryTokenType.FunctionName)
            {
                ValidateValueToken(meetComma, meetColon, "(");

                _positionStack.Push(ReaderStackPosition.InParenthesis);
            }

            TokenType = JsonQueryTokenType.StartParenthesis;
            _index++;

            return true;
        }

        if (_jsonQuery[_index] == ')')
        {
            ValidateEndParenthesisChar(meetComma);

            position = _positionStack.Pop();

            if (position == ReaderStackPosition.MeetOperator)
            {
                position = _positionStack.Pop();
            }

            Debug.Assert(position == ReaderStackPosition.InFunction || position == ReaderStackPosition.InParenthesis);

            TokenType = JsonQueryTokenType.EndParenthesis;
            _index++;

            return true;
        }

        if (_jsonQuery[_index] == '[')
        {
            ValidateValueToken(meetComma, meetColon, "[");

            TokenType = JsonQueryTokenType.StartBracket;
            _index++;
            _positionStack.Push(ReaderStackPosition.InArray);

            return true;
        }

        if (_jsonQuery[_index] == ']')
        {
            ValidateEndBracketChar(meetComma);

            position = _positionStack.Pop();
            if (position == ReaderStackPosition.MeetOperator)
            {
                position = _positionStack.Pop();
            }
            Debug.Assert(position == ReaderStackPosition.InArray);

            TokenType = JsonQueryTokenType.EndBracket;
            _index++;

            return true;
        }

        if (_jsonQuery[_index] == '{')
        {
            ValidateValueToken(meetComma, meetColon, "{");

            TokenType = JsonQueryTokenType.StartBrace;
            _index++;
            _positionStack.Push(ReaderStackPosition.InObject);

            return true;
        }

        if (_jsonQuery[_index] == '|')
        {
            ValidatePipeChar(meetComma);

            if (_positionStack.TryPeek(out position) && position == ReaderStackPosition.MeetOperator)
            {
                _positionStack.Pop();
            }

            TokenType = JsonQueryTokenType.Pipe;
            _index++;
            return true;
        }

        if (ValidateOperator(meetComma))
        {
            string? operatorValue = FindOperator();
            if (operatorValue is not null)
            {
                _operatorValue = operatorValue;

                _positionStack.Push(ReaderStackPosition.MeetOperator);
                TokenType = JsonQueryTokenType.Operator;
                _index += _operatorValue.Length;

                return true;
            }
        }

        int idx = _index;

        while (!HitSeparators(idx))
        {
            idx++;
        }

        ReadOnlySpan<char> tokenSegment = _jsonQuery.Slice(_index, idx - _index);

        ValidateValueToken(meetComma, meetColon, tokenSegment.ToString());

        // This is for "foo ()", which should be treated as a valid function.
        WalkOutOfWhiteSpaces(ref idx);

        if (!OutOfJsonQuery(idx) && _jsonQuery[idx] == '(')
        {
            _functionName = tokenSegment.ToString();

            if (!JsonQueryableRegistry.TryGetQueryableType(_functionName, out _))
            {
                throw new JsonQueryParseException($"Invalid function: '{_functionName}'", _index);
            }
            
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

        if (TryParseNumericValue(out _decimalValue, out idx))
        {
            TokenType = JsonQueryTokenType.Number;
            _index = idx;

            return true;
        }

        throw new JsonQueryParseException($"Unexpected token: '{tokenSegment.ToString()}'", _index);
    }

    private bool TryParseNumericValue(out decimal numericValue, out int idx)
    {
        idx = _index;

        if (_jsonQuery[idx] == '+' || _jsonQuery[idx] == '-') // jump out first '+' or '-'
        {
            idx++;
        }

        while (!HitSeparators(idx))
        {
            idx++;
        }

        if (decimal.TryParse(_jsonQuery.Slice(_index, idx - _index), NumberStyles.Any, CultureInfo.InvariantCulture, out numericValue))
        {
            return true;
        }

        if (!OutOfJsonQuery(idx) 
            && (_jsonQuery[idx] == '+' || _jsonQuery[idx] == '-') 
            && idx > 0 
            && (_jsonQuery[idx - 1] == 'e' || _jsonQuery[idx - 1] == 'E')) // 12e+3
        {
            idx++;

            while (!HitSeparators(idx))
            {
                idx++;
            }

            var numericString = _jsonQuery.Slice(_index, idx - _index).ToString();
            if (decimal.TryParse(numericString, NumberStyles.AllowExponent | NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out numericValue))
            {
                return true;
            }
        }

        return false;
    }

    private bool ValidateOperator(bool meetComma)
    {
        if (_positionStack.TryPeek(out ReaderStackPosition position) && position == ReaderStackPosition.MeetOperator)
        {
            return false;
        }

        if (meetComma || TokenType == JsonQueryTokenType.PropertyName || TokenType == JsonQueryTokenType.None
            || TokenType == JsonQueryTokenType.StartBrace || TokenType == JsonQueryTokenType.StartBracket
            || TokenType == JsonQueryTokenType.StartParenthesis || TokenType == JsonQueryTokenType.FunctionName
            || TokenType == JsonQueryTokenType.Pipe)
        {
            return false;
        }

        return true;
    }

    private void ValidateEndParenthesisChar(bool meetComma)
    {
        bool needToRestoreOperator = false;

        if (_positionStack.TryPeek(out ReaderStackPosition position) && position == ReaderStackPosition.MeetOperator)
        {
            if (TokenType == JsonQueryTokenType.Operator)
            {
                throw new JsonQueryParseException("Unexpected ')'", _index);
            }

            _positionStack.Pop();
            needToRestoreOperator = true;
        }

        bool shouldThrow = false;

        if (_positionStack.TryPeek(out position) && (position == ReaderStackPosition.InFunction || position == ReaderStackPosition.InParenthesis))
        {
            if (meetComma || TokenType == JsonQueryTokenType.Operator || TokenType == JsonQueryTokenType.Pipe)
            {
                shouldThrow = true;
            }
        }
        else
        {
            shouldThrow = true;
        }

        if (shouldThrow)
        {
            throw new JsonQueryParseException("Unexpected ')'", _index);
        }

        if (needToRestoreOperator)
        {
            _positionStack.Push(ReaderStackPosition.MeetOperator);
        }
    }

    private void ValidateEndBracketChar(bool meetComma)
    {
        bool needToRestoreOperator = false;

        if (_positionStack.TryPeek(out ReaderStackPosition position) && position == ReaderStackPosition.MeetOperator)
        {
            if (TokenType == JsonQueryTokenType.Operator)
            {
                throw new JsonQueryParseException("Unexpected ']'", _index);
            }

            _positionStack.Pop();
            needToRestoreOperator = true;
        }

        bool shouldThrow = false;

        if (_positionStack.TryPeek(out position) && position == ReaderStackPosition.InArray)
        {
            if (meetComma || TokenType == JsonQueryTokenType.Operator || TokenType == JsonQueryTokenType.Pipe)
            {
                shouldThrow = true;
            }
        }
        else
        {
            shouldThrow = true;
        }

        if (shouldThrow)
        {
            throw new JsonQueryParseException("Unexpected ']'", _index);
        }

        if (needToRestoreOperator)
        {
            _positionStack.Push(ReaderStackPosition.MeetOperator);
        }
    }

    private void ValidatePipeChar(bool meetComma)
    {
        bool needToRestoreOperator = false;

        if (_positionStack.TryPeek(out ReaderStackPosition position) && position == ReaderStackPosition.MeetOperator)
        {
            if (TokenType == JsonQueryTokenType.Operator)
            {
                throw new JsonQueryParseException("Unexpected '|'", _index);
            }

            _positionStack.Pop();
            needToRestoreOperator = true;
        }

        bool shouldThrow = false;

        if (_positionStack.TryPeek(out position))
        {
            if (position == ReaderStackPosition.InArray)
            {
                if (TokenType == JsonQueryTokenType.StartBracket || meetComma || TokenType == JsonQueryTokenType.Operator)
                {
                    shouldThrow = true;
                }
            }
            else if (position == ReaderStackPosition.InFunction || position == ReaderStackPosition.InParenthesis)
            {
                if (meetComma || TokenType == JsonQueryTokenType.StartParenthesis || TokenType == JsonQueryTokenType.Operator)
                {
                    shouldThrow = true;
                }
            }
            else if (position == ReaderStackPosition.InObject)
            {
                shouldThrow = true;
            }
            else
            {
                Debug.Assert(position == ReaderStackPosition.InObjectKeyValuePair);

                if (TokenType == JsonQueryTokenType.PropertyName || TokenType == JsonQueryTokenType.Operator)
                {
                    shouldThrow = true;
                }
            }
        }
        else
        {
            if (TokenType == JsonQueryTokenType.None || TokenType == JsonQueryTokenType.Operator)
            {
                shouldThrow = true;
            }
        }

        if (shouldThrow)
        {
            throw new JsonQueryParseException("Unexpected '|'", _index);
        }

        if (needToRestoreOperator)
        {
            _positionStack.Push(ReaderStackPosition.MeetOperator);
        }
    }

    private void ValidateValueToken(bool meetComma, bool meetColon, string segment)
    {
        bool shouldThrow = false;

        if (_positionStack.TryPeek(out ReaderStackPosition position))
        {
            if (position == ReaderStackPosition.MeetOperator)
            {
                if (TokenType != JsonQueryTokenType.Operator) // a + b value
                {
                    shouldThrow = true;
                }
            }
            else if (position == ReaderStackPosition.InArray)
            {
                if (!meetComma && TokenType != JsonQueryTokenType.StartBracket && TokenType != JsonQueryTokenType.Pipe && TokenType != JsonQueryTokenType.Operator)
                {
                    shouldThrow = true;
                }
            }
            else if (position == ReaderStackPosition.InFunction)
            {
                if (!meetComma && TokenType != JsonQueryTokenType.StartParenthesis && TokenType != JsonQueryTokenType.Pipe && TokenType != JsonQueryTokenType.Operator)
                {
                    shouldThrow = true;
                }
            }
            else if (position == ReaderStackPosition.InParenthesis)
            {
                if (TokenType != JsonQueryTokenType.StartParenthesis && TokenType != JsonQueryTokenType.Pipe && TokenType != JsonQueryTokenType.Operator)
                {
                    shouldThrow = true;
                }
            }
            else if (position == ReaderStackPosition.InObject)
            {
                shouldThrow = true;
            }
            else
            {
                Debug.Assert(position == ReaderStackPosition.InObjectKeyValuePair);

                if (!meetColon && TokenType != JsonQueryTokenType.Pipe && TokenType != JsonQueryTokenType.Operator)
                {
                    shouldThrow = true;
                }
            }
        }
        else
        {
            if (TokenType != JsonQueryTokenType.None && TokenType != JsonQueryTokenType.Pipe && TokenType != JsonQueryTokenType.Operator)
            {
                shouldThrow = true;
            }
        }

        if (shouldThrow)
        {
            throw new JsonQueryParseException($"Unexpected '{segment}'", _index);
        }
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

    private void ReadObjectPropertyName()
    {
        ReadOnlySpan<char> propertyName;

        if (_jsonQuery[_index] == '"')
        {
            propertyName = ReadQuotedString(ref _index);
            _index++; // make idx be after right side '"'
        }
        else
        {
            int idx = _index;
            while (!HitSeparators(idx))
            {
                idx++;
            }

            propertyName = _jsonQuery.Slice(_index, idx - _index);
            VerifySingleUnquotedPropertyName(propertyName);

            _index = idx;
        }

        _propertyName = propertyName.ToString();
        TokenType = JsonQueryTokenType.PropertyName;
        _positionStack.Push(ReaderStackPosition.InObjectKeyValuePair);
    }

    private void ReadStringLiteral(bool meetComma, bool meetColon)
    {
        Debug.Assert(_jsonQuery[_index] == '"');

        ValidateValueToken(meetComma, meetColon, "\"");
        
        _stringLiteral = ReadQuotedString(ref _index);
        TokenType = JsonQueryTokenType.String;
        _index++;
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

    private void WalkOutOfWhiteSpaces(ref int idx)
    {
        while (!OutOfJsonQuery(idx))
        {
            if (char.IsWhiteSpace(_jsonQuery[idx]))
            {
                idx++;
            }
            else
            {
                return;
            }
        }
    }

    private void ReadPropertyPath()
    {
        Debug.Assert(_jsonQuery[_index] == '.');

        var propertiesPath = new List<object>();
        int idx = _index + 1;
        bool isPathEnd = false;

        while (!isPathEnd)
        {
            ReadOnlySpan<char> onePropertySpan;
            int startIdx = idx;

            if (HitSeparators(startIdx) || _jsonQuery[startIdx] != '"')
            {
                while (!HitSeparators(idx) && _jsonQuery[idx] != '.')
                {
                    idx++;
                }

                onePropertySpan = _jsonQuery.Slice(startIdx, idx - startIdx);
                VerifySingleUnquotedPropertyName(onePropertySpan);

                if (HitSeparators(idx))
                {
                    isPathEnd = true;
                }
                else
                {
                    Debug.Assert(_jsonQuery[idx] == '.');
                    idx++;
                }
            }
            else
            {
                Debug.Assert(_jsonQuery[startIdx] == '"');

                onePropertySpan = ReadQuotedString(ref idx);

                idx++; // make idx be after right side '"'

                if (!OutOfJsonQuery(idx) && _jsonQuery[idx] == '.')
                {
                    idx++;
                }
                else
                {
                    isPathEnd = true;
                }
            }

            if (int.TryParse(onePropertySpan, out int arrayIdx))
            {
                propertiesPath.Add(arrayIdx);
            }
            else
            {
                propertiesPath.Add(onePropertySpan.ToString());
            }
        }

        TokenType = JsonQueryTokenType.PropertyPath;
        _propertyPath = propertiesPath.ToArray();
        _index = idx;
    }

    private string ReadQuotedString(ref int idx)
    {
        Debug.Assert(_jsonQuery[idx] == '"');

        int startIdx = idx;
        
        idx++;

        for (; !OutOfJsonQuery(idx); idx++)
        {
            if (_jsonQuery[idx] == '\\')
            {
                idx++;
                if (OutOfJsonQuery(idx))
                {
                    throw new JsonQueryParseException("Unrecognized escape sequence", idx);
                }
            }
            else if (_jsonQuery[idx] == '"') // found end of string
            {
                ReadOnlySpan<char> escapedString = _jsonQuery.Slice(startIdx, idx - startIdx + 1);
                try
                {
                    return JsonSerializer.Deserialize<string>(escapedString)!;
                }
                catch (Exception)
                {
                    throw new JsonQueryParseException($"Invalid quoted string: {escapedString.ToString()}", idx);
                }
            }
        }

        throw new JsonQueryParseException("Missing '\"'", idx);
    }

    private void VerifySingleUnquotedPropertyName(ReadOnlySpan<char> propertyName)
    {
        if (propertyName.IsEmpty)
        {
            throw new JsonQueryParseException("Unquoted empty property name is invalid", _index);
        }

        if (propertyName[0] >= '0' && propertyName[0] <= '9')
        {
            if (uint.TryParse(propertyName, out _))
            {
                return;
            }

            throw new JsonQueryParseException($"Invalid unquoted property name: '{propertyName.ToString()}'", _index);
        }

        foreach (char c in propertyName)
        {
            if ((c >= '0' && c <= '9') || (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || c == '_' || c == '$')
            {
                continue;
            }

            throw new JsonQueryParseException($"Invalid unquoted property name: '{propertyName.ToString()}'", _index);
        }
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

public class JsonQueryParseException : Exception
{
    public JsonQueryParseException(string message, int pos) : base($"{message} (pos: {pos})")
    {
    }
}

public enum JsonQueryTokenType
{
    /// <summary>
    /// There is no value (as distinct from <see cref="Null"/>). This is the default token type if no data has been read by the <see cref="JsonQueryReader"/>.
    /// </summary>
    None,

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
    InObjectKeyValuePair,
    MeetOperator
}