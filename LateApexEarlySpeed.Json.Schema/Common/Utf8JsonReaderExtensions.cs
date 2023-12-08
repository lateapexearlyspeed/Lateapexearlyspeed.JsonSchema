using System;
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
}