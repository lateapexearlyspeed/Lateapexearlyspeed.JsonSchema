using System.Buffers;
using System.Text.Json;

namespace LateApexEarlySpeed.Json.Schema.Common;

internal static class Utf8JsonReaderExtensions
{
    public static bool TryGetUInt32ForJsonSchema(this Utf8JsonReader reader, out uint value)
    {
        if (reader.TryGetUInt32(out value))
        {
            return true;
        }

        if (reader.TokenType != JsonTokenType.Number)
        {
            return false;
        }

        ReadOnlySpan<byte> span;

        if (reader.HasValueSequence)
        {
            byte[] buffer = new byte[reader.ValueSequence.Length];
            reader.ValueSequence.CopyTo(buffer);

            span = buffer;
        }
        else
        {
            span = reader.ValueSpan;
        }

        if (span[0] == (byte)'-')
        {
            return false;
        }

        if (span.IndexOf((byte)'e') != -1)
        {
            return false;
        }

        int dotIdx = span.IndexOf((byte)'.');
        if (dotIdx != -1)
        {
            ReadOnlySpan<byte> decimalPart = span.Slice(dotIdx + 1);

            foreach (byte b in decimalPart)
            {
                if (b != (byte)'0')
                {
                    return false;
                }
            }
        }

        value = (uint)reader.GetDouble();
        return true;
    }

    /// <summary>
    /// This method will check the numeric range and convert to corresponding type. The matching order is: long -> ulong -> decimal -> double.
    /// This method will ensure one parameter will be set as <see cref="Nullable{T}.HasValue"/> unless exception thrown.
    /// </summary>
    public static void GetNumericValue(this Utf8JsonReader reader, out long? longValue, out ulong? unsignedLongValue, out decimal? decimalValue, out double? doubleValue)
    {
        if (reader.TryGetInt64(out long tmpLong))
        {
            longValue = tmpLong;
            
            unsignedLongValue = null;
            decimalValue = null;
            doubleValue = null;
            return;
        }

        if (reader.TryGetUInt64(out ulong tmpULong))
        {
            unsignedLongValue = tmpULong;

            longValue = null;
            decimalValue = null;
            doubleValue = null;
            return;
        }

        if (reader.TryGetDecimal(out decimal tmpDecimal))
        {
            decimalValue = tmpDecimal;

            longValue = null;
            unsignedLongValue = null;
            doubleValue = null;
            return;
        }

        doubleValue = reader.GetDouble();
        
        longValue = null;
        unsignedLongValue = null;
        decimalValue = null;
    }
}