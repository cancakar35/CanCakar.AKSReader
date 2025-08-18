namespace CanCakar.AKSReader.Communication
{
    internal interface IAksDeviceCommunication : IDisposable
    {
        bool IsConnected { get; }
        void Connect();
        void Disconnect();
        Task ConnectAsync();
        void Write(byte[] data);
        Task WriteAsync(byte[] data, CancellationToken cancellationToken = default);
        void Read(byte[] buffer, int offset, int count);
        Task ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken = default);
    }
}
