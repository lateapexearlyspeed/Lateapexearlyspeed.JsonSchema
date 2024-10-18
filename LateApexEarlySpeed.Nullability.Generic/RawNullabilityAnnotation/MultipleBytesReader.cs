namespace LateApexEarlySpeed.Nullability.Generic.RawNullabilityAnnotation;

internal class MultipleBytesReader : IAnnotationBytesReader
{
    private ReadOnlyMemory<byte> _bytes;

    public MultipleBytesReader(ReadOnlyMemory<byte> bytes)
    {
        _bytes = bytes;
    }

    public byte ReadByte()
    {
        byte b = _bytes.Span[0];
        _bytes = _bytes.Slice(1);

        return b;
    }

    public bool IsEnd => _bytes.IsEmpty;
}