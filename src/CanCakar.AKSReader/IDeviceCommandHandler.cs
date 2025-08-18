namespace CanCakar.AKSReader
{
    internal interface IDeviceCommandHandler
    {
        string CreateBcc(ReadOnlySpan<byte> bytes);
        byte[] CreateCommand(byte ReaderId, ReadOnlySpan<byte> data);
        byte[]? GetDataPart(ReadOnlySpan<byte> buffer);
    }
}
