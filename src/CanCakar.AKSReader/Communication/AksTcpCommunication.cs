using CanCakar.AKSReader.Extensions;
using System.Net;
using System.Net.Sockets;

namespace CanCakar.AKSReader.Communication
{
    internal class AksTcpCommunication : IAksDeviceCommunication, IDisposable
    {
        private readonly IPAddress ipAddress;
        private readonly int port;
        private int timeout = 500;
        private TcpClient? tcpClient;
        private NetworkStream? networkStream;

        public AksTcpCommunication(IPAddress ipAddress, int port)
        {
            this.ipAddress = ipAddress;
            this.port = port;
        }

        public bool IsConnected => tcpClient?.Connected ?? false;

        public int Timeout
        {
            get => timeout;
            set
            {
                if (value < -1)
                {
                    throw new ArgumentOutOfRangeException(nameof(Timeout));
                }
                timeout = value;
                if (IsConnected && networkStream != null)
                {
                    networkStream.ReadTimeout = value;
                    networkStream.WriteTimeout = value;
                }
            }
        }

        public void Connect()
        {
            tcpClient = new TcpClient();
            tcpClient.Connect(ipAddress, port);
            networkStream = tcpClient.GetStream();
            networkStream.ReadTimeout = timeout;
            networkStream.WriteTimeout = timeout;
        }

        public void Disconnect()
        {
            networkStream?.Close();
            tcpClient?.Dispose();
            networkStream = null;
            tcpClient = null;
        }

        public async Task ConnectAsync()
        {
            tcpClient = new TcpClient();
            await tcpClient.ConnectAsync(ipAddress, port);
            networkStream = tcpClient.GetStream();
            networkStream.ReadTimeout = timeout;
            networkStream.WriteTimeout = timeout;
        }

        public void Write(byte[] data)
        {
            if (networkStream == null)
                throw new InvalidOperationException("No open connection.");

            networkStream.Write(data, 0, data.Length);
        }

        public async Task WriteAsync(byte[] data, CancellationToken cancellationToken = default)
        {
            if (networkStream == null)
                throw new InvalidOperationException("No open connection.");

            if (tcpClient?.Available > 0) // discard previous data if available
            {
                byte[] buffer = new byte[tcpClient.Available];
            #pragma warning disable CA2022 // Avoid inexact read with 'Stream.Read'
            #if NET8_0_OR_GREATER
                await networkStream.ReadAsync(buffer, cancellationToken);
            #else
                await networkStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken);
            #endif
            #pragma warning restore CA2022 // Avoid inexact read with 'Stream.Read'
            }
            using var timeoutCts = new CancellationTokenSource(networkStream.WriteTimeout);
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);
            try
            {
                // send command to device
#if NET8_0_OR_GREATER
                await networkStream.WriteAsync(data, linkedCts.Token);
#else
            await networkStream.WriteAsync(data, 0, data.Length, linkedCts.Token);
#endif
            }
            catch (OperationCanceledException) when (timeoutCts.IsCancellationRequested && !cancellationToken.IsCancellationRequested)
            {
                throw new TimeoutException("Operation Timed out.");
            }
        }

        public void Read(byte[] buffer, int offset, int count)
        {
            if (networkStream == null)
                throw new InvalidOperationException("No open connection.");

            networkStream.ReadExactlyPolyfill(buffer, offset, count);
        }

        public async Task ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken = default)
        {
            if (networkStream == null)
                throw new InvalidOperationException("No open connection.");

            using var timeoutCts = new CancellationTokenSource(networkStream.WriteTimeout);
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);


            try
            {
                await networkStream.ReadExactlyAsyncPolyfill(buffer, offset, count, linkedCts.Token);
            }
            catch (OperationCanceledException) when (timeoutCts.IsCancellationRequested && !cancellationToken.IsCancellationRequested)
            {
                throw new TimeoutException("Operation Timed out.");
            }
        }

        public void Dispose()
        {
            networkStream?.Dispose();
            tcpClient?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
