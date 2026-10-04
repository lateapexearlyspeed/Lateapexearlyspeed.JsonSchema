using System.Runtime.CompilerServices;

namespace LateApexEarlySpeed.Json.Schema.Common;

internal static class BitmapHelper
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte[] CreateBitmap(int length)
    {
        return new byte[(length + 7) / 8];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SetBit(byte[] bitmap, int index)
    {
        bitmap[index / 8] |= (byte)(1 << (index % 8));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsBitSet(byte[] bitmap, int index)
    {
        return (bitmap[index / 8] & (0x1 << (index % 8))) != 0;
    }
}