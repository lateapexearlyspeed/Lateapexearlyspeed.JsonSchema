namespace LateApexEarlySpeed.Nullability.Generic.RawNullabilityAnnotation;

internal class SingleByteReader : IAnnotationBytesReader
{
    private readonly byte _byte;

    public SingleByteReader(byte b)
    {
        _byte = b;
    }

    public byte ReadByte()
    {
        return _byte;
    }
}