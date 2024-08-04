using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace LateApexEarlySpeed.Json.Schema.Common;

internal class LazyCompiledRegex
{
    private const int CountToCompileRegex = 100;
    private const int FinalStateCount = -1;

    private readonly string _pattern;
    private readonly TimeSpan _matchTimeout;

    /// <summary>
    /// When current value exceeds <see cref="CountToCompileRegex"/>, it will turn to <see cref="FinalStateCount"/>, which represents final-state
    /// </summary>
    private volatile int _counter;
    private volatile Regex _regex;

    public LazyCompiledRegex([StringSyntax(StringSyntaxAttribute.Regex, "options")] string pattern, TimeSpan matchTimeout)
    {
        _pattern = pattern;
        _matchTimeout = matchTimeout;
        _regex = RegexFactory.Create(pattern, RegexOptions.None, matchTimeout);
    }

    /// <summary>
    /// This method counts calls. In initial calls within <see cref="CountToCompileRegex"/>, it uses default <see cref="RegexOptions"/> to check match;
    /// after calls exceeds <see cref="CountToCompileRegex"/>, it will generate <see cref="_regex"/> with <see cref="RegexOptions.Compiled"/> and reuse this new regex instance after that.
    /// </summary>
    /// <remarks>
    /// This method is thread-safe and is a no-lock implementation. The logic is similar as follows:
    ///
    /// if (_counter >= 100)<br/>
    /// {<br/>
    ///     _regex = RegexFactory.Create(_pattern, RegexOptions.Compiled);<br/>
    ///     _counter = -1;<br/>
    /// }<br/>
    /// else if (_counter != -1)<br/>
    /// {<br/>
    ///    _counter++;<br/>
    /// }<br/>
    /// return _regex.IsMatch(input);
    /// 
    /// </remarks>
    public bool IsMatch(string input)
    {
        int localCount = _counter;

        // Final state, just use compiled '_regex'
        if (localCount == FinalStateCount)
        {
            return _regex.IsMatch(input);
        }

        // Time to compile regex
        if (localCount >= CountToCompileRegex)
        {
            _regex = RegexFactory.Create(_pattern, RegexOptions.Compiled, _matchTimeout);
            _counter = FinalStateCount;

            return _regex.IsMatch(input);
        }

        // Increment '_count'. Be careful '_count' is modified by another thread and also take care whether another thread already updated '_count' to 'CountToCompileRegex' or 'FinalStateCount'
        while (true)
        {
            int originalCount = Interlocked.CompareExchange(ref _counter, localCount + 1, localCount);
            if (originalCount == localCount)
            {
                break;
            }

            localCount = _counter;
            if (localCount == FinalStateCount || localCount >= CountToCompileRegex)
            {
                break;
            }
        }

        return _regex.IsMatch(input);
    }

    public override string ToString()
    {
        return _pattern;
    }
}