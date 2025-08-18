using CanCakar.AKSReader.Extensions;
using System.IO.Ports;

namespace CanCakar.AKSReader.Communication
{
    internal class AksSerialPortCommunication : IAksDeviceCommunication, IDisposable
    {
        private readonly SerialPort serialPort;

        public AksSerialPortCommunication(string portName, int baudRate, int readTimeout = 500, int writeTimeout = 500)
            : this(new SerialPort(portName, baudRate) { ReadTimeout = readTimeout, WriteTimeout = writeTimeout })
        {
        }


        internal AksSerialPortCommunication(SerialPort serialPortObject)
        {
            serialPort = serialPortObject ?? throw new ArgumentNullException(nameof(serialPortObject));
            serialPort.DtrEnable = true;
            serialPort.RtsEnable = true;
        }

        public bool IsConnected => serialPort.IsOpen;

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
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
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
                catch (OperationCanceledException) when (timeoutCts.IsCancellationRequested)
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
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            using (linkedCts.Token.Register(serialPort.DiscardInBuffer))
            {
                try
                {
                    await serialPort.BaseStream.ReadExactlyAsyncPolyfill(buffer, offset, count, cancellationToken);
                }
                catch (OperationCanceledException) when (timeoutCts.IsCancellationRequested)
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
