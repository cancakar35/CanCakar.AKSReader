using CanCakar.AKSReader.Extensions;
using System.IO.Ports;

namespace CanCakar.AKSReader.Communication
{
    internal class AksSerialPortCommunication : IAksDeviceCommunication, IDisposable
    {
        private readonly SerialPort serialPort;
        private int timeout = 500;

        public AksSerialPortCommunication(string portName, int baudRate)
            : this(new SerialPort(portName, baudRate))
        {
            serialPort.DtrEnable = true;
            serialPort.RtsEnable = true;
            serialPort.ReadTimeout = timeout;
            serialPort.WriteTimeout = timeout;
        }

        internal AksSerialPortCommunication(SerialPort serialPortObject)
        {
            serialPort = serialPortObject ?? throw new ArgumentNullException(nameof(serialPortObject));
        }

        public bool IsConnected => serialPort.IsOpen;

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
                serialPort.ReadTimeout = value;
                serialPort.WriteTimeout = value;
            }
        }

        public void Connect()
        {
            serialPort.Open();
            serialPort.DiscardInBuffer();
        }

        public void Disconnect()
        {
            if (serialPort.IsOpen)
                serialPort.Close();
        }

        public async Task ConnectAsync()
        {
            await Task.Run(() => serialPort.Open());
        }

        public void Write(byte[] data)
        {
            serialPort.DiscardInBuffer();
            serialPort.Write(data, 0, data.Length);
        }

        public async Task WriteAsync(byte[] data, CancellationToken cancellationToken = default)
        {
            serialPort.DiscardInBuffer();
            using var timeoutCts = new CancellationTokenSource(serialPort.WriteTimeout);
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);
            using (linkedCts.Token.Register(serialPort.DiscardOutBuffer))
            {
                try
                {
#if NET8_0_OR_GREATER
                    await serialPort.BaseStream.WriteAsync(data, linkedCts.Token);
#else
                    await serialPort.BaseStream.WriteAsync(data, 0, data.Length, linkedCts.Token);
#endif
                }
                catch (OperationCanceledException) when (timeoutCts.IsCancellationRequested && !cancellationToken.IsCancellationRequested)
                {
                    throw new TimeoutException("Operation Timed out.");
                }
            }
        }

        public void Read(byte[] buffer, int offset, int count)
        {
            serialPort.BaseStream.ReadExactlyPolyfill(buffer, offset, count);
        }

        public async Task ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken = default)
        {
            using var timeoutCts = new CancellationTokenSource(serialPort.ReadTimeout);
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);
            using (linkedCts.Token.Register(serialPort.DiscardInBuffer))
            {
                try
                {
                    await serialPort.BaseStream.ReadExactlyAsyncPolyfill(buffer, offset, count, cancellationToken);
                }
                catch (OperationCanceledException) when (timeoutCts.IsCancellationRequested && !cancellationToken.IsCancellationRequested)
                {
                    throw new TimeoutException("Operation Timed out.");
                }
            }

        }

        public void Dispose()
        {
            serialPort?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
